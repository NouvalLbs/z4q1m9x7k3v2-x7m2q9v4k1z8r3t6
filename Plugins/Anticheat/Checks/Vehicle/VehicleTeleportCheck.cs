using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class VehicleTeleportCheck
{
    private const float MaxEnterDist = 10f;
    private const float MaxPickupTeleportDist = 20f;
    private const float MaxVehicleToPlayerDist = 15f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public VehicleTeleportCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerEnterVehicle(BasePlayer player, EnterVehicleEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("TeleportVehicleEnter").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetPosTick < 2000) return;
        if (now - st.PutInVehicleTick < 2000) return;

        var vpos = e.Vehicle.Position;
        float dist = VectorMath.Dist(st.X, st.Y, st.Z, vpos.X, vpos.Y, vpos.Z);

        if (dist > MaxEnterDist)
            _warnings.AddWarning(player.Id, "TeleportVehicleEnter", $"d={dist:F1}");
    }

    public void OnPlayerPickUpPickup(BasePlayer player, PickUpPickupEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("TeleportPickup").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetPosTick < 2000) return;

        var ppos = e.Pickup.Position;
        float dist = VectorMath.Dist(st.X, st.Y, st.Z, ppos.X, ppos.Y, ppos.Z);

        if (dist > MaxPickupTeleportDist)
            _warnings.AddWarning(player.Id, "TeleportPickup", $"d={dist:F1}");
    }
}