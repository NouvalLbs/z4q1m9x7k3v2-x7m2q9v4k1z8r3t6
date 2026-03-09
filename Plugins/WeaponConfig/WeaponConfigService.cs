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

        public ShotInfo? LastShot;
        public readonly int[] LastHitIssuers = new int[10];
        public readonly int[] LastHitWeaponsPerTarget = new int[10];

        public ResyncSnapshot? ResyncSnap;

        public const int MaxRejectedHits = 15;
        public readonly RejectedHit?[] RejectedHits = new RejectedHit?[MaxRejectedHits];
        public int RejectedHitIdx;

        public CancellationTokenSource? DeathCts;
        public CancellationTokenSource? ResyncCts;
    }

    internal sealed class ShotInfo
    {
        public int Tick;
        public int WeaponId;
        public int HitType;
        public int HitId;
        public int Hits;
        public Vector3 Origin;
        public Vector3 HitPos;
        public bool Valid;
    }

    public static class WeaponConfigService
    {
        // ── Constants ────────────────────────────────────────────────────

        private const int DeathWorld = 0x00DEAD00;
        private const int BodypartTorso = 1;
        private const int BodypartHead = 9;
        private const float MaxDistFromShot = 5f;
        private const float MaxDistFromOrigin = 5f;
        private const float PlayerStreamDistance = 200f;
        private const int ShotTimeoutMs = 1000;

        // ── State ────────────────────────────────────────────────────────

        private static WeaponConfig _cfg = new();
        private static WeaponEntry[] _weapons = Array.Empty<WeaponEntry>();
        private static readonly Dictionary<int, PlayerWcState> _states = new();

        // ── Public events ────────────────────────────────────────────────

        public static event EventHandler<PlayerDamageArgs>? PlayerDamage;
        public static event EventHandler<PlayerDamageArgs>? PlayerDamageDone;
        public static event EventHandler<RejectedHitArgs>? RejectedHit;
        public static event EventHandler<PrepareDeathArgs>? PlayerPrepareDeath;
        public static event EventHandler<DeathFinishedArgs>? PlayerDeathFinished;
        public static event EventHandler<InvalidWeaponDamageArgs>? InvalidWeaponDamage;
        public static event EventHandler<VendingMachineArgs>? PlayerUseVendingMachine;

        // ── Valid-weapon lookup tables ────────────────────────────────────

        private static readonly int[] _validTaken =
        {
            1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,0,0,0,0,
            0,0,1,1,1,1,1,1,1,1,
            1,1,1,1,1,0,0,2,1,0,
            0,1,1,0,0,0,1,0,0,2,
            2,2,0,2,2
        };

        private static readonly bool[] _validGiven =
        {
            true,  true,  true,  true,  true,  true,  true,  true,  true,  true,
            true,  true,  true,  true,  true,  true,  false, false, false, false,
            false, false, true,  true,  true,  true,  true,  true,  true,  true,
            true,  true,  true,  true,  true,  false, false, false, true,  false,
            false, true,  true,  false, false, false, true,  false, true
        };

        // ── Init ─────────────────────────────────────────────────────────

        public static void Init(WeaponConfig cfg, WeaponEntry[] weapons)
        {
            _cfg = cfg;
            _weapons = weapons;
            WeaponConfigDamageFeed.Init(cfg.EnableDamageFeed, cfg.DamageFeedHideDelay);
            WeaponConfigHealthBar.Init();
            WeaponConfigVendingMachines.Init(cfg.CustomVendingMachines);

            // Forward vending machine event so callers can subscribe via the service
            WeaponConfigVendingMachines.PlayerUseVendingMachine += (s, e)
                => PlayerUseVendingMachine?.Invoke(s, e);
        }

        public static void Shutdown()
        {
            WeaponConfigHealthBar.Dispose();
            WeaponConfigVendingMachines.Dispose();
        }

        // ── Lifecycle ────────────────────────────────────────────────────

        public static void OnConnect(Player p)
        {
            _states[p.Id] = new PlayerWcState { CbugAllowed = _cfg.CbugAllowed };
            WeaponConfigHealthBar.OnConnect(p);
            WeaponConfigDamageFeed.OnConnect(p);
            WeaponConfigVendingMachines.OnConnect(p);

            if (!_cfg.EnableHealthBar) WeaponConfigHealthBar.SetEnabled(p, false);
            if (!_cfg.EnableDamageFeed) WeaponConfigDamageFeed.SetEnabled(p, false);
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
            WeaponConfigVendingMachines.OnDisconnect(p);
        }

        public static void OnSpawn(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.DeathCts?.Cancel();

            var snap = s.ResyncSnap;
            s.ResyncSnap = null;

            if (snap != null)
            {
                // Restore health/armour only; weapons are handled by the game-mode spawn logic.
                s.Health = snap.Health;
                s.Armour = snap.Armour;
            }
            else
            {
                s.Health = s.MaxHealth;
                s.Armour = 0;
            }

            s.IsDying = false;
            s.BeingResynced = false;
            s.ShotsFired = 0;
            s.HitsIssued = 0;
            s.LastShot = null;
            s.IntendedWorld = p.VirtualWorld;
            p.Health = 99999f;
            p.Armour = 0f;
            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
            WeaponConfigHealthBar.Show(p);
        }

        public static void OnDeath(Player p, Player? killer, int reason)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.IsDying = true;
            s.IntendedWorld = p.VirtualWorld;
            if (_cfg.CbugDeathDelay && !s.CbugAllowed)
                s.CbugFrozeTick = Environment.TickCount;

            var (lib, anim) = GetDeathAnim(reason, 0); // bodypart not available from OnPlayerDeath
            var prep = new PrepareDeathArgs
            {
                Player = p,
                AnimLib = lib,
                AnimName = anim,
                AnimLock = true,
                RespawnTime = _cfg.RespawnTime
            };
            PlayerPrepareDeath?.Invoke(null, prep);
            if (prep.Cancel) return;

            p.ApplyAnimation(prep.AnimLib, prep.AnimName, 4.0f, false, false, false, prep.AnimLock, 0, true);

            var cts = new CancellationTokenSource();
            s.DeathCts = cts;
            _ = RunDeathAsync(p, s, prep.RespawnTime, cts.Token, cancelable: false)
                .ContinueWith(t => Console.WriteLine(
                    $"[WeaponConfig] RunDeath: {t.Exception?.InnerException?.Message}"),
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void OnUpdate(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.LastUpdateTick = Environment.TickCount;
            if (_cfg.CustomFallDamage && !s.IsDying) CheckFallDamage(p, s);
            if (_cfg.CustomVendingMachines) WeaponConfigVendingMachines.OnUpdate(p);
        }

        // ── Spectate ─────────────────────────────────────────────────────

        /// <summary>
        /// Call when a player starts spectating another.
        /// Their on-screen damage feed will mirror the spectated player's feed.
        /// </summary>
        public static void OnStartSpectating(Player spectator, Player target)
            => WeaponConfigDamageFeed.SetSpectating(spectator, target.Id);

        /// <summary>Call when a player stops spectating.</summary>
        public static void OnStopSpectating(Player spectator)
            => WeaponConfigDamageFeed.ClearSpectating(spectator);

        // ── Weapon shot & damage handlers ────────────────────────────────

        public static void HandleWeaponShot(Player p, int weaponId, int hitType, int hitId,
            Vector3 origin, Vector3 hitPos)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;

            s.LastShot = new ShotInfo
            {
                Tick = Environment.TickCount,
                WeaponId = weaponId,
                HitType = hitType,
                HitId = hitId,
                Hits = 0,
                Origin = origin,
                HitPos = hitPos,
                Valid = hitType == 1 || !IsBulletWeapon(weaponId)
            };
            TrackShot(s, weaponId);
        }

        public static void HandleGiveDamage(Player issuer, Player? damaged, float amount,
            int weaponId, int bodypart)
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

            if (issuerVeh != -1 && damagedVeh == -1 && !_cfg.VehiclePassengerDamage)
            {
                Reject(issuer, weaponId, HitRejectReason.OwnVehicle, targetName: damaged.Name);
                return;
            }

            if (!iState.CbugAllowed && iState.IsDying &&
                Environment.TickCount - iState.CbugFrozeTick < 500)
            {
                Reject(issuer, weaponId, HitRejectReason.InvalidWeapon, targetName: damaged.Name);
                return;
            }

            if (IsBulletWeapon(weaponId))
            {
                if (iState.LastShot == null || !iState.LastShot.Valid ||
                    Environment.TickCount - iState.LastShot.Tick > ShotTimeoutMs)
                {
                    Reject(issuer, weaponId, HitRejectReason.LastShotInvalid, targetName: damaged.Name);
                    return;
                }

                if (iState.LastShot.WeaponId != weaponId)
                {
                    iState.LastShot.Valid = false;
                    Reject(issuer, weaponId, HitRejectReason.LastShotInvalid, targetName: damaged.Name);
                    return;
                }

                if (iState.LastShot.HitType != -1 && iState.LastShot.HitType != 1)
                {
                    iState.LastShot.Valid = false;
                    Reject(issuer, weaponId, HitRejectReason.InvalidHitType,
                        iState.LastShot.HitType, targetName: damaged.Name);
                    return;
                }

                var distToTarget = Dist(issuer.Position, damaged.Position);
                if (distToTarget > PlayerStreamDistance)
                {
                    Reject(issuer, weaponId, HitRejectReason.Unstreamed, distToTarget, targetName: damaged.Name);
                    return;
                }

                iState.LastShot.Hits++;

                if (IsShotgunWeapon(weaponId) && iState.LastShot.Hits > 1)
                {
                    iState.LastShot.Valid = false;
                    Reject(issuer, weaponId, HitRejectReason.MultiplePlayersShotgun,
                        iState.LastShot.Hits, targetName: damaged.Name);
                    return;
                }

                if (!IsShotgunWeapon(weaponId) && iState.LastShot.Hits > 1)
                {
                    iState.LastShot.Valid = false;
                    Reject(issuer, weaponId, HitRejectReason.MultiplePlayers,
                        iState.LastShot.Hits, targetName: damaged.Name);
                    return;
                }

                var hitDist = Dist(iState.LastShot.Origin, damaged.Position);
                if (hitDist > MaxDistFromShot)
                {
                    Reject(issuer, weaponId, HitRejectReason.TooFarFromShot, hitDist, targetName: damaged.Name);
                    return;
                }

                var originDist = Dist(iState.LastShot.Origin, issuer.Position);
                if (originDist > MaxDistFromOrigin)
                {
                    Reject(issuer, weaponId, HitRejectReason.TooFarFromOrigin, originDist, targetName: damaged.Name);
                    return;
                }

                var w = GetEntry(weaponId);
                if (w.Range > 0)
                {
                    var dist = Dist(issuer.Position, damaged.Position);
                    if (dist > w.Range * 1.5f)
                    {
                        Reject(issuer, weaponId, HitRejectReason.OutOfRange,
                            dist, w.Range * 1.5f, targetName: damaged.Name);
                        return;
                    }
                }
            }

            if (!CheckShootRate(issuer, iState, weaponId)) return;
            if (!CheckHitRate(issuer, iState, weaponId)) return;

            TrackHit(iState, weaponId);

            var entry = GetEntry(weaponId);
            var actual = CalcDamage(entry, amount, issuer.Position, damaged.Position);
            if (actual < 0)
            {
                FireInvalid(issuer, damaged, amount, weaponId, bodypart, 3, true);
                Reject(issuer, weaponId, HitRejectReason.InvalidDamage, amount, targetName: damaged.Name);
                return;
            }

            var args = new PlayerDamageArgs
            {
                Player = damaged,
                Issuer = issuer,
                Amount = actual,
                Weapon = weaponId,
                Bodypart = bodypart
            };
            PlayerDamage?.Invoke(null, args);
            if (args.Cancel) return;

            Inflict(damaged, dState, args.Amount, weaponId, bodypart, issuer);
            WeaponConfigDamageFeed.AddGiven(issuer, damaged.Name, args.Amount, weaponId);
            WeaponConfigDamageFeed.AddTaken(damaged, issuer.Name, args.Amount, weaponId);
            PlayerDamageDone?.Invoke(null, args);
        }

        public static void HandleVehicleDamage(Player issuer, int vehicleId, float amount, int weaponId)
        {
            if (!_states.TryGetValue(issuer.Id, out var iState)) return;
            if (iState.IsDying || !IsSpawned(issuer, iState)) return;

            var vehicle = BaseVehicle.Find(vehicleId);
            if (vehicle == null) return;

            if (!_cfg.VehicleUnoccupiedDamage && !IsVehicleOccupied(vehicle)) return;
            if (!_cfg.VehiclePassengerDamage && issuer.Vehicle?.Id == vehicleId) return;

            foreach (var bp in BasePlayer.All)
            {
                if (bp is not Player target || target.IsDisposed || target.Id == issuer.Id) continue;
                if (target.Vehicle?.Id != vehicleId) continue;
                if (!_states.TryGetValue(target.Id, out var tState) || tState.IsDying) continue;
                if (iState.Team != 255 && iState.Team == tState.Team) continue;

                var entry = GetEntry(weaponId);
                var actual = CalcDamage(entry, amount, issuer.Position, target.Position);
                if (actual <= 0) continue;

                var args = new PlayerDamageArgs
                {
                    Player = target,
                    Issuer = issuer,
                    Amount = actual,
                    Weapon = weaponId,
                    Bodypart = 0
                };
                PlayerDamage?.Invoke(null, args);
                if (args.Cancel) continue;

                Inflict(target, tState, args.Amount, weaponId, 0, issuer);
                WeaponConfigDamageFeed.AddGiven(issuer, target.Name, args.Amount, weaponId);
                WeaponConfigDamageFeed.AddTaken(target, issuer.Name, args.Amount, weaponId);
                PlayerDamageDone?.Invoke(null, args);
            }
        }

        public static void HandleTakeDamage(Player damaged, Player? issuer, float amount,
            int weaponId, int bodypart)
        {
            if (issuer != null && weaponId >= 0 && weaponId < _validGiven.Length
                && _validGiven[weaponId])
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

            var args = new PlayerDamageArgs
            {
                Player = damaged,
                Issuer = issuer,
                Amount = actual,
                Weapon = weaponId,
                Bodypart = bodypart
            };
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

        // ── Core damage application ───────────────────────────────────────

        private static void Inflict(Player p, PlayerWcState s, float amount, int weaponId,
            int bodypart, Player? issuer = null, bool ignoreArmour = false)
        {
            if (amount <= 0) return;
            var w = GetEntry(weaponId);

            var affectsArmour = !ignoreArmour && !_cfg.GlobalArmourRules && w.AffectsArmour;
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

            p.Health = 99999f;
            p.Armour = 0f;

            if (hDmg > 0 || aDmg > 0)
            {
                p.PlaySound(_cfg.DamageTakenSound);
                issuer?.PlaySound(_cfg.DamageGivenSound);
            }

            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);

            if (s.Health <= 0 && !s.IsDying)
                TriggerDeath(p, s, weaponId, bodypart, cancelable: true);
        }

        private static void TriggerDeath(Player p, PlayerWcState s, int weaponId,
            int bodypart = 0, bool cancelable = true)
        {
            s.IsDying = true;
            s.Armour = 0;
            s.IntendedWorld = p.VirtualWorld;
            if (_cfg.CbugDeathDelay && !s.CbugAllowed)
                s.CbugFrozeTick = Environment.TickCount;

            try { Plugins.SKY.SkyNatives.Instance.SendDeath(p.Id); } catch { }

            var (lib, anim) = GetDeathAnim(weaponId, bodypart);
            var prep = new PrepareDeathArgs
            {
                Player = p,
                AnimLib = lib,
                AnimName = anim,
                AnimLock = true,
                RespawnTime = _cfg.RespawnTime
            };
            PlayerPrepareDeath?.Invoke(null, prep);
            if (prep.Cancel) return;

            p.ApplyAnimation(prep.AnimLib, prep.AnimName, 4.0f, false, false, false,
                prep.AnimLock, 0, true);

            var cts = new CancellationTokenSource();
            s.DeathCts = cts;
            _ = RunDeathAsync(p, s, prep.RespawnTime, cts.Token, cancelable)
                .ContinueWith(t => Console.WriteLine(
                    $"[WeaponConfig] RunDeath: {t.Exception?.InnerException?.Message}"),
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        // ── Death animation selection ─────────────────────────────────────

        /// <summary>
        /// Returns the (animLib, animName) pair for a death.
        /// Respects bodypart for headshots and uses appropriate animations
        /// for explosions, falls, heli kills, and shotgun kills.
        /// </summary>
        private static (string Lib, string Name) GetDeathAnim(int wid, int bodypart)
        {
            // Explosive / grenade deaths
            if (wid is 16 or 18 or 35 or 36 or 39 or 51)
                return ("PED", "DEAD_SKYDIVE");

            // Fall / splat
            if (wid == 54)
                return ("PED", "DEAD_SKYDIVE");

            // Helicopter blades
            if (wid == 50)
                return ("PED", "KD_SKYDIVE_DIVE");

            // Headshot — fall backward with hands on face
            if (bodypart == BodypartHead)
                return ("PED", "KD_WEAPON_FALL");

            // Shotgun — fly back like GTA:VC
            if (wid is 25 or 26 or 27)
                return ("PED", "KD_WEAPON_FALL");

            // Default ground death
            return ("PED", "CRACK_DEAD_FLOOR");
        }

        // ── Fall damage ───────────────────────────────────────────────────

        private static void CheckFallDamage(Player p, PlayerWcState s)
        {
            var zVelo = p.Velocity.Z;
            if (zVelo < _cfg.FallDeathVelocity && s.LastZVelo >= _cfg.FallDeathVelocity)
                Inflict(p, s, MathF.Abs(zVelo) * _cfg.FallDamageMultiplier, 54, 0);
            s.LastZVelo = zVelo;
        }

        // ── Shoot / hit rate checks ───────────────────────────────────────

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
            if (avg >= w.MaxShootRate) return true;

            Reject(p, weaponId,
                multi ? HitRejectReason.ShootRateTooFastMultiple : HitRejectReason.ShootRateTooFast,
                avg, n, multi ? 0 : w.MaxShootRate);
            return false;
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
            if (avg >= w.MaxShootRate) return true;

            Reject(p, weaponId,
                multi ? HitRejectReason.HitRateTooFastMultiple : HitRejectReason.HitRateTooFast,
                avg, n, multi ? 0 : w.MaxShootRate);
            return false;
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

        // ── Death async runner ────────────────────────────────────────────

        private static async Task RunDeathAsync(Player p, PlayerWcState s, int delayMs,
            CancellationToken ct, bool cancelable)
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

                PlayerDeathFinished?.Invoke(null,
                    new DeathFinishedArgs { Player = p, Cancelable = cancelable });
            }
            catch (OperationCanceledException) { }
        }

        // ── Resync – full state save & restore ────────────────────────────

        /// <summary>
        /// Captures the player's full in-game state (health, armour, skin, position,
        /// facing angle, all 13 weapon slots) into a <see cref="ResyncSnapshot"/>.
        /// </summary>
        private static ResyncSnapshot CaptureSnapshot(Player p, PlayerWcState s)
        {
            var snap = new ResyncSnapshot
            {
                Health = s.Health,
                Armour = s.Armour,
                Skin = p.Skin,
                Team = s.Team,
                Position = p.Position,
                FacingAngle = p.Angle
            };
            for (var slot = 0; slot < 13; slot++)
            {
                p.GetWeaponData(slot, out var wep, out var ammo);
                snap.Weapons[slot] = (wep, ammo);
            }
            return snap;
        }

        /// <summary>
        /// Restores health/armour and optionally all weapons from a snapshot.
        /// </summary>
        private static void RestoreFromSnapshot(Player p, PlayerWcState s,
            ResyncSnapshot snap, bool restoreWeapons)
        {
            s.Health = snap.Health;
            s.Armour = snap.Armour;

            if (restoreWeapons)
            {
                p.ResetWeapons();
                foreach (var (wep, ammo) in snap.Weapons)
                    if (ammo > 0 && wep != Weapon.None)
                        p.GiveWeapon(wep, ammo);
            }

            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
        }

        public static void ResyncPlayer(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.ResyncCts?.Cancel();
            s.BeingResynced = true;
            s.ResyncSnap = CaptureSnapshot(p, s);
            p.Position = p.Position; // force re-stream
            var cts = new CancellationTokenSource();
            s.ResyncCts = cts;
            _ = ClearResyncAsync(p, s, cts.Token);
        }

        private static async Task ClearResyncAsync(Player p, PlayerWcState s, CancellationToken ct)
        {
            try
            {
                await Task.Delay(500, ct);
                if (p.IsDisposed) return;

                s.BeingResynced = false;
                var snap = s.ResyncSnap;
                s.ResyncSnap = null;

                if (snap == null) return;

                // Full restore: health, armour, skin, position, facing angle, all weapons
                p.Skin = snap.Skin;
                p.Position = snap.Position;
                p.Angle = snap.FacingAngle;
                RestoreFromSnapshot(p, s, snap, restoreWeapons: true);
            }
            catch (OperationCanceledException) { }
        }

        // ── Reject / invalid helpers ──────────────────────────────────────

        private static void Reject(Player p, int wid, HitRejectReason r,
            float i1 = 0, float i2 = 0, float i3 = 0, string targetName = "")
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;

            var now = DateTime.Now;
            s.RejectedHits[s.RejectedHitIdx % PlayerWcState.MaxRejectedHits] = new RejectedHit
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

        private static void FireInvalid(Player p, Player? damaged, float amt,
            int wid, int bp, int err, bool given)
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

        // ── Sync helpers ──────────────────────────────────────────────────

        private static void SyncFakeVitals(Player p, PlayerWcState s)
        {
            try
            {
                Plugins.SKY.SkyNatives.Instance
                    .SetFakeHealth(p.Id, (int)MathF.Round(MathF.Min(s.Health, 100f)));
                Plugins.SKY.SkyNatives.Instance
                    .SetFakeArmour(p.Id, (int)MathF.Round(MathF.Min(s.Armour, 100f)));
            }
            catch { }
        }

        // ── Damage calculation ────────────────────────────────────────────

        private static float CalcDamage(WeaponEntry w, float given, Vector3 a, Vector3 b)
            => w.Type switch
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

        // ── Misc helpers ──────────────────────────────────────────────────

        private static bool IsVehicleOccupied(BaseVehicle vehicle)
        {
            foreach (var p in BasePlayer.All)
                if (p is Player pl && !pl.IsDisposed && pl.Vehicle?.Id == vehicle.Id)
                    return true;
            return false;
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

        // ── Public API ────────────────────────────────────────────────────

        public static int GetLastShotVehicleId(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return -1;
            if (s.LastShot == null || s.LastShot.HitType != 2) return -1;
            return s.LastShot.HitId;
        }

        public static bool IsBulletWeapon(int id) => (id >= 22 && id <= 34) || id == 38;
        public static bool IsMeleeWeapon(int id) => (id >= 0 && id <= 15) || id == 48;
        public static bool IsHighRateWeapon(int id) => id == 37 || id == 41 || id == 42 || id == 52 || id == 53;
        public static bool IsShotgunWeapon(int id) => id == 25 || id == 26 || id == 27;

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
            BasePlayer.SendDeathMessageToAll(killer, killee, (Weapon)mapped);
        }

        // ── Health / armour setters ────────────────────────────────────────

        public static void SetPlayerHealth(Player p, float health)
        {
            if (!_states.TryGetValue(p.Id, out var s) || s.IsDying) return;
            s.Health = MathF.Max(0, MathF.Min(health, s.MaxHealth));
            p.Health = 99999f;
            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
            if (s.Health <= 0) TriggerDeath(p, s, 55, 0, cancelable: true);
        }

        public static void SetPlayerArmour(Player p, float armour)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.Armour = MathF.Max(0, MathF.Min(armour, s.MaxArmour));
            p.Armour = MathF.Min(s.Armour, 100f);
            SyncFakeVitals(p, s);
        }

        /// <summary>
        /// Adds health to a player (used by the vending machine subsystem and external calls).
        /// Clamped to MaxHealth. Does not trigger death.
        /// </summary>
        public static void HealPlayer(Player p, float amount)
        {
            if (!_states.TryGetValue(p.Id, out var s) || s.IsDying) return;
            s.Health = MathF.Min(s.Health + amount, s.MaxHealth);
            p.Health = 99999f;
            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
        }

        public static float GetWcHealth(Player p) => _states.TryGetValue(p.Id, out var s) ? s.Health : 0f;
        public static float GetWcArmour(Player p) => _states.TryGetValue(p.Id, out var s) ? s.Armour : 0f;

        public static void DamagePlayer(Player p, float amount, Player? issuer = null,
            int weapon = 55, int bodypart = 0, bool ignoreArmour = false)
        {
            if (!_states.TryGetValue(p.Id, out var s) || s.IsDying || p.IsDisposed) return;
            Inflict(p, s, amount, weapon, bodypart, issuer, ignoreArmour);
        }

        // ── Damage feed ────────────────────────────────────────────────────

        public static void SetDamageFeed(bool enable)
        {
            _cfg.EnableDamageFeed = enable;
            foreach (var id in _states.Keys)
            {
                if (BasePlayer.Find(id) is not Player p) continue;
                WeaponConfigDamageFeed.SetEnabled(p, enable);
            }
        }

        public static bool IsDamageFeedActive(Player? p = null)
            => p != null ? WeaponConfigDamageFeed.IsEnabled(p) : _cfg.EnableDamageFeed;

        // ── Weapon data ────────────────────────────────────────────────────

        public static void SetWeaponDamage(int id, float dmg, DamageType type = DamageType.Static)
        {
            if (id >= 0 && id < _weapons.Length)
            { _weapons[id].Damage = dmg; _weapons[id].Type = type; }
        }
        public static float GetWeaponDamage(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].Damage : 0f;

        public static void SetWeaponMaxRange(int id, float range)
        { if (id >= 0 && id < _weapons.Length) _weapons[id].Range = range; }
        public static float GetWeaponMaxRange(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].Range : 0f;

        public static void SetWeaponShootRate(int id, int rate)
        { if (id >= 0 && id < _weapons.Length) _weapons[id].MaxShootRate = rate; }
        public static int GetWeaponShootRate(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].MaxShootRate : 0;

        public static void SetWeaponName(int id, string name)
        { if (id >= 0 && id < _weapons.Length) _weapons[id].Name = name; }
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

        // ── Player max health/armour ───────────────────────────────────────

        public static void SetPlayerMaxHealth(Player p, float v)
        { if (_states.TryGetValue(p.Id, out var s)) s.MaxHealth = v; }
        public static void SetPlayerMaxArmour(Player p, float v)
        { if (_states.TryGetValue(p.Id, out var s)) s.MaxArmour = v; }
        public static float GetPlayerMaxHealth(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.MaxHealth : 0f;
        public static float GetPlayerMaxArmour(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.MaxArmour : 0f;

        public static float GetLastDamageHealth(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.LastDamageHealth : 0f;
        public static float GetLastDamageArmour(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.LastDamageArmour : 0f;

        // ── Status queries ─────────────────────────────────────────────────

        public static bool IsPlayerDying(Player p)
            => _states.TryGetValue(p.Id, out var s) && s.IsDying;
        public static bool IsPlayerPaused(Player p)
            => _states.TryGetValue(p.Id, out var s)
            && Environment.TickCount - s.LastUpdateTick > 2000;

        // ── Team / cbug ────────────────────────────────────────────────────

        public static void SetPlayerTeam(Player p, int team)
        {
            if (_states.TryGetValue(p.Id, out var s)) { s.Team = team; p.Team = team; }
        }

        public static void SetCbugAllowed(bool allowed, Player? p = null)
        {
            if (p != null)
            {
                if (_states.TryGetValue(p.Id, out var s)) s.CbugAllowed = allowed;
                return;
            }
            _cfg.CbugAllowed = allowed;
            foreach (var s in _states.Values) s.CbugAllowed = allowed;
        }
        public static bool GetCbugAllowed(Player? p = null)
        {
            if (p != null) return _states.TryGetValue(p.Id, out var s) && s.CbugAllowed;
            return _cfg.CbugAllowed;
        }
        public static void SetCbugDeathDelay(bool v) => _cfg.CbugDeathDelay = v;

        // ── Vehicle damage settings ────────────────────────────────────────

        public static void SetVehiclePassengerDamage(bool v) => _cfg.VehiclePassengerDamage = v;
        public static void SetVehicleUnoccupiedDamage(bool v) => _cfg.VehicleUnoccupiedDamage = v;

        // ── Fall damage ────────────────────────────────────────────────────

        public static void SetCustomFallDamage(bool toggle, float mult = 25f, float deathVel = -0.6f)
        {
            _cfg.CustomFallDamage = toggle;
            _cfg.FallDamageMultiplier = mult;
            _cfg.FallDeathVelocity = -MathF.Abs(deathVel);
            if (toggle && 54 < _weapons.Length) _weapons[54].Damage = mult;
        }

        // ── Vending machines ───────────────────────────────────────────────

        public static void SetCustomVendingMachines(bool enable)
        {
            _cfg.CustomVendingMachines = enable;
            // Objects are created at Init; toggling at runtime disposes/recreates them.
            WeaponConfigVendingMachines.Dispose();
            WeaponConfigVendingMachines.Init(enable);
            // Re-remove buildings for all currently connected players.
            if (enable)
                foreach (var bp in BasePlayer.All)
                    if (bp is Player p)
                        WeaponConfigVendingMachines.OnConnect(p);
        }

        // ── Respawn ────────────────────────────────────────────────────────

        public static void SetRespawnTime(int ms) => _cfg.RespawnTime = Math.Max(0, ms);
        public static int GetRespawnTime() => _cfg.RespawnTime;

        // ── Sounds ────────────────────────────────────────────────────────

        public static void SetDamageSounds(int taken, int given)
        {
            _cfg.DamageTakenSound = taken;
            _cfg.DamageGivenSound = given;
        }

        // ── Per-player UI toggles ──────────────────────────────────────────

        public static void EnableHealthBarForPlayer(Player p, bool enable)
            => WeaponConfigHealthBar.SetEnabled(p, enable);
        public static void SetDamageFeedForPlayer(Player p, bool enable)
            => WeaponConfigDamageFeed.SetEnabled(p, enable);

        // ── Shot / hit rate stats ──────────────────────────────────────────

        /// <summary>
        /// Average milliseconds between the last <paramref name="shots"/> shots.
        /// Returns -1 if there is not enough data.
        /// </summary>
        public static int AverageShootRate(Player p, int shots)
            => AverageShootRate(p, shots, out _);

        /// <summary>
        /// Overload that also returns whether different weapons were used in the sampled window.
        /// </summary>
        public static int AverageShootRate(Player p, int shots, out bool multipleWeapons)
        {
            multipleWeapons = false;
            if (!_states.TryGetValue(p.Id, out var s) || s.ShotsFired < shots) return -1;

            var n = Math.Min(shots, s.ShotTicks.Length);
            if (n < 2) return 1;

            var total = 0;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.ShotIdx - i - 1 + s.ShotTicks.Length) % s.ShotTicks.Length;
                var bi = (s.ShotIdx - i - 2 + s.ShotTicks.Length) % s.ShotTicks.Length;
                total += s.ShotTicks[ai] - s.ShotTicks[bi];
                if (s.ShotWeapons[ai] != s.ShotWeapons[bi]) multipleWeapons = true;
            }
            return total / (n - 1);
        }

        /// <summary>
        /// Average milliseconds between the last <paramref name="hits"/> hits given.
        /// Returns -1 if there is not enough data.
        /// </summary>
        public static int AverageHitRate(Player p, int hits)
            => AverageHitRate(p, hits, out _);

        /// <summary>
        /// Overload that also returns whether different weapons were used in the sampled window.
        /// </summary>
        public static int AverageHitRate(Player p, int hits, out bool multipleWeapons)
        {
            multipleWeapons = false;
            if (!_states.TryGetValue(p.Id, out var s) || s.HitsIssued < hits) return -1;

            var n = Math.Min(hits, s.HitTicks.Length);
            if (n < 2) return 1;

            var total = 0;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.HitIdx - i - 1 + s.HitTicks.Length) % s.HitTicks.Length;
                var bi = (s.HitIdx - i - 2 + s.HitTicks.Length) % s.HitTicks.Length;
                total += s.HitTicks[ai] - s.HitTicks[bi];
                if (s.HitWeapons[ai] != s.HitWeapons[bi]) multipleWeapons = true;
            }
            return total / (n - 1);
        }

        // ── Rejected hits ──────────────────────────────────────────────────

        public static RejectedHit? GetRejectedHit(Player p, int idx)
        {
            if (!_states.TryGetValue(p.Id, out var s) || idx >= PlayerWcState.MaxRejectedHits)
                return null;
            var realIdx = ((s.RejectedHitIdx - idx - 1) % PlayerWcState.MaxRejectedHits
                + PlayerWcState.MaxRejectedHits) % PlayerWcState.MaxRejectedHits;
            return s.RejectedHits[realIdx];
        }
    }
}