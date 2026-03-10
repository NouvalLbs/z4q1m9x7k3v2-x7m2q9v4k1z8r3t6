using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Spawn;

public class AfkGhostCheck
{
    private const long AfkGhostMs = 30_000;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AfkGhostCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null) return;
        st.UpdateTick = Environment.TickCount64;
    }

    public void Tick()
    {
        if (!_config.Enabled || !_config.GetCheck("AfkGhost").Enabled) return;

        long now = Environment.TickCount64;

        foreach (var (id, st) in _players.All)
        {
            if (!st.IsOnline || st.IsDead || st.IsSpectating) continue;
            if (st.UpdateTick == 0) continue;
            if (now - st.SpawnTick < 5000) continue;

            if (now - st.UpdateTick > AfkGhostMs)
            {
                _warnings.AddWarning(id, "AfkGhost",
                    $"no update for {(now - st.UpdateTick) / 1000}s");
                st.UpdateTick = now;
            }
        }
    }
}