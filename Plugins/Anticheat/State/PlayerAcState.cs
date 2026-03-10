using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Plugins.Anticheat.State;

public class PlayerAcState
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float LastX { get; set; }
    public float LastY { get; set; }
    public float SpawnX { get; set; }
    public float SpawnY { get; set; }
    public float SpawnZ { get; set; }
    public float SetX { get; set; }
    public float SetY { get; set; }
    public float SetZ { get; set; }
    public float PutVehX { get; set; }
    public float PutVehY { get; set; }
    public float PutVehZ { get; set; }
    public float DropJumpX { get; set; }
    public float DropJumpY { get; set; }

    public float Health { get; set; }
    public float Armour { get; set; }
    public int SetHealth { get; set; } = -1;
    public int SetArmour { get; set; } = -1;
    public float SetVehicleHealth { get; set; }

    public int Money { get; set; }
    public float Speed { get; set; }
    public int CameraMode { get; set; }
    public int Animation { get; set; }

    public int SpecialAction { get; set; }
    public int NextSpecialAction { get; set; } = -1;
    public int LastSpecialAction { get; set; }
    public int SetSpecialActionId { get; set; } = -1;

    public int[] Weapons { get; } = new int[13];
    public int[] Ammo { get; } = new int[13];
    public int[] SetWeapon { get; } = Enumerable.Repeat(-1, 13).ToArray();
    public int[] GiveAmmo { get; } = Enumerable.Repeat(-65535, 13).ToArray();
    public long[] SetWeaponTick { get; } = new long[13];
    public long[] GiveAmmoTick { get; } = new long[13];
    public int ShotWeapon { get; set; }
    public int HeldWeapon { get; set; }
    public int LastWeapon { get; set; }

    public int SpawnWeapon1 { get; set; }
    public int SpawnWeapon2 { get; set; }
    public int SpawnWeapon3 { get; set; }
    public int SpawnAmmo1 { get; set; }
    public int SpawnAmmo2 { get; set; }
    public int SpawnAmmo3 { get; set; }

    public int VehicleId { get; set; } = -1;
    public int Seat { get; set; } = -1;
    public int LastVehicleModel { get; set; }
    public int EnteringVehicle { get; set; } = -1;
    public int EnteringSeat { get; set; }
    public int KickVehicle { get; set; } = -1;
    public int SetVehicle { get; set; } = -1;
    public int SetSeat { get; set; } = -1;

    public int Interior { get; set; }
    public int SetInterior { get; set; } = -1;
    public int Dialog { get; set; } = -1;
    public int NextDialog { get; set; } = -1;

    public int LastPickup { get; set; } = -1;
    public int Parachute { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public int IpInt { get; set; }
    public float LastFacingAngle { get; set; } = -1f;

    public long InteriorTick { get; set; }
    public long PutInVehicleTick { get; set; }
    public long SetHealthTick { get; set; }
    public long DamageTick { get; set; }
    public long SetVehicleHealthTick { get; set; }
    public long SetArmourTick { get; set; }
    public long SetSpecialActionTick { get; set; }
    public long ResetWeaponsTick { get; set; }
    public long RemoveFromVehicleTick { get; set; }
    public long VehicleVelocityTick { get; set; }
    public long PlayerVelocityTick { get; set; }
    public long SetPosTick { get; set; }
    public long SpectateTick { get; set; }
    public long SpawnTick { get; set; }
    public long ReloadTick { get; set; }
    public long ShotTick { get; set; }
    public long UpdateTick { get; set; }
    public long EnterVehicleTick { get; set; }
    public long SetPosModeExpiry { get; set; }
    public long TimerTick { get; set; }

    public int SetPositionMode { get; set; } = -1;
    public int SpawnSetFlag { get; set; }
    public int SpawnResultCount { get; set; }
    public int SpectateSetState { get; set; } = -1;
    public int RemoveFromVehicleFlag { get; set; }

    public bool IsOnline { get; set; }
    public bool IsDead { get; set; }
    public bool IsSpectating { get; set; }
    public bool IsUnfrozen { get; set; } = true;
    public bool IsInModShop { get; set; }
    public bool HasStuntBonus { get; set; } = true;
    public bool IsForceClass { get; set; }
    public bool IsTeleportingToZ { get; set; }
    public bool HasInteriorEnterExits { get; set; } = true;
    public bool IsKicked { get; set; }
    public bool PendingEnterResult { get; set; }
    public bool PendingDeathResult { get; set; }
    public bool PendingDamageResult { get; set; }
    public bool PendingVehicleDamageResult { get; set; }
    public bool PendingClassResult { get; set; }

    private readonly ConcurrentDictionary<string, byte> _disabledChecks = new();
    public void DisableCheck(string name) => _disabledChecks.TryAdd(name, 0);
    public void EnableCheck(string name) => _disabledChecks.TryRemove(name, out _);
    public bool IsCheckEnabled(string name) => !_disabledChecks.ContainsKey(name);

    public int[] CallbackCounts { get; } = new int[28];
    public int[] FloodCounts { get; } = new int[28];
    public long[] CallbackLastTick { get; } = new long[28];

    public Dictionary<string, int> WarningCounts { get; } = new();

    public int GetWarning(string check) => WarningCounts.GetValueOrDefault(check);
    public int AddWarning(string check) => WarningCounts[check] = GetWarning(check) + 1;
    public void ResetWarning(string check) => WarningCounts.Remove(check);
    public void ResetAllWarnings() => WarningCounts.Clear();
}