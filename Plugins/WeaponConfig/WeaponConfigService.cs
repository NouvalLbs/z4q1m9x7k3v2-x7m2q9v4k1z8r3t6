#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal sealed class PlayerWcState
    {
        public float Health = 100f, Armour = 0f;
        public float MaxHealth = 100f, MaxArmour = 100f;
        public bool IsDying, BeingResynced;
        public int Team = 255;
        public bool CbugAllowed = true;
        public int IntendedWorld;

        public int ShotsFired;
        public readonly int[] ShotTicks = new int[10];
        public readonly int[] ShotWeapons = new int[10];
        public int ShotIdx;

        public int HitsIssued;
        public readonly int[] HitTicks = new int[10];
        public readonly int[] HitWeapons = new int[10];
        public int HitIdx;

        public float LastDamageHealth;
        public float LastDamageArmour;
        public float LastZVelo;
        public int LastUpdateTick;
        public int CbugFrozeTick;

        public const int MaxRejectedHits = 15;
        public readonly RejectedHit?[] RejectedHits = new RejectedHit?[MaxRejectedHits];
        public int RejectedHitIdx;

        public CancellationTokenSource? DeathCts;
        public CancellationTokenSource? ResyncCts;
    }

    public static class WeaponConfigService
    {
        private const int DeathWorld = 0x00DEAD00;
        private const int BodypartTorso = 1;

        private static WeaponConfig _cfg = new();
        private static WeaponEntry[] _weapons = Array.Empty<WeaponEntry>();
        private static readonly Dictionary<int, PlayerWcState> _states = new();

        public static event EventHandler<PlayerDamageArgs>? PlayerDamage;
        public static event EventHandler<PlayerDamageArgs>? PlayerDamageDone;
        public static event EventHandler<RejectedHitArgs>? RejectedHit;
        public static event EventHandler<PrepareDeathArgs>? PlayerPrepareDeath;
        public static event EventHandler<DeathFinishedArgs>? PlayerDeathFinished;
        public static event EventHandler<InvalidWeaponDamageArgs>? InvalidWeaponDamage;

        private static readonly int[] _validTaken = {
            1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,0,0,0,0,
            0,0,1,1,1,1,1,1,1,1,
            1,1,1,1,1,0,0,2,1,0,
            0,1,1,0,0,0,1,0,0,2,
            2,2,0,2,2
        };

        private static readonly bool[] _validGiven = {
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, false,false,false,false,
            false,false,true, true, true, true, true, true, true, true,
            true, true, true, true, true, false,false,false,true, false,
            false,true, true, false,false,false,true
        };

        // ── Init / Lifecycle ──────────────────────────────────────────────────

        public static void Init(WeaponConfig cfg, WeaponEntry[] weapons)
        {
            _cfg = cfg;
            _weapons = weapons;
            WeaponConfigDamageFeed.Init(cfg.EnableDamageFeed);
        }

        public static void OnConnect(Player p)
        {
            _states[p.Id] = new PlayerWcState { CbugAllowed = _cfg.CbugAllowed };
            WeaponConfigHealthBar.OnConnect(p);
            WeaponConfigDamageFeed.OnConnect(p);
        }

        public static void OnDisconnect(Player p)
        {
            if (_states.TryGetValue(p.Id, out var s))
            {
                s.DeathCts?.Cancel();
                s.ResyncCts?.Cancel();
            }
            _states.Remove(p.Id);
            WeaponConfigHealthBar.OnDisconnect(p);
            WeaponConfigDamageFeed.OnDisconnect(p);
        }

        public static void OnSpawn(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.DeathCts?.Cancel();
            s.Health = s.MaxHealth;
            s.Armour = 0;
            s.IsDying = false;
            s.BeingResynced = false;
            s.ShotsFired = 0;
            s.HitsIssued = 0;
            s.IntendedWorld = p.VirtualWorld;
            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
            WeaponConfigHealthBar.Show(p);
        }

        public static void OnDeath(Player p, Player? killer, int reason)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.IsDying = true;
            s.IntendedWorld = p.VirtualWorld;
            if (!s.CbugAllowed) s.CbugFrozeTick = Environment.TickCount;

            var prep = new PrepareDeathArgs
            {
                Player = p,
                AnimLib = "PED",
                AnimName = GetDeathAnim(reason),
                AnimLock = true,
                RespawnTime = _cfg.RespawnTime
            };
            PlayerPrepareDeath?.Invoke(null, prep);
            if (prep.Cancel) return;

            p.ApplyAnimation(prep.AnimLib, prep.AnimName, 4.0f, false, false, false, prep.AnimLock, 0);

            var cts = new CancellationTokenSource();
            s.DeathCts = cts;
            _ = RunDeathAsync(p, s, prep.RespawnTime, cts.Token);
        }

        public static void OnUpdate(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.LastUpdateTick = Environment.TickCount;
            if (_cfg.CustomFallDamage && !s.IsDying)
                CheckFallDamage(p, s);
        }

        // ── Damage Handlers ───────────────────────────────────────────────────

        public static void HandleGiveDamage(Player issuer, Player? damaged, float amount, int weaponId, int bodypart)
        {
            if (damaged == null || damaged.IsDisposed)
            {
                Reject(issuer, weaponId, HitRejectReason.Disconnected);
                return;
            }

            if (!_states.TryGetValue(issuer.Id, out var iState)) return;
            if (!_states.TryGetValue(damaged.Id, out var dState)) return;

            if (weaponId < 0 || weaponId >= _validGiven.Length || !_validGiven[weaponId])
            {
                FireInvalid(issuer, damaged, amount, weaponId, bodypart, 1, true);
                Reject(issuer, weaponId, HitRejectReason.InvalidWeapon, targetName: damaged.Name);
                return;
            }

            if (dState.IsDying)
            {
                Reject(issuer, weaponId, HitRejectReason.DyingPlayer, targetName: damaged.Name);
                return;
            }

            if (dState.BeingResynced)
            {
                Reject(issuer, weaponId, HitRejectReason.BeingResynced, targetName: damaged.Name);
                return;
            }

            if (!IsSpawned(damaged, dState))
            {
                Reject(issuer, weaponId, HitRejectReason.NotSpawned, targetName: damaged.Name);
                return;
            }

            if (iState.Team != 255 && iState.Team == dState.Team)
            {
                Reject(issuer, weaponId, HitRejectReason.SameTeam, targetName: damaged.Name);
                return;
            }

            var issuerVeh = issuer.Vehicle?.Id ?? -1;
            var damagedVeh = damaged.Vehicle?.Id ?? -1;
            if (issuerVeh != -1 && issuerVeh == damagedVeh)
            {
                Reject(issuer, weaponId, HitRejectReason.SameVehicle, targetName: damaged.Name);
                return;
            }

            if (!iState.CbugAllowed && iState.IsDying &&
                Environment.TickCount - iState.CbugFrozeTick < 500)
            {
                Reject(issuer, weaponId, HitRejectReason.InvalidWeapon, targetName: damaged.Name);
                return;
            }

            var w = GetEntry(weaponId);
            if (IsBulletWeapon(weaponId) && w.Range > 0)
            {
                var dist = Dist(issuer.Position, damaged.Position);
                if (dist > w.Range * 1.5f)
                {
                    Reject(issuer, weaponId, HitRejectReason.OutOfRange, dist, w.Range * 1.5f, targetName: damaged.Name);
                    return;
                }
            }

            if (!CheckShootRate(issuer, iState, weaponId)) return;
            if (!CheckHitRate(issuer, iState, weaponId)) return;

            TrackShot(iState, weaponId);
            TrackHit(iState, weaponId);

            var actual = CalcDamage(w, amount, issuer.Position, damaged.Position);
            if (actual < 0)
            {
                FireInvalid(issuer, damaged, amount, weaponId, bodypart, 3, true);
                Reject(issuer, weaponId, HitRejectReason.InvalidDamage, amount, targetName: damaged.Name);
                return;
            }

            var args = new PlayerDamageArgs { Player = damaged, Issuer = issuer, Amount = actual, Weapon = weaponId, Bodypart = bodypart };
            PlayerDamage?.Invoke(null, args);
            if (args.Cancel) return;

            Inflict(damaged, dState, args.Amount, weaponId, bodypart, issuer);
            WeaponConfigDamageFeed.AddGiven(issuer, damaged.Name, args.Amount, weaponId);
            WeaponConfigDamageFeed.AddTaken(damaged, issuer.Name, args.Amount, weaponId);
            PlayerDamageDone?.Invoke(null, args);
        }

        public static void HandleTakeDamage(Player damaged, Player? issuer, float amount, int weaponId, int bodypart)
        {
            if (issuer != null && weaponId >= 0 && weaponId < _validGiven.Length && _validGiven[weaponId])
                return;

            if (!_states.TryGetValue(damaged.Id, out var s) || s.IsDying) return;

            if (weaponId < 0 || weaponId >= _validTaken.Length || _validTaken[weaponId] == 0)
            {
                FireInvalid(damaged, issuer, amount, weaponId, bodypart, 1, false);
                return;
            }

            var w = GetEntry(weaponId);
            var actual = w.Type == DamageType.Static ? w.Damage : amount * w.Damage;
            if (actual < 0) { FireInvalid(damaged, issuer, amount, weaponId, bodypart, 3, false); return; }

            var args = new PlayerDamageArgs { Player = damaged, Issuer = issuer, Amount = actual, Weapon = weaponId, Bodypart = bodypart };
            PlayerDamage?.Invoke(null, args);
            if (args.Cancel) return;

            Inflict(damaged, s, args.Amount, weaponId, bodypart, issuer);
            if (issuer != null)
            {
                WeaponConfigDamageFeed.AddGiven(issuer, damaged.Name, args.Amount, weaponId);
                WeaponConfigDamageFeed.AddTaken(damaged, issuer.Name, args.Amount, weaponId);
            }
            PlayerDamageDone?.Invoke(null, args);
        }

        public static void HandleWeaponShot(Player p, int weaponId)
        {
            if (_states.TryGetValue(p.Id, out var s))
                TrackShot(s, weaponId);
        }

        // ── Core Logic ────────────────────────────────────────────────────────

        private static void Inflict(Player p, PlayerWcState s, float amount, int weaponId, int bodypart, Player? issuer = null)
        {
            if (amount <= 0) return;
            var w = GetEntry(weaponId);

            var affectsArmour = !_cfg.GlobalArmourRules && w.AffectsArmour;
            var torsoOnly = !_cfg.GlobalTorsoRules && w.TorsoOnly;

            float aDmg = 0, hDmg;
            if (affectsArmour && (!torsoOnly || bodypart == BodypartTorso))
            {
                aDmg = MathF.Min(amount, s.Armour);
                s.Armour -= aDmg;
                hDmg = amount - aDmg;
            }
            else hDmg = amount;

            s.LastDamageHealth = hDmg;
            s.LastDamageArmour = aDmg;
            s.Health = MathF.Max(0, s.Health - hDmg);

            if (s.Health <= 0)
            {
                s.Armour = 0;
                p.Health = 0f;
            }
            else
            {
                p.Health = MathF.Min(s.Health, 100f);
                p.Armour = MathF.Min(s.Armour, 100f);
            }

            if (hDmg > 0 || aDmg > 0)
            {
                p.PlaySound(_cfg.DamageTakenSound);
                issuer?.PlaySound(_cfg.DamageGivenSound);
            }

            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
        }

        private static void CheckFallDamage(Player p, PlayerWcState s)
        {
            var zVelo = p.Velocity.Z;
            if (zVelo < _cfg.FallDeathVelocity && s.LastZVelo >= _cfg.FallDeathVelocity)
            {
                var dmg = MathF.Abs(zVelo) * _cfg.FallDamageMultiplier;
                Inflict(p, s, dmg, 54, 0);
            }
            s.LastZVelo = zVelo;
        }

        private static bool CheckShootRate(Player p, PlayerWcState s, int weaponId)
        {
            var w = GetEntry(weaponId);
            if (w.MaxShootRate <= 0 || s.ShotsFired < 2) return true;

            var n = Math.Min(s.ShotsFired, _cfg.MaxShootRateSamples);
            if (n < 2) return true;

            var total = 0;
            var multi = false;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.ShotIdx - i - 1 + s.ShotTicks.Length) % s.ShotTicks.Length;
                var bi = (s.ShotIdx - i - 2 + s.ShotTicks.Length) % s.ShotTicks.Length;
                total += s.ShotTicks[ai] - s.ShotTicks[bi];
                if (s.ShotWeapons[ai] != s.ShotWeapons[bi]) multi = true;
            }
            var avg = total / (n - 1);
            if (avg < w.MaxShootRate)
            {
                Reject(p, weaponId, multi ? HitRejectReason.ShootRateTooFastMultiple : HitRejectReason.ShootRateTooFast,
                    avg, n, multi ? 0 : w.MaxShootRate);
                return false;
            }
            return true;
        }

        private static bool CheckHitRate(Player p, PlayerWcState s, int weaponId)
        {
            var w = GetEntry(weaponId);
            if (w.MaxShootRate <= 0 || s.HitsIssued < 2) return true;

            var n = Math.Min(s.HitsIssued, _cfg.MaxHitRateSamples);
            if (n < 2) return true;

            var total = 0;
            var multi = false;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.HitIdx - i - 1 + s.HitTicks.Length) % s.HitTicks.Length;
                var bi = (s.HitIdx - i - 2 + s.HitTicks.Length) % s.HitTicks.Length;
                total += s.HitTicks[ai] - s.HitTicks[bi];
                if (s.HitWeapons[ai] != s.HitWeapons[bi]) multi = true;
            }
            var avg = total / (n - 1);
            if (avg < w.MaxShootRate)
            {
                Reject(p, weaponId, multi ? HitRejectReason.HitRateTooFastMultiple : HitRejectReason.HitRateTooFast,
                    avg, n, multi ? 0 : w.MaxShootRate);
                return false;
            }
            return true;
        }

        private static void TrackShot(PlayerWcState s, int wid)
        {
            s.ShotTicks[s.ShotIdx] = Environment.TickCount;
            s.ShotWeapons[s.ShotIdx] = wid;
            s.ShotIdx = (s.ShotIdx + 1) % s.ShotTicks.Length;
            s.ShotsFired++;
        }

        private static void TrackHit(PlayerWcState s, int wid)
        {
            s.HitTicks[s.HitIdx] = Environment.TickCount;
            s.HitWeapons[s.HitIdx] = wid;
            s.HitIdx = (s.HitIdx + 1) % s.HitTicks.Length;
            s.HitsIssued++;
        }

        private static async Task RunDeathAsync(Player p, PlayerWcState s, int delayMs, CancellationToken ct)
        {
            try
            {
                p.VirtualWorld = DeathWorld;
                await Task.Delay(delayMs, ct);
                if (p.IsDisposed) return;

                s.IsDying = false;
                s.Health = s.MaxHealth;
                s.Armour = 0;
                p.VirtualWorld = s.IntendedWorld;

                PlayerDeathFinished?.Invoke(null, new DeathFinishedArgs { Player = p, Cancelable = true });
            }
            catch (OperationCanceledException) { }
        }

        private static void Reject(Player p, int wid, HitRejectReason r,
            float i1 = 0, float i2 = 0, float i3 = 0, string targetName = "")
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;

            var now = DateTime.Now;
            var hit = new RejectedHit
            {
                Time = Environment.TickCount,
                Hour = now.Hour,
                Minute = now.Minute,
                Second = now.Second,
                Weapon = wid,
                Reason = r,
                Info1 = i1,
                Info2 = i2,
                Info3 = i3,
                TargetName = targetName
            };
            s.RejectedHits[s.RejectedHitIdx % PlayerWcState.MaxRejectedHits] = hit;
            s.RejectedHitIdx++;

            RejectedHit?.Invoke(null, new RejectedHitArgs
            {
                Player = p,
                Weapon = wid,
                Reason = r,
                Info1 = i1,
                Info2 = i2,
                Info3 = i3,
                TargetName = targetName
            });
        }

        private static void FireInvalid(Player p, Player? damaged, float amt, int wid, int bp, int err, bool given)
            => InvalidWeaponDamage?.Invoke(null, new InvalidWeaponDamageArgs
            {
                Player = p,
                Damaged = damaged,
                Amount = amt,
                Weapon = wid,
                Bodypart = bp,
                Error = err,
                Given = given
            });

        private static void SyncFakeVitals(Player p, PlayerWcState s)
        {
            try
            {
                Plugins.SKY.SkyNatives.Instance.SetFakeHealth(p.Id, (int)MathF.Round(MathF.Min(s.Health, 100f)));
                Plugins.SKY.SkyNatives.Instance.SetFakeArmour(p.Id, (int)MathF.Round(MathF.Min(s.Armour, 100f)));
            }
            catch { }
        }

        private static float CalcDamage(WeaponEntry w, float given, Vector3 a, Vector3 b) => w.Type switch
        {
            DamageType.Static => w.Damage,
            DamageType.Multiplier => given * w.Damage,
            DamageType.Range => CalcRange(w, Dist(a, b)),
            DamageType.RangeMultiplier => CalcRange(w, Dist(a, b)) * given,
            _ => w.Damage
        };

        private static float CalcRange(WeaponEntry w, float dist)
        {
            var dmg = w.Damage;
            foreach (var step in w.RangeSteps)
            {
                if (dist > step.Range) dmg = step.Damage;
                else break;
            }
            return dmg;
        }

        private static bool IsSpawned(Player p, PlayerWcState s)
        {
            if (s.IsDying || s.BeingResynced) return false;
            var st = (int)p.State;
            return st >= 1 && st <= 6;
        }

        private static WeaponEntry GetEntry(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id] : new WeaponEntry { Id = id };

        private static float Dist(Vector3 a, Vector3 b)
        {
            var dx = a.X - b.X; var dy = a.Y - b.Y; var dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static string GetDeathAnim(int wid) => wid switch
        {
            >= 22 and <= 34 or 38 => "CRACK_DEAD_FLOOR",
            50 => "KD_SKYDIVE_DIVE",
            _ => "CRACK_DEAD_FLOOR"
        };

        // ── Public API ────────────────────────────────────────────────────────

        public static bool IsBulletWeapon(int id) => (id >= 22 && id <= 34) || id == 38;
        public static bool IsMeleeWeapon(int id) => (id >= 0 && id <= 15) || id == 48;
        public static bool IsHighRateWeapon(int id) => id == 37 || id == 41 || id == 42;

        public static bool IsPlayerSpawned(Player p)
            => _states.TryGetValue(p.Id, out var s) && IsSpawned(p, s);

        public static int GetIntendedVirtualWorld(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.IntendedWorld : p.VirtualWorld;

        public static void SendWcDeathMessage(Player? killer, Player killee, int weapon)
        {
            var mapped = weapon switch
            {
                19 => 31,
                20 => 38,
                21 => 35,
                48 => 0,
                52 => 49,
                55 => 53,
                _ => weapon
            };
            BasePlayer.SendDeathMessageToAll(killer, killee, (SampSharp.GameMode.Definitions.Weapon)mapped);
        }

        public static void SetDamageFeed(bool enable)
        {
            _cfg.EnableDamageFeed = enable;
            foreach (var id in _states.Keys)
            {
                var p = BasePlayer.Find(id) as Player;
                if (p == null) continue;
                WeaponConfigDamageFeed.SetEnabled(p, enable);
            }
        }

        public static bool IsDamageFeedActive(Player? p = null)
        {
            if (p != null) return WeaponConfigDamageFeed.IsEnabled(p);
            return _cfg.EnableDamageFeed;
        }

        public static void DamagePlayer(Player p, float amount, Player? issuer = null, int weapon = 55, int bodypart = 0)
        {
            if (!_states.TryGetValue(p.Id, out var s) || s.IsDying || p.IsDisposed) return;
            Inflict(p, s, amount, weapon, bodypart, issuer);
        }

        public static void SetWeaponDamage(int id, float dmg, DamageType type = DamageType.Static)
        {
            if (id >= 0 && id < _weapons.Length) { _weapons[id].Damage = dmg; _weapons[id].Type = type; }
        }
        public static float GetWeaponDamage(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].Damage : 0f;

        public static void SetWeaponMaxRange(int id, float range)
        {
            if (id >= 0 && id < _weapons.Length) _weapons[id].Range = range;
        }
        public static float GetWeaponMaxRange(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].Range : 0f;

        public static void SetWeaponShootRate(int id, int rate)
        {
            if (id >= 0 && id < _weapons.Length) _weapons[id].MaxShootRate = rate;
        }
        public static int GetWeaponShootRate(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].MaxShootRate : 0;

        public static void SetWeaponName(int id, string name)
        {
            if (id >= 0 && id < _weapons.Length) _weapons[id].Name = name;
        }
        public static string GetWeaponName(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].Name : $"Weapon {id}";

        public static void SetWeaponArmourRule(int id, bool affectsArmour, bool torsoOnly = false)
        {
            if (id >= 0 && id < _weapons.Length)
            { _weapons[id].AffectsArmour = affectsArmour; _weapons[id].TorsoOnly = torsoOnly; }
        }
        public static void SetCustomArmourRules(bool armourRules, bool torsoRules = false)
        {
            _cfg.GlobalArmourRules = armourRules;
            _cfg.GlobalTorsoRules = torsoRules;
        }

        public static WeaponEntry? GetWeaponEntry(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id] : null;

        public static void SetPlayerMaxHealth(Player p, float v)
        { if (_states.TryGetValue(p.Id, out var s)) s.MaxHealth = v; }
        public static void SetPlayerMaxArmour(Player p, float v)
        { if (_states.TryGetValue(p.Id, out var s)) s.MaxArmour = v; }
        public static float GetPlayerMaxHealth(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.MaxHealth : 0f;
        public static float GetPlayerMaxArmour(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.MaxArmour : 0f;

        public static float GetWcHealth(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.Health : 0f;
        public static float GetWcArmour(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.Armour : 0f;

        public static float GetLastDamageHealth(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.LastDamageHealth : 0f;
        public static float GetLastDamageArmour(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.LastDamageArmour : 0f;

        public static bool IsPlayerDying(Player p)
            => _states.TryGetValue(p.Id, out var s) && s.IsDying;
        public static bool IsPlayerPaused(Player p)
            => _states.TryGetValue(p.Id, out var s) && Environment.TickCount - s.LastUpdateTick > 2000;

        public static void SetPlayerTeam(Player p, int team)
        {
            if (_states.TryGetValue(p.Id, out var s)) { s.Team = team; p.Team = team; }
        }

        public static void SetCbugAllowed(bool allowed, Player? p = null)
        {
            if (p != null) { if (_states.TryGetValue(p.Id, out var s)) s.CbugAllowed = allowed; return; }
            _cfg.CbugAllowed = allowed;
            foreach (var s in _states.Values) s.CbugAllowed = allowed;
        }
        public static bool GetCbugAllowed(Player? p = null)
        {
            if (p != null) return _states.TryGetValue(p.Id, out var s) && s.CbugAllowed;
            return _cfg.CbugAllowed;
        }
        public static void SetCbugDeathDelay(bool v) => _cfg.CbugDeathDelay = v;

        public static void SetVehiclePassengerDamage(bool v) => _cfg.VehiclePassengerDamage = v;
        public static void SetVehicleUnoccupiedDamage(bool v) => _cfg.VehicleUnoccupiedDamage = v;

        public static void SetCustomFallDamage(bool toggle, float mult = 25f, float deathVel = -0.6f)
        {
            _cfg.CustomFallDamage = toggle;
            _cfg.FallDamageMultiplier = mult;
            _cfg.FallDeathVelocity = -MathF.Abs(deathVel);
            if (toggle && 54 < _weapons.Length) _weapons[54].Damage = mult;
        }

        public static void SetRespawnTime(int ms) => _cfg.RespawnTime = Math.Max(0, ms);
        public static int GetRespawnTime() => _cfg.RespawnTime;

        public static void SetDamageSounds(int taken, int given)
        {
            _cfg.DamageTakenSound = taken;
            _cfg.DamageGivenSound = given;
        }

        public static void EnableHealthBarForPlayer(Player p, bool enable)
            => WeaponConfigHealthBar.SetEnabled(p, enable);
        public static void SetDamageFeedForPlayer(Player p, bool enable)
            => WeaponConfigDamageFeed.SetEnabled(p, enable);

        public static int AverageShootRate(Player p, int shots)
        {
            if (!_states.TryGetValue(p.Id, out var s) || s.ShotsFired < shots) return -1;
            var n = Math.Min(shots, s.ShotTicks.Length);
            if (n < 2) return 1;
            var total = 0;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.ShotIdx - i - 1 + s.ShotTicks.Length) % s.ShotTicks.Length;
                var bi = (s.ShotIdx - i - 2 + s.ShotTicks.Length) % s.ShotTicks.Length;
                total += s.ShotTicks[ai] - s.ShotTicks[bi];
            }
            return total / (n - 1);
        }

        public static int AverageHitRate(Player p, int hits)
        {
            if (!_states.TryGetValue(p.Id, out var s) || s.HitsIssued < hits) return -1;
            var n = Math.Min(hits, s.HitTicks.Length);
            if (n < 2) return 1;
            var total = 0;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.HitIdx - i - 1 + s.HitTicks.Length) % s.HitTicks.Length;
                var bi = (s.HitIdx - i - 2 + s.HitTicks.Length) % s.HitTicks.Length;
                total += s.HitTicks[ai] - s.HitTicks[bi];
            }
            return total / (n - 1);
        }

        public static RejectedHit? GetRejectedHit(Player p, int idx)
        {
            if (!_states.TryGetValue(p.Id, out var s) || idx >= PlayerWcState.MaxRejectedHits) return null;
            var realIdx = ((s.RejectedHitIdx - idx - 1) % PlayerWcState.MaxRejectedHits + PlayerWcState.MaxRejectedHits) % PlayerWcState.MaxRejectedHits;
            return s.RejectedHits[realIdx];
        }

        public static void ResyncPlayer(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.ResyncCts?.Cancel();
            s.BeingResynced = true;
            p.Position = p.Position;
            var cts = new CancellationTokenSource();
            s.ResyncCts = cts;
            _ = ClearResyncAsync(s, cts.Token);
        }

        private static async Task ClearResyncAsync(PlayerWcState s, CancellationToken ct)
        {
            try { await Task.Delay(500, ct); s.BeingResynced = false; }
            catch (OperationCanceledException) { }
        }
    }
}