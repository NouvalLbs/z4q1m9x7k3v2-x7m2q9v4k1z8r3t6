using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class MenuCrasherCheck
{
    private const int MaxMenuResponses = 20;
    private const long MenuResponseWindowMs = 1000;
    private const int MaxMenus = 128;
    private const int MaxMenuRows = 12;
    private const int MaxMenuColumns = 2;
    private const long MinMenuResponseTime = 50;
    private const long MaxMenuOpenTime = 300000;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly ConcurrentDictionary<int, MenuInfo> _menus = new();

    public MenuCrasherCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnMenuCreated(int menuId, int rows, int columns)
    {
        _menus.TryAdd(menuId, new MenuInfo
        {
            Rows = rows,
            Columns = columns,
            CreatedTick = Environment.TickCount64
        });
    }

    public void OnMenuDestroyed(int menuId)
    {
        _menus.TryRemove(menuId, out _);
    }

    public void OnPlayerShowMenu(int playerId, int menuId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.CurrentMenuId = menuId;
        st.MenuShownTick = Environment.TickCount64;
    }

    public void OnPlayerExitMenu(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.CurrentMenuId = -1;
        st.MenuShownTick = 0;
    }

    public bool OnPlayerMenuResponse(BasePlayer player, MenuRowEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("MenuCrasher").Enabled) return true;

        var st = _players.Get(player.Id);
        if (st is null) return true;

        long now = Environment.TickCount64;

        if (st.CurrentMenuId == -1)
        {
            _warnings.AddWarning(player.Id, "MenuCrasher",
                $"response without menu shown");
            return false;
        }

        long responseTime = now - st.MenuShownTick;
        if (responseTime < MinMenuResponseTime)
        {
            _warnings.AddWarning(player.Id, "MenuCrasher",
                $"instant response time={responseTime}ms");
            return false;
        }

        if (responseTime > MaxMenuOpenTime)
        {
            _warnings.AddWarning(player.Id, "MenuCrasher",
                $"menu timeout time={responseTime}ms");
            return false;
        }

        if (!_menus.TryGetValue(st.CurrentMenuId, out var menuInfo))
        {
            _warnings.AddWarning(player.Id, "MenuCrasher",
                $"invalid menu id={st.CurrentMenuId}");
            return false;
        }

        int row = e.Row;
        if (row < 0 || row >= menuInfo.Rows)
        {
            _warnings.AddWarning(player.Id, "MenuCrasher",
                $"invalid row={row} max={menuInfo.Rows}");
            return false;
        }

        st.MenuResponseHistory.Enqueue(now);
        while (st.MenuResponseHistory.Count > 0 &&
               now - st.MenuResponseHistory.Peek() > MenuResponseWindowMs)
        {
            st.MenuResponseHistory.Dequeue();
        }

        if (st.MenuResponseHistory.Count > MaxMenuResponses)
        {
            _warnings.AddWarning(player.Id, "MenuCrasher",
                $"spam count={st.MenuResponseHistory.Count}");
            return false;
        }

        st.CurrentMenuId = -1;
        st.MenuShownTick = 0;

        return true;
    }

    public bool ValidateMenuCreate(int rows, int columns)
    {
        if (!_config.Enabled || !_config.GetCheck("MenuCrasher").Enabled) return true;

        if (_menus.Count >= MaxMenus)
        {
            return false;
        }

        if (rows < 0 || rows > MaxMenuRows)
        {
            return false;
        }

        if (columns < 0 || columns > MaxMenuColumns)
        {
            return false;
        }

        return true;
    }

    private class MenuInfo
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public long CreatedTick { get; set; }
    }
}