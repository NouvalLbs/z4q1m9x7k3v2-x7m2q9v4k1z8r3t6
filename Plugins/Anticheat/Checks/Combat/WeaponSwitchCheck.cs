using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class WeaponSwitchCheck
{
    private const long MinSwitchInterval = 150;
    private const int MaxSwitchesPerSec = 8;
    private const long SwitchWindowMs = 1000;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public WeaponSwitchCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("WeaponSwitchSpam").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        int currentWeapon = (int)player.Weapon;
        if (currentWeapon == 0) return;

        if (st.LastWeaponId != currentWeapon && st.LastWeaponId != 0)
        {
            long elapsed = now - st.LastWeaponSwitchTick;

            if (elapsed < MinSwitchInterval)
            {
                _warnings.AddWarning(player.Id, "WeaponSwitchSpam",
                    $"interval={elapsed}ms min={MinSwitchInterval}ms");
            }

            st.WeaponSwitchHistory.Enqueue(now);
            while (st.WeaponSwitchHistory.Count > 0 &&
                   now - st.WeaponSwitchHistory.Peek() > SwitchWindowMs)
            {
                st.WeaponSwitchHistory.Dequeue();
            }

            if (st.WeaponSwitchHistory.Count > MaxSwitchesPerSec)
            {
                _warnings.AddWarning(player.Id, "WeaponSwitchSpam",
                    $"count={st.WeaponSwitchHistory.Count} in 1s");
            }

            st.LastWeaponSwitchTick = now;
        }

        st.LastWeaponId = currentWeapon;
    }
}