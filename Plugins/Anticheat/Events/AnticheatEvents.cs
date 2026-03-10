#nullable enable
using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using System;

namespace ProjectSMP.Plugins.Anticheat.Events;

public class AnticheatEvents
{
    public event EventHandler<CheatDetectedEventArgs>? CheatDetected;
    public event Action<int, string>? PlayerKicked;
    public event Action<int, string>? PlayerBanned;

    internal void Wire(WarningManager wm)
    {
        wm.CheatDetected += (s, e) => CheatDetected?.Invoke(s, e);
        wm.PunishmentRequired += (pid, check, action) =>
        {
            if (action == PunishAction.Kick) PlayerKicked?.Invoke(pid, check);
            else if (action == PunishAction.Ban) PlayerBanned?.Invoke(pid, check);
        };
    }
}