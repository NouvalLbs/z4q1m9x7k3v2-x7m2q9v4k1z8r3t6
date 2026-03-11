using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.State;

public class VehicleAcState
{
    public int PaintJob { get; set; } = 3;
    public Queue<long> PaintjobChangeHistory { get; } = new();
    public long LastServerPaintjobTick { get; set; }
    public bool HasNitro { get; set; }
    public int NitroType { get; set; } = -1;
    public HashSet<int> InstalledComponents { get; } = new();
    public Queue<long> ComponentChangeHistory { get; } = new();
    public long LastServerModTick { get; set; }
    public float LastHealth { get; set; }
    public long LastPayNSprayTick { get; set; }

    // ── Anti-NOP: SetVehicleHealth ───────────────────────────────────────
    public float NopSetHealthExpected { get; set; } = -1f;
    public long NopSetHealthDeadline { get; set; }
}