#nullable enable
using System;

namespace ProjectSMP.Plugins.RakNet
{
    public static class RakNetService
    {
        private static RakNetNatives N => RakNetNatives.Instance;

        public static event Action<int, int, int>? OnIncomingPacket;
        public static event Action<int, int, int>? OnIncomingRPC;
        public static event Action<int, int, int>? OnOutgoingPacket;
        public static event Action<int, int, int>? OnOutgoingRPC;

        public static void HandleIncomingPacket(int playerId, int packetId, int bs)
            => OnIncomingPacket?.Invoke(playerId, packetId, bs);

        public static void HandleIncomingRPC(int playerId, int rpcId, int bs)
            => OnIncomingRPC?.Invoke(playerId, rpcId, bs);

        public static void HandleOutgoingPacket(int playerId, int packetId, int bs)
            => OnOutgoingPacket?.Invoke(playerId, packetId, bs);

        public static void HandleOutgoingRPC(int playerId, int rpcId, int bs)
            => OnOutgoingRPC?.Invoke(playerId, rpcId, bs);

        public static int BS_New() => N.RNB_BS_New();
        public static int BS_NewCopy(int bs) => N.RNB_BS_NewCopy(bs);
        public static void BS_Delete(int bs) => N.RNB_BS_Delete(bs);
        public static void BS_Reset(int bs) => N.RNB_BS_Reset(bs);
        public static void BS_ResetReadPointer(int bs) => N.RNB_BS_ResetReadPointer(bs);
        public static void BS_ResetWritePointer(int bs) => N.RNB_BS_ResetWritePointer(bs);
        public static void BS_IgnoreBits(int bs, int numberOfBits) => N.RNB_BS_IgnoreBits(bs, numberOfBits);

        public static void BS_SetWriteOffset(int bs, int offset) => N.RNB_BS_SetWriteOffset(bs, offset);
        public static int BS_GetWriteOffset(int bs) => N.RNB_BS_GetWriteOffset(bs);
        public static void BS_SetReadOffset(int bs, int offset) => N.RNB_BS_SetReadOffset(bs, offset);
        public static int BS_GetReadOffset(int bs) => N.RNB_BS_GetReadOffset(bs);

        public static int BS_GetNumberOfBitsUsed(int bs) => N.RNB_BS_GetNumberOfBitsUsed(bs);
        public static int BS_GetNumberOfBytesUsed(int bs) => N.RNB_BS_GetNumberOfBytesUsed(bs);
        public static int BS_GetNumberOfUnreadBits(int bs) => N.RNB_BS_GetNumberOfUnreadBits(bs);
        public static int BS_GetNumberOfBitsAllocated(int bs) => N.RNB_BS_GetNumberOfBitsAllocated(bs);

        public static void BS_WriteInt8(int bs, int value) => N.RNB_BS_WriteInt8(bs, value);
        public static void BS_WriteInt16(int bs, int value) => N.RNB_BS_WriteInt16(bs, value);
        public static void BS_WriteInt32(int bs, int value) => N.RNB_BS_WriteInt32(bs, value);
        public static void BS_WriteUint8(int bs, int value) => N.RNB_BS_WriteUint8(bs, value);
        public static void BS_WriteUint16(int bs, int value) => N.RNB_BS_WriteUint16(bs, value);
        public static void BS_WriteUint32(int bs, int value) => N.RNB_BS_WriteUint32(bs, value);
        public static void BS_WriteFloat(int bs, float value) => N.RNB_BS_WriteFloat(bs, value);
        public static void BS_WriteBool(int bs, bool value) => N.RNB_BS_WriteBool(bs, value ? 1 : 0);
        public static void BS_WriteString(int bs, string value) => N.RNB_BS_WriteString(bs, value);

        public static int BS_ReadInt8(int bs) => N.RNB_BS_ReadInt8(bs);
        public static int BS_ReadInt16(int bs) => N.RNB_BS_ReadInt16(bs);
        public static int BS_ReadInt32(int bs) => N.RNB_BS_ReadInt32(bs);
        public static int BS_ReadUint8(int bs) => N.RNB_BS_ReadUint8(bs);
        public static int BS_ReadUint16(int bs) => N.RNB_BS_ReadUint16(bs);
        public static int BS_ReadUint32(int bs) => N.RNB_BS_ReadUint32(bs);
        public static float BS_ReadFloat(int bs) => N.RNB_BS_ReadFloat(bs);
        public static bool BS_ReadBool(int bs) => N.RNB_BS_ReadBool(bs) != 0;
        public static string BS_ReadString(int bs, int maxSize = 256)
        {
            var output = new string(' ', maxSize);
            N.RNB_BS_ReadString(bs, output, maxSize);
            return output.TrimEnd('\0');
        }

        public static void PR_SendPacket(
            int bs,
            int playerId,
            PR_PacketPriority priority = PR_PacketPriority.PR_HIGH_PRIORITY,
            PR_PacketReliability reliability = PR_PacketReliability.PR_RELIABLE_ORDERED,
            int orderingChannel = 0)
            => N.RNB_PR_SendPacket(bs, playerId, (int)priority, (int)reliability, orderingChannel);

        public static void PR_SendRPC(
            int bs,
            int playerId,
            int rpcId,
            PR_PacketPriority priority = PR_PacketPriority.PR_HIGH_PRIORITY,
            PR_PacketReliability reliability = PR_PacketReliability.PR_RELIABLE_ORDERED,
            int orderingChannel = 0)
            => N.RNB_PR_SendRPC(bs, playerId, rpcId, (int)priority, (int)reliability, orderingChannel);

        public static void PR_EmulateIncomingPacket(int bs, int playerId)
            => N.RNB_PR_EmulateIncomingPacket(bs, playerId);

        public static void PR_EmulateIncomingRPC(int bs, int playerId, int rpcId)
            => N.RNB_PR_EmulateIncomingRPC(bs, playerId, rpcId);
    }
}