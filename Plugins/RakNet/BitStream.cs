using System;

namespace ProjectSMP.Plugins.RakNet
{
    public class BitStream : IDisposable
    {
        public int Id { get; private set; }
        private readonly bool _autoDelete;

        public BitStream()
        {
            Id = RakNetNatives.Instance.BS_New();
            _autoDelete = true;
        }

        public BitStream(int id, bool autoDelete = false)
        {
            Id = id;
            _autoDelete = autoDelete;
        }

        public BitStream Copy()
        {
            return new BitStream(RakNetNatives.Instance.BS_NewCopy(Id), true);
        }

        public void Reset() => RakNetNatives.Instance.BS_Reset(Id);
        public void ResetReadPointer() => RakNetNatives.Instance.BS_ResetReadPointer(Id);
        public void ResetWritePointer() => RakNetNatives.Instance.BS_ResetWritePointer(Id);
        public void IgnoreBits(int numberOfBits) => RakNetNatives.Instance.BS_IgnoreBits(Id, numberOfBits);

        public void SetWriteOffset(int offset) => RakNetNatives.Instance.BS_SetWriteOffset(Id, offset);
        public int GetWriteOffset() { RakNetNatives.Instance.BS_GetWriteOffset(Id, out var offset); return offset; }
        public void SetReadOffset(int offset) => RakNetNatives.Instance.BS_SetReadOffset(Id, offset);
        public int GetReadOffset() { RakNetNatives.Instance.BS_GetReadOffset(Id, out var offset); return offset; }

        public int GetNumberOfBitsUsed() { RakNetNatives.Instance.BS_GetNumberOfBitsUsed(Id, out var number); return number; }
        public int GetNumberOfBytesUsed() { RakNetNatives.Instance.BS_GetNumberOfBytesUsed(Id, out var number); return number; }
        public int GetNumberOfUnreadBits() { RakNetNatives.Instance.BS_GetNumberOfUnreadBits(Id, out var number); return number; }
        public int GetNumberOfBitsAllocated() { RakNetNatives.Instance.BS_GetNumberOfBitsAllocated(Id, out var number); return number; }

        public void WriteInt8(sbyte value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.Int8, value);
        public void WriteInt16(short value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.Int16, value);
        public void WriteInt32(int value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.Int32, value);
        public void WriteUInt8(byte value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.UInt8, value);
        public void WriteUInt16(ushort value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.UInt16, value);
        public void WriteUInt32(uint value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.UInt32, (int)value);
        public void WriteFloat(float value) => RakNetNatives.Instance.BS_WriteValue_Float(Id, (int)ValueType.Float, value);
        public void WriteBool(bool value) => RakNetNatives.Instance.BS_WriteValue_Bool(Id, (int)ValueType.Bool, value);
        public void WriteString(string value) => RakNetNatives.Instance.BS_WriteValue_String(Id, (int)ValueType.String, value);

        public void WriteCompressedInt8(sbyte value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.CInt8, value);
        public void WriteCompressedInt16(short value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.CInt16, value);
        public void WriteCompressedInt32(int value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.CInt32, value);
        public void WriteCompressedUInt8(byte value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.CUInt8, value);
        public void WriteCompressedUInt16(ushort value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.CUInt16, value);
        public void WriteCompressedUInt32(uint value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.CUInt32, (int)value);
        public void WriteCompressedFloat(float value) => RakNetNatives.Instance.BS_WriteValue_Float(Id, (int)ValueType.CFloat, value);
        public void WriteCompressedBool(bool value) => RakNetNatives.Instance.BS_WriteValue_Bool(Id, (int)ValueType.CBool, value);
        public void WriteCompressedString(string value) => RakNetNatives.Instance.BS_WriteValue_String(Id, (int)ValueType.CString, value);

        public void WriteBits(int value, int numberOfBits) => RakNetNatives.Instance.BS_WriteValue_Bits(Id, (int)ValueType.Bits, value, numberOfBits);
        public void WriteFloat3(float[] value) => RakNetNatives.Instance.BS_WriteValue_Float3(Id, (int)ValueType.Float3, value[0], value[1], value[2]);
        public void WriteFloat4(float[] value) => RakNetNatives.Instance.BS_WriteValue_Float4(Id, (int)ValueType.Float4, value[0], value[1], value[2], value[3]);
        public void WriteVector(float[] value) => RakNetNatives.Instance.BS_WriteValue_Float3(Id, (int)ValueType.Vector, value[0], value[1], value[2]);
        public void WriteNormQuat(float[] value) => RakNetNatives.Instance.BS_WriteValue_Float4(Id, (int)ValueType.NormQuat, value[0], value[1], value[2], value[3]);
        public void WriteString8(string value) => RakNetNatives.Instance.BS_WriteValue_String(Id, (int)ValueType.String8, value);
        public void WriteString32(string value) => RakNetNatives.Instance.BS_WriteValue_String(Id, (int)ValueType.String32, value);

