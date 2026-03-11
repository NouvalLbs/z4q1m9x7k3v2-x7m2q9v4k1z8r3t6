using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class CheckpointTeleportCheck
{
    private const float MaxInstantCheckpointDist = 15f; // Max distance to instant-complete checkpoint
    private const float MaxRaceCheckpointDist = 20f;
    private const long MinCheckpointTravelTime = 200; // Min 200ms to reach checkpoint

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public CheckpointTeleportCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerEnterCheckpoint(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("CheckpointTeleport").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetPosTick < 2000) return;

        // Skip if no checkpoint tracked
        if (!st.HasActiveCheckpoint) return;

        // Check time since checkpoint was set
        long timeSinceSet = now - st.CheckpointSetTick;
        if (timeSinceSet < MinCheckpointTravelTime)
        {
            // Too fast to reach checkpoint
            _warnings.AddWarning(player.Id, "CheckpointTeleport",
                $"instant time={timeSinceSet}ms");
            return;
        }

        // Check distance from last position to checkpoint
        float distToCheckpoint = VectorMath.Dist(
            st.CheckpointEnterX, st.CheckpointEnterY, st.CheckpointEnterZ,
            st.CheckpointX, st.CheckpointY, st.CheckpointZ
        );

        if (distToCheckpoint > MaxInstantCheckpointDist)
        {
            // Calculate expected travel time
            float speed = player.State == PlayerState.Driving && player.Vehicle is not null
                ? VectorMath.Speed(player.Vehicle.Velocity.X, player.Vehicle.Velocity.Y, player.Vehicle.Velocity.Z)
                : VectorMath.Speed(player.Velocity.X, player.Velocity.Y, player.Velocity.Z);

            // Expected time = distance / speed (in game units per ms)
            // Typical max speed ~0.5 units/ms, so min time = dist / 0.5
            long minExpectedTime = (long)(distToCheckpoint / 0.5f);

            if (timeSinceSet < minExpectedTime * 0.3f) // 30% of expected time = suspicious
            {
                _warnings.AddWarning(player.Id, "CheckpointTeleport",
                    $"dist={distToCheckpoint:F1} time={timeSinceSet}ms expected>{minExpectedTime}ms");
            }
        }

        st.HasActiveCheckpoint = false;
    }

    public void OnPlayerEnterRaceCheckpoint(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("RaceCheckpointTeleport").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetPosTick < 2000) return;

        if (!st.HasActiveRaceCheckpoint) return;

        long timeSinceSet = now - st.RaceCheckpointSetTick;
        if (timeSinceSet < MinCheckpointTravelTime)
        {
            _warnings.AddWarning(player.Id, "RaceCheckpointTeleport",
                $"instant time={timeSinceSet}ms");
            return;
        }

        float distToCheckpoint = VectorMath.Dist(
            st.RaceCheckpointEnterX, st.RaceCheckpointEnterY, st.RaceCheckpointEnterZ,
            st.RaceCheckpointX, st.RaceCheckpointY, st.RaceCheckpointZ
        );

        if (distToCheckpoint > MaxRaceCheckpointDist)
        {
            _warnings.AddWarning(player.Id, "RaceCheckpointTeleport",
                $"dist={distToCheckpoint:F1} time={timeSinceSet}ms");
        }

        st.HasActiveRaceCheckpoint = false;
    }

    public void OnCheckpointSet(int playerId, float x, float y, float z, float size)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        var player = BasePlayer.Find(playerId);
        if (player is null) return;

        var pos = player.Position;

        st.CheckpointX = x;
        st.CheckpointY = y;
        st.CheckpointZ = z;
        st.CheckpointSize = size;
        st.CheckpointEnterX = pos.X;
        st.CheckpointEnterY = pos.Y;
        st.CheckpointEnterZ = pos.Z;
        st.CheckpointSetTick = Environment.TickCount64;
        st.HasActiveCheckpoint = true;
    }

    public void OnRaceCheckpointSet(int playerId, float x, float y, float z, float nextX, float nextY, float nextZ, float size)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        var player = BasePlayer.Find(playerId);
        if (player is null) return;

        var pos = player.Position;

        st.RaceCheckpointX = x;
        st.RaceCheckpointY = y;
        st.RaceCheckpointZ = z;
        st.RaceCheckpointSize = size;
        st.RaceCheckpointEnterX = pos.X;
        st.RaceCheckpointEnterY = pos.Y;
        st.RaceCheckpointEnterZ = pos.Z;
        st.RaceCheckpointSetTick = Environment.TickCount64;
        st.HasActiveRaceCheckpoint = true;
    }

    public void OnCheckpointDisabled(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.HasActiveCheckpoint = false;
    }

    public void OnRaceCheckpointDisabled(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.HasActiveRaceCheckpoint = false;
    }
}