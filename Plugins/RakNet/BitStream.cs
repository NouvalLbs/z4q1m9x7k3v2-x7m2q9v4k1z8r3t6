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

        public void WriteInt8(sbyte value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.Int8, value);
        public void WriteInt16(short value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.Int16, value);
        public void WriteInt32(int value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.Int32, value);
        public void WriteUInt8(byte value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.UInt8, value);
        public void WriteUInt16(ushort value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.UInt16, value);
        public void WriteUInt32(uint value) => RakNetNatives.Instance.BS_WriteValue_Int(Id, (int)ValueType.UInt32, (int)value);
        public void WriteFloat(float value) => RakNetNatives.Instance.BS_WriteValue_Float(Id, (int)ValueType.Float, value);
        public void WriteBool(bool value) => RakNetNatives.Instance.BS_WriteValue_Bool(Id, (int)ValueType.Bool, value);
        public void WriteString(string value) => RakNetNatives.Instance.BS_WriteValue_String(Id, (int)ValueType.String, value);

        public sbyte ReadInt8() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.Int8, out var val); return (sbyte)val; }
        public short ReadInt16() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.Int16, out var val); return (short)val; }
        public int ReadInt32() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.Int32, out var val); return val; }
        public byte ReadUInt8() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.UInt8, out var val); return (byte)val; }
        public ushort ReadUInt16() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.UInt16, out var val); return (ushort)val; }
        public uint ReadUInt32() { RakNetNatives.Instance.BS_ReadValue_Int(Id, (int)ValueType.UInt32, out var val); return (uint)val; }
        public float ReadFloat() { RakNetNatives.Instance.BS_ReadValue_Float(Id, (int)ValueType.Float, out var val); return val; }
        public bool ReadBool() { RakNetNatives.Instance.BS_ReadValue_Bool(Id, (int)ValueType.Bool, out var val); return val; }
        public string ReadString(int size) { RakNetNatives.Instance.BS_ReadValue_String(Id, (int)ValueType.String, out var val, size); return val; }

        public void SendPacket(int playerId, PacketPriority priority = PacketPriority.HighPriority, PacketReliability reliability = PacketReliability.ReliableOrdered)
            => RakNetNatives.Instance.PR_SendPacket(Id, playerId, (int)priority, (int)reliability);

        public void SendRPC(int playerId, int rpcId, PacketPriority priority = PacketPriority.HighPriority, PacketReliability reliability = PacketReliability.ReliableOrdered)
            => RakNetNatives.Instance.PR_SendRPC(Id, playerId, rpcId, (int)priority, (int)reliability);

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