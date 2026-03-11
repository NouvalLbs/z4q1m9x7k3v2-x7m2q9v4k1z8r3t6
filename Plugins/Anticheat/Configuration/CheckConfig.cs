namespace ProjectSMP.Plugins.Anticheat.Configuration;

public enum PunishAction { Warn, Kick, Ban }

public class CheckConfig
{
    public bool Enabled { get; set; } = true;
    public int MaxWarnings { get; set; } = 3;
    public PunishAction Action { get; set; } = PunishAction.Kick;
    public int KickDelay { get; set; } = 0; // Delay in ms before kicking
    public bool AutoBan { get; set; } = false; // Auto-ban after kick
    public int AutoBanAfterKicks { get; set; } = 3; // Ban after X kicks
    public string CustomMessage { get; set; } = ""; // Custom kick/ban message
    public bool LogToFile { get; set; } = true; // Log this check
    public bool NotifyAdmins { get; set; } = false; // Notify online admins
}