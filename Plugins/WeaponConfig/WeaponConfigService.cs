#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal sealed class PlayerWcState
    {
        public float Health = 100f, Armour = 0f;
        public float MaxHealth = 100f, MaxArmour = 100f;
        public bool IsDying;
        public int Team = 255;
        public bool CbugAllowed = true;
        public int ShotsFired;
        public readonly int[] ShotTicks = new int[10];
        public readonly int[] ShotWeapons = new int[10];
        public int ShotIdx;
    }

    public static class WeaponConfigService
    {
        private static WeaponConfig _cfg = new();
        private static WeaponEntry[] _weapons = Array.Empty<WeaponEntry>();
        private static readonly Dictionary<int, PlayerWcState> _states = new();

        public static event EventHandler<PlayerDamageArgs>? PlayerDamage;
        public static event EventHandler<PlayerDamageArgs>? PlayerDamageDone;
        public static event EventHandler<RejectedHitArgs>? RejectedHit;
        public static event EventHandler<PrepareDeathArgs>? PlayerPrepareDeath;
        public static event EventHandler<DeathFinishedArgs>? PlayerDeathFinished;

        // Weapons fired via OnPlayerGiveDamage (direct combat)
        private static readonly bool[] _validGiven =
        {
            true, true, true, true, true, true, true, true, true, true,   // 0-9
            true, true, true, true, true, true, false,false,false,false,  // 10-19
            false,false,true, true, true, true, true, true, true, true,   // 20-29
            true, true, true, true, true, false,false,false,true, false,  // 30-39
            false,true, true, false,false,false,true                       // 40-46
        };

        public static void Init(WeaponConfig cfg, WeaponEntry[] weapons)
        {
            _cfg = cfg;
            _weapons = weapons;
        }

        public static void OnConnect(Player p)
            => _states[p.Id] = new PlayerWcState { CbugAllowed = _cfg.CbugAllowed };

        public static void OnDisconnect(Player p) => _states.Remove(p.Id);

        public static void OnSpawn(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.Health = s.MaxHealth;
            s.Armour = 0;
            s.IsDying = false;
            s.ShotsFired = 0;
        }

        public static void OnDeath(Player p, Player? killer, int reason)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.IsDying = true;
            var prep = new PrepareDeathArgs { Player = p };
            PlayerPrepareDeath?.Invoke(null, prep);
            if (!prep.Cancel)
                _ = RespawnAsync(p, s);
        }

        private static async Task RespawnAsync(Player p, PlayerWcState s)
        {
            await Task.Delay(_cfg.RespawnTime);
            if (p.IsDisposed) return;
            s.IsDying = false;
            s.Health = s.MaxHealth;
            s.Armour = 0;
            PlayerDeathFinished?.Invoke(null, new DeathFinishedArgs { Player = p });
        }

        public static void HandleGiveDamage(Player issuer, Player? damaged, float amount, int weaponId)
        {
            if (damaged == null || damaged.IsDisposed)
            { Reject(issuer, weaponId, HitRejectReason.Disconnected); return; }

            if (!_states.TryGetValue(damaged.Id, out var dState)) return;
            if (dState.IsDying) { Reject(issuer, weaponId, HitRejectReason.DyingPlayer); return; }

            if (!IsValidGiven(weaponId)) { Reject(issuer, weaponId, HitRejectReason.InvalidWeapon); return; }

            if (!_states.TryGetValue(issuer.Id, out var iState)) return;
            if (iState.Team != 255 && iState.Team == dState.Team)
            { Reject(issuer, weaponId, HitRejectReason.SameTeam); return; }

            var w = GetEntry(weaponId);

            if (IsBullet(weaponId) && w.Range > 0)
            {
                var dist = Distance(issuer.Position, damaged.Position);
                if (dist > w.Range * 1.5f) { Reject(issuer, weaponId, HitRejectReason.OutOfRange); return; }
            }

            if (!CheckRate(issuer, iState, weaponId)) return;

            TrackShot(iState, weaponId);

            var actual = CalcDamage(w, amount, issuer.Position, damaged.Position);
            var args = new PlayerDamageArgs { Player = damaged, Issuer = issuer, Amount = actual, Weapon = weaponId, Bodypart = 0 };
            PlayerDamage?.Invoke(null, args);
            if (args.Cancel) return;

            Inflict(damaged, dState, args.Amount, weaponId);
            PlayerDamageDone?.Invoke(null, args);
        }

        public static void HandleTakeDamage(Player damaged, Player? issuer, float amount, int weaponId)
        {
            if (issuer != null && IsValidGiven(weaponId)) return;

            if (!_states.TryGetValue(damaged.Id, out var s) || s.IsDying) return;

            var w = GetEntry(weaponId);
            var actual = w.Type == DamageType.Static ? w.Damage : amount * w.Damage;

            var args = new PlayerDamageArgs { Player = damaged, Issuer = issuer, Amount = actual, Weapon = weaponId, Bodypart = 0 };
            PlayerDamage?.Invoke(null, args);
            if (args.Cancel) return;

            Inflict(damaged, s, args.Amount, weaponId);
            PlayerDamageDone?.Invoke(null, args);
        }

        public static void HandleWeaponShot(Player p, int weaponId)
        {
            if (_states.TryGetValue(p.Id, out var s))
                TrackShot(s, weaponId);
        }

        private static void Inflict(Player p, PlayerWcState s, float amount, int weaponId)
        {
            if (amount <= 0) return;
            var w = GetEntry(weaponId);

            float hDmg;
            if (w.AffectsArmour)
            {
                var aDmg = MathF.Min(amount, s.Armour);
                s.Armour -= aDmg;
                hDmg = amount - aDmg;
            }
            else hDmg = amount;

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
        }

        private static float CalcDamage(WeaponEntry w, float given, SampSharp.GameMode.Vector3 aPos, SampSharp.GameMode.Vector3 bPos)
        {
            return w.Type switch
            {
                DamageType.Static => w.Damage,
                DamageType.Multiplier => given * w.Damage,
                DamageType.Range => CalcRange(w, Distance(aPos, bPos)),
                DamageType.RangeMultiplier => CalcRange(w, Distance(aPos, bPos)) * given,
                _ => w.Damage
            };
        }

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

        private static bool CheckRate(Player p, PlayerWcState s, int weaponId)
        {
            var w = GetEntry(weaponId);
            if (w.MaxShootRate <= 0 || s.ShotsFired < 2) return true;

            var n = Math.Min(s.ShotsFired, _cfg.MaxShootRateSamples);
            if (n < 2) return true;

            var total = 0;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.ShotIdx - i - 1 + s.ShotTicks.Length) % s.ShotTicks.Length;
                var bi = (s.ShotIdx - i - 2 + s.ShotTicks.Length) % s.ShotTicks.Length;
                total += s.ShotTicks[ai] - s.ShotTicks[bi];
            }

            if (total / (n - 1) < w.MaxShootRate)
            {
                Reject(p, weaponId, HitRejectReason.ShootRateTooFast);
                return false;
            }
            return true;
        }

        private static void TrackShot(PlayerWcState s, int weaponId)
        {
            s.ShotTicks[s.ShotIdx] = Environment.TickCount;
            s.ShotWeapons[s.ShotIdx] = weaponId;
            s.ShotIdx = (s.ShotIdx + 1) % s.ShotTicks.Length;
            s.ShotsFired++;
        }

        private static void Reject(Player p, int wid, HitRejectReason r)
            => RejectedHit?.Invoke(null, new RejectedHitArgs { Player = p, Weapon = wid, Reason = r });

        private static WeaponEntry GetEntry(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id] : new WeaponEntry { Id = id };

        private static bool IsValidGiven(int id) => id >= 0 && id < _validGiven.Length && _validGiven[id];

        private static bool IsBullet(int id) => (id is >= 22 and <= 34) || id == 38;

        private static float Distance(SampSharp.GameMode.Vector3 a, SampSharp.GameMode.Vector3 b)
        {
            var dx = a.X - b.X; var dy = a.Y - b.Y; var dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        // ── Public API ────────────────────────────────────────────────────────

        public static void DamagePlayer(Player p, float amount, Player? issuer = null, int weapon = 55)
        {
            if (!_states.TryGetValue(p.Id, out var s) || s.IsDying || p.IsDisposed) return;
            Inflict(p, s, amount, weapon);
        }

        public static void SetWeaponDamage(int id, float dmg, DamageType type = DamageType.Static)
        {
            if (id >= 0 && id < _weapons.Length) { _weapons[id].Damage = dmg; _weapons[id].Type = type; }
        }

        public static float GetWeaponDamage(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].Damage : 0f;

        public static WeaponEntry? GetWeaponEntry(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id] : null;

        public static void SetPlayerMaxHealth(Player p, float v)
        { if (_states.TryGetValue(p.Id, out var s)) s.MaxHealth = v; }

        public static void SetPlayerMaxArmour(Player p, float v)
        { if (_states.TryGetValue(p.Id, out var s)) s.MaxArmour = v; }

        public static float GetWcHealth(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.Health : 0f;

        public static float GetWcArmour(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.Armour : 0f;

        public static bool IsPlayerDying(Player p)
            => _states.TryGetValue(p.Id, out var s) && s.IsDying;

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

        public static void SetVehiclePassengerDamage(bool v) => _cfg.VehiclePassengerDamage = v;
        public static void SetRespawnTime(int ms) => _cfg.RespawnTime = Math.Max(0, ms);
        public static int GetRespawnTime() => _cfg.RespawnTime;
    }
}