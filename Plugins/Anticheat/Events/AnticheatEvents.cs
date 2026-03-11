#nullable enable
using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using System;

namespace ProjectSMP.Plugins.Anticheat.Events;

public class AnticheatEvents
{
    // Main detection event - fired BEFORE punishment
    public event EventHandler<CheatDetectedEventArgs>? CheatDetected;

    // Punishment events - fired AFTER punishment decision
    public event Action<int, string>? PlayerWarned;
    public event Action<int, string>? PlayerKicked;
    public event Action<int, string>? PlayerBanned;

    // Global toggle event
    public event Action<bool>? AnticheatToggled;

    // Check-specific toggle events
    public event Action<string, bool>? CheckToggled;

    // Config reload event
    public event Action? ConfigReloaded;

    internal void Wire(WarningManager wm)
    {
        wm.CheatDetected += OnCheatDetectedInternal;
        wm.PunishmentRequired += (pid, check, action) =>
        {
            switch (action)
            {
                case PunishAction.Warn:
                    PlayerWarned?.Invoke(pid, check);
                    break;
                case PunishAction.Kick:
                    PlayerKicked?.Invoke(pid, check);
                    break;
                case PunishAction.Ban:
                    PlayerBanned?.Invoke(pid, check);
                    break;
            }
        };
    }

    private void OnCheatDetectedInternal(object? sender, CheatDetectedEventArgs e)
    {
        // Allow external handlers to modify or cancel
        CheatDetected?.Invoke(sender, e);
    }

    internal void RaiseAnticheatToggled(bool enabled)
        => AnticheatToggled?.Invoke(enabled);

    internal void RaiseCheckToggled(string checkName, bool enabled)
        => CheckToggled?.Invoke(checkName, enabled);

    internal void RaiseConfigReloaded()
        => ConfigReloaded?.Invoke();
}