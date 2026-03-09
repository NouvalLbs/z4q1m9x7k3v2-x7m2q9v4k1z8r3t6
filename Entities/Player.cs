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
        // ── Character Identity ────────────────────────────────────────────────
        public string CitizenId { get; set; } = "";
        public string Ucp { get; set; } = "";
        public string RegDate { get; set; } = "";
        public string LastLogin { get; set; } = "";
        public bool IsCharLoaded { get; set; }

        // ── Character Info ────────────────────────────────────────────────────
        public string Username { get; set; } = "";
        public int Gender { get; set; }
        public string BirthDate { get; set; } = "";
        public int Height { get; set; } = 150;
        public string Hair { get; set; } = "";
        public string Eye { get; set; } = "";
        public int VerifiedChar { get; set; }

        // ── Stats ─────────────────────────────────────────────────────────────
        public int Level { get; set; } = 1;
        public int LevelPoints { get; set; }
        public int LevelPointsExp { get; set; }
        public int CharMoney { get; set; }
        public int Admin { get; set; }
        public int MaskId { get; set; }
        public int Warn { get; set; }
        public int Paycheck { get; set; }

        // ── JSON Data ─────────────────────────────────────────────────────────
        public CharPosition CharSpawnPos { get; set; } = new();
        public CharVitals Vitals { get; set; } = new();
        public CharPlaytime Playtime { get; set; } = new();
        public CharBackpack Backpack { get; set; } = new();
        public CharPhone Phone { get; set; } = new();
        public CharJailInfo JailInfo { get; set; } = new();
        public CharBanInfo BanInfo { get; set; } = new();

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