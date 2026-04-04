using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetNatives : NativeObjectSingleton<RakNetNatives>
    {
        [NativeMethod]
        public virtual int RNB_BS_New() => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_NewCopy(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_Delete(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_Reset(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ResetReadPointer(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ResetWritePointer(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_IgnoreBits(int bs, int numberOfBits) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_SetWriteOffset(int bs, int offset) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_GetWriteOffset(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_SetReadOffset(int bs, int offset) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_GetReadOffset(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_GetNumberOfBitsUsed(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_GetNumberOfBytesUsed(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_GetNumberOfUnreadBits(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_GetNumberOfBitsAllocated(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteInt8(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteInt16(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteInt32(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteUint8(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteUint16(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteUint32(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteFloat(int bs, float value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteBool(int bs, int value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_WriteString(int bs, string value) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ReadInt8(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ReadInt16(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ReadInt32(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ReadUint8(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ReadUint16(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ReadUint32(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual float RNB_BS_ReadFloat(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ReadBool(int bs) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_BS_ReadString(int bs, string output, int size) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_PR_SendPacket(int bs, int playerId, int priority, int reliability, int orderingChannel) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_PR_SendRPC(int bs, int playerId, int rpcId, int priority, int reliability, int orderingChannel) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_PR_EmulateIncomingPacket(int bs, int playerId) => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int RNB_PR_EmulateIncomingRPC(int bs, int playerId, int rpcId) => throw new NativeNotImplementedException();
    }
}