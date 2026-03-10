using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.IO;
using System.Text.Json;

namespace ProjectSMP.Plugins.Anticheat;

public class AnticheatPlugin
{
    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly FloodRateLimiter _flood;
    private readonly AcLogger _logger;
    private readonly AnticheatConfig _config;

    public PlayerStateManager Players => _players;
    public VehicleStateManager Vehicles => _vehicles;
    public WarningManager Warnings => _warnings;
    public AnticheatConfig Config => _config;

    public AnticheatPlugin(
        PlayerStateManager players,
        VehicleStateManager vehicles,
        WarningManager warnings,
        FloodRateLimiter flood,
        AcLogger logger,
        AnticheatConfig config)
    {
        _players = players;
        _vehicles = vehicles;
        _warnings = warnings;
        _flood = flood;
        _logger = logger;
        _config = config;
    }

    public static AnticheatPlugin Create(string configPath = "anticheat.json")
    {
        var config = LoadConfig(configPath);
        var players = new PlayerStateManager();
        var vehicles = new VehicleStateManager();
        var logger = new AcLogger(config);
        var flood = new FloodRateLimiter();
        var warnings = new WarningManager(players, config, logger);
        return new AnticheatPlugin(players, vehicles, warnings, flood, logger, config);
    }

    public static AnticheatConfig LoadConfig(string path = "anticheat.json")
    {
        if (!File.Exists(path))
        {
            var def = new AnticheatConfig();
            File.WriteAllText(path, JsonSerializer.Serialize(def,
                new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        return JsonSerializer.Deserialize<AnticheatConfig>(File.ReadAllText(path))
               ?? new AnticheatConfig();
    }

    public void RegisterEvents(BaseMode gameMode)
    {
        gameMode.PlayerConnected += OnPlayerConnected;
        gameMode.PlayerDisconnected += OnPlayerDisconnected;
        _warnings.PunishmentRequired += OnPunishment;
    }

    // SampSharp: PlayerConnected sender IS the BasePlayer
    private void OnPlayerConnected(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer player) return;
        var state = _players.GetOrCreate(player.Id);
        state.IsOnline = true;
        state.IpAddress = player.IP;
        _logger.Log($"Player {player.Id} connected from {player.IP}");
    }

    // SampSharp: PlayerDisconnected uses DisconnectEventArgs
    private void OnPlayerDisconnected(object? sender, DisconnectEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        _flood.ClearPlayer(player.Id);
        _players.Remove(player.Id);
    }

    private void OnPunishment(int playerId, string checkName, PunishAction action)
    {
        var player = BasePlayer.Find(playerId);
        if (player is null) return;

        switch (action)
        {
            case PunishAction.Kick:
                _logger.LogKick(playerId, checkName);
                player.Kick();
                break;
            case PunishAction.Ban:
                _logger.LogBan(playerId, checkName);
                player.Ban();
                break;
        }
    }
}