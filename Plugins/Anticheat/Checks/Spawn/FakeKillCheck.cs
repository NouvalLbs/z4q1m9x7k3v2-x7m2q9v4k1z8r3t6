using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Spawn;

/// <summary>Check 28 — laporan kematian tidak konsisten dengan state server</summary>
public class FakeKillCheck
{
    // Batas: peluru tidak bisa membunuh dari jarak ini (cek killanim)
    private const float MaxKillShotRange = 350f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public FakeKillCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    /// <summary>Dipanggil dari OnPlayerDeath.</summary>
    public void OnPlayerDied(BasePlayer player, DeathEventArgs e)
    {
        var st = _players.Get(player.Id);
        if (st is null || !_config.Enabled) return;
        if (!_config.GetCheck("FakeKill").Enabled) return;

        long now = Environment.TickCount64;

        // Jika pemain sudah ter-flag IsDead sebelum event ini = double death = suspect
        if (st.IsDead)
        {
            _warnings.AddWarning(player.Id, "FakeKill", "double death event");
            return;
        }

        // Validasi: health harus ~0 saat mati (bukan mati dari state tidak konsisten)
        // Kita tidak set IsDead = true di sini; biarkan OnPlayerSpawn yg reset
        float hp = player.Health;
        if (hp > 1.0f && now - st.SetHealthTick > 1000)
        {
            _warnings.AddWarning(player.Id, "FakeKill", $"hp={hp:F1} at death");
        }

        st.IsDead = true;
        st.PendingDeathResult = false;
    }
}