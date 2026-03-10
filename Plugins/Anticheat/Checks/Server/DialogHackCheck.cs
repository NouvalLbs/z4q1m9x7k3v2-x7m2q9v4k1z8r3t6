using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class DialogHackCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public DialogHackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnDialogResponse(BasePlayer player, DialogResponseEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("DialogHack").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        int responded = e.DialogId;

        if (st.NextDialog >= 0 && responded != st.NextDialog)
        {
            _warnings.AddWarning(player.Id, "DialogHack",
                $"got={responded} expected={st.NextDialog}");
        }

        st.Dialog = -1;
        st.NextDialog = -1;
    }

    public void OnDialogShown(int playerId, int dialogId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NextDialog = dialogId;
    }
}