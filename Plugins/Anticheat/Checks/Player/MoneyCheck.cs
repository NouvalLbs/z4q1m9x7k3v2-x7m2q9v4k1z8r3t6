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

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public MoneyCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;
        if (!_config.GetCheck("MoneyHack").Enabled) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 2500) { st.Money = player.Money; return; }

        int cur = player.Money;
        int gain = cur - st.Money;

        if (gain <= AllowedGain) { st.Money = cur; return; }

        // Izinkan jika server baru saja beri GivePlayerMoney — tidak ada tick khusus
        // di state, tapi kita bisa cek apakah gain masuk akal untuk pickup/dll.
        // Untuk detection ketat, semua gain tanpa izin server = warn.
        _warnings.AddWarning(player.Id, "MoneyHack", $"gain={gain} cur={cur}");
        st.Money = cur;
    }

    public void OnPlayerSpawned(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is not null) st.Money = player.Money;
    }

    /// <summary>Panggil ini setiap kali script memberi uang ke pemain.</summary>
    public void AllowMoneyGain(int playerId, int amount)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.Money += amount;
    }
}