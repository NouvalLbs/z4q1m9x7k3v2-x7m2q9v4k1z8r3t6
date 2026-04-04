using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.RakNet
{
    public enum PacketPriority
    {
        System = 0,
        High = 1,
        Medium = 2,
        Low = 3
    }

    public enum PacketReliability
    {
        Unreliable = 6,
        UnreliableSequenced = 7,
        Reliable = 8,
        ReliableOrdered = 9,
        ReliableSequenced = 10
    }

    public class PacketEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public int PacketId { get; set; }
        public BitStream BitStream { get; set; }
        public bool Block { get; set; }
    }

    public class RPCEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public int RpcId { get; set; }
        public BitStream BitStream { get; set; }
        public bool Block { get; set; }
    }

    public static class RakNetService
    {
        private static readonly RakNetBridge N = RakNetBridge.Instance;
        private static readonly Dictionary<int, BitStream> _tempStreams = new();

        public static event EventHandler<PacketEventArgs> IncomingPacket;
        public static event EventHandler<PacketEventArgs> OutgoingPacket;
        public static event EventHandler<RPCEventArgs> IncomingRPC;
        public static event EventHandler<RPCEventArgs> OutgoingRPC;

        public static void SendPacket(BitStream bs, int playerId, PacketPriority priority = PacketPriority.High, PacketReliability reliability = PacketReliability.ReliableOrdered)
        {
            N.RakNetBridge_SendPacket(bs.Handle, playerId, (int)priority, (int)reliability);
        }

        public static void SendRPC(BitStream bs, int playerId, int rpcId, PacketPriority priority = PacketPriority.High, PacketReliability reliability = PacketReliability.ReliableOrdered)
        {
            N.RakNetBridge_SendRPC(bs.Handle, playerId, rpcId, (int)priority, (int)reliability);
        }

        public static void EmulateIncomingPacket(BitStream bs, int playerId)
        {
            N.RakNetBridge_EmulateIncomingPacket(bs.Handle, playerId);
        }

        public static void EmulateIncomingRPC(BitStream bs, int playerId, int rpcId)
        {
            N.RakNetBridge_EmulateIncomingRPC(bs.Handle, playerId, rpcId);
        }

        public static BitStream GetTempStream(int playerId)
        {
            if (!_tempStreams.ContainsKey(playerId))
            {
                var handle = N.RakNetBridge_GetTempBS(playerId);
                _tempStreams[playerId] = new BitStream(handle);
            }
            return _tempStreams[playerId];
        }

        public static void OnPlayerConnect(int playerId)
        {
            if (_tempStreams.ContainsKey(playerId))
                _tempStreams.Remove(playerId);
        }

        public static void OnPlayerDisconnect(int playerId)
        {
            if (_tempStreams.ContainsKey(playerId))
                _tempStreams.Remove(playerId);
        }

        internal static int HandleIncomingPacket(int playerId, int packetId, int bsHandle)
        {
            var bs = new BitStream(bsHandle);
            var args = new PacketEventArgs { PlayerId = playerId, PacketId = packetId, BitStream = bs };
            IncomingPacket?.Invoke(null, args);
            return args.Block ? 0 : 1;
        }

        internal static int HandleOutgoingPacket(int playerId, int packetId, int bsHandle)
        {
            var bs = new BitStream(bsHandle);
            var args = new PacketEventArgs { PlayerId = playerId, PacketId = packetId, BitStream = bs };
            OutgoingPacket?.Invoke(null, args);
            return args.Block ? 0 : 1;
        }

        internal static int HandleIncomingRPC(int playerId, int rpcId, int bsHandle)
        {
            var bs = new BitStream(bsHandle);
            var args = new RPCEventArgs { PlayerId = playerId, RpcId = rpcId, BitStream = bs };
            IncomingRPC?.Invoke(null, args);
            return args.Block ? 0 : 1;
        }

        internal static int HandleOutgoingRPC(int playerId, int rpcId, int bsHandle)
        {
            var bs = new BitStream(bsHandle);
            var args = new RPCEventArgs { PlayerId = playerId, RpcId = rpcId, BitStream = bs };
            OutgoingRPC?.Invoke(null, args);
            return args.Block ? 0 : 1;
        }
    }
}