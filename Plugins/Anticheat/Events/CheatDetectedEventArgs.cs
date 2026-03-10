using System;

namespace ProjectSMP.Plugins.Anticheat.Events;

public class CheatDetectedEventArgs : EventArgs
{
    public int PlayerId { get; }
    public string CheckName { get; }
    public int WarningCount { get; }
    public string Details { get; }

    public CheatDetectedEventArgs(int playerId, string checkName, int warningCount, string details = "")
    {
        PlayerId = playerId;
        CheckName = checkName;
        WarningCount = warningCount;
        Details = details;
    }
}