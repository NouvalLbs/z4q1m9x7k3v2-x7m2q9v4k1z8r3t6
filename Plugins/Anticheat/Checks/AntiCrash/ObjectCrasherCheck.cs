using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class ObjectCrasherCheck
{
    private const int MinObjectModel = 321;
    private const int MaxObjectModel = 20000;
    private const int MaxObjectsPerPlayer = 1000;
    private const int MaxObjectCreatesPerSec = 50;
    private const long ObjectSpamWindowMs = 1000;
    private const float MaxObjectDrawDistance = 500f;

    private static readonly int[] _crashModels =
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 18632, 18633, 18634, 18635,
        18636, 18637, 18638, 18639, 18640, 18641, 18642, 18643
    };

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly ConcurrentDictionary<int, int> _playerObjectCounts = new();

    public ObjectCrasherCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public bool ValidateObjectCreate(BasePlayer player, int modelId, float x, float y, float z, float drawDistance)
    {
        if (!_config.Enabled || !_config.GetCheck("ObjectCrasher").Enabled) return true;

        var st = _players.Get(player.Id);
        if (st is null) return true;

        long now = Environment.TickCount64;

        if (modelId < MinObjectModel || modelId > MaxObjectModel)
        {
            _warnings.AddWarning(player.Id, "ObjectCrasher",
                $"invalid model={modelId}");
            return false;
        }

        foreach (int crashModel in _crashModels)
        {
            if (modelId == crashModel)
            {
                _warnings.AddWarning(player.Id, "ObjectCrasher",
                    $"crash model={modelId}");
                return false;
            }
        }

        if (drawDistance > MaxObjectDrawDistance)
        {
            _warnings.AddWarning(player.Id, "ObjectCrasher",
                $"excessive draw distance={drawDistance:F1}");
            return false;
        }

        int objectCount = _playerObjectCounts.GetOrAdd(player.Id, 0);
        if (objectCount >= MaxObjectsPerPlayer)
        {
            _warnings.AddWarning(player.Id, "ObjectCrasher",
                $"object limit={objectCount}");
            return false;
        }

        st.ObjectCreateHistory.Enqueue(now);
        while (st.ObjectCreateHistory.Count > 0 &&
               now - st.ObjectCreateHistory.Peek() > ObjectSpamWindowMs)
        {
            st.ObjectCreateHistory.Dequeue();
        }

        if (st.ObjectCreateHistory.Count > MaxObjectCreatesPerSec)
        {
            _warnings.AddWarning(player.Id, "ObjectCrasher",
                $"spam count={st.ObjectCreateHistory.Count}");
            return false;
        }

        return true;
    }

    public void OnObjectCreated(int playerId)
    {
        _playerObjectCounts.AddOrUpdate(playerId, 1, (_, v) => v + 1);
    }

    public void OnObjectDestroyed(int playerId)
    {
        _playerObjectCounts.AddOrUpdate(playerId, 0, (_, v) => Math.Max(0, v - 1));
    }

    public void OnPlayerDisconnected(int playerId)
    {
        _playerObjectCounts.TryRemove(playerId, out _);
    }
}