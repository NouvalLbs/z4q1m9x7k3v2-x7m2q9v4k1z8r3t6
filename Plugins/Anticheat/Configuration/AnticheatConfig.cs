using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Configuration;

public class AnticheatConfig
{
    public bool Enabled { get; set; } = true;
    public string LogPath { get; set; } = "logs/anticheat.log";
    public int MaxPing { get; set; } = 500;
    public int MaxConnectsPerIp { get; set; } = 1;
    public int MinReconnectSeconds { get; set; } = 12;
    public int SpeedHackVehResetDelay { get; set; } = 3;
    public Dictionary<string, CheckConfig> Checks { get; set; } = BuildDefaults();

    public CheckConfig GetCheck(string name) =>
        Checks.TryGetValue(name, out var c) ? c : Checks[name] = new CheckConfig();

    private static Dictionary<string, CheckConfig> BuildDefaults() => new()
    {
        ["AirBreakOnfoot"] = new(),
        ["AirBreakVehicle"] = new(),
        ["TeleportOnfoot"] = new(),
        ["TeleportVehicle"] = new(),
        ["TeleportVehicleEnter"] = new(),
        ["TeleportPickup"] = new(),
        ["TeleportVehicleToPlayer"] = new(),
        ["FlyHackOnfoot"] = new(),
        ["FlyHackVehicle"] = new(),
        ["SpeedHackOnfoot"] = new(),
        ["SpeedHackVehicle"] = new(),
        ["HealthHackOnfoot"] = new(),
        ["HealthHackVehicle"] = new(),
        ["ArmourHack"] = new(),
        ["MoneyHack"] = new(),
        ["WeaponHack"] = new(),
        ["AmmoHackAdd"] = new(),
        ["AmmoHackInfinite"] = new(),
        ["SpecialActionHack"] = new(),
        ["GodModeOnfoot"] = new(),
        ["GodModeVehicle"] = new(),
        ["InvisibleHack"] = new(),
        ["CjRun"] = new(),
        ["LagCompSpoof"] = new(),
        ["QuickTurn"] = new(),
        ["RapidFire"] = new(),
        ["ProAim"] = new(),
        ["CarShot"] = new(),
        ["FullAiming"] = new(),
        ["FakeSpawn"] = new(),
        ["FakeKill"] = new(),
        ["AfkGhost"] = new(),
        ["CarJack"] = new(),
        ["TuningHack"] = new(),
        ["TuningCrasher"] = new(),
        ["Reconnect"] = new(),
        ["HighPing"] = new(),
        ["DialogHack"] = new(),
        ["Sandbox"] = new(),
        ["InvalidVersion"] = new(),
        ["RconHack"] = new(),
        ["InvalidSeatCrasher"] = new(),
        ["DialogCrasher"] = new(),
        ["AttachedObjectCrasher"] = new(),
        ["WeaponCrasher"] = new(),
        ["ConnectionFlood"] = new(),
        ["CallbackFlood"] = new(),
        ["SeatFlood"] = new(),
        ["DoS"] = new(),
        ["ParkourMod"] = new() { Enabled = false },
        ["UnFreeze"] = new() { Enabled = false },
        ["FakeNpc"] = new() { Enabled = false },
    };
}