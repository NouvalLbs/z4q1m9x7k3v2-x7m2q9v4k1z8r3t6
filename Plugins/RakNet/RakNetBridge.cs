using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetBridge : NativeObjectSingleton<RakNetBridge>
    {
        [NativeMethod]
        public virtual int CallRemoteFunction(string function, string format, params object[] args)
            => throw new NativeNotImplementedException();

        // BitStream Management
        public int RakNetBridge_BSNew()
            => CallRemoteFunction("RakNetBridge_BSNew", "");

        public void RakNetBridge_BSDelete(int bs)
            => CallRemoteFunction("RakNetBridge_BSDelete", "d", bs);

        public void RakNetBridge_BSReset(int bs)
            => CallRemoteFunction("RakNetBridge_BSReset", "d", bs);

        public void RakNetBridge_BSResetReadPointer(int bs)
            => CallRemoteFunction("RakNetBridge_BSResetReadPointer", "d", bs);

        public void RakNetBridge_BSResetWritePointer(int bs)
            => CallRemoteFunction("RakNetBridge_BSResetWritePointer", "d", bs);

        // Write Functions
        public void RakNetBridge_BSWriteUint8(int bs, int value)
            => CallRemoteFunction("RakNetBridge_BSWriteUint8", "dd", bs, value);

        public void RakNetBridge_BSWriteUint16(int bs, int value)
            => CallRemoteFunction("RakNetBridge_BSWriteUint16", "dd", bs, value);

        public void RakNetBridge_BSWriteUint32(int bs, int value)
            => CallRemoteFunction("RakNetBridge_BSWriteUint32", "dd", bs, value);

        public void RakNetBridge_BSWriteInt8(int bs, int value)
            => CallRemoteFunction("RakNetBridge_BSWriteInt8", "dd", bs, value);

        public void RakNetBridge_BSWriteInt16(int bs, int value)
            => CallRemoteFunction("RakNetBridge_BSWriteInt16", "dd", bs, value);

        public void RakNetBridge_BSWriteInt32(int bs, int value)
            => CallRemoteFunction("RakNetBridge_BSWriteInt32", "dd", bs, value);

        public void RakNetBridge_BSWriteFloat(int bs, float value)
            => CallRemoteFunction("RakNetBridge_BSWriteFloat", "df", bs, value);

        public void RakNetBridge_BSWriteBool(int bs, bool value)
            => CallRemoteFunction("RakNetBridge_BSWriteBool", "dd", bs, value ? 1 : 0);

        public void RakNetBridge_BSWriteString(int bs, string value)
            => CallRemoteFunction("RakNetBridge_BSWriteString", "ds", bs, value);

        // Read Functions
        public int RakNetBridge_BSReadUint8(int bs, out int value)
        {
            value = 0;
            return CallRemoteFunction("RakNetBridge_BSReadUint8", "dI", bs, ref value);
        }

        public int RakNetBridge_BSReadUint16(int bs, out int value)
        {
            value = 0;
            return CallRemoteFunction("RakNetBridge_BSReadUint16", "dI", bs, ref value);
        }

        public int RakNetBridge_BSReadUint32(int bs, out int value)
        {
            value = 0;
            return CallRemoteFunction("RakNetBridge_BSReadUint32", "dI", bs, ref value);
        }

        public int RakNetBridge_BSReadInt8(int bs, out int value)
        {
            value = 0;
            return CallRemoteFunction("RakNetBridge_BSReadInt8", "dI", bs, ref value);
        }

        public int RakNetBridge_BSReadInt16(int bs, out int value)
        {
            value = 0;
            return CallRemoteFunction("RakNetBridge_BSReadInt16", "dI", bs, ref value);
        }

        public int RakNetBridge_BSReadInt32(int bs, out int value)
        {
            value = 0;
            return CallRemoteFunction("RakNetBridge_BSReadInt32", "dI", bs, ref value);
        }

        public int RakNetBridge_BSReadFloat(int bs, out float value)
        {
            value = 0f;
            return CallRemoteFunction("RakNetBridge_BSReadFloat", "dF", bs, ref value);
        }

        public int RakNetBridge_BSReadBool(int bs, out bool value)
        {
            int temp = 0;
            int result = CallRemoteFunction("RakNetBridge_BSReadBool", "dI", bs, ref temp);
            value = temp != 0;
            return result;
        }

        public int RakNetBridge_BSReadString(int bs, out string value, int maxlen)
        {
            value = new string(' ', maxlen);
            return CallRemoteFunction("RakNetBridge_BSReadString", "dSd", bs, ref value, maxlen);
        }

        // Info Functions
        public int RakNetBridge_BSGetNumberOfBitsUsed(int bs)
            => CallRemoteFunction("RakNetBridge_BSGetNumberOfBitsUsed", "d", bs);

        public int RakNetBridge_BSGetNumberOfBytesUsed(int bs)
            => CallRemoteFunction("RakNetBridge_BSGetNumberOfBytesUsed", "d", bs);

        // Send Functions
        public void RakNetBridge_SendPacket(int bs, int playerid, int priority, int reliability)
            => CallRemoteFunction("RakNetBridge_SendPacket", "dddd", bs, playerid, priority, reliability);

        public void RakNetBridge_SendRPC(int bs, int playerid, int rpcid, int priority, int reliability)
            => CallRemoteFunction("RakNetBridge_SendRPC", "ddddd", bs, playerid, rpcid, priority, reliability);

        // Emulate Functions
        public void RakNetBridge_EmulateIncomingPacket(int bs, int playerid)
            => CallRemoteFunction("RakNetBridge_EmulateIncomingPacket", "dd", bs, playerid);

        public void RakNetBridge_EmulateIncomingRPC(int bs, int playerid, int rpcid)
            => CallRemoteFunction("RakNetBridge_EmulateIncomingRPC", "ddd", bs, playerid, rpcid);

        // Temp BitStream
        public int RakNetBridge_GetTempBS(int playerid)
            => CallRemoteFunction("RakNetBridge_GetTempBS", "d", playerid);
    }
}