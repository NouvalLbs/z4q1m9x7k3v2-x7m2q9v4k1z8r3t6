using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class Text3DCrasherCheck
{
    private const int Max3DTexts = 1024;
    private const int Max3DTextsPerSec = 50;
    private const long Text3DSpamWindowMs = 1000;
    private const int MaxTextLength = 1024;
    private const float MaxDrawDistance = 500f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly ConcurrentDictionary<int, int> _player3DTextCounts = new();

    public Text3DCrasherCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public bool Validate3DText(BasePlayer player, string text, float x, float y, float z, float drawDistance, int color)
    {
        if (!_config.Enabled || !_config.GetCheck("Text3DCrasher").Enabled) return true;

        var st = _players.Get(player.Id);
        if (st is null) return true;

        if (text.Length > MaxTextLength)
        {
            _warnings.AddWarning(player.Id, "Text3DCrasher",
                $"text too long len={text.Length}");
            return false;
        }

        if (drawDistance > MaxDrawDistance)
        {
            _warnings.AddWarning(player.Id, "Text3DCrasher",
                $"excessive draw distance={drawDistance:F1}");
            return false;
        }

        if (text.Contains('\0'))
        {
            _warnings.AddWarning(player.Id, "Text3DCrasher",
                $"null byte detected");
            return false;
        }

        int count = _player3DTextCounts.GetOrAdd(player.Id, 0);
        if (count >= Max3DTexts)
        {
            _warnings.AddWarning(player.Id, "Text3DCrasher",
                $"3D text limit={count}");
            return false;
        }

        long now = Environment.TickCount64;
        st.Text3DCreateHistory.Enqueue(now);
        while (st.Text3DCreateHistory.Count > 0 &&
               now - st.Text3DCreateHistory.Peek() > Text3DSpamWindowMs)
        {
            st.Text3DCreateHistory.Dequeue();
        }

        if (st.Text3DCreateHistory.Count > Max3DTextsPerSec)
        {
            _warnings.AddWarning(player.Id, "Text3DCrasher",
                $"spam count={st.Text3DCreateHistory.Count}");
            return false;
        }

        return true;
    }

    public void On3DTextCreated(int playerId)
    {
        _player3DTextCounts.AddOrUpdate(playerId, 1, (_, v) => v + 1);
    }

    public void On3DTextDestroyed(int playerId)
    {
        _player3DTextCounts.AddOrUpdate(playerId, 0, (_, v) => Math.Max(0, v - 1));
    }

    public void OnPlayerDisconnected(int playerId)
    {
        _player3DTextCounts.TryRemove(playerId, out _);
    }
}