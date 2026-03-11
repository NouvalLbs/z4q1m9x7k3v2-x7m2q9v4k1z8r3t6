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

    // NEW: Enhanced configuration
    public List<string> WhitelistedIPs { get; set; } = new();
    public List<int> WhitelistedPlayerIds { get; set; } = new();
    public bool EnableAdminImmunity { get; set; } = true;
    public bool EnableDiscordWebhook { get; set; } = false;
    public string DiscordWebhookUrl { get; set; } = "";
    public bool EnableAutoSave { get; set; } = true;
    public int AutoSaveIntervalMinutes { get; set; } = 10;
    public bool EnableVerboseLogging { get; set; } = false;
    public int MaxWarningsBeforeBan { get; set; } = 10; // Global ban threshold

    public Dictionary<string, CheckConfig> Checks { get; set; } = BuildDefaults();

    public CheckConfig GetCheck(string name) =>
        Checks.TryGetValue(name, out var c) ? c : Checks[name] = new CheckConfig();

    public bool IsWhitelisted(string ip) => WhitelistedIPs.Contains(ip);

    public bool IsWhitelisted(int playerId) => WhitelistedPlayerIds.Contains(playerId);

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
        ["SilentAim"] = new() { MaxWarnings = 2, Action = PunishAction.Kick },
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
        ["JetpackHack"] = new(),
        ["NitroHack"] = new(),
        ["VehicleModHack"] = new(),
        ["VehicleHealthHack"] = new(),
        ["PaintJobHack"] = new() { MaxWarnings = 1, Action = PunishAction.Kick },
        ["InteriorWeaponHack"] = new(),
        ["InteriorWeaponShot"] = new(),
        ["InteriorWeaponClear"] = new() { Enabled = false },
        ["InteriorWeaponShot"] = new(),

        // Anti-NOP checks
        ["NopGiveWeapon"] = new() { MaxWarnings = 8 },
        ["NopSetAmmo"] = new() { MaxWarnings = 8 },
        ["NopSetInterior"] = new() { MaxWarnings = 8 },
        ["NopSetHealth"] = new() { MaxWarnings = 8 },
        ["NopSetVehicleHealth"] = new() { MaxWarnings = 8 },
        ["NopSetArmour"] = new() { MaxWarnings = 8 },
        ["NopSetSpecialAction"] = new() { MaxWarnings = 8 },
        ["NopPutInVehicle"] = new() { MaxWarnings = 8 },
        ["NopToggleSpectating"] = new() { MaxWarnings = 8 },
        ["NopSpawnPlayer"] = new() { MaxWarnings = 8 },
        ["NopSetPos"] = new() { MaxWarnings = 8 },
        ["NopRemoveFromVehicle"] = new() { MaxWarnings = 8 },
    };
}