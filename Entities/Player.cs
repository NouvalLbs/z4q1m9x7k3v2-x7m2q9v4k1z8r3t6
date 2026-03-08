using ProjectSMP.Entities.Players.Account;
using ProjectSMP.Feature.CinematicCamera;
using ProjectSMP.Features.PreviewModelDialog;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP {
    [PooledType]
    public class Player : BasePlayer {
        public override void OnConnected(EventArgs e) {
            base.OnConnected(e);
            CinematicCameraService.Start(this);
            UserControlService.InitAsync(this);
        }

        public override void OnDisconnected(DisconnectEventArgs e) {
            CinematicCameraService.Stop(this);
            UserControlService.Cleanup(this);
            base.OnDisconnected(e);
        }

        public override void OnClickTextDraw(ClickTextDrawEventArgs e) {
            base.OnClickTextDraw(e);
            PreviewModelDialog.HandleClick(this, e.TextDraw);
        }

        public override void OnClickPlayerTextDraw(ClickPlayerTextDrawEventArgs e) {
            base.OnClickPlayerTextDraw(e);
            PreviewModelDialog.HandlePlayerTextDrawClick(this, e.PlayerTextDraw);
        }


        public override void OnCancelClickTextDraw(PlayerEventArgs e) {
            PreviewModelDialog.HandleCancel(this);
            base.OnCancelClickTextDraw(e);
        }
    }
}