#nullable enable
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public enum DamageType { Multiplier = 0, Static = 1, RangeMultiplier = 2, Range = 3 }

    public enum HitRejectReason
    {
        NoIssuer,
        InvalidWeapon,
        LastShotInvalid,
        MultiplePlayers,
        MultiplePlayersShotgun,
        DyingPlayer,
        SameTeam,
        Unstreamed,
        InvalidHitType,
        BeingResynced,
        NotSpawned,
        OutOfRange,
        TooFarFromShot,
        ShootRateTooFast,
        ShootRateTooFastMultiple,
        HitRateTooFast,
        HitRateTooFastMultiple,
        TooFarFromOrigin,
        InvalidDamage,
        SameVehicle,
        OwnVehicle,
        InvalidVehicle,
        Disconnected
    }

    public class RangeDamageStep
    {
        public float Range { get; set; }
        public float Damage { get; set; }
    }

    public class WeaponEntry
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public float Damage { get; set; }
        public DamageType Type { get; set; } = DamageType.Static;
        public float Range { get; set; }
        public int MaxShootRate { get; set; }
        public bool AffectsArmour { get; set; } = true;
        public bool TorsoOnly { get; set; } = true;
        public List<RangeDamageStep> RangeSteps { get; set; } = new();
    }

    public class WeaponConfig
    {
        public bool VehiclePassengerDamage { get; set; } = false;
        public bool VehicleUnoccupiedDamage { get; set; } = false;
        public bool CbugAllowed { get; set; } = true;
        public bool CbugDeathDelay { get; set; } = true;
        public bool CustomFallDamage { get; set; } = false;
        public float FallDamageMultiplier { get; set; } = 25.0f;
        public float FallDeathVelocity { get; set; } = -0.6f;
        public int RespawnTime { get; set; } = 3000;
        public int MaxShootRateSamples { get; set; } = 4;
        public int MaxHitRateSamples { get; set; } = 4;
        public bool GlobalArmourRules { get; set; } = false;
        public bool GlobalTorsoRules { get; set; } = false;
        public bool EnableDamageFeed { get; set; } = true;
        public int DamageFeedHideDelay { get; set; } = 3000;
        public bool EnableHealthBar { get; set; } = true;
        public int DamageTakenSound { get; set; } = 1190;
        public int DamageGivenSound { get; set; } = 17802;
    }

    public class WeaponConfigRoot
    {
        public WeaponConfig Config { get; set; } = new();
        public List<WeaponEntry> Weapons { get; set; } = new();
    }

    public sealed class RejectedHit
    {
        public long Time { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Weapon { get; set; }
        public HitRejectReason Reason { get; set; }
        public float Info1 { get; set; }
        public float Info2 { get; set; }
        public float Info3 { get; set; }
        public string TargetName { get; set; } = "";
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
        public float Info1 { get; init; }
        public float Info2 { get; init; }
        public float Info3 { get; init; }
        public string TargetName { get; init; } = "";
    }

    public class PrepareDeathArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public string AnimLib { get; set; } = "PED";
        public string AnimName { get; set; } = "DEAD_chest";
        public bool AnimLock { get; set; } = true;
        public int RespawnTime { get; set; }
        public bool Cancel { get; set; }
    }

    public class DeathFinishedArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public bool Cancelable { get; init; }
    }

    public class InvalidWeaponDamageArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public Player? Damaged { get; init; }
        public float Amount { get; init; }
        public int Weapon { get; init; }
        public int Bodypart { get; init; }
        public int Error { get; init; }
        public bool Given { get; init; }
    }
}