using System;
using SampSharp.Core.Callbacks;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetService
    {
        public event EventHandler<RakNetEventArgs> OnIncomingPacket;
        public event EventHandler<RakNetEventArgs> OnIncomingRPC;
        public event EventHandler<RakNetEventArgs> OnOutgoingPacket;
        public event EventHandler<RakNetEventArgs> OnOutgoingRPC;
        public event EventHandler<RakNetEventArgs> OnIncomingCustomRPC;

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

        [Callback("OnOutgoingPacket")]
        public bool InternalOnOutgoingPacket(int playerid, int packetid, int bsId)
        {
            var args = new RakNetEventArgs(playerid, packetid, new BitStream(bsId, false));
            OnOutgoingPacket?.Invoke(this, args);
            return !args.PreventDefault;
        }

        [Callback("OnOutgoingRPC")]
        public bool InternalOnOutgoingRPC(int playerid, int rpcid, int bsId)
        {
            var args = new RakNetEventArgs(playerid, rpcid, new BitStream(bsId, false));
            OnOutgoingRPC?.Invoke(this, args);
            return !args.PreventDefault;
        }

        [Callback("OnIncomingCustomRPC")]
        public bool InternalOnIncomingCustomRPC(int playerid, int rpcid, int bsId)
        {
            var args = new RakNetEventArgs(playerid, rpcid, new BitStream(bsId, false));
            OnIncomingCustomRPC?.Invoke(this, args);
            return !args.PreventDefault;
        }
    }
}