using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.State;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Spawn;

public class AfkGhostCheck
{
    private const long AfkGhostMs = 30_000;
    private const long RoboticKeyMs = 20_000;
    private const long RepetitivePatternMs = 15_000;
    private const float MinMovementSpeed = 0.05f;
    private const int MaxSameKeyPatterns = 5;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AfkGhostCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;
        st.UpdateTick = now;

        CheckRoboticBehavior(player, st, now);
        CheckRepetitivePattern(player, st, now);
    }

    private void CheckRoboticBehavior(BasePlayer player, PlayerAcState st, long now)
    {
        if (!_config.GetCheck("AFKBot").Enabled) return;
        if (now - st.SpawnTick < 5000) return;

        var pos = player.Position;
        var vel = player.Velocity;
        float speed = MathF.Sqrt(vel.X * vel.X + vel.Y * vel.Y);
        float dist = VectorMath.Dist2D(st.X, st.Y, pos.X, pos.Y);

        if (dist > MinMovementSpeed || speed > MinMovementSpeed)
        {
            player.GetKeys(out Keys currentKeys, out _, out _);

            if (st.LastKeys != Keys.No)
            {
                if (currentKeys == st.LastKeys)
                {
                    long keyHoldTime = now - st.LastKeyChangeTick;

                    if (keyHoldTime > RoboticKeyMs)
                    {
                        _warnings.AddWarning(player.Id, "AFKBot",
                            $"robotic movement keys_held={keyHoldTime}ms dist={dist:F2}");
                        st.LastKeyChangeTick = now;
                    }
                }
                else
                {
                    st.LastKeyChangeTick = now;
                }
            }
            else
            {
                st.LastKeyChangeTick = now;
            }

            st.LastKeys = currentKeys;
        }
    }

    private void CheckRepetitivePattern(BasePlayer player, PlayerAcState st, long now)
    {
        if (!_config.GetCheck("AFKBotPattern").Enabled) return;

        player.GetKeys(out Keys currentKeys, out _, out _);

        if (currentKeys != Keys.No)
        {
            st.KeyPressHistory.Enqueue((currentKeys, now));

            while (st.KeyPressHistory.Count > 20)
                st.KeyPressHistory.Dequeue();

            if (st.KeyPressHistory.Count >= 10)
            {
                var patterns = new Dictionary<Keys, int>();
                long oldestTime = long.MaxValue;

                foreach (var (key, time) in st.KeyPressHistory)
                {
                    if (!patterns.ContainsKey(key))
                        patterns[key] = 0;
                    patterns[key]++;

                    if (time < oldestTime)
                        oldestTime = time;
                }

                int maxCount = 0;
                foreach (var count in patterns.Values)
                {
                    if (count > maxCount)
                        maxCount = count;
                }

                long patternDuration = now - oldestTime;

                if (maxCount >= MaxSameKeyPatterns && patternDuration > RepetitivePatternMs)
                {
                    _warnings.AddWarning(player.Id, "AFKBotPattern",
                        $"repetitive pattern count={maxCount} duration={patternDuration}ms");
                    st.KeyPressHistory.Clear();
                }
            }
        }
    }

    public void Tick()
    {
        if (!_config.Enabled || !_config.GetCheck("AfkGhost").Enabled) return;

        long now = Environment.TickCount64;

        foreach (var (id, st) in _players.All)
        {
            if (!st.IsOnline || st.IsDead || st.IsSpectating) continue;
            if (st.UpdateTick == 0) continue;
            if (now - st.SpawnTick < 5000) continue;

            if (now - st.UpdateTick > AfkGhostMs)
            {
                _warnings.AddWarning(id, "AfkGhost",
                    $"no update for {(now - st.UpdateTick) / 1000}s");
                st.UpdateTick = now;
            }
        }
    }
}