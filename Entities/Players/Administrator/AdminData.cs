using SampSharp.GameMode.Display;

namespace ProjectSMP.Entities.Players.Administrator
{
    public partial class Player
    {
        public bool AdminOnDuty { get; set; }
        public bool MaskActive { get; set; }
        public int MaskId { get; set; }

        public int JailTime { get; set; }
        public int JailTimestamp { get; set; }
        public string JailReason { get; set; } = "";
        public PlayerTextDraw JailTextDraw { get; set; }

        public int BanTime { get; set; }
        public int BanExpire { get; set; }
        public string BanReason { get; set; } = "";
        public string BanAdmin { get; set; } = "";

        public bool Cuffed { get; set; }

        public int SpawnedAdminVehicle { get; set; } = -1;

        public float SavedPosX { get; set; }
        public float SavedPosY { get; set; }
        public float SavedPosZ { get; set; }
        public int SavedInterior { get; set; }
        public int SavedWorld { get; set; }
    }
}