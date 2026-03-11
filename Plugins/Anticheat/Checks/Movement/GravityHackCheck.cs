using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.State;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class GravityHackCheck
{
    private const float NormalGravity = -0.008f; // SA-MP normal gravity per update
    private const float MaxGravityDeviation = 0.005f; // Max deviation from normal
    private const float MaxSlideSpeed = 0.15f; // Max speed while "sliding" on ground
    private const float MinFriction = 0.92f; // Min velocity decay per update (friction)
    private const int ConsecutiveSlideChecks = 4; // Need 4 consecutive to confirm

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public GravityHackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.PlayerVelocityTick < 2000) return;
        if (now - st.SetPosTick < 2000) return;

        var pos = player.Position;
        var vel = player.Velocity;

        // Skip special actions
        if (player.SpecialAction == SpecialAction.Usejetpack) return;
        if (player.AnimationIndex == 1133) return; // Parachute

        // Check moon gravity
        if (_config.GetCheck("MoonGravity").Enabled)
        {
            CheckMoonGravity(player, st, vel);
        }

        // Check ice/slide hack
        if (_config.GetCheck("IceSlide").Enabled)
        {
            CheckIceSlide(player, st, pos, vel);
        }
    }

    private void CheckMoonGravity(BasePlayer player, PlayerAcState st, SampSharp.GameMode.Vector3 vel)
    {
        // Moon gravity detection: abnormally slow fall speed
        if (vel.Z < 0f && vel.Z > NormalGravity + MaxGravityDeviation)
        {
            st.AbnormalGravityCount++;

            if (st.AbnormalGravityCount >= ConsecutiveSlideChecks)
            {
                _warnings.AddWarning(player.Id, "MoonGravity",
                    $"slow fall vz={vel.Z:F4} normal={NormalGravity:F4}");
                st.AbnormalGravityCount = 0; // Reset to avoid spam
            }
        }
        else
        {
            st.AbnormalGravityCount = 0;
        }
    }

    private void CheckIceSlide(BasePlayer player, PlayerAcState st, SampSharp.GameMode.Vector3 pos, SampSharp.GameMode.Vector3 vel)
    {
        // Ice slide detection: maintaining horizontal velocity without input
        float horizontalSpeed = VectorMath.Speed(vel.X, vel.Y, 0f);

        // Check if player is on ground (low Z velocity)
        bool onGround = Math.Abs(vel.Z) < 0.02f && Math.Abs(pos.Z - st.Z) < 0.5f;

        if (onGround && horizontalSpeed > MaxSlideSpeed)
        {
            // Player is moving fast horizontally while on ground
            player.GetKeys(out Keys keys, out _, out _);

            bool hasMovementInput = (keys & (Keys.Up | Keys.Down | Keys.Left | Keys.Right)) != 0;
            bool isSprinting = (keys & Keys.Sprint) != 0;

            // If no input but still moving fast = sliding
            if (!hasMovementInput && !isSprinting)
            {
                st.SlideDetectionCount++;

                if (st.SlideDetectionCount >= ConsecutiveSlideChecks)
                {
                    _warnings.AddWarning(player.Id, "IceSlide",
                        $"sliding speed={horizontalSpeed:F3} no input");
                    st.SlideDetectionCount = 0;
                }
            }
            else
            {
                st.SlideDetectionCount = 0;
            }

            // Check velocity decay (friction)
            if (st.LastHorizontalSpeed > 0f)
            {
                float velocityRatio = horizontalSpeed / st.LastHorizontalSpeed;

                // Normal friction should reduce velocity each update
                if (velocityRatio > 1.0f || velocityRatio > MinFriction)
                {
                    st.NoFrictionCount++;

                    if (st.NoFrictionCount >= ConsecutiveSlideChecks)
                    {
                        _warnings.AddWarning(player.Id, "IceSlide",
                            $"no friction ratio={velocityRatio:F3}");
                        st.NoFrictionCount = 0;
                    }
                }
                else
                {
                    st.NoFrictionCount = 0;
                }
            }

            st.LastHorizontalSpeed = horizontalSpeed;
        }
        else
        {
            st.SlideDetectionCount = 0;
            st.NoFrictionCount = 0;
            st.LastHorizontalSpeed = 0f;
        }
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.AbnormalGravityCount = 0;
        st.SlideDetectionCount = 0;
        st.NoFrictionCount = 0;
        st.LastHorizontalSpeed = 0f;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.AbnormalGravityCount = 0;
        st.SlideDetectionCount = 0;
        st.NoFrictionCount = 0;
        st.LastHorizontalSpeed = 0f;
    }

    public void OnPlayerEnterVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.AbnormalGravityCount = 0;
        st.SlideDetectionCount = 0;
        st.NoFrictionCount = 0;
        st.LastHorizontalSpeed = 0f;
    }
}