using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

/// <summary>Check 15 — senjata tak sah di slot pemain</summary>
public class WeaponCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public WeaponCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;
        if (!_config.GetCheck("WeaponHack").Enabled) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.ResetWeaponsTick < 1500) return;

        for (int slot = 0; slot < 13; slot++)
        {
            player.GetWeaponData(slot, out Weapon weapon, out int ammo);
            int wid = (int)weapon;
            if (wid == 0) continue; // slot kosong

            if (!WeaponData.IsValid(wid))
            {
                _warnings.AddWarning(player.Id, "WeaponHack", $"slot={slot} wid={wid} (invalid id)");
                continue;
            }

            int trackedWid = st.Weapons[slot];
            if (trackedWid == wid) continue;

            long setTick = st.SetWeaponTick[slot];
            if (now - setTick < 1500) continue; // grace server GivePlayerWeapon

            _warnings.AddWarning(player.Id, "WeaponHack",
                $"slot={slot} got={wid} expected={trackedWid}");
        }
    }

    /// <summary>Panggil setiap kali server GivePlayerWeapon.</summary>
    public void OnWeaponGiven(int playerId, int weaponId, int ammo)
    {
        var st = _players.Get(playerId);
        if (st is null || !WeaponData.IsValid(weaponId)) return;
        int slot = WeaponData.Slot[weaponId];
        st.Weapons[slot] = weaponId;
        st.Ammo[slot] = ammo;
        st.SetWeapon[slot] = weaponId;
        st.SetWeaponTick[slot] = Environment.TickCount64;
    }

    /// <summary>Panggil setiap kali server ResetPlayerWeapons.</summary>
    public void OnWeaponsReset(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        Array.Clear(st.Weapons, 0, 13);
        Array.Clear(st.Ammo, 0, 13);
        st.ResetWeaponsTick = Environment.TickCount64;
    }

    public void OnPlayerSpawned(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null) return;
        OnWeaponGiven(player.Id, st.SpawnWeapon1, st.SpawnAmmo1);
        OnWeaponGiven(player.Id, st.SpawnWeapon2, st.SpawnAmmo2);
        OnWeaponGiven(player.Id, st.SpawnWeapon3, st.SpawnAmmo3);
    }
}