using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class CarwarpCheck
{
    private const float MaxVehicleWarpDist = 25f; // Max vehicle movement to player
    private const long MinWarpInterval = 1000; // Min time between warps

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public CarwarpCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnPlayerStateChanged(BasePlayer player, StateEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("Carwarp").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        // Detect when player enters vehicle as driver
        if (e.NewState == PlayerState.Driving && e.OldState == PlayerState.OnFoot)
        {
            var vehicle = player.Vehicle;
            if (vehicle is null) return;

            var vst = _vehicles.GetOrCreate(vehicle.Id);

            // Check if vehicle position was tracked
            if (vst.LastKnownX == 0f && vst.LastKnownY == 0f && vst.LastKnownZ == 0f)
            {
                // First time tracking this vehicle
                var vpos = vehicle.Position;
                vst.LastKnownX = vpos.X;
                vst.LastKnownY = vpos.Y;
                vst.LastKnownZ = vpos.Z;
                return;
            }

            // Calculate distance vehicle moved
            var currentVpos = vehicle.Position;
            float vehicleMoveDist = VectorMath.Dist(
                vst.LastKnownX, vst.LastKnownY, vst.LastKnownZ,
                currentVpos.X, currentVpos.Y, currentVpos.Z
            );

            // Calculate distance from player to vehicle's last position
            float playerToOldVehicleDist = VectorMath.Dist(
                st.X, st.Y, st.Z,
                vst.LastKnownX, vst.LastKnownY, vst.LastKnownZ
            );

            // Calculate distance from player to vehicle's current position
            float playerToCurrentVehicleDist = VectorMath.Dist(
                st.X, st.Y, st.Z,
                currentVpos.X, currentVpos.Y, currentVpos.Z
            );

            // Grace periods
            bool graced = now - st.SetPosTick < 2000
                       || now - st.PutInVehicleTick < 2000
                       || now - st.EnterVehicleTick < 500
                       || now - vst.LastServerVehiclePosTick < 2000;

            if (graced) return;

            // Carwarp detection logic:
            // Vehicle was far from player, but is now near player
            // AND vehicle moved significantly
            if (playerToOldVehicleDist > MaxVehicleWarpDist
                && playerToCurrentVehicleDist < 5f
                && vehicleMoveDist > MaxVehicleWarpDist)
            {
                long timeSinceLastWarp = now - st.LastCarwarpTick;

                if (timeSinceLastWarp < MinWarpInterval)
                {
                    _warnings.AddWarning(player.Id, "Carwarp",
                        $"vehicle warped dist={vehicleMoveDist:F1} veh={vehicle.Id}");
                }

                st.LastCarwarpTick = now;
            }

            // Update vehicle position
            vst.LastKnownX = currentVpos.X;
            vst.LastKnownY = currentVpos.Y;
            vst.LastKnownZ = currentVpos.Z;
        }
    }

    public void OnVehicleUpdate(int vehicleId)
    {
        var vehicle = BaseVehicle.Find(vehicleId);
        if (vehicle is null) return;

        var vst = _vehicles.GetOrCreate(vehicleId);
        var pos = vehicle.Position;

        vst.LastKnownX = pos.X;
        vst.LastKnownY = pos.Y;
        vst.LastKnownZ = pos.Z;
        vst.LastTrackedTick = Environment.TickCount64;
    }

    public void OnServerSetVehiclePos(int vehicleId, float x, float y, float z)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.LastKnownX = x;
        vst.LastKnownY = y;
        vst.LastKnownZ = z;
        vst.LastServerVehiclePosTick = Environment.TickCount64;
    }

    public void OnVehicleSpawned(int vehicleId, float x, float y, float z)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.LastKnownX = x;
        vst.LastKnownY = y;
        vst.LastKnownZ = z;
        vst.LastServerVehiclePosTick = Environment.TickCount64;
    }

    public void OnVehicleDestroyed(int vehicleId)
    {
        _vehicles.Remove(vehicleId);
    }
}