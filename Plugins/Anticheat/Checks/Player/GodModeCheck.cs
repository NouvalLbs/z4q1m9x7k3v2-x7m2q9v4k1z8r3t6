using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

/// <summary>Checks 19 (onfoot) dan 20 (vehicle) — pemain tidak berkurang HP saat ditembak</summary>
public class GodModeCheck
{
    // Batas toleransi selisih HP yang harus turun setelah damage event
    private const float MinExpectedDecrease = 0.5f;
    private const long DamageWindowMs = 800;  // window menunggu update setelah damage

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public GodModeCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    /// <summary>Dipanggil saat OnPlayerTakeDamage diterima.</summary>
    public void OnPlayerTakeDamage(BasePlayer player, DamageEventArgs e)
    {
        var st = _players.Get(player.Id);
        if (st is null || !_config.Enabled) return;

        // Tandai: ada damage masuk, simpan HP saat ini
        st.PendingDamageResult = true;
        st.PendingVehicleDamageResult = player.State == PlayerState.Driving;
        // Reuse SetHealthTick sebagai damage receipt timestamp
        st.SetHealthTick = Environment.TickCount64;
    }

    /// <summary>Dipanggil dari OnPlayerUpdate setelah damage di-flag.</summary>
    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;
        if (!st.PendingDamageResult) return;

        long now = Environment.TickCount64;
        long elapsed = now - st.SetHealthTick;

        // Belum cukup waktu — tunggu update berikutnya
        if (elapsed < 100) return;
        // Sudah lewat window — reset flag tanpa penalty
        if (elapsed > DamageWindowMs) { st.PendingDamageResult = false; return; }

        float curHp = player.Health;
        bool inVeh = st.PendingVehicleDamageResult;
        string name = inVeh ? "GodModeVehicle" : "GodModeOnfoot";

        if (!_config.GetCheck(name).Enabled) { st.PendingDamageResult = false; return; }

        if (curHp >= st.Health - MinExpectedDecrease && curHp > 0)
        {
            _warnings.AddWarning(player.Id, name, $"hp={curHp:F1} prev={st.Health:F1}");
        }

        st.PendingDamageResult = false;
    }
}