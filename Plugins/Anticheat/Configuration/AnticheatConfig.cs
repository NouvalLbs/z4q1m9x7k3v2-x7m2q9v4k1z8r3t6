using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Configuration;

public class AnticheatConfig
{
    public bool Enabled { get; set; } = true;
    public string LogPath { get; set; } = "logs/anticheat.log";
    public int MaxPing { get; set; } = 500;
    public int MaxConnectsPerIp { get; set; } = 1;
    public int MinReconnectSeconds { get; set; } = 12;
    public int SpeedHackVehResetDelay { get; set; } = 3;
    public Dictionary<string, CheckConfig> Checks { get; set; } = new();

    public CheckConfig GetCheck(string name) =>
        Checks.TryGetValue(name, out var c) ? c : new CheckConfig();
}