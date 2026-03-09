#nullable enable
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public enum DamageType { Multiplier = 0, Static = 1, RangeMultiplier = 2, Range = 3 }

    public enum HitRejectReason
    {
        NoIssuer, InvalidWeapon, DyingPlayer, SameTeam,
        OutOfRange, ShootRateTooFast, HitRateTooFast, Disconnected
    }

    public class WeaponEntry
    {
        public int Id { get; set; }
        public float Damage { get; set; }
        public DamageType Type { get; set; } = DamageType.Static;
        public float Range { get; set; }
        public int MaxShootRate { get; set; }
        public bool AffectsArmour { get; set; } = true;
        public bool TorsoOnly { get; set; } = true;
        public List<RangeDamageStep> RangeSteps { get; set; } = new();
    }

    public class RangeDamageStep
    {
        public float Range { get; set; }
        public float Damage { get; set; }
    }

    public class WeaponConfig
    {
        public bool VehiclePassengerDamage { get; set; } = false;
        public bool VehicleUnoccupiedDamage { get; set; } = false;
        public bool CbugAllowed { get; set; } = true;
        public bool CustomFallDamage { get; set; } = false;
        public float FallDamageMultiplier { get; set; } = 25.0f;
        public float FallDeathVelocity { get; set; } = -0.6f;
        public int RespawnTime { get; set; } = 3000;
        public int MaxShootRateSamples { get; set; } = 4;
    }

    public class WeaponConfigRoot
    {
        public WeaponConfig Config { get; set; } = new();
        public List<WeaponEntry> Weapons { get; set; } = new();
    }

    public class PlayerDamageArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public Player? Issuer { get; init; }
        public float Amount { get; set; }
        public int Weapon { get; init; }
        public int Bodypart { get; init; }
        public bool Cancel { get; set; }
    }

    public class RejectedHitArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public int Weapon { get; init; }
        public HitRejectReason Reason { get; init; }
    }

    public class PrepareDeathArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public bool Cancel { get; set; }
    }

    public class DeathFinishedArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
    }
}