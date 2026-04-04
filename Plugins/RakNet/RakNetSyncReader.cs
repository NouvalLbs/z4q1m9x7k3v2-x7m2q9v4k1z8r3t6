using System;

namespace ProjectSMP.Plugins.RakNet
{
    public static class RakNetSyncReader
    {
        public static void ReadOnFootSync(BitStream bs, OnFootSync data, bool outgoing = false)
        {
            if (outgoing)
            {
                bool hasLeftRight = bs.ReadBool();
                data.LrKey = hasLeftRight ? bs.ReadUInt16() : (ushort)0;

                bool hasUpDown = bs.ReadBool();
                data.UdKey = hasUpDown ? bs.ReadUInt16() : (ushort)0;

                data.Keys = bs.ReadUInt16();
                data.Position = bs.ReadFloat3();
                data.Quaternion = bs.ReadNormQuat();
                byte healthArmour = bs.ReadUInt8();
                data.WeaponId = bs.ReadUInt8();
                data.SpecialAction = bs.ReadUInt8();
                data.Velocity = bs.ReadVector();

                RakNetUtilities.UnpackHealthArmour(healthArmour, out int health, out int armour);
                data.Health = (byte)health;
                data.Armour = (byte)armour;

                bool hasSurfInfo = bs.ReadBool();
                if (hasSurfInfo)
                {
                    data.SurfingVehicleId = bs.ReadUInt16();
                    data.SurfingOffsets = bs.ReadFloat3();
                }
                else
                {
                    data.SurfingVehicleId = 0;
                }

                bool hasAnimation = bs.ReadBool();
                if (hasAnimation)
                {
                    data.AnimationId = bs.ReadInt16();
                    data.AnimationFlags = bs.ReadInt16();
                }
                else
                {
                    data.AnimationId = 0;
                    data.AnimationFlags = 0;
                }
            }
            else
            {
                data.LrKey = bs.ReadUInt16();
                data.UdKey = bs.ReadUInt16();
                data.Keys = bs.ReadUInt16();
                data.Position = bs.ReadFloat3();
                data.Quaternion = bs.ReadFloat4();
                data.Health = bs.ReadUInt8();
                data.Armour = bs.ReadUInt8();
                data.AdditionalKey = (byte)bs.ReadBits(2);
                data.WeaponId = (byte)bs.ReadBits(6);
                data.SpecialAction = bs.ReadUInt8();
                data.Velocity = bs.ReadFloat3();
                data.SurfingOffsets = bs.ReadFloat3();
                data.SurfingVehicleId = bs.ReadUInt16();
                data.AnimationId = bs.ReadInt16();
                data.AnimationFlags = bs.ReadInt16();
            }
        }

        public static void ReadInCarSync(BitStream bs, InCarSync data, bool outgoing = false)
        {
            if (outgoing)
            {
                data.VehicleId = bs.ReadUInt16();
                data.LrKey = bs.ReadUInt16();
                data.UdKey = bs.ReadUInt16();
                data.Keys = bs.ReadUInt16();
                data.Quaternion = bs.ReadNormQuat();
                data.Position = bs.ReadFloat3();
                data.Velocity = bs.ReadVector();
                ushort vehicleHealth = bs.ReadUInt16();
                data.VehicleHealth = vehicleHealth;
                byte healthArmour = bs.ReadUInt8();
                data.WeaponId = bs.ReadUInt8();
                data.SirenState = bs.ReadBool();
                data.LandingGearState = bs.ReadBool();

                RakNetUtilities.UnpackHealthArmour(healthArmour, out int playerHealth, out int armour);
                data.PlayerHealth = (byte)playerHealth;
                data.Armour = (byte)armour;

                bool hasTrainSpeed = bs.ReadBool();
                data.TrainSpeed = hasTrainSpeed ? bs.ReadFloat() : 0.0f;

                bool hasTrailer = bs.ReadBool();
                data.TrailerId = hasTrailer ? bs.ReadUInt16() : (ushort)0;
            }
            else
            {
                data.VehicleId = bs.ReadUInt16();
                data.LrKey = bs.ReadUInt16();
                data.UdKey = bs.ReadUInt16();
                data.Keys = bs.ReadUInt16();
                data.Quaternion = bs.ReadFloat4();
                data.Position = bs.ReadFloat3();
                data.Velocity = bs.ReadFloat3();
                data.VehicleHealth = bs.ReadFloat();
                data.PlayerHealth = bs.ReadUInt8();
                data.Armour = bs.ReadUInt8();
                data.AdditionalKey = (byte)bs.ReadBits(2);
                data.WeaponId = (byte)bs.ReadBits(6);
                data.SirenState = bs.ReadUInt8() != 0;
                data.LandingGearState = bs.ReadUInt8() != 0;
                data.TrailerId = bs.ReadUInt16();
                data.TrainSpeed = bs.ReadFloat();
            }
        }

