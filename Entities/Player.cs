using ProjectSMP.Entities.Players.Account;
using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Feature.CinematicCamera;
using ProjectSMP.Features.PreviewModelDialog;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP {
    [PooledType]
    public partial class Player : BasePlayer {
        public override void OnConnected(EventArgs e) {
            base.OnConnected(e);
            CinematicCameraService.Start(this);
            UserControlService.InitAsync(this);
        }

        public override void OnDisconnected(DisconnectEventArgs e) {
            CinematicCameraService.Stop(this);
            _ = CharacterService.SaveAsync(this);
            UserControlService.Cleanup(this);
            CharacterService.Cleanup(this);
            base.OnDisconnected(e);
        }
        public override void OnSpawned(SpawnEventArgs e)
        {
            base.OnSpawned(e);
            CharacterService.HandleSpawn(this);
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