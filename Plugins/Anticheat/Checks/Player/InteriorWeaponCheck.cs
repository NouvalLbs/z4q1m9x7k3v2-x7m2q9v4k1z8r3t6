using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class InteriorWeaponCheck
{
    // Forbidden interior IDs where weapons are not allowed
    private static readonly HashSet<int> _forbiddenInteriors = new()
    {
        1,   // 24/7 stores
        3,   // Police stations
        4,   // Liberty City interiors
        5,   // San Fierro PD
        6,   // LSPD
        10,  // SFPD
        15,  // Jefferson Motel
        18,  // Fern Ridge apartments
    };

    // Hospital interiors
    private static readonly HashSet<int> _hospitalInteriors = new()
    {
        17,  // County General Hospital (LS)
        18,  // San Fierro Medical Center
    };

    // Ammunation interiors (allowed but monitored)
    private static readonly HashSet<int> _ammunationInteriors = new()
    {
        1,   // Ammunation 1
        4,   // Ammunation 2
        6,   // Ammunation 3
    };

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public InteriorWeaponCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("InteriorWeaponHack").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        int interior = player.Interior;
        int weaponId = (int)player.Weapon;

        // Skip if no weapon
        if (weaponId is 0 or 1) return;

        // Check if in forbidden interior
        bool isForbidden = _forbiddenInteriors.Contains(interior)
                        || _hospitalInteriors.Contains(interior);

        if (isForbidden)
        {
            // Allow if server just gave weapon (grace period)
            bool recentlyGiven = false;
            for (int slot = 0; slot < 13; slot++)
            {
                if (now - st.SetWeaponTick[slot] < 2000)
                {
                    recentlyGiven = true;
                    break;
                }
            }

            if (!recentlyGiven)
            {
                _warnings.AddWarning(player.Id, "InteriorWeaponHack",
                    $"wid={weaponId} interior={interior}");
            }
        }
    }

    public void OnPlayerWeaponShot(BasePlayer player, WeaponShotEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("InteriorWeaponShot").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        int interior = player.Interior;

        // Forbidden shooting in these interiors
        if (_forbiddenInteriors.Contains(interior) || _hospitalInteriors.Contains(interior))
        {
            _warnings.AddWarning(player.Id, "InteriorWeaponShot",
                $"wid={e.Weapon} interior={interior}");
        }
    }

    public void OnPlayerInteriorChanged(int playerId, int newInterior, int oldInterior)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.LastInteriorChangeTick = Environment.TickCount64;

        // Clear weapons when entering forbidden interior (optional, configurable)
        if (_config.GetCheck("InteriorWeaponClear").Enabled)
        {
            if (_forbiddenInteriors.Contains(newInterior) || _hospitalInteriors.Contains(newInterior))
            {
                st.ShouldClearWeaponsOnInterior = true;
            }
        }
    }

    public bool IsForbiddenInterior(int interiorId)
        => _forbiddenInteriors.Contains(interiorId) || _hospitalInteriors.Contains(interiorId);

    public bool IsHospitalInterior(int interiorId)
        => _hospitalInteriors.Contains(interiorId);

    public void AddForbiddenInterior(int interiorId)
        => _forbiddenInteriors.Add(interiorId);

    public void RemoveForbiddenInterior(int interiorId)
        => _forbiddenInteriors.Remove(interiorId);
}