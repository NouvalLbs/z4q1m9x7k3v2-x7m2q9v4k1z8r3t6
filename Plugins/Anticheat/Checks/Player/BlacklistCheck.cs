using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class BlacklistCheck
{
    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public BlacklistCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    // ══════════════════════════════════════════════════════════
    // WEAPON BLACKLIST CHECK
    // ══════════════════════════════════════════════════════════

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        // Check current weapon
        if (_config.GetCheck("BlacklistedWeapon").Enabled)
        {
            int weaponId = (int)player.Weapon;
            if (weaponId > 0 && _config.BlacklistedWeapons.Contains(weaponId))
            {
                _warnings.AddWarning(player.Id, "BlacklistedWeapon",
                    $"wid={weaponId} name={GetWeaponName(weaponId)}");
            }
        }

        // Check player skin
        if (_config.GetCheck("BlacklistedSkin").Enabled)
        {
            int skin = player.Skin;
            if (_config.BlacklistedSkins.Contains(skin))
            {
                _warnings.AddWarning(player.Id, "BlacklistedSkin",
                    $"skin={skin}");
            }
        }

        // Check all weapon slots for blacklisted weapons
        if (_config.GetCheck("BlacklistedWeaponSlots").Enabled)
        {
            CheckAllWeaponSlots(player);
        }
    }

    private void CheckAllWeaponSlots(BasePlayer player)
    {
        long now = Environment.TickCount64;
        var st = _players.Get(player.Id);
        if (st is null) return;

        // Avoid spam checking
        if (now - st.LastBlacklistCheckTick < 500) return;
        st.LastBlacklistCheckTick = now;

        for (int slot = 0; slot < 13; slot++)
        {
            player.GetWeaponData(slot, out Weapon weapon, out int ammo);
            int weaponId = (int)weapon;

            if (weaponId > 0 && _config.BlacklistedWeapons.Contains(weaponId))
            {
                _warnings.AddWarning(player.Id, "BlacklistedWeapon",
                    $"slot={slot} wid={weaponId} name={GetWeaponName(weaponId)}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════
    // VEHICLE MOD BLACKLIST CHECK
    // ══════════════════════════════════════════════════════════

    public void OnVehicleModAdded(BaseVehicle vehicle, BasePlayer player, int componentId)
    {
        if (!_config.Enabled || !_config.GetCheck("BlacklistedVehicleMod").Enabled) return;

        if (_config.BlacklistedVehicleMods.Contains(componentId))
        {
            _warnings.AddWarning(player.Id, "BlacklistedVehicleMod",
                $"comp={componentId} veh={vehicle.Id} model={vehicle.Model}");
        }
    }

    public void OnPlayerEnterVehicle(BasePlayer player, int vehicleId)
    {
        if (!_config.Enabled || !_config.GetCheck("BlacklistedVehicleMod").Enabled) return;

        var vehicle = BaseVehicle.Find(vehicleId);
        if (vehicle is null) return;

        var vst = _vehicles.Get(vehicleId);
        if (vst is null) return;

        // Check all installed components for blacklisted mods
        foreach (var componentId in vst.InstalledComponents)
        {
            if (_config.BlacklistedVehicleMods.Contains(componentId))
            {
                _warnings.AddWarning(player.Id, "BlacklistedVehicleMod",
                    $"existing comp={componentId} veh={vehicleId}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════
    // VEHICLE MODEL BLACKLIST CHECK
    // ══════════════════════════════════════════════════════════

    public void OnPlayerStateChanged(BasePlayer player, StateEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("BlacklistedVehicle").Enabled) return;

        // Check when player enters vehicle as driver
        if (e.NewState != PlayerState.Driving) return;

        var vehicle = player.Vehicle;
        if (vehicle is null) return;

        int model = (int)vehicle.Model;
        if (_config.BlacklistedVehicles.Contains(model))
        {
            _warnings.AddWarning(player.Id, "BlacklistedVehicle",
                $"model={model} veh={vehicle.Id}");
        }
    }

    // ══════════════════════════════════════════════════════════
    // SPECIAL ACTION BLACKLIST CHECK
    // ══════════════════════════════════════════════════════════

    public void CheckSpecialAction(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("BlacklistedSpecialAction").Enabled) return;

        int specialAction = (int)player.SpecialAction;
        if (specialAction > 0 && _config.BlacklistedSpecialActions.Contains(specialAction))
        {
            _warnings.AddWarning(player.Id, "BlacklistedSpecialAction",
                $"action={specialAction}");
        }
    }

    // ══════════════════════════════════════════════════════════
    // ANIMATION BLACKLIST CHECK
    // ══════════════════════════════════════════════════════════

    public void OnAnimationApplied(int playerId, string animLib, string animName)
    {
        if (!_config.Enabled || !_config.GetCheck("BlacklistedAnimation").Enabled) return;

        string fullAnim = $"{animLib}:{animName}";

        if (_config.BlacklistedAnimations.Contains(fullAnim))
        {
            _warnings.AddWarning(playerId, "BlacklistedAnimation",
                $"anim={fullAnim}");
        }
    }

    // ══════════════════════════════════════════════════════════
    // HELPER METHODS
    // ══════════════════════════════════════════════════════════

    private static string GetWeaponName(int weaponId) => weaponId switch
    {
        0 => "Fist",
        1 => "Brass Knuckles",
        2 => "Golf Club",
        3 => "Nightstick",
        4 => "Knife",
        5 => "Baseball Bat",
        6 => "Shovel",
        7 => "Pool Cue",
        8 => "Katana",
        9 => "Chainsaw",
        10 => "Purple Dildo",
        11 => "Dildo",
        12 => "Vibrator",
        13 => "Silver Vibrator",
        14 => "Flowers",
        15 => "Cane",
        16 => "Grenade",
        17 => "Tear Gas",
        18 => "Molotov Cocktail",
        22 => "9mm",
        23 => "Silenced 9mm",
        24 => "Desert Eagle",
        25 => "Shotgun",
        26 => "Sawnoff Shotgun",
        27 => "Combat Shotgun",
        28 => "Micro SMG/Uzi",
        29 => "MP5",
        30 => "AK-47",
        31 => "M4",
        32 => "Tec-9",
        33 => "Country Rifle",
        34 => "Sniper Rifle",
        35 => "RPG",
        36 => "HS Rocket",
        37 => "Flamethrower",
        38 => "Minigun",
        39 => "Satchel Charge",
        40 => "Detonator",
        41 => "Spraycan",
        42 => "Fire Extinguisher",
        43 => "Camera",
        44 => "Night Vision Goggles",
        45 => "Thermal Goggles",
        46 => "Parachute",
        _ => $"Unknown ({weaponId})"
    };

    // ══════════════════════════════════════════════════════════
    // PUBLIC API - Manage Blacklists at Runtime
    // ══════════════════════════════════════════════════════════

    public void AddBlacklistedWeapon(int weaponId)
    {
        if (!_config.BlacklistedWeapons.Contains(weaponId))
            _config.BlacklistedWeapons.Add(weaponId);
    }

    public void RemoveBlacklistedWeapon(int weaponId)
        => _config.BlacklistedWeapons.Remove(weaponId);

    public void AddBlacklistedSkin(int skin)
    {
        if (!_config.BlacklistedSkins.Contains(skin))
            _config.BlacklistedSkins.Add(skin);
    }

    public void RemoveBlacklistedSkin(int skin)
        => _config.BlacklistedSkins.Remove(skin);

    public void AddBlacklistedVehicleMod(int componentId)
    {
        if (!_config.BlacklistedVehicleMods.Contains(componentId))
            _config.BlacklistedVehicleMods.Add(componentId);
    }

    public void RemoveBlacklistedVehicleMod(int componentId)
        => _config.BlacklistedVehicleMods.Remove(componentId);

    public void AddBlacklistedVehicle(int model)
    {
        if (!_config.BlacklistedVehicles.Contains(model))
            _config.BlacklistedVehicles.Add(model);
    }

    public void RemoveBlacklistedVehicle(int model)
        => _config.BlacklistedVehicles.Remove(model);

    public void AddBlacklistedSpecialAction(int action)
    {
        if (!_config.BlacklistedSpecialActions.Contains(action))
            _config.BlacklistedSpecialActions.Add(action);
    }

    public void RemoveBlacklistedSpecialAction(int action)
        => _config.BlacklistedSpecialActions.Remove(action);

    public void AddBlacklistedAnimation(string animLib, string animName)
    {
        string fullAnim = $"{animLib}:{animName}";
        if (!_config.BlacklistedAnimations.Contains(fullAnim))
            _config.BlacklistedAnimations.Add(fullAnim);
    }

    public void RemoveBlacklistedAnimation(string animLib, string animName)
    {
        string fullAnim = $"{animLib}:{animName}";
        _config.BlacklistedAnimations.Remove(fullAnim);
    }

    public bool IsWeaponBlacklisted(int weaponId)
        => _config.BlacklistedWeapons.Contains(weaponId);

    public bool IsSkinBlacklisted(int skin)
        => _config.BlacklistedSkins.Contains(skin);

    public bool IsVehicleModBlacklisted(int componentId)
        => _config.BlacklistedVehicleMods.Contains(componentId);

    public bool IsVehicleBlacklisted(int model)
        => _config.BlacklistedVehicles.Contains(model);
}