using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class MacroDetectionCheck
{
    private const int PatternSampleSize = 10; // Track last 10 actions
    private const float MaxTimingVariance = 15f; // Max 15ms variance = macro
    private const int MinPatternLength = 5; // Min 5 identical intervals = pattern
    private const float InhumanPrecision = 5f; // < 5ms variance = inhuman

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public MacroDetectionCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerWeaponShot(BasePlayer player, WeaponShotEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("MacroShoot").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 5000) return;

        int weaponId = (int)e.Weapon;

        // Track shot timing
        st.ShotTimings.Enqueue(now);
        if (st.ShotTimings.Count > PatternSampleSize)
            st.ShotTimings.Dequeue();

        // Analyze pattern if enough samples
        if (st.ShotTimings.Count >= MinPatternLength)
        {
            var intervals = CalculateIntervals(st.ShotTimings);
            float variance = CalculateVariance(intervals);

            // Inhuman precision detection
            if (variance < InhumanPrecision && intervals.Count >= MinPatternLength)
            {
                _warnings.AddWarning(player.Id, "MacroShoot",
                    $"inhuman precision wid={weaponId} variance={variance:F2}ms");
            }
            // Macro pattern detection
            else if (variance < MaxTimingVariance && intervals.Count >= MinPatternLength)
            {
                // Check if intervals are nearly identical
                float avgInterval = intervals.Average();
                int identicalCount = intervals.Count(x => Math.Abs(x - avgInterval) < 10f);

                if (identicalCount >= MinPatternLength)
                {
                    _warnings.AddWarning(player.Id, "MacroShoot",
                        $"pattern detected wid={weaponId} interval={avgInterval:F0}ms variance={variance:F2}ms");
                }
            }
        }
    }

    public void OnPlayerKeyStateChange(BasePlayer player, Keys newKeys, Keys oldKeys)
    {
        if (!_config.Enabled || !_config.GetCheck("MacroKeys").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;

        // Track jump timing (common macro target)
        bool jumpPressed = (newKeys & Keys.Jump) != 0 && (oldKeys & Keys.Jump) == 0;
        if (jumpPressed)
        {
            st.JumpTimings.Enqueue(now);
            if (st.JumpTimings.Count > PatternSampleSize)
                st.JumpTimings.Dequeue();

            if (st.JumpTimings.Count >= MinPatternLength)
            {
                var intervals = CalculateIntervals(st.JumpTimings);
                float variance = CalculateVariance(intervals);

                if (variance < MaxTimingVariance)
                {
                    _warnings.AddWarning(player.Id, "MacroKeys",
                        $"jump macro variance={variance:F2}ms");
                }
            }
        }

        // Track sprint timing
        bool sprintPressed = (newKeys & Keys.Sprint) != 0 && (oldKeys & Keys.Sprint) == 0;
        if (sprintPressed)
        {
            st.SprintTimings.Enqueue(now);
            if (st.SprintTimings.Count > PatternSampleSize)
                st.SprintTimings.Dequeue();

            if (st.SprintTimings.Count >= MinPatternLength)
            {
                var intervals = CalculateIntervals(st.SprintTimings);
                float variance = CalculateVariance(intervals);

                if (variance < InhumanPrecision)
                {
                    _warnings.AddWarning(player.Id, "MacroKeys",
                        $"sprint macro variance={variance:F2}ms");
                }
            }
        }
    }

    public void OnPlayerCommandText(BasePlayer player, string command)
    {
        if (!_config.Enabled || !_config.GetCheck("MacroCommand").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;

        // Track command timing
        st.CommandTimings.Enqueue(now);
        if (st.CommandTimings.Count > PatternSampleSize)
            st.CommandTimings.Dequeue();

        // Check for command spam macro
        if (st.CommandTimings.Count >= 5)
        {
            var intervals = CalculateIntervals(st.CommandTimings);
            float variance = CalculateVariance(intervals);

            if (variance < MaxTimingVariance)
            {
                _warnings.AddWarning(player.Id, "MacroCommand",
                    $"cmd={command} variance={variance:F2}ms");
            }
        }
    }

    public void OnPlayerText(BasePlayer player, string text)
    {
        if (!_config.Enabled || !_config.GetCheck("MacroChat").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;

        // Track chat timing
        st.ChatTimings.Enqueue(now);
        if (st.ChatTimings.Count > PatternSampleSize)
            st.ChatTimings.Dequeue();

        // Detect chat spam bots
        if (st.ChatTimings.Count >= 5)
        {
            var intervals = CalculateIntervals(st.ChatTimings);
            float variance = CalculateVariance(intervals);

            if (variance < MaxTimingVariance)
            {
                _warnings.AddWarning(player.Id, "MacroChat",
                    $"variance={variance:F2}ms msg='{text.Substring(0, Math.Min(20, text.Length))}'");
            }
        }

        // Track message similarity (spam detection)
        if (!string.IsNullOrEmpty(st.LastChatMessage) && st.LastChatMessage == text)
        {
            st.DuplicateChatCount++;
            if (st.DuplicateChatCount >= 3)
            {
                _warnings.AddWarning(player.Id, "MacroChat",
                    $"duplicate msg count={st.DuplicateChatCount}");
            }
        }
        else
        {
            st.DuplicateChatCount = 0;
        }

        st.LastChatMessage = text;
    }

    public void OnPlayerDisconnected(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.ShotTimings.Clear();
        st.JumpTimings.Clear();
        st.SprintTimings.Clear();
        st.CommandTimings.Clear();
        st.ChatTimings.Clear();
    }

    private static List<float> CalculateIntervals(Queue<long> timings)
    {
        var intervals = new List<float>();
        var array = timings.ToArray();

        for (int i = 1; i < array.Length; i++)
        {
            intervals.Add(array[i] - array[i - 1]);
        }

        return intervals;
    }

    private static float CalculateVariance(List<float> values)
    {
        if (values.Count < 2) return float.MaxValue;

        float mean = values.Average();
        float sumSquaredDiff = values.Sum(x => (x - mean) * (x - mean));
        return MathF.Sqrt(sumSquaredDiff / values.Count);
    }
}