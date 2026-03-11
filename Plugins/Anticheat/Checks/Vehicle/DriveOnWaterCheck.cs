using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class DriveOnWaterCheck
{
    private const float WaterLevelTolerance = 0.5f; // Z +/- 0.5 from water = on water
    private const float MinWaterSpeed = 0.1f; // Minimum speed to consider "driving"
    private const float SeaLevel = 0.0f; // Default sea level Z
    private const int ConsecutiveChecks = 3; // Must be on water for 3 checks

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public DriveOnWaterCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("DriveOnWater").Enabled) return;
        if (player.State != PlayerState.Driving) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        var vehicle = player.Vehicle;
        if (vehicle is null) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.EnterVehicleTick < 2000) return;
        if (now - st.SetPosTick < 2000) return;

        int model = (int)vehicle.Model;

        // Skip boats, helicopters, planes, and special vehicles
        if (VehicleData.IsBoat(model) || VehicleData.IsHelicopter(model)
            || VehicleData.IsAircraft(model) || VehicleData.IsRC(model))
        {
            st.DriveOnWaterCount = 0;
            return;
        }

        var pos = vehicle.Position;
        var vel = vehicle.Velocity;
        float speed = VectorMath.Speed(vel.X, vel.Y, 0f); // Horizontal speed only

        // Check if vehicle is on water surface
        bool onWater = IsOnWaterSurface(pos.X, pos.Y, pos.Z);

        if (onWater && speed > MinWaterSpeed)
        {
            st.DriveOnWaterCount++;

            // Warn after consecutive detections
            if (st.DriveOnWaterCount >= ConsecutiveChecks)
            {
                _warnings.AddWarning(player.Id, "DriveOnWater",
                    $"model={model} z={pos.Z:F2} spd={speed:F2}");
                st.DriveOnWaterCount = 0; // Reset to avoid spam
            }
        }
        else
        {
            st.DriveOnWaterCount = 0;
        }
    }

    private static bool IsOnWaterSurface(float x, float y, float z)
    {
        // Sea level check (most common)
        if (Math.Abs(z - SeaLevel) < WaterLevelTolerance)
            return true;

        // Known water areas in SA-MP map
        // Los Santos ocean
        if (x < -1500f || x > 3000f || y < -3000f || y > 500f)
        {
            if (z < WaterLevelTolerance && z > -WaterLevelTolerance)
                return true;
        }

        // San Fierro bay area
        if (x > -3000f && x < -1500f && y > -800f && y < 2000f)
        {
            if (z < WaterLevelTolerance && z > -WaterLevelTolerance)
                return true;
        }

        // Las Venturas lake area
        if (x > 500f && x < 1500f && y > 500f && y < 1500f)
        {
            if (z < 10f && z > -2f) // Lake is slightly above sea level
                return true;
        }

        // Dam area
        if (x > -1200f && x < -800f && y > 1200f && y < 1800f)
        {
            if (z < 50f && z > 45f) // Dam water level
                return true;
        }

        return false;
    }

    public void OnPlayerEnterVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.DriveOnWaterCount = 0;
    }

    public void OnPlayerExitVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.DriveOnWaterCount = 0;
    }
}