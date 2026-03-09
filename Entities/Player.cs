#nullable enable
using ProjectSMP.Entities.Players.Account;
using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Feature.CinematicCamera;
using ProjectSMP.Features.Bank;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.World;
using System;
using System.Threading.Tasks;

namespace ProjectSMP
{
    [PooledType]
    public partial class Player : BasePlayer
    {
        public override void OnConnected(EventArgs e)
        {
            base.OnConnected(e);
            WeaponConfigService.OnConnect(this);
            WeaponConfigService.PlayerDeathFinished += OnDeathFinished;
            CinematicCameraService.Start(this);
            UserControlService.InitAsync(this);
        }

        public override void OnDisconnected(DisconnectEventArgs e)
        {
            WeaponConfigService.PlayerDeathFinished -= OnDeathFinished;
            CinematicCameraService.Stop(this);
            _ = SaveOnDisconnectAsync();
            UserControlService.Cleanup(this);
            CharacterService.Cleanup(this);
            WeaponConfigService.OnDisconnect(this);
            base.OnDisconnected(e);
        }

        private async Task SaveOnDisconnectAsync()
        {
            try { await CharacterService.SaveAsync(this); }
            catch (Exception ex) { Console.WriteLine($"[Player] Save failed for {Name}: {ex.Message}"); }
        }

        public override void OnSpawned(SpawnEventArgs e)
        {
            base.OnSpawned(e);
            WeaponConfigService.OnSpawn(this);
            CharacterService.HandleSpawn(this);
            if (IsCharLoaded) _ = BankService.LoadAsync(this);
        }

        public override void OnDeath(DeathEventArgs e)
        {
            base.OnDeath(e);
            if (IsCharLoaded)
            {
                var p = Position;
                CharSpawnPos = new CharPosition
                {
                    X = p.X,
                    Y = p.Y,
                    Z = p.Z,
                    A = Angle,
                    Interior = Interior,
                    World = VirtualWorld
                };
            }
            WeaponConfigService.OnDeath(this, e.Killer as Player, (int)e.DeathReason);
        }

        public override void OnRequestClass(RequestClassEventArgs e)
        {
            base.OnRequestClass(e);
            WeaponConfigService.OnRequestClass(this);

            if (!IsCharLoaded || WeaponConfigService.IsPlayerInClassSelection(this)) return;

            SetSpawnInfo(0, CharSkin,
                new Vector3(CharSpawnPos.X, CharSpawnPos.Y, CharSpawnPos.Z),
                CharSpawnPos.A);
            Spawn();
        }

        public override void OnUpdate(PlayerUpdateEventArgs e)
        {
            base.OnUpdate(e);
            WeaponConfigService.OnUpdate(this);
        }

        private void OnDeathFinished(object? sender, DeathFinishedArgs e)
        {
            if (e.Player != this || IsDisposed) return;
            CharacterService.RespawnCharacter(this);
        }

        public override void OnGiveDamage(DamageEventArgs e)
        {
            if (e.OtherPlayer == null)
            {
                var vid = WeaponConfigService.GetLastShotVehicleId(this);
                if (vid >= 0)
                    WeaponConfigService.HandleVehicleDamage(this, vid, e.Amount, (int)e.Weapon);
            }
            else
            {
                WeaponConfigService.HandleGiveDamage(this, e.OtherPlayer as Player,
                    e.Amount, (int)e.Weapon, (int)e.BodyPart);
            }
            base.OnGiveDamage(e);
        }

        public override void OnTakeDamage(DamageEventArgs e)
        {
            WeaponConfigService.HandleTakeDamage(this, e.OtherPlayer as Player, e.Amount, (int)e.Weapon, (int)e.BodyPart);
            base.OnTakeDamage(e);
        }

        public override void OnWeaponShot(WeaponShotEventArgs e)
        {
            WeaponConfigService.HandleWeaponShot(
                this,
                (int)e.Weapon,
                (int)e.BulletHitType,
                e.HitId,
                Position,
                e.Position);

            if (WeaponConfigService.IsBulletWeapon((int)e.Weapon))
                e.PreventDamage = true;

            base.OnWeaponShot(e);
        }

        public override void OnClickTextDraw(ClickTextDrawEventArgs e)
        {
            base.OnClickTextDraw(e);
            PreviewModelDialog.HandleClick(this, e.TextDraw);
        }

        public override void OnClickPlayerTextDraw(ClickPlayerTextDrawEventArgs e)
        {
            base.OnClickPlayerTextDraw(e);
            PreviewModelDialog.HandlePlayerTextDrawClick(this, e.PlayerTextDraw);
        }

        public override void OnCancelClickTextDraw(PlayerEventArgs e)
        {
            PreviewModelDialog.HandleCancel(this);
            base.OnCancelClickTextDraw(e);
        }

        public override void OnEnterVehicle(EnterVehicleEventArgs e)
        {
            base.OnEnterVehicle(e);
        }

        public override void OnExitVehicle(PlayerVehicleEventArgs e)
        {
            base.OnExitVehicle(e);
        }
    }
}