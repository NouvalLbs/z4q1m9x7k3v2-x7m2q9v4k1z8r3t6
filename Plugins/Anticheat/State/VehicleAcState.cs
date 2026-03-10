using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.State;

public class VehicleAcState
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float SpawnX { get; set; }
    public float SpawnY { get; set; }
    public float SpawnZ { get; set; }
    public float SpawnZAngle { get; set; }
    public float VelX { get; set; }
    public float VelY { get; set; }
    public float VelZ { get; set; }
    public float TrailerVelX { get; set; }
    public float TrailerVelY { get; set; }
    public float TrailerVelZ { get; set; }
    public float TrailerX { get; set; }
    public float TrailerY { get; set; }
    public float TrailerZ { get; set; }
    public float PosDiff { get; set; }
    public float TrailerPosDiff { get; set; }
    public float ZAngle { get; set; }
    public float Health { get; set; } = 1000f;
    public int Speed { get; set; }
    public int LastSpeed { get; set; }
    public int SpeedDiff { get; set; }
    public int TrailerSpeed { get; set; } = -1;
    public int TrailerSpeedDiff { get; set; }
    public int Driver { get; set; } = -1;
    public int Interior { get; set; }
    public int PaintJob { get; set; } = 3;
    public int Panels { get; set; }
    public int Doors { get; set; }
    public int Lights { get; set; }
    public int Tires { get; set; }
    public bool IsSpawned { get; set; }
    public Dictionary<int, bool> LockedForPlayer { get; } = new();
}