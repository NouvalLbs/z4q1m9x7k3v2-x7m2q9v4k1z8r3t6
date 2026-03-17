namespace ProjectSMP
{
    public partial class Player
    {
        public bool AdminOnDuty { get; set; }
        public bool MaskActive { get; set; }
        public int SpawnedAdminVehicle { get; set; } = -1;
        public float SavedPosX { get; set; }
        public float SavedPosY { get; set; }
        public float SavedPosZ { get; set; }
        public int SavedInterior { get; set; }
        public int SavedWorld { get; set; }
    }
}