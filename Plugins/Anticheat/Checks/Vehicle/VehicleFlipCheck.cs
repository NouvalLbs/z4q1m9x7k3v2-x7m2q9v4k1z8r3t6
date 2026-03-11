using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class VehicleFlipCheck
{
    private const float MaxAngleChangePerUpdate = 45f; // Max 45° rotation per update
    private const float MaxRollChange = 60f; // Max roll change per update
    private const int MaxFlipsPerMinute = 5; // Max vehicle flips in 1 minute
    private const long FlipWindowMs = 60000; // 60 second window

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public VehicleFlipCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("VehicleFlipSpam").Enabled) return;
        if (player.State != PlayerState.Driving) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        var vehicle = player.Vehicle;
        if (vehicle is null) return;

        var vst = _vehicles.GetOrCreate(vehicle.Id);
        long now = Environment.TickCount64;

        if (now - st.SpawnTick < 3000) return;
        if (now - st.EnterVehicleTick < 2000) return;

        // Get current vehicle rotation
        float currentAngle = vehicle.Angle;
        var quat = vehicle.Rotation;

        // Calculate roll and pitch from quaternion
        float roll = CalculateRoll(quat);
        float pitch = CalculatePitch(quat);

        // Check if this is first update for this vehicle
        if (vst.LastUpdateTick == 0)
        {
            vst.LastVehicleAngle = currentAngle;
            vst.LastVehicleRoll = roll;
            vst.LastVehiclePitch = pitch;
            vst.LastUpdateTick = now;
            return;
        }

        // Calculate angle changes
        float angleDiff = AngleHelper.Diff(currentAngle, vst.LastVehicleAngle);
        float rollDiff = Math.Abs(roll - vst.LastVehicleRoll);
        float pitchDiff = Math.Abs(pitch - vst.LastVehiclePitch);

        // Detect rapid rotation spam
        if (angleDiff > MaxAngleChangePerUpdate)
        {
            _warnings.AddWarning(player.Id, "VehicleFlipSpam",
                $"rapid rotation angle={angleDiff:F1}° veh={vehicle.Id}");
        }

        // Detect roll spam
        if (rollDiff > MaxRollChange)
        {
            _warnings.AddWarning(player.Id, "VehicleFlipSpam",
                $"rapid roll change={rollDiff:F1}° veh={vehicle.Id}");
        }

        // Detect impossible orientation (upside down fixes)
        if (Math.Abs(roll) > 160f || Math.Abs(pitch) > 160f)
        {
            // Vehicle is upside down or nearly vertical
            long timeSinceLastFlip = now - vst.LastFlipTick;

            if (timeSinceLastFlip < 500 && vst.FlipCount > 0)
            {
                _warnings.AddWarning(player.Id, "VehicleFlipSpam",
                    $"instant flip recovery veh={vehicle.Id}");
            }
        }

        // Track flip events (when vehicle goes from upside down to upright rapidly)
        bool wasUpsideDown = Math.Abs(vst.LastVehicleRoll) > 140f;
        bool isUpright = Math.Abs(roll) < 30f;

        if (wasUpsideDown && isUpright && now - vst.LastFlipTick > 100)
        {
            vst.FlipHistory.Enqueue(now);
            vst.FlipCount++;
            vst.LastFlipTick = now;

            // Clean old flip history
            while (vst.FlipHistory.Count > 0 && now - vst.FlipHistory.Peek() > FlipWindowMs)
            {
                vst.FlipHistory.Dequeue();
            }

            // Check flip spam
            if (vst.FlipHistory.Count > MaxFlipsPerMinute)
            {
                _warnings.AddWarning(player.Id, "VehicleFlipSpam",
                    $"flip spam count={vst.FlipHistory.Count} in 60s veh={vehicle.Id}");
            }
        }

        // Update last values
        vst.LastVehicleAngle = currentAngle;
        vst.LastVehicleRoll = roll;
        vst.LastVehiclePitch = pitch;
        vst.LastUpdateTick = now;
    }

    public void OnPlayerEnterVehicle(int playerId, int vehicleId)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.LastUpdateTick = 0; // Reset for new driver
        vst.FlipCount = 0;
    }

    public void OnVehicleRespawned(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is null) return;

        vst.LastVehicleAngle = 0f;
        vst.LastVehicleRoll = 0f;
        vst.LastVehiclePitch = 0f;
        vst.LastUpdateTick = 0;
        vst.FlipCount = 0;
        vst.FlipHistory.Clear();
    }

    // Calculate roll from quaternion
    private static float CalculateRoll(SampSharp.GameMode.Quaternion q)
    {
        float sinr_cosp = 2f * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1f - 2f * (q.X * q.X + q.Y * q.Y);
        return MathF.Atan2(sinr_cosp, cosr_cosp) * (180f / MathF.PI);
    }

    // Calculate pitch from quaternion
    private static float CalculatePitch(SampSharp.GameMode.Quaternion q)
    {
        float sinp = 2f * (q.W * q.Y - q.Z * q.X);
        if (MathF.Abs(sinp) >= 1f)
            return MathF.CopySign(90f, sinp);
        return MathF.Asin(sinp) * (180f / MathF.PI);
    }
}