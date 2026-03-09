using ProjectSMP.Entities.Players.Account;
using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Feature.CinematicCamera;
using ProjectSMP.Features.Bank;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP {
    [PooledType]
    public partial class Player : BasePlayer {
        public override void OnConnected(EventArgs e) {
            base.OnConnected(e);
            WeaponConfigService.OnConnect(this);
            WeaponConfigService.PlayerDeathFinished += OnDeathFinished;
            CinematicCameraService.Start(this);
            UserControlService.InitAsync(this);
        }

        public override void OnDisconnected(DisconnectEventArgs e) {
            WeaponConfigService.PlayerDeathFinished -= OnDeathFinished;
            CinematicCameraService.Stop(this);
            _ = CharacterService.SaveAsync(this);
            UserControlService.Cleanup(this);
            CharacterService.Cleanup(this);
            WeaponConfigService.OnDisconnect(this);
            base.OnDisconnected(e);
        }

        public override void OnSpawned(SpawnEventArgs e) {
            base.OnSpawned(e);
            WeaponConfigService.OnSpawn(this);
            CharacterService.HandleSpawn(this);
            _ = BankService.LoadAsync(this);
        }

        public override void OnDeath(DeathEventArgs e) {
            base.OnDeath(e);
            if (IsCharLoaded) {
                var p = Position;
                CharSpawnPos = new CharPosition { X = p.X, Y = p.Y, Z = p.Z, A = Angle, Interior = Interior, World = VirtualWorld };
            }
            WeaponConfigService.OnDeath(this, e.Killer as Player, (int)e.DeathReason);
        }

        public override void OnRequestClass(RequestClassEventArgs e) {
            base.OnRequestClass(e);
            if (!IsCharLoaded) return;

            SetSpawnInfo(0, CharSkin, new SampSharp.GameMode.Vector3(CharSpawnPos.X, CharSpawnPos.Y, CharSpawnPos.Z), CharSpawnPos.A);
            Spawn();
        }

        private void OnDeathFinished(object? sender, DeathFinishedArgs e) {
            if (e.Player != this || IsDisposed) return;
            CharacterService.RespawnCharacter(this);
        }

        public override void OnGiveDamage(DamageEventArgs e) {
            WeaponConfigService.HandleGiveDamage(this, e.OtherPlayer as Player, e.Amount, (int)e.Weapon);
            base.OnGiveDamage(e);
        }

        public override void OnTakeDamage(DamageEventArgs e) {
            WeaponConfigService.HandleTakeDamage(this, e.OtherPlayer as Player, e.Amount, (int)e.Weapon);
            base.OnTakeDamage(e);
        }

        public override void OnWeaponShot(WeaponShotEventArgs e) {
            WeaponConfigService.HandleWeaponShot(this, (int)e.Weapon);
            base.OnWeaponShot(e);
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