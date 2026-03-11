using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class InfiniteRunCheck
{
    private const long MaxContinuousSprintMs = 15000; // Max 15 seconds continuous sprint
    private const long MinRestTimeMs = 3000; // Min 3 seconds rest between sprints
    private const long StaminaRegenMs = 5000; // 5 seconds to regenerate stamina

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public InfiniteRunCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("InfiniteRun").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.PlayerVelocityTick < 2000) return;

        // Get current keys
        player.GetKeys(out Keys keys, out _, out _);
        bool isSprinting = (keys & Keys.Sprint) != 0;

        // Check if player is actually moving (not just holding sprint key)
        var vel = player.Velocity;
        float speed = MathF.Sqrt(vel.X * vel.X + vel.Y * vel.Y);
        bool isMoving = speed > 0.05f;

        if (isSprinting && isMoving)
        {
            // Player is sprinting
            if (!st.IsSprinting)
            {
                // Started sprinting
                long restTime = now - st.LastSprintEndTick;

                // Check if had enough rest
                if (st.TotalSprintTime > MaxContinuousSprintMs * 0.8f && restTime < MinRestTimeMs)
                {
                    _warnings.AddWarning(player.Id, "InfiniteRun",
                        $"no rest sprint={st.TotalSprintTime}ms rest={restTime}ms");
                }

                st.IsSprinting = true;
                st.SprintStartTick = now;
            }
            else
            {
                // Continue sprinting
                long sprintDuration = now - st.SprintStartTick;

                // Check for infinite sprint
                if (sprintDuration > MaxContinuousSprintMs)
                {
                    _warnings.AddWarning(player.Id, "InfiniteRun",
                        $"duration={sprintDuration}ms max={MaxContinuousSprintMs}ms");

                    // Reset to avoid spam
                    st.SprintStartTick = now;
                }

                st.TotalSprintTime = sprintDuration;
            }
        }
        else
        {
            // Player stopped sprinting or not moving
            if (st.IsSprinting)
            {
                // Just stopped sprinting
                st.IsSprinting = false;
                st.LastSprintEndTick = now;
                st.TotalSprintTime = now - st.SprintStartTick;
            }
            else
            {
                // Regenerate stamina while resting
                long restDuration = now - st.LastSprintEndTick;
                if (restDuration > StaminaRegenMs)
                {
                    st.TotalSprintTime = 0; // Full stamina regenerated
                }
            }
        }
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.IsSprinting = false;
        st.SprintStartTick = 0;
        st.LastSprintEndTick = 0;
        st.TotalSprintTime = 0;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.IsSprinting = false;
        st.SprintStartTick = 0;
        st.LastSprintEndTick = 0;
        st.TotalSprintTime = 0;
    }

    public void OnPlayerEnterVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.IsSprinting = false;
        if (st.SprintStartTick > 0)
        {
            st.LastSprintEndTick = Environment.TickCount64;
            st.TotalSprintTime = 0;
        }
    }
}