using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat;

public class AnticheatCommands
{
    private readonly AnticheatPlugin _ac;
    private readonly HashSet<int> _adminIds;

    public AnticheatCommands(AnticheatPlugin ac, HashSet<int> adminIds)
    {
        _ac = ac;
        _adminIds = adminIds;
    }

    public void HandleCommand(BasePlayer caller, string cmd, string[] args)
    {
        if (!_adminIds.Contains(caller.Id))
        {
            caller.SendClientMessage(-1, "No permission.");
            return;
        }

        switch (cmd)
        {
            case "/accheck":
                if (!TryGetTarget(caller, args, out var target)) return;
                var st = _ac.Players.Get(target.Id);
                if (st is null) { caller.SendClientMessage(-1, "Player state not found."); return; }
                var lines = new System.Text.StringBuilder($"[AC] P:{target.Id} warnings: ");
                foreach (var (k, v) in st.WarningCounts) lines.Append($"{k}={v} ");
                caller.SendClientMessage(-1, lines.ToString());
                break;

            case "/acreset":
                if (!TryGetTarget(caller, args, out target)) return;
                _ac.Warnings.Reset(target.Id);
                caller.SendClientMessage(-1, $"[AC] Warnings reset for P:{target.Id}");
                break;

            case "/acexempt":
                if (args.Length < 2) { caller.SendClientMessage(-1, "Usage: /acexempt [id] [check]"); return; }
                if (!TryGetTarget(caller, args, out target)) return;
                _ac.Warnings.DisableCheckForPlayer(target.Id, args[1]);
                caller.SendClientMessage(-1, $"[AC] {args[1]} exempted for P:{target.Id}");
                break;

            case "/acunexempt":
                if (args.Length < 2) { caller.SendClientMessage(-1, "Usage: /acunexempt [id] [check]"); return; }
                if (!TryGetTarget(caller, args, out target)) return;
                _ac.Warnings.EnableCheckForPlayer(target.Id, args[1]);
                caller.SendClientMessage(-1, $"[AC] {args[1]} re-enabled for P:{target.Id}");
                break;

            case "/acreload":
                var fresh = AnticheatPlugin.LoadConfig();
                _ac.Config.Enabled = fresh.Enabled;
                _ac.Config.MaxPing = fresh.MaxPing;
                _ac.Config.LogPath = fresh.LogPath;
                _ac.Config.SpeedHackVehResetDelay = fresh.SpeedHackVehResetDelay;
                _ac.Config.MaxConnectsPerIp = fresh.MaxConnectsPerIp;
                _ac.Config.MinReconnectSeconds = fresh.MinReconnectSeconds;
                foreach (var (k, v) in fresh.Checks) _ac.Config.Checks[k] = v;
                caller.SendClientMessage(-1, "[AC] Config reloaded.");
                break;
            case "/acstats":
                string report = _ac.GenerateStatsReport();
                foreach (var line in report.Split('\n'))
                    caller.SendClientMessage(-1, line);
                break;

            case "/achistory":
                if (!TryGetTarget(caller, args, out target)) return;
                var history = _ac.GetPlayerHistory(target.Id);
                if (history is null) { caller.SendClientMessage(-1, "No history found."); return; }
                string histReport = history.GenerateReport();
                foreach (var line in histReport.Split('\n'))
                    caller.SendClientMessage(-1, line);
                break;

            case "/acresetstats":
                _ac.ResetStatistics();
                caller.SendClientMessage(-1, "[AC] Statistics reset.");
                break;
            case "/acblacklist":
                if (args.Length < 3)
                {
                    caller.SendClientMessage(-1, "Usage: /acblacklist <add|remove|list> <type> [value]");
                    caller.SendClientMessage(-1, "Types: weapon, skin, vehiclemod, vehicle, action");
                    return;
                }
                HandleBlacklistCommand(caller, args);
                break;
        }
    }

    private bool TryGetTarget(BasePlayer caller, string[] args, out BasePlayer target)
    {
        target = null!;
        if (args.Length < 1 || !int.TryParse(args[0], out int id))
        {
            caller.SendClientMessage(-1, "Invalid player ID.");
            return false;
        }
        target = BasePlayer.Find(id)!;
        if (target is null) { caller.SendClientMessage(-1, "Player not connected."); return false; }
        return true;
    }

    private void HandleBlacklistCommand(BasePlayer caller, string[] args)
    {
        string action = args[0].ToLower();
        string type = args[1].ToLower();

        if (action == "list")
        {
            ShowBlacklist(caller, type);
            return;
        }

        if (args.Length < 3)
        {
            caller.SendClientMessage(-1, "Usage: /acblacklist <add|remove> <type> <value>");
            return;
        }

        if (!int.TryParse(args[2], out int value))
        {
            caller.SendClientMessage(-1, "Invalid value. Must be a number.");
            return;
        }

        switch (type)
        {
            case "weapon":
                if (action == "add")
                {
                    _ac.AddBlacklistedWeapon(value);
                    caller.SendClientMessage(-1, $"[AC] Weapon {value} added to blacklist");
                }
                else if (action == "remove")
                {
                    _ac.RemoveBlacklistedWeapon(value);
                    caller.SendClientMessage(-1, $"[AC] Weapon {value} removed from blacklist");
                }
                break;

            case "skin":
                if (action == "add")
                {
                    _ac.AddBlacklistedSkin(value);
                    caller.SendClientMessage(-1, $"[AC] Skin {value} added to blacklist");
                }
                else if (action == "remove")
                {
                    _ac.RemoveBlacklistedSkin(value);
                    caller.SendClientMessage(-1, $"[AC] Skin {value} removed from blacklist");
                }
                break;

            case "vehiclemod":
                if (action == "add")
                {
                    _ac.AddBlacklistedVehicleMod(value);
                    caller.SendClientMessage(-1, $"[AC] Vehicle mod {value} added to blacklist");
                }
                else if (action == "remove")
                {
                    _ac.RemoveBlacklistedVehicleMod(value);
                    caller.SendClientMessage(-1, $"[AC] Vehicle mod {value} removed from blacklist");
                }
                break;

            case "vehicle":
                if (action == "add")
                {
                    _ac.AddBlacklistedVehicle(value);
                    caller.SendClientMessage(-1, $"[AC] Vehicle model {value} added to blacklist");
                }
                else if (action == "remove")
                {
                    _ac.RemoveBlacklistedVehicle(value);
                    caller.SendClientMessage(-1, $"[AC] Vehicle model {value} removed from blacklist");
                }
                break;

            case "action":
                if (action == "add")
                {
                    _ac.AddBlacklistedSpecialAction(value);
                    caller.SendClientMessage(-1, $"[AC] Special action {value} added to blacklist");
                }
                else if (action == "remove")
                {
                    _ac.RemoveBlacklistedSpecialAction(value);
                    caller.SendClientMessage(-1, $"[AC] Special action {value} removed from blacklist");
                }
                break;

            default:
                caller.SendClientMessage(-1, "Unknown type. Use: weapon, skin, vehiclemod, vehicle, action");
                break;
        }
    }

    private void ShowBlacklist(BasePlayer caller, string type)
    {
        var config = _ac.Config;

        switch (type)
        {
            case "weapon":
                if (config.BlacklistedWeapons.Count == 0)
                    caller.SendClientMessage(-1, "[AC] No blacklisted weapons");
                else
                    caller.SendClientMessage(-1, $"[AC] Blacklisted weapons: {string.Join(", ", config.BlacklistedWeapons)}");
                break;

            case "skin":
                if (config.BlacklistedSkins.Count == 0)
                    caller.SendClientMessage(-1, "[AC] No blacklisted skins");
                else
                    caller.SendClientMessage(-1, $"[AC] Blacklisted skins: {string.Join(", ", config.BlacklistedSkins)}");
                break;

            case "vehiclemod":
                if (config.BlacklistedVehicleMods.Count == 0)
                    caller.SendClientMessage(-1, "[AC] No blacklisted vehicle mods");
                else
                    caller.SendClientMessage(-1, $"[AC] Blacklisted mods: {string.Join(", ", config.BlacklistedVehicleMods)}");
                break;

            case "vehicle":
                if (config.BlacklistedVehicles.Count == 0)
                    caller.SendClientMessage(-1, "[AC] No blacklisted vehicles");
                else
                    caller.SendClientMessage(-1, $"[AC] Blacklisted vehicles: {string.Join(", ", config.BlacklistedVehicles)}");
                break;

            case "action":
                if (config.BlacklistedSpecialActions.Count == 0)
                    caller.SendClientMessage(-1, "[AC] No blacklisted special actions");
                else
                    caller.SendClientMessage(-1, $"[AC] Blacklisted actions: {string.Join(", ", config.BlacklistedSpecialActions)}");
                break;

            default:
                caller.SendClientMessage(-1, "Unknown type. Use: weapon, skin, vehiclemod, vehicle, action");
                break;
        }
    }
}