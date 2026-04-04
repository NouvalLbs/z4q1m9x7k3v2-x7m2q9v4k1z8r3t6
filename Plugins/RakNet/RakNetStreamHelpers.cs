using SampSharp.GameMode;
using SampSharp.GameMode.World;
using System.Linq;

namespace ProjectSMP.Plugins.RakNet
{
    public static class RakNetStreamHelpers
    {
        public static void SendPacketToPlayerStream(BitStream bs, int playerId,
            PacketPriority priority = PacketPriority.HighPriority,
            PacketReliability reliability = PacketReliability.ReliableOrdered,
            int orderingChannel = 0)
        {
            var targetPlayer = BasePlayer.Find(playerId);
            if (targetPlayer == null) return;

            foreach (var player in BasePlayer.All.OfType<BasePlayer>())
            {
                if (player.IsPlayerStreamedIn(targetPlayer))
                {
                    bs.SendPacket(player.Id, priority, reliability, orderingChannel);
                }
            }
        }

        public static void SendRPCToPlayerStream(BitStream bs, int playerId, int rpcId,
            PacketPriority priority = PacketPriority.HighPriority,
            PacketReliability reliability = PacketReliability.ReliableOrdered,
            int orderingChannel = 0)
        {
            var targetPlayer = BasePlayer.Find(playerId);
            if (targetPlayer == null) return;

            foreach (var player in BasePlayer.All.OfType<BasePlayer>())
            {
                if (player.IsPlayerStreamedIn(targetPlayer))
                {
                    bs.SendRPC(player.Id, rpcId, priority, reliability, orderingChannel);
                }
            }
        }

        public static void SendPacketToVehicleStream(BitStream bs, int vehicleId,
            int excludedPlayerId = BasePlayer.InvalidId,
            PacketPriority priority = PacketPriority.HighPriority,
            PacketReliability reliability = PacketReliability.ReliableOrdered,
            int orderingChannel = 0)
        {
            var vehicle = BaseVehicle.Find(vehicleId);
            if (vehicle == null) return;

            foreach (var player in BasePlayer.All.OfType<BasePlayer>())
            {
                if (player.Id == excludedPlayerId) continue;
                if (vehicle.IsStreamedIn(player))
                {
                    bs.SendPacket(player.Id, priority, reliability, orderingChannel);
                }
            }
        }

        public static void SendRPCToVehicleStream(BitStream bs, int vehicleId, int rpcId,
            int excludedPlayerId = BasePlayer.InvalidId,
            PacketPriority priority = PacketPriority.HighPriority,
            PacketReliability reliability = PacketReliability.ReliableOrdered,
            int orderingChannel = 0)
        {
            var vehicle = BaseVehicle.Find(vehicleId);
            if (vehicle == null) return;

            foreach (var player in BasePlayer.All.OfType<BasePlayer>())
            {
                if (player.Id == excludedPlayerId) continue;
                if (vehicle.IsStreamedIn(player))
                {
                    bs.SendRPC(player.Id, rpcId, priority, reliability, orderingChannel);
                }
            }
        }
    }
}