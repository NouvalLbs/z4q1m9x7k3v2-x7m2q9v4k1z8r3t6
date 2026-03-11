using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.State;

public class PlayerAcState
{
    // Position (TeleportCheck, AirBreakCheck, VehicleTeleportCheck, SpeedHackCheck)
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    // Health / Armour (HealthCheck, ArmourCheck, GodModeCheck)
    public float Health { get; set; }
    public float Armour { get; set; }
    public int SetHealth { get; set; } = -1;
    public int SetArmour { get; set; } = -1;

    // Money (MoneyCheck)
    public int Money { get; set; }

    // Facing angle (QuickTurnCheck)
    public float LastFacingAngle { get; set; } = -1f;

    // Special action (SpecialActionCheck)
    public int SetSpecialActionId { get; set; } = -1;

    // Weapons & Ammo (WeaponCheck, AmmoCheck)
    public int[] Weapons { get; } = new int[13];
    public int[] Ammo { get; } = new int[13];
    public int[] PreShotAmmo { get; } = new int[13];
    public long[] SetWeaponTick { get; } = new long[13];
    public long[] GiveAmmoTick { get; } = new long[13];
    public long[] ShotAmmoTick { get; } = new long[13];

    // Spawn weapon loadout (WeaponCheck.OnPlayerSpawned)
    public int SpawnWeapon1 { get; set; }
    public int SpawnWeapon2 { get; set; }
    public int SpawnWeapon3 { get; set; }
    public int SpawnAmmo1 { get; set; }
    public int SpawnAmmo2 { get; set; }
    public int SpawnAmmo3 { get; set; }

    // Vehicle (CarJackCheck, VehicleTeleportCheck, TuningHackCheck, SpeedHackCheck)
    public int VehicleId { get; set; } = -1;

    // Dialog (DialogHackCheck, DialogCrasherCheck)
    public int NextDialog { get; set; } = -1;

    // Network
    public string IpAddress { get; set; } = string.Empty;

    // Ticks
    public long PutInVehicleTick { get; set; }
    public long SetHealthTick { get; set; }
    public long DamageTick { get; set; }
    public long SetArmourTick { get; set; }
    public long SetSpecialActionTick { get; set; }
    public long ResetWeaponsTick { get; set; }
    public long RemoveFromVehicleTick { get; set; }
    public long VehicleVelocityTick { get; set; }
    public long PlayerVelocityTick { get; set; }
    public long SetPosTick { get; set; }
    public long SpectateTick { get; set; }
    public long SpawnTick { get; set; }
    public long UpdateTick { get; set; }
    public long EnterVehicleTick { get; set; }

    // Flags
    public int SpawnSetFlag { get; set; }
    public bool IsOnline { get; set; }
    public bool IsDead { get; set; }
    public bool IsSpectating { get; set; }
    public bool IsInModShop { get; set; }
    public bool IsFrozen { get; set; }
    public bool PendingDamageResult { get; set; }
    public bool PendingVehicleDamageResult { get; set; }
    public bool PendingClassResult { get; set; }

    // Per-player check exemptions
    private readonly ConcurrentDictionary<string, byte> _disabledChecks = new();
    public void DisableCheck(string name) => _disabledChecks.TryAdd(name, 0);
    public void EnableCheck(string name) => _disabledChecks.TryRemove(name, out _);
    public bool IsCheckEnabled(string name) => !_disabledChecks.ContainsKey(name);

    // Warning counters
    public ConcurrentDictionary<string, int> WarningCounts { get; } = new();
    public int GetWarning(string check) => WarningCounts.GetValueOrDefault(check);
    public int AddWarning(string check) => WarningCounts.AddOrUpdate(check, 1, (_, v) => v + 1);
    public void ResetWarning(string check) => WarningCounts.TryRemove(check, out _);
    public void ResetAllWarnings() => WarningCounts.Clear();
}