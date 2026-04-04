using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetBridge : NativeObjectSingleton<RakNetBridge>
    {
        [NativeMethod]
        public virtual int RakNetBridge_BSNew() => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSDelete(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSReset(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSResetReadPointer(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSResetWritePointer(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteUint8(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteUint16(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteUint32(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteInt8(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteInt16(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteInt32(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteFloat(int bs, float value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteBool(int bs, bool value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_BSWriteString(int bs, string value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadUint8(int bs, out int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadUint16(int bs, out int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadUint32(int bs, out int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadInt8(int bs, out int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadInt16(int bs, out int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadInt32(int bs, out int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadFloat(int bs, out float value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadBool(int bs, out bool value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSReadString(int bs, out string value, int maxlen) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSGetNumberOfBitsUsed(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_BSGetNumberOfBytesUsed(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_SendPacket(int bs, int playerid, int priority, int reliability) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_SendRPC(int bs, int playerid, int rpcid, int priority, int reliability) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_EmulateIncomingPacket(int bs, int playerid) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void RakNetBridge_EmulateIncomingRPC(int bs, int playerid, int rpcid) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RakNetBridge_GetTempBS(int playerid) => throw new NativeNotImplementedException();
    }
}