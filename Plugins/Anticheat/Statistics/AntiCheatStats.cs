using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Plugins.Anticheat.Statistics;

public class AntiCheatStats
{
    // Global statistics
    public ConcurrentDictionary<string, int> DetectionCounts { get; } = new();
    public ConcurrentDictionary<string, int> KickCounts { get; } = new();
    public ConcurrentDictionary<string, int> BanCounts { get; } = new();
    public ConcurrentDictionary<int, PlayerCheatHistory> PlayerHistory { get; } = new();

    public DateTime StartTime { get; }
    public int TotalDetections { get; private set; }
    public int TotalKicks { get; private set; }
    public int TotalBans { get; private set; }
    public int TotalPlayersChecked { get; private set; }

    public AntiCheatStats()
    {
        StartTime = DateTime.Now;
    }

    public void RecordDetection(int playerId, string checkName, string details)
    {
        DetectionCounts.AddOrUpdate(checkName, 1, (_, v) => v + 1);
        TotalDetections++;

        var history = PlayerHistory.GetOrAdd(playerId, _ => new PlayerCheatHistory(playerId));
        history.RecordDetection(checkName, details);
    }

    public void RecordKick(int playerId, string checkName)
    {
        KickCounts.AddOrUpdate(checkName, 1, (_, v) => v + 1);
        TotalKicks++;

        var history = PlayerHistory.GetOrAdd(playerId, _ => new PlayerCheatHistory(playerId));
        history.RecordKick(checkName);
    }

    public void RecordBan(int playerId, string checkName)
    {
        BanCounts.AddOrUpdate(checkName, 1, (_, v) => v + 1);
        TotalBans++;

        var history = PlayerHistory.GetOrAdd(playerId, _ => new PlayerCheatHistory(playerId));
        history.RecordBan(checkName);
    }

    public void RecordPlayerChecked(int playerId)
    {
        if (PlayerHistory.TryAdd(playerId, new PlayerCheatHistory(playerId)))
            TotalPlayersChecked++;
    }

    public PlayerCheatHistory? GetPlayerHistory(int playerId)
        => PlayerHistory.TryGetValue(playerId, out var history) ? history : null;

    public Dictionary<string, int> GetTopDetections(int limit = 10)
        => DetectionCounts.OrderByDescending(x => x.Value).Take(limit).ToDictionary(x => x.Key, x => x.Value);

    public Dictionary<string, int> GetTopKicks(int limit = 10)
        => KickCounts.OrderByDescending(x => x.Value).Take(limit).ToDictionary(x => x.Key, x => x.Value);

    public List<PlayerCheatHistory> GetTopOffenders(int limit = 10)
        => PlayerHistory.Values.OrderByDescending(x => x.TotalDetections).Take(limit).ToList();

    public string GenerateReport()
    {
        var uptime = DateTime.Now - StartTime;
        var report = new System.Text.StringBuilder();

        report.AppendLine("═══════════════════════════════════════════════════");
        report.AppendLine("           ANTICHEAT STATISTICS REPORT");
        report.AppendLine("═══════════════════════════════════════════════════");
        report.AppendLine($"Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
        report.AppendLine($"Total Players Checked: {TotalPlayersChecked}");
        report.AppendLine($"Total Detections: {TotalDetections}");
        report.AppendLine($"Total Kicks: {TotalKicks}");
        report.AppendLine($"Total Bans: {TotalBans}");
        report.AppendLine();

        report.AppendLine("Top 10 Detected Cheats:");
        report.AppendLine("───────────────────────────────────────────────────");
        foreach (var (check, count) in GetTopDetections(10))
            report.AppendLine($"  {check,-30} {count,5}x");
        report.AppendLine();

        report.AppendLine("Top 10 Kick Reasons:");
        report.AppendLine("───────────────────────────────────────────────────");
        foreach (var (check, count) in GetTopKicks(10))
            report.AppendLine($"  {check,-30} {count,5}x");
        report.AppendLine();

        report.AppendLine("Top 10 Offenders:");
        report.AppendLine("───────────────────────────────────────────────────");
        foreach (var player in GetTopOffenders(10))
            report.AppendLine($"  Player #{player.PlayerId,-5} Detections: {player.TotalDetections,4}  Kicks: {player.TotalKicks,3}  Bans: {player.TotalBans,2}");

        report.AppendLine("═══════════════════════════════════════════════════");
        return report.ToString();
    }

    public void Reset()
    {
        DetectionCounts.Clear();
        KickCounts.Clear();
        BanCounts.Clear();
        PlayerHistory.Clear();
        TotalDetections = 0;
        TotalKicks = 0;
        TotalBans = 0;
        TotalPlayersChecked = 0;
    }
}