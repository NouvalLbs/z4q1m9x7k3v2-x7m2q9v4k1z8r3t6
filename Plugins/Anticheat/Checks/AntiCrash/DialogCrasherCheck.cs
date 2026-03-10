using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class DialogCrasherCheck
{
    private const int MaxDialogId = 32767;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public DialogCrasherCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public bool OnDialogResponse(BasePlayer player, DialogResponseEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("DialogCrasher").Enabled) return true;

        if (e.DialogId < 0 || e.DialogId > MaxDialogId)
        {
            _warnings.AddWarning(player.Id, "DialogCrasher",
                $"dialogId={e.DialogId}");
            return false;
        }

        var st = _players.Get(player.Id);
        if (st is null) return true;

        if (st.NextDialog < 0)
        {
            _warnings.AddWarning(player.Id, "DialogCrasher", "no dialog shown");
            return false;
        }

        return true;
    }
}