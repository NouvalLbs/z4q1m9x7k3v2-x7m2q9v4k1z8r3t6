namespace ProjectSMP.Plugins.RakNet
{
    public class OnFootSync
    {
        public ushort LrKey;
        public ushort UdKey;
        public ushort Keys;
        public float[] Position = new float[3];
        public float[] Quaternion = new float[4];
        public byte Health;
        public byte Armour;
        public byte WeaponId;
        public byte AdditionalKey;
        public byte SpecialAction;
        public float[] Velocity = new float[3];
        public float[] SurfingOffsets = new float[3];
        public ushort SurfingVehicleId;
        public short AnimationId;
        public short AnimationFlags;
    }

    public class InCarSync
    {
        public ushort VehicleId;
        public ushort LrKey;
        public ushort UdKey;
        public ushort Keys;
        public float[] Quaternion = new float[4];
        public float[] Position = new float[3];
        public float[] Velocity = new float[3];
        public float VehicleHealth;
        public byte PlayerHealth;
        public byte Armour;
        public byte WeaponId;
        public byte AdditionalKey;
        public bool SirenState;
        public bool LandingGearState;
        public ushort TrailerId;
        public float TrainSpeed;
    }

    public class TrailerSync
    {
        public ushort TrailerId;
        public float[] Position = new float[3];
        public float[] Quaternion = new float[4];
        public float[] Velocity = new float[3];
        public float[] AngularVelocity = new float[3];
    }

    public class PassengerSync
    {
        public ushort VehicleId;
        public byte SeatId;
        public byte DriveBy;
        public byte WeaponId;
        public byte AdditionalKey;
        public byte PlayerHealth;
        public byte PlayerArmour;
        public ushort LrKey;
        public ushort UdKey;
        public ushort Keys;
        public float[] Position = new float[3];
    }

    public class UnoccupiedSync
    {
        public ushort VehicleId;
        public byte SeatId;
        public float[] Roll = new float[3];
        public float[] Direction = new float[3];
        public float[] Position = new float[3];
        public float[] Velocity = new float[3];
        public float[] AngularVelocity = new float[3];
        public float VehicleHealth;
    }

    public class AimSync
    {
        public byte CamMode;
        public float[] CamFrontVec = new float[3];
        public float[] CamPos = new float[3];
        public float AimZ;
        public byte CamZoom;
        public byte WeaponState;
        public byte AspectRatio;
    }

    public class BulletSync
    {
        public byte HitType;
        public ushort HitId;
        public float[] Origin = new float[3];
        public float[] HitPos = new float[3];
        public float[] Offsets = new float[3];
        public byte WeaponId;
    }

    public class SpectatingSync
    {
        public ushort LrKey;
        public ushort UdKey;
        public ushort Keys;
        public float[] Position = new float[3];
    }

    public class MarkersSync
    {
        public const int MaxPlayers = 1000;
        public int NumberOfPlayers;
        public bool[] PlayerIsActive = new bool[MaxPlayers];
        public short[] PlayerPositionX = new short[MaxPlayers];
        public short[] PlayerPositionY = new short[MaxPlayers];
        public short[] PlayerPositionZ = new short[MaxPlayers];
        public bool[] PlayerIsParticipant = new bool[MaxPlayers];
    }

    public class WeaponsUpdate
    {
        public const int MaxWeaponSlots = 13;
        public ushort TargetId;
        public ushort TargetActorId;
        public byte[] SlotWeaponId = new byte[MaxWeaponSlots];
        public ushort[] SlotWeaponAmmo = new ushort[MaxWeaponSlots];
        public bool[] SlotUpdated = new bool[MaxWeaponSlots];
    }

    public class StatsUpdate
    {
        public int Money;
        public int DrunkLevel;
    }

    public class RconCommand
    {
        public string Command = "";
    }
}