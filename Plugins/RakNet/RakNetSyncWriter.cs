using System;

namespace ProjectSMP.Plugins.RakNet
{
    public static class RakNetSyncWriter
    {
        public static void WriteOnFootSync(BitStream bs, OnFootSync data, bool outgoing = false)
        {
            if (outgoing)
            {
                if (data.LrKey != 0)
                {
                    bs.WriteBool(true);
                    bs.WriteUInt16(data.LrKey);
                }
                else
                {
                    bs.WriteBool(false);
                }

                if (data.UdKey != 0)
                {
                    bs.WriteBool(true);
                    bs.WriteUInt16(data.UdKey);
                }
                else
                {
                    bs.WriteBool(false);
                }

                byte healthArmour = RakNetUtilities.PackHealthArmour(data.Health, data.Armour);

                bs.WriteUInt16(data.Keys);
                bs.WriteFloat3(data.Position);
                bs.WriteNormQuat(data.Quaternion);
                bs.WriteUInt8(healthArmour);
                bs.WriteUInt8(data.WeaponId);
                bs.WriteUInt8(data.SpecialAction);
                bs.WriteVector(data.Velocity);

                if (data.SurfingVehicleId != 0)
                {
                    bs.WriteBool(true);
                    bs.WriteUInt16(data.SurfingVehicleId);
                    bs.WriteFloat3(data.SurfingOffsets);
                }
                else
                {
                    bs.WriteBool(false);
                }

                if (data.AnimationId != 0 || data.AnimationFlags != 0)
                {
                    bs.WriteBool(true);
                    bs.WriteInt16(data.AnimationId);
                    bs.WriteInt16(data.AnimationFlags);
                }
                else
                {
                    bs.WriteBool(false);
                }
            }
            else
            {
                bs.WriteUInt16(data.LrKey);
                bs.WriteUInt16(data.UdKey);
                bs.WriteUInt16(data.Keys);
                bs.WriteFloat3(data.Position);
                bs.WriteFloat4(data.Quaternion);
                bs.WriteUInt8(data.Health);
                bs.WriteUInt8(data.Armour);
                bs.WriteBits(data.AdditionalKey, 2);
                bs.WriteBits(data.WeaponId, 6);
                bs.WriteUInt8(data.SpecialAction);
                bs.WriteFloat3(data.Velocity);
                bs.WriteFloat3(data.SurfingOffsets);
                bs.WriteUInt16(data.SurfingVehicleId);
                bs.WriteInt16(data.AnimationId);
                bs.WriteInt16(data.AnimationFlags);
            }
        }

        public static void WriteInCarSync(BitStream bs, InCarSync data, bool outgoing = false)
        {
            if (outgoing)
            {
                byte healthArmour = RakNetUtilities.PackHealthArmour(data.PlayerHealth, data.Armour);

                bs.WriteUInt16(data.VehicleId);
                bs.WriteUInt16(data.LrKey);
                bs.WriteUInt16(data.UdKey);
                bs.WriteUInt16(data.Keys);
                bs.WriteNormQuat(data.Quaternion);
                bs.WriteFloat3(data.Position);
                bs.WriteVector(data.Velocity);
                bs.WriteUInt16((ushort)Math.Round(data.VehicleHealth));
                bs.WriteUInt8(healthArmour);
                bs.WriteUInt8(data.WeaponId);
                bs.WriteBool(data.SirenState);
                bs.WriteBool(data.LandingGearState);

                if (data.TrainSpeed != 0)
                {
                    bs.WriteBool(true);
                    bs.WriteFloat(data.TrainSpeed);
                }
                else
                {
                    bs.WriteBool(false);
                }

                if (data.TrailerId != 0)
                {
                    bs.WriteBool(true);
                    bs.WriteUInt16(data.TrailerId);
                }
                else
                {
                    bs.WriteBool(false);
                }
            }
            else
            {
                bs.WriteUInt16(data.VehicleId);
                bs.WriteUInt16(data.LrKey);
                bs.WriteUInt16(data.UdKey);
                bs.WriteUInt16(data.Keys);
                bs.WriteFloat4(data.Quaternion);
                bs.WriteFloat3(data.Position);
                bs.WriteFloat3(data.Velocity);
                bs.WriteFloat(data.VehicleHealth);
                bs.WriteUInt8(data.PlayerHealth);
                bs.WriteUInt8(data.Armour);
                bs.WriteBits(data.AdditionalKey, 2);
                bs.WriteBits(data.WeaponId, 6);
                bs.WriteUInt8((byte)(data.SirenState ? 1 : 0));
                bs.WriteUInt8((byte)(data.LandingGearState ? 1 : 0));
                bs.WriteUInt16(data.TrailerId);
                bs.WriteFloat(data.TrainSpeed);
            }
        }

        public static void WriteTrailerSync(BitStream bs, TrailerSync data)
        {
            bs.WriteUInt16(data.TrailerId);
            bs.WriteFloat3(data.Position);
            bs.WriteFloat4(data.Quaternion);
            bs.WriteFloat3(data.Velocity);
            bs.WriteFloat3(data.AngularVelocity);
        }

        public static void WritePassengerSync(BitStream bs, PassengerSync data)
        {
            bs.WriteUInt16(data.VehicleId);
            bs.WriteBits(data.DriveBy, 2);
            bs.WriteBits(data.SeatId, 6);
            bs.WriteBits(data.AdditionalKey, 2);
            bs.WriteBits(data.WeaponId, 6);
            bs.WriteUInt8(data.PlayerHealth);
            bs.WriteUInt8(data.PlayerArmour);
            bs.WriteUInt16(data.LrKey);
            bs.WriteUInt16(data.UdKey);
            bs.WriteUInt16(data.Keys);
            bs.WriteFloat3(data.Position);
        }

        public static void WriteUnoccupiedSync(BitStream bs, UnoccupiedSync data)
        {
            bs.WriteUInt16(data.VehicleId);
            bs.WriteUInt8(data.SeatId);
            bs.WriteFloat3(data.Roll);
            bs.WriteFloat3(data.Direction);
            bs.WriteFloat3(data.Position);
            bs.WriteFloat3(data.Velocity);
            bs.WriteFloat3(data.AngularVelocity);
            bs.WriteFloat(data.VehicleHealth);
        }

        public static void WriteAimSync(BitStream bs, AimSync data)
        {
            bs.WriteUInt8(data.CamMode);
            bs.WriteFloat3(data.CamFrontVec);
            bs.WriteFloat3(data.CamPos);
            bs.WriteFloat(data.AimZ);
            bs.WriteBits(data.WeaponState, 2);
            bs.WriteBits(data.CamZoom, 6);
            bs.WriteUInt8(data.AspectRatio);
        }

        public static void WriteBulletSync(BitStream bs, BulletSync data)
        {
            bs.WriteUInt8(data.HitType);
            bs.WriteUInt16(data.HitId);
            bs.WriteFloat3(data.Origin);
            bs.WriteFloat3(data.HitPos);
            bs.WriteFloat3(data.Offsets);
            bs.WriteUInt8(data.WeaponId);
        }

        public static void WriteSpectatingSync(BitStream bs, SpectatingSync data)
        {
            bs.WriteUInt16(data.LrKey);
            bs.WriteUInt16(data.UdKey);
            bs.WriteUInt16(data.Keys);
            bs.WriteFloat3(data.Position);
        }

        public static void WriteMarkersSync(BitStream bs, MarkersSync data)
        {
            bs.WriteInt32(data.NumberOfPlayers);

            for (int i = 0; i < MarkersSync.MaxPlayers; i++)
            {
                if (!data.PlayerIsParticipant[i]) continue;

                bs.WriteUInt16((ushort)i);
                bs.WriteCompressedBool(data.PlayerIsActive[i]);

                if (data.PlayerIsActive[i])
                {
                    bs.WriteInt16(data.PlayerPositionX[i]);
                    bs.WriteInt16(data.PlayerPositionY[i]);
                    bs.WriteInt16(data.PlayerPositionZ[i]);
                }
            }
        }

        public static void WriteWeaponsUpdate(BitStream bs, WeaponsUpdate data)
        {
            bs.WriteUInt16(data.TargetId);
            bs.WriteUInt16(data.TargetActorId);

            for (int slotId = 0; slotId < WeaponsUpdate.MaxWeaponSlots; slotId++)
            {
                if (!data.SlotUpdated[slotId]) continue;

                bs.WriteUInt8((byte)slotId);
                bs.WriteUInt8(data.SlotWeaponId[slotId]);
                bs.WriteUInt16(data.SlotWeaponAmmo[slotId]);
            }
        }

        public static void WriteStatsUpdate(BitStream bs, StatsUpdate data)
        {
            bs.WriteInt32(data.Money);
            bs.WriteInt32(data.DrunkLevel);
        }

        public static void WriteRconCommand(BitStream bs, RconCommand data)
        {
            bs.WriteString32(data.Command);
        }
    }
}