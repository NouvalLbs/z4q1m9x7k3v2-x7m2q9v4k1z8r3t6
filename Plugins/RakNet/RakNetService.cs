using System;
using SampSharp.Core.Callbacks;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetService
    {
        public event EventHandler<RakNetEventArgs> OnIncomingPacket;
        public event EventHandler<RakNetEventArgs> OnIncomingRPC;
        public event EventHandler<RakNetEventArgs> OnOutcomingPacket;
        public event EventHandler<RakNetEventArgs> OnOutcomingRPC;

        public RakNetService()
        {
            RakNetNatives.Instance.PR_Init();
        }

        [Callback("OnIncomingPacket")]
        public bool InternalOnIncomingPacket(int playerid, int packetid, int bsId)
        {
            var args = new RakNetEventArgs(playerid, packetid, new BitStream(bsId, false));
            OnIncomingPacket?.Invoke(this, args);
            return !args.PreventDefault;
        }

        [Callback("OnIncomingRPC")]
        public bool InternalOnIncomingRPC(int playerid, int rpcid, int bsId)
        {
            var args = new RakNetEventArgs(playerid, rpcid, new BitStream(bsId, false));
            OnIncomingRPC?.Invoke(this, args);
            return !args.PreventDefault;
        }

        [Callback("OnOutcomingPacket")]
        public bool InternalOnOutcomingPacket(int playerid, int packetid, int bsId)
        {
            var args = new RakNetEventArgs(playerid, packetid, new BitStream(bsId, false));
            OnOutcomingPacket?.Invoke(this, args);
            return !args.PreventDefault;
        }

        [Callback("OnOutcomingRPC")]
        public bool InternalOnOutcomingRPC(int playerid, int rpcid, int bsId)
        {
            var args = new RakNetEventArgs(playerid, rpcid, new BitStream(bsId, false));
            OnOutcomingRPC?.Invoke(this, args);
            return !args.PreventDefault;
        }
    }
}