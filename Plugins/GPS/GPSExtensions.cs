using SampSharp.GameMode;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.GPS
{
    public static class GPSExtensions
    {
        /// <summary>
        /// Get closest map node to player position
        /// </summary>
        public static MapNode GetClosestMapNode(this BasePlayer player, MapNode ignoredNode = default)
        {
            return GPSService.GetClosestNodeToPoint(player.Position, ignoredNode);
        }

        /// <summary>
        /// Get closest map node to vehicle position
        /// </summary>
        public static MapNode GetClosestMapNode(this BaseVehicle vehicle, MapNode ignoredNode = default)
        {
            return GPSService.GetClosestNodeToPoint(vehicle.Position, ignoredNode);
        }

        /// <summary>
        /// Find path from player position to target node
        /// </summary>
        public static Path FindPathToNode(this BasePlayer player, MapNode targetNode)
        {
            var startNode = player.GetClosestMapNode();
            if (!startNode.IsValid) return Path.Invalid;

            return GPSService.FindPath(startNode, targetNode);
        }

        /// <summary>
        /// Find path from player position to target position
        /// </summary>
        public static Path FindPathToPosition(this BasePlayer player, Vector3 targetPosition)
        {
            var startNode = player.GetClosestMapNode();
            var targetNode = GPSService.GetClosestNodeToPoint(targetPosition);

            if (!startNode.IsValid || !targetNode.IsValid) return Path.Invalid;

            return GPSService.FindPath(startNode, targetNode);
        }

        /// <summary>
        /// Find path from player to another player
        /// </summary>
        public static Path FindPathToPlayer(this BasePlayer player, BasePlayer targetPlayer)
        {
            return player.FindPathToPosition(targetPlayer.Position);
        }

        /// <summary>
        /// Find path asynchronously
        /// </summary>
        public static void FindPathToPositionAsync(this BasePlayer player, Vector3 targetPosition, Action<Path> callback)
        {
            var startNode = player.GetClosestMapNode();
            var targetNode = GPSService.GetClosestNodeToPoint(targetPosition);

            if (!startNode.IsValid || !targetNode.IsValid)
            {
                callback?.Invoke(Path.Invalid);
                return;
            }

            GPSService.FindPathThreaded(startNode, targetNode, callback);
        }

        /// <summary>
        /// Get distance to node
        /// </summary>
        public static float GetDistanceToNode(this BasePlayer player, MapNode node)
        {
            return GPSService.GetNodeDistanceFromPoint(node, player.Position);
        }

        /// <summary>
        /// Get angle to node
        /// </summary>
        public static float GetAngleToNode(this BasePlayer player, MapNode node)
        {
            return GPSService.GetNodeAngleFromPoint(node, new Vector2(player.Position.X, player.Position.Y));
        }
    }
}