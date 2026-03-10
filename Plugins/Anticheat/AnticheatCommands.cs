using ProjectSMP.Plugins.Anticheat.Managers;
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
                _ac.Config.MaxConnectsPerIp = fresh.MaxConnectsPerIp;
                _ac.Config.MinReconnectSeconds = fresh.MinReconnectSeconds;
                foreach (var (k, v) in fresh.Checks) _ac.Config.Checks[k] = v;
                caller.SendClientMessage(-1, "[AC] Config reloaded.");
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
}