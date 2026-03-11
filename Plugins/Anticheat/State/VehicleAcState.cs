using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.State;

public class VehicleAcState
{
    public float NopSetHealthExpected { get; set; } = -1f;
    public long NopSetHealthDeadline { get; set; }
}