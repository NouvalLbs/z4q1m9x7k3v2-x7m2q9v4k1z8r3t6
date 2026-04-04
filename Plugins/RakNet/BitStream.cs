using System;

namespace ProjectSMP.Plugins.RakNet
{
    public class BitStream : IDisposable
    {
        private static readonly RakNetBridge N = RakNetBridge.Instance;
        private readonly int _handle;
        private bool _disposed;

        public int Handle => _handle;

        public BitStream()
        {
            _handle = N.RakNetBridge_BSNew();
        }

        public BitStream(int handle)
        {
            _handle = handle;
        }

        public void Reset() => N.RakNetBridge_BSReset(_handle);
        public void ResetReadPointer() => N.RakNetBridge_BSResetReadPointer(_handle);
        public void ResetWritePointer() => N.RakNetBridge_BSResetWritePointer(_handle);

        public void WriteUInt8(byte value) => N.RakNetBridge_BSWriteUint8(_handle, value);
        public void WriteUInt16(ushort value) => N.RakNetBridge_BSWriteUint16(_handle, value);
        public void WriteUInt32(uint value) => N.RakNetBridge_BSWriteUint32(_handle, (int)value);
        public void WriteInt8(sbyte value) => N.RakNetBridge_BSWriteInt8(_handle, value);
        public void WriteInt16(short value) => N.RakNetBridge_BSWriteInt16(_handle, value);
        public void WriteInt32(int value) => N.RakNetBridge_BSWriteInt32(_handle, value);
        public void WriteFloat(float value) => N.RakNetBridge_BSWriteFloat(_handle, value);
        public void WriteBool(bool value) => N.RakNetBridge_BSWriteBool(_handle, value);
        public void WriteString(string value) => N.RakNetBridge_BSWriteString(_handle, value);

        public byte ReadUInt8() { N.RakNetBridge_BSReadUint8(_handle, out int v); return (byte)v; }
        public ushort ReadUInt16() { N.RakNetBridge_BSReadUint16(_handle, out int v); return (ushort)v; }
        public uint ReadUInt32() { N.RakNetBridge_BSReadUint32(_handle, out int v); return (uint)v; }
        public sbyte ReadInt8() { N.RakNetBridge_BSReadInt8(_handle, out int v); return (sbyte)v; }
        public short ReadInt16() { N.RakNetBridge_BSReadInt16(_handle, out int v); return (short)v; }
        public int ReadInt32() { N.RakNetBridge_BSReadInt32(_handle, out int v); return v; }
        public float ReadFloat() { N.RakNetBridge_BSReadFloat(_handle, out float v); return v; }
        public bool ReadBool() { N.RakNetBridge_BSReadBool(_handle, out bool v); return v; }
        public string ReadString(int maxlen = 256) { N.RakNetBridge_BSReadString(_handle, out string v, maxlen); return v; }

        public int GetNumberOfBitsUsed() => N.RakNetBridge_BSGetNumberOfBitsUsed(_handle);
        public int GetNumberOfBytesUsed() => N.RakNetBridge_BSGetNumberOfBytesUsed(_handle);

        public void Dispose()
        {
            if (_disposed) return;
            N.RakNetBridge_BSDelete(_handle);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~BitStream() => Dispose();
    }
}