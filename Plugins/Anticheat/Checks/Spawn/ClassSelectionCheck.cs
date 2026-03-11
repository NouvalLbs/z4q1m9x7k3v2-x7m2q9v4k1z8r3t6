using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Spawn;

public class ClassSelectionCheck
{
    private const int MaxClassChangesPerMinute = 30;
    private const long ClassChangeWindowMs = 60000; // 60 seconds
    private const long MinTimeBetweenClassChanges = 100; // 100ms minimum
    private const long MaxClassSelectionTime = 120000; // 2 minutes max in class selection

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly HashSet<int> _validClassIds = new();

    public ClassSelectionCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerRequestClass(BasePlayer player, RequestClassEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("ClassSelectionSpam").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;

        // Track class selection spam
        st.ClassChangeHistory.Enqueue(now);

        // Clean old history
        while (st.ClassChangeHistory.Count > 0 &&
               now - st.ClassChangeHistory.Peek() > ClassChangeWindowMs)
        {
            st.ClassChangeHistory.Dequeue();
        }

        // Check spam
        if (st.ClassChangeHistory.Count > MaxClassChangesPerMinute)
        {
            _warnings.AddWarning(player.Id, "ClassSelectionSpam",
                $"count={st.ClassChangeHistory.Count} in 60s");
            e.PreventSpawning = true;
            return;
        }

        // Check rapid class switching
        if (st.LastClassChangeTick > 0)
        {
            long timeSinceLastChange = now - st.LastClassChangeTick;
            if (timeSinceLastChange < MinTimeBetweenClassChanges)
            {
                _warnings.AddWarning(player.Id, "ClassSelectionSpam",
                    $"rapid switch interval={timeSinceLastChange}ms");
            }
        }

        // Validate class ID if we have registered classes
        if (_config.GetCheck("InvalidClassId").Enabled && _validClassIds.Count > 0)
        {
            if (!_validClassIds.Contains(e.ClassId))
            {
                _warnings.AddWarning(player.Id, "InvalidClassId",
                    $"classId={e.ClassId}");
                e.PreventSpawning = true;
                return;
            }
        }

        st.LastClassChangeTick = now;
        st.SelectedClassId = e.ClassId;
        st.IsInClassSelection = true;
        st.ClassSelectionStartTick = st.ClassSelectionStartTick == 0 ? now : st.ClassSelectionStartTick;
        st.PendingClassResult = true;
    }

    public void OnPlayerRequestSpawn(BasePlayer player, RequestSpawnEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("ClassSelectionExploit").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;

        // Check if player was in class selection
        if (!st.IsInClassSelection && !st.PendingClassResult)
        {
            _warnings.AddWarning(player.Id, "ClassSelectionExploit",
                "spawn without class selection");
            e.PreventSpawning = true;
            return;
        }

        // Check excessive time in class selection (AFK exploit)
        if (st.ClassSelectionStartTick > 0)
        {
            long timeInSelection = now - st.ClassSelectionStartTick;
            if (timeInSelection > MaxClassSelectionTime)
            {
                _warnings.AddWarning(player.Id, "ClassSelectionExploit",
                    $"excessive time={timeInSelection}ms in selection");
            }
        }

        st.IsInClassSelection = false;
        st.ClassSelectionStartTick = 0;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.IsInClassSelection = false;
        st.ClassSelectionStartTick = 0;
        st.PendingClassResult = false;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        // Reset class selection state on death
        st.IsInClassSelection = false;
        st.ClassSelectionStartTick = 0;
    }

    public void OnPlayerConnected(int playerId)
    {
        var st = _players.GetOrCreate(playerId);
        st.ClassChangeHistory.Clear();
        st.LastClassChangeTick = 0;
        st.IsInClassSelection = false;
        st.ClassSelectionStartTick = 0;
    }

    // Server registers valid class IDs
    public void RegisterClass(int classId)
    {
        _validClassIds.Add(classId);
    }

    public void ClearRegisteredClasses()
    {
        _validClassIds.Clear();
    }

    public bool IsValidClass(int classId)
        => _validClassIds.Count == 0 || _validClassIds.Contains(classId);
}