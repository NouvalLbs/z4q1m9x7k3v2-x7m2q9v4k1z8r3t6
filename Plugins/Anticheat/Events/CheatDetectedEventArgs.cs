using ProjectSMP.Plugins.Anticheat.Configuration;
using System;

namespace ProjectSMP.Plugins.Anticheat.Events;

public class CheatDetectedEventArgs : EventArgs
{
    public int PlayerId { get; }
    public string CheckName { get; }
    public int WarningCount { get; }
    public string Details { get; }
    public PunishAction SuggestedAction { get; }
    public bool Cancel { get; set; } // Allow canceling punishment
    public PunishAction? OverrideAction { get; set; } // Override suggested action

    public CheatDetectedEventArgs(int playerId, string checkName, int warningCount, string details, PunishAction suggestedAction)
    {
        PlayerId = playerId;
        CheckName = checkName;
        WarningCount = warningCount;
        Details = details;
        SuggestedAction = suggestedAction;
        Cancel = false;
        OverrideAction = null;
    }
}