        public static void ReadTrailerSync(BitStream bs, TrailerSync data)
        {
            data.TrailerId = bs.ReadUInt16();
            data.Position = bs.ReadFloat3();
            data.Quaternion = bs.ReadFloat4();
            data.Velocity = bs.ReadFloat3();
            data.AngularVelocity = bs.ReadFloat3();
        }

        public static void ReadPassengerSync(BitStream bs, PassengerSync data)
        {
            data.VehicleId = bs.ReadUInt16();
            data.DriveBy = (byte)bs.ReadBits(2);
            data.SeatId = (byte)bs.ReadBits(6);
            data.AdditionalKey = (byte)bs.ReadBits(2);
            data.WeaponId = (byte)bs.ReadBits(6);
            data.PlayerHealth = bs.ReadUInt8();
            data.PlayerArmour = bs.ReadUInt8();
            data.LrKey = bs.ReadUInt16();
            data.UdKey = bs.ReadUInt16();
            data.Keys = bs.ReadUInt16();
            data.Position = bs.ReadFloat3();
        }

        public static void ReadUnoccupiedSync(BitStream bs, UnoccupiedSync data)
        {
            data.VehicleId = bs.ReadUInt16();
            data.SeatId = bs.ReadUInt8();
            data.Roll = bs.ReadFloat3();
            data.Direction = bs.ReadFloat3();
            data.Position = bs.ReadFloat3();
            data.Velocity = bs.ReadFloat3();
            data.AngularVelocity = bs.ReadFloat3();
            data.VehicleHealth = bs.ReadFloat();
        }

        public static void ReadAimSync(BitStream bs, AimSync data)
        {
            data.CamMode = bs.ReadUInt8();
            data.CamFrontVec = bs.ReadFloat3();
            data.CamPos = bs.ReadFloat3();
            data.AimZ = bs.ReadFloat();
            data.WeaponState = (byte)bs.ReadBits(2);
            data.CamZoom = (byte)bs.ReadBits(6);
            data.AspectRatio = bs.ReadUInt8();
        }

        public static void ReadBulletSync(BitStream bs, BulletSync data)
        {
            data.HitType = bs.ReadUInt8();
            data.HitId = bs.ReadUInt16();
            data.Origin = bs.ReadFloat3();
            data.HitPos = bs.ReadFloat3();
            data.Offsets = bs.ReadFloat3();
            data.WeaponId = bs.ReadUInt8();
        }

        public static void ReadSpectatingSync(BitStream bs, SpectatingSync data)
        {
            data.LrKey = bs.ReadUInt16();
            data.UdKey = bs.ReadUInt16();
            data.Keys = bs.ReadUInt16();
            data.Position = bs.ReadFloat3();
        }

        public static void ReadMarkersSync(BitStream bs, MarkersSync data)
        {
            int numberOfPlayers = bs.ReadInt32();
            if (numberOfPlayers < 0 || numberOfPlayers > MarkersSync.MaxPlayers) return;

            data.NumberOfPlayers = numberOfPlayers;

            for (int i = 0; i < numberOfPlayers; i++)
            {
                int playerId = bs.ReadUInt16();
                if (playerId >= MarkersSync.MaxPlayers) return;

                data.PlayerIsParticipant[playerId] = true;
                bool isActive = bs.ReadCompressedBool();

                if (isActive)
                {
                    data.PlayerIsActive[playerId] = true;
                    data.PlayerPositionX[playerId] = bs.ReadInt16();
                    data.PlayerPositionY[playerId] = bs.ReadInt16();
                    data.PlayerPositionZ[playerId] = bs.ReadInt16();
                }
            }
        }

        public static void ReadWeaponsUpdate(BitStream bs, WeaponsUpdate data)
        {
            int numberOfBytes = bs.GetNumberOfBytesUsed();
            int numberOfSlots = 0;

            if (numberOfBytes > 5)
            {
                numberOfSlots = (numberOfBytes - 5) / 4;
            }

            data.TargetId = bs.ReadUInt16();
            data.TargetActorId = bs.ReadUInt16();

            while (numberOfSlots-- > 0)
            {
                byte slotId = bs.ReadUInt8();
                byte weaponId = bs.ReadUInt8();
                ushort ammo = bs.ReadUInt16();

                if (slotId < WeaponsUpdate.MaxWeaponSlots)
                {
                    data.SlotWeaponId[slotId] = weaponId;
                    data.SlotWeaponAmmo[slotId] = ammo;
                    data.SlotUpdated[slotId] = true;
                }
            }
        }

        public static void ReadStatsUpdate(BitStream bs, StatsUpdate data)
        {
            data.Money = bs.ReadInt32();
            data.DrunkLevel = bs.ReadInt32();
        }

        public static void ReadRconCommand(BitStream bs, RconCommand data)
        {
            data.Command = bs.ReadString32();
        }
    }
}