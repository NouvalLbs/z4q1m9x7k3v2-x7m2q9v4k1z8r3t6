namespace ProjectSMP.Features.Jobs.Side.Forklifter
{
    public enum ForklifterPhase { GoToLoad, Loading, GoToUnload, Unloading }

    public class ForklifterSession
    {
        public bool IsActive { get; set; }
        public ForklifterPhase Phase { get; set; }
        public int LoadCount { get; set; }
        public int UnloadCount { get; set; }
        public int CurrentLoadIndex { get; set; }
        public int CurrentUnloadIndex { get; set; }
    }
}