using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

/// <summary>Check 16 (ammo bertambah) dan 17 (ammo infinite)</summary>
public class AmmoCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AmmoCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.ResetWeaponsTick < 1500) return;

        for (int slot = 1; slot < 13; slot++)
        {
            if (!WeaponData.SlotHasAmmo(slot)) continue;

            player.GetWeaponData(slot, out Weapon _, out int ammo);
            int stored = st.Ammo[slot];
            if (ammo == stored) continue;

            long giveTick = st.GiveAmmoTick[slot];
            bool serverGave = now - giveTick < 1500;
            int gain = ammo - stored;

            // Check 16: ammo bertambah tanpa izin server
            if (gain > 0 && !serverGave && _config.GetCheck("AmmoHackAdd").Enabled)
            {
                _warnings.AddWarning(player.Id, "AmmoHackAdd",
                    $"slot={slot} +{gain} total={ammo}");
            }
            // Check 17: ammo tidak berkurang setelah tembak
            else if (gain == 0 && stored > 0
                     && st.ShotTick > 0 && now - st.ShotTick < 2000
                     && _config.GetCheck("AmmoHackInfinite").Enabled)
            {
                _warnings.AddWarning(player.Id, "AmmoHackInfinite",
                    $"slot={slot} noDecrement");
            }

            st.Ammo[slot] = ammo;
        }
    }

    public void OnAmmoGiven(int playerId, int weaponId, int ammo)
    {
        var st = _players.Get(playerId);
        if (st is null || !WeaponData.IsValid(weaponId)) return;
        int slot = WeaponData.Slot[weaponId];
        st.GiveAmmo[slot] = ammo;
        st.GiveAmmoTick[slot] = Environment.TickCount64;
        st.Ammo[slot] += ammo;
    }
}