using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class AirBreakCheck
{
    private const float MaxZGainNoVelocity = 2.0f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AirBreakCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled) return;
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || st.IsSpectating) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetPosTick < 2000) return;
        if (now - st.PlayerVelocityTick < 2000) return;

        var pState = player.State;
        var vel = player.Velocity;
        var pos = player.Position;

        // Skip jetpack (SpecialAction 2 = UseJetpack)
        if (player.SpecialAction == SpecialAction.Usejetpack) return;

        float zDiff = pos.Z - st.Z;

        if (pState == PlayerState.OnFoot)
        {
            if (!_config.Enabled || !_config.GetCheck("AirBreakOnfoot").Enabled) return;
            if (zDiff > MaxZGainNoVelocity && MathF.Abs(vel.Z) < 0.05f)
                _warnings.AddWarning(player.Id, "AirBreakOnfoot", $"dz={zDiff:F2} vz={vel.Z:F3}");
        }
        else if (pState == PlayerState.Driving)
        {
            if (!_config.Enabled || !_config.GetCheck("AirBreakVehicle").Enabled) return;
            if (now - st.VehicleVelocityTick < 2000) return;
            if (zDiff > MaxZGainNoVelocity * 1.5f && MathF.Abs(vel.Z) < 0.05f)
                _warnings.AddWarning(player.Id, "AirBreakVehicle", $"dz={zDiff:F2}");
        }
    }
}