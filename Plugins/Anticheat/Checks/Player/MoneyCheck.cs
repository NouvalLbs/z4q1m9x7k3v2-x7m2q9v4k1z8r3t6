using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

/// <summary>Check 14</summary>
public class MoneyCheck
{
    // Toleransi interest / rounding dari SA-MP sendiri
    private const int AllowedGain = 1;
    private const int AllowedLoss = -1; // Tolerance for rounding
    private const int MaxNaturalLoss = -5000; // Max natural loss per update

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public MoneyCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 2500) { st.Money = player.Money; return; }

        int cur = player.Money;
        int change = cur - st.Money;

        // Money gain detection (existing)
        if (change > AllowedGain)
        {
            if (!_config.GetCheck("MoneyHack").Enabled) { st.Money = cur; return; }

            // Check if server authorized this gain
            bool authorized = now - st.MoneyGivenTick < 1500;
            if (!authorized)
            {
                _warnings.AddWarning(player.Id, "MoneyHack", $"gain={change} cur={cur}");
            }
            st.Money = cur;
            return;
        }

        // Money loss detection (NEW)
        if (change < AllowedLoss)
        {
            if (!_config.GetCheck("MoneyLossHack").Enabled) { st.Money = cur; return; }

            // Check if server authorized this loss
            bool authorized = now - st.MoneyTakenTick < 1500;

            // Abnormal loss detection
            if (!authorized && change < MaxNaturalLoss)
            {
                _warnings.AddWarning(player.Id, "MoneyLossHack",
                    $"abnormal loss={change} cur={cur}");
            }

            st.Money = cur;
            return;
        }

        st.Money = cur;
    }

    public void OnPlayerSpawned(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is not null) st.Money = player.Money;
    }

    /// <summary>Call when server takes money from player.</summary>
    public void AllowMoneyLoss(int playerId, int amount)
    {
        var st = _players.Get(playerId);
        if (st is not null)
        {
            st.Money -= amount;
            st.MoneyTakenTick = Environment.TickCount64;
        }
    }

    /// <summary>Call when server gives money to player.</summary>
    public void AllowMoneyGain(int playerId, int amount)
    {
        var st = _players.Get(playerId);
        if (st is not null)
        {
            st.Money += amount;
            st.MoneyGivenTick = Environment.TickCount64;
        }
    }

    public void OnResetPlayerMoney(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.Money = 0;
    }
}