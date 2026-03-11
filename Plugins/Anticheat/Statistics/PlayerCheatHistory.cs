using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Plugins.Anticheat.Statistics;

public class PlayerCheatHistory
{
    public int PlayerId { get; }
    public DateTime FirstSeen { get; }
    public DateTime LastSeen { get; private set; }
    public int TotalDetections { get; private set; }
    public int TotalKicks { get; private set; }
    public int TotalBans { get; private set; }

    public Dictionary<string, int> DetectionsByCheck { get; } = new();
    public List<CheatRecord> Records { get; } = new();

    public PlayerCheatHistory(int playerId)
    {
        PlayerId = playerId;
        FirstSeen = DateTime.Now;
        LastSeen = DateTime.Now;
    }

    public void RecordDetection(string checkName, string details)
    {
        TotalDetections++;
        LastSeen = DateTime.Now;

        if (!DetectionsByCheck.ContainsKey(checkName))
            DetectionsByCheck[checkName] = 0;
        DetectionsByCheck[checkName]++;

        Records.Add(new CheatRecord
        {
            Timestamp = DateTime.Now,
            CheckName = checkName,
            Details = details,
            Type = CheatRecordType.Detection
        });

        // Keep only last 100 records
        if (Records.Count > 100)
            Records.RemoveAt(0);
    }

    public void RecordKick(string checkName)
    {
        TotalKicks++;
        LastSeen = DateTime.Now;

        Records.Add(new CheatRecord
        {
            Timestamp = DateTime.Now,
            CheckName = checkName,
            Type = CheatRecordType.Kick
        });
    }

    public void RecordBan(string checkName)
    {
        TotalBans++;
        LastSeen = DateTime.Now;

        Records.Add(new CheatRecord
        {
            Timestamp = DateTime.Now,
            CheckName = checkName,
            Type = CheatRecordType.Ban
        });
    }

    public string GenerateReport()
    {
        var report = new System.Text.StringBuilder();

        report.AppendLine($"Player #{PlayerId} Cheat History");
        report.AppendLine("═══════════════════════════════════════════════════");
        report.AppendLine($"First Seen: {FirstSeen}");
        report.AppendLine($"Last Seen: {LastSeen}");
        report.AppendLine($"Total Detections: {TotalDetections}");
        report.AppendLine($"Total Kicks: {TotalKicks}");
        report.AppendLine($"Total Bans: {TotalBans}");
        report.AppendLine();

        if (DetectionsByCheck.Count > 0)
        {
            report.AppendLine("Detections by Check:");
            foreach (var (check, count) in DetectionsByCheck.OrderByDescending(x => x.Value))
                report.AppendLine($"  {check,-30} {count,3}x");
            report.AppendLine();
        }

        report.AppendLine($"Recent Activity (Last {Math.Min(10, Records.Count)} records):");
        foreach (var record in Records.TakeLast(10))
        {
            report.AppendLine($"  [{record.Timestamp:HH:mm:ss}] {record.Type} - {record.CheckName}");
            if (!string.IsNullOrEmpty(record.Details))
                report.AppendLine($"    └─ {record.Details}");
        }

        return report.ToString();
    }
}

public class CheatRecord
{
    public DateTime Timestamp { get; set; }
    public string CheckName { get; set; } = "";
    public string Details { get; set; } = "";
    public CheatRecordType Type { get; set; }
}

public enum CheatRecordType
{
    Detection,
    Kick,
    Ban
}