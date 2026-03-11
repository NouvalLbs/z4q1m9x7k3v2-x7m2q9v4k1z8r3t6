namespace ProjectSMP.Plugins.Anticheat.State;

public class VehicleAcState
{
    public int PaintJob { get; set; } = 3;

    // ── Anti-NOP: SetVehicleHealth ───────────────────────────────────────
    public float NopSetHealthExpected { get; set; } = -1f;
    public long NopSetHealthDeadline { get; set; }
}