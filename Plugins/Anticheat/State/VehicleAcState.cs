using System.Collections.Concurrent;

namespace ProjectSMP.Plugins.Anticheat.State;

public class VehicleAcState
{
    public int Paintjob { get; set; } = 3;
    public int Interior { get; set; }
    public int Panels { get; set; }
    public int Doors { get; set; }
    public int Lights { get; set; }
    public int Tires { get; set; }
    public bool DoorsLocked { get; set; }
    public ConcurrentDictionary<int, bool> DoorsLockedPerPlayer { get; } = new();
    public float NopSetHealthExpected { get; set; } = -1f;
    public long NopSetHealthDeadline { get; set; }
}