        public sbyte ReadInt8() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.Int8, out var val); return (sbyte)val; }
        public short ReadInt16() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.Int16, out var val); return (short)val; }
        public int ReadInt32() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.Int32, out var val); return val; }
        public byte ReadUInt8() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.UInt8, out var val); return (byte)val; }
        public ushort ReadUInt16() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.UInt16, out var val); return (ushort)val; }
        public uint ReadUInt32() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.UInt32, out var val); return (uint)val; }
        public float ReadFloat() { RakNetNatives.Instance.BS_ReadValue_Float(Id, (int)ValueType.Float, out var val); return val; }
        public bool ReadBool() { RakNetNatives.Instance.BS_ReadValue_Bool(Id, (int)ValueType.Bool, out var val); return val; }
        public string ReadString(int size) { RakNetNatives.Instance.BS_ReadValue_String(Id, (int)ValueType.String, out var val, size); return val; }

        public sbyte ReadCompressedInt8() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.CInt8, out var val); return (sbyte)val; }
        public short ReadCompressedInt16() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.CInt16, out var val); return (short)val; }
        public int ReadCompressedInt32() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.CInt32, out var val); return val; }
        public byte ReadCompressedUInt8() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.CUInt8, out var val); return (byte)val; }
        public ushort ReadCompressedUInt16() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.CUInt16, out var val); return (ushort)val; }
        public uint ReadCompressedUInt32() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.CUInt32, out var val); return (uint)val; }
        public float ReadCompressedFloat() { RakNetNatives.Instance.BS_ReadValue_Float(Id, (int)ValueType.CFloat, out var val); return val; }
        public bool ReadCompressedBool() { RakNetNatives.Instance.BS_ReadValue_Bool(Id, (int)ValueType.CBool, out var val); return val; }
        public string ReadCompressedString(int size) { RakNetNatives.Instance.BS_ReadValue_String(Id, (int)ValueType.CString, out var val, size); return val; }

        public int ReadBits(int numberOfBits) { RakNetNatives.Instance.BS_ReadValue_Bits(Id, (int)ValueType.Bits, out var val, numberOfBits); return val; }
        public float[] ReadFloat3()
        {
            RakNetNatives.Instance.BS_ReadValue_Float3(Id, (int)ValueType.Float3, out var v1, out var v2, out var v3);
            return new[] { v1, v2, v3 };
        }
        public float[] ReadFloat4()
        {
            RakNetNatives.Instance.BS_ReadValue_Float4(Id, (int)ValueType.Float4, out var v1, out var v2, out var v3, out var v4);
            return new[] { v1, v2, v3, v4 };
        }
        public float[] ReadVector()
        {
            RakNetNatives.Instance.BS_ReadValue_Float3(Id, (int)ValueType.Vector, out var v1, out var v2, out var v3);
            return new[] { v1, v2, v3 };
        }
        public float[] ReadNormQuat()
        {
            RakNetNatives.Instance.BS_ReadValue_Float4(Id, (int)ValueType.NormQuat, out var v1, out var v2, out var v3, out var v4);
            return new[] { v1, v2, v3, v4 };
        }
        public string ReadString8() { RakNetNatives.Instance.BS_ReadValue_String(Id, (int)ValueType.String8, out var val, 256); return val; }
        public string ReadString32() { RakNetNatives.Instance.BS_ReadValue_String(Id, (int)ValueType.String32, out var val, 1024); return val; }

        public void SendPacket(int playerId, PacketPriority priority = PacketPriority.HighPriority, PacketReliability reliability = PacketReliability.ReliableOrdered, int orderingChannel = 0)
            => RakNetNatives.Instance.PR_SendPacket(Id, playerId, (int)priority, (int)reliability, orderingChannel);

        public void SendRPC(int playerId, int rpcId, PacketPriority priority = PacketPriority.HighPriority, PacketReliability reliability = PacketReliability.ReliableOrdered, int orderingChannel = 0)
            => RakNetNatives.Instance.PR_SendRPC(Id, playerId, rpcId, (int)priority, (int)reliability, orderingChannel);

        public void EmulateIncomingPacket(int playerId)
            => RakNetNatives.Instance.PR_EmulateIncomingPacket(Id, playerId);

        public void EmulateIncomingRPC(int playerId, int rpcId)
            => RakNetNatives.Instance.PR_EmulateIncomingRPC(Id, playerId, rpcId);

        public void Dispose()
        {
            if (Id != 0 && _autoDelete)
            {
                RakNetNatives.Instance.BS_Delete(Id);
                Id = 0;
            }
        }
    }
}