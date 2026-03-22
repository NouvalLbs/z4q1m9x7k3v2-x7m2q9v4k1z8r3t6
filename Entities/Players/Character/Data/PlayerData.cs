using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Features.Bank;
using SampSharp.GameMode;
using System.Collections.Generic;

namespace ProjectSMP
{
    public partial class Player
    {
        public string CitizenId { get; set; } = "";
        public string Ucp { get; set; } = "";
        public string RegDate { get; set; } = "";
        public string LastLogin { get; set; } = "";
        public bool IsCharLoaded { get; set; }

        public string Username { get; set; } = "";
        public int CharSkin { get; set; }
        public int Gender { get; set; }
        public string BirthDate { get; set; } = "";
        public int Height { get; set; } = 150;
        public string Hair { get; set; } = "";
        public string Eye { get; set; } = "";
        public int VerifiedChar { get; set; }

        public int Level { get; set; } = 1;
        public int LevelPoints { get; set; }
        public int LevelPointsExp { get; set; }
        public int CharMoney { get; set; }
        public int Admin { get; set; }
        public int MaskId { get; set; }
        public int Warn { get; set; }
        public int Paycheck { get; set; }
        public bool Cuffed { get; set; }

        public CharPosition CharSpawnPos { get; set; } = new();
        public CharVitals Vitals { get; set; } = new();
        public CharPlaytime Playtime { get; set; } = new();
        public CharBackpack Backpack { get; set; } = new();
        public CharPhone Phone { get; set; } = new();
        public CharJailInfo JailInfo { get; set; } = new();
        public CharBanInfo BanInfo { get; set; } = new();
        public CharCondition Condition { get; set; } = new();
        public CharSettings Settings { get; set; } = new();
        public List<CharJob> Jobs { get; set; } = new();

        public List<PlayerBankAccount> BankAccounts { get; set; } = new();
        public long LastSpawnTick { get; set; }
    }
}