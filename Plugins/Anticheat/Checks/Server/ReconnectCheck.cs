using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class ReconnectCheck
{
    private readonly ConcurrentDictionary<string, long> _lastDisconnect = new();
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public ReconnectCheck(WarningManager w, AnticheatConfig c, AcLogger l)
        => (_warnings, _config, _logger) = (w, c, l);

    public void OnPlayerConnected(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("Reconnect").Enabled) return;

        string ip = player.IP;
        long now = Environment.TickCount64;

        if (_lastDisconnect.TryGetValue(ip, out long lastTick))
        {
            long elapsed = now - lastTick;
            long minMs = _config.MinReconnectSeconds * 1000L;
            if (elapsed < minMs)
            {
                _logger.Log($"Reconnect block: {ip} elapsed={elapsed}ms");
                player.Kick();
                return;
            }
        }

        _lastDisconnect[ip] = 0;
    }

    public void OnPlayerDisconnected(BasePlayer player)
    {
        _lastDisconnect[player.IP] = Environment.TickCount64;
    }
}