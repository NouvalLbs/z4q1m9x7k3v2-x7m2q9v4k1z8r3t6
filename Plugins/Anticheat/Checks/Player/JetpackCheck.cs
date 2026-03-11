using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class JetpackCheck
{
    private const long DeadlineMs = 2500;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public JetpackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("JetpackHack").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        int specialAction = (int)player.SpecialAction;
        bool hasJetpack = specialAction == (int)SpecialAction.Usejetpack;

        if (!hasJetpack) return;

        // Check if server authorized jetpack
        bool authorized = st.JetpackAuthorized
                       || now - st.JetpackGivenTick < DeadlineMs
                       || now - st.SpawnTick < 5000; // Allow spawn jetpack

        if (!authorized)
            _warnings.AddWarning(player.Id, "JetpackHack", $"sa={specialAction}");
    }

    public void OnJetpackGiven(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.JetpackGivenTick = Environment.TickCount64;
        st.JetpackAuthorized = true;
    }

    public void OnJetpackRemoved(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.JetpackAuthorized = false;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.JetpackAuthorized = false;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.JetpackAuthorized = false;
    }
}