using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.State;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class WallClipCheck
{
    private const float MaxSwimHeight = 0.5f; // Max Z above water for swimming
    private const float MinFlySpeed = 0.15f; // Min speed to consider flying
    private const float MaxInteriorClipDist = 5f; // Max movement through interior
    private const float WaterLevel = 0.0f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public WallClipCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
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

        var pos = player.Position;
        var vel = player.Velocity;
        var pState = player.State;

        // Check swimming above water
        if (pState == PlayerState.Swimming)
        {
            CheckSwimmingAboveWater(player, st, pos);
        }

        // Check flying through walls
        if (pState == PlayerState.OnFoot)
        {
            CheckFlyingThroughWalls(player, st, pos, vel);
        }

        // Check interior clipping
        if (player.Interior > 0)
        {
            CheckInteriorClipping(player, st, pos);
        }
    }

    private void CheckSwimmingAboveWater(BasePlayer player, PlayerAcState st, SampSharp.GameMode.Vector3 pos)
    {
        if (!_config.GetCheck("SwimThroughWall").Enabled) return;

        // Swimming should only happen at/below water level
        if (pos.Z > WaterLevel + MaxSwimHeight)
        {
            // Check if in known water areas but elevated
            bool inWaterArea = IsInWaterArea(pos.X, pos.Y);

            if (!inWaterArea)
            {
                _warnings.AddWarning(player.Id, "SwimThroughWall",
                    $"swimming z={pos.Z:F2} (not in water)");
            }
            else if (pos.Z > WaterLevel + 2f) // Significantly above water
            {
                _warnings.AddWarning(player.Id, "SwimThroughWall",
                    $"swimming elevated z={pos.Z:F2}");
            }
        }
    }

    private void CheckFlyingThroughWalls(BasePlayer player, PlayerAcState st, SampSharp.GameMode.Vector3 pos, SampSharp.GameMode.Vector3 vel)
    {
        if (!_config.GetCheck("FlyThroughWall").Enabled) return;

        // Detect flying (sustained upward velocity)
        if (vel.Z > 0.05f && player.SpecialAction != SpecialAction.Usejetpack)
        {
            float speed = VectorMath.Speed(vel.X, vel.Y, vel.Z);

            if (speed > MinFlySpeed)
            {
                // Check if in enclosed space (building)
                if (IsInKnownBuilding(pos.X, pos.Y, pos.Z, player.Interior))
                {
                    _warnings.AddWarning(player.Id, "FlyThroughWall",
                        $"flying in building z={pos.Z:F2} vz={vel.Z:F3}");
                }
            }
        }
    }

    private void CheckInteriorClipping(BasePlayer player, PlayerAcState st, SampSharp.GameMode.Vector3 pos)
    {
        if (!_config.GetCheck("InteriorClip").Enabled) return;

        long now = Environment.TickCount64;

        // Track movement distance in interiors
        if (st.UpdateTick > 0 && now - st.UpdateTick < 200)
        {
            float dist = VectorMath.Dist(st.X, st.Y, st.Z, pos.X, pos.Y, pos.Z);

            // Rapid movement through interior = potential clip
            if (dist > MaxInteriorClipDist && now - st.LastInteriorChangeTick > 2000)
            {
                _warnings.AddWarning(player.Id, "InteriorClip",
                    $"rapid movement dist={dist:F1} interior={player.Interior}");
            }
        }
    }

    private static bool IsInWaterArea(float x, float y)
    {
        // Ocean (most of map edges)
        if (x < -1500f || x > 3000f || y < -3000f || y > 500f)
            return true;

        // San Fierro bay
        if (x > -3000f && x < -1500f && y > -800f && y < 2000f)
            return true;

        // Las Venturas lake
        if (x > 500f && x < 1500f && y > 500f && y < 1500f)
            return true;

        // Dam area
        if (x > -1200f && x < -800f && y > 1200f && y < 1800f)
            return true;

        return false;
    }

    private static bool IsInKnownBuilding(float x, float y, float z, int interior)
    {
        // If in interior ID > 0, assume enclosed
        if (interior > 0) return true;

        // Check known building areas in exterior world
        // Los Santos - Jefferson area buildings
        if (x > 2000f && x < 2500f && y > -1500f && y < -1000f && z > 10f && z < 50f)
            return true;

        // Los Santos - Downtown high-rises
        if (x > 1000f && x < 2000f && y > -2000f && y < -1000f && z > 20f && z < 100f)
            return true;

        // San Fierro - Downtown buildings
        if (x > -2500f && x < -2000f && y > 0f && y < 500f && z > 10f && z < 80f)
            return true;

        // Las Venturas - Casinos
        if (x > 1500f && x < 2500f && y > 500f && y < 2000f && z > 10f && z < 50f)
            return true;

        return false;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.LastInteriorChangeTick = Environment.TickCount64;
    }
}