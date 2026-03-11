using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class AnimationHackCheck
{
    private const int MaxAnimationIndex = 1812; // Max valid animation in SA-MP
    private const int MinAnimSpamInterval = 500; // 500ms between animations
    private const int MaxAnimChangesPerSec = 5;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AnimationHackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("AnimationHack").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        int animIndex = player.AnimationIndex;
        if (animIndex == 0) return; // No animation

        // Check 1: Invalid animation index
        if (animIndex < 0 || animIndex > MaxAnimationIndex)
        {
            _warnings.AddWarning(player.Id, "AnimationHack", $"invalid index={animIndex}");
            return;
        }

        // Check 2: Animation spam detection
        if (animIndex != st.LastAnimationIndex)
        {
            long elapsed = now - st.LastAnimationChangeTick;

            if (elapsed > 0 && elapsed < MinAnimSpamInterval)
            {
                st.AnimationSpamCount++;
                if (st.AnimationSpamCount >= MaxAnimChangesPerSec)
                {
                    _warnings.AddWarning(player.Id, "AnimationHack",
                        $"spam count={st.AnimationSpamCount} interval={elapsed}ms");
                    st.AnimationSpamCount = 0;
                }
            }
            else
            {
                st.AnimationSpamCount = 0;
            }

            st.LastAnimationIndex = animIndex;
            st.LastAnimationChangeTick = now;
        }

        // Reset spam counter after 1 second of no changes
        if (now - st.LastAnimationChangeTick > 1000)
            st.AnimationSpamCount = 0;
    }

    public void OnAnimationApplied(int playerId, int animLib, string animName)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        // Mark as server-authorized
        st.LastServerAnimTick = Environment.TickCount64;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.LastAnimationIndex = 0;
        st.AnimationSpamCount = 0;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.LastAnimationIndex = 0;
        st.AnimationSpamCount = 0;
    }
}