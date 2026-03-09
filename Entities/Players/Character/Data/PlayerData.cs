using ProjectSMP.Entities.Players.Character;

namespace ProjectSMP
{
    public partial class Player
    {
        // ── Character Identity ────────────────────────────────────────────
        public string CitizenId { get; set; } = "";
        public string Ucp { get; set; } = "";
        public string RegDate { get; set; } = "";
        public string LastLogin { get; set; } = "";
        public bool IsCharLoaded { get; set; }
        public long LoadTick { get; set; }

        // ── Character Info ────────────────────────────────────────────────
        public string Username { get; set; } = "";
        public int Gender { get; set; }
        public string BirthDate { get; set; } = "";
        public int Height { get; set; } = 150;
        public string Hair { get; set; } = "";
        public string Eye { get; set; } = "";
        public int VerifiedChar { get; set; }

        // ── Stats ─────────────────────────────────────────────────────────
        public int Level { get; set; } = 1;
        public int LevelPoints { get; set; }
        public int LevelPointsExp { get; set; }
        public int CharMoney { get; set; }
        public int Admin { get; set; }
        public int MaskId { get; set; }
        public int Warn { get; set; }
        public int Paycheck { get; set; }

        // ── JSON Data ─────────────────────────────────────────────────────
        public CharPosition CharSpawnPos { get; set; } = new();
        public CharVitals Vitals { get; set; } = new();
        public CharPlaytime Playtime { get; set; } = new();
        public CharBackpack Backpack { get; set; } = new();
        public CharPhone Phone { get; set; } = new();
        public CharJailInfo JailInfo { get; set; } = new();
        public CharBanInfo BanInfo { get; set; } = new();
    }
}