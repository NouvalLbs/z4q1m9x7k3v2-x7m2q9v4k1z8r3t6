using SampSharp.GameMode;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.GPS
{
    public static class GPSService
    {
        private static readonly Dictionary<int, Action<Path>> _pathCallbacks = new Dictionary<int, Action<Path>>();
        private static int _callbackCounter = 0;

        // ============================================
        // MapNode Functions
        // ============================================

        public static MapNode CreateMapNode(Vector3 position)
        {
            var error = GPSNatives.Instance.CreateMapNode(position.X, position.Y, position.Z, out int nodeId);
            if (error != (int)GPSError.None)
            {
                Console.WriteLine($"[GPS] CreateMapNode failed: {(GPSError)error}");
                return MapNode.Invalid;
            }
            return new MapNode(nodeId);
        }

        public static bool DestroyMapNode(MapNode nodeId)
        {
            if (!nodeId.IsValid) return false;
            var error = GPSNatives.Instance.DestroyMapNode(nodeId);
            return error == (int)GPSError.None;
        }

        public static bool IsValidMapNode(MapNode nodeId)
        {
            return GPSNatives.Instance.IsValidMapNode(nodeId);
        }

        public static Vector3? GetMapNodePosition(MapNode nodeId)
        {
            if (!nodeId.IsValid) return null;

            var error = GPSNatives.Instance.GetMapNodePos(nodeId, out float x, out float y, out float z);
            if (error != (int)GPSError.None) return null;

            return new Vector3(x, y, z);
        }

        public static MapNodeInfo? GetMapNodeInfo(MapNode nodeId)
        {
            var pos = GetMapNodePosition(nodeId);
            if (pos == null) return null;

            return new MapNodeInfo(nodeId, pos.Value);
        }

        // ============================================
        // Connection Functions
        // ============================================

        public static Connection CreateConnection(MapNode source, MapNode target)
        {
            if (!source.IsValid || !target.IsValid) return Connection.Invalid;

            var error = GPSNatives.Instance.CreateConnection(source, target, out int connectionId);
            if (error != (int)GPSError.None)
            {
                Console.WriteLine($"[GPS] CreateConnection failed: {(GPSError)error}");
                return Connection.Invalid;
            }
            return new Connection(connectionId);
        }

        public static bool DestroyConnection(Connection connectionId)
        {
            if (!connectionId.IsValid) return false;
            var error = GPSNatives.Instance.DestroyConnection(connectionId);
            return error == (int)GPSError.None;
        }

        public static MapNode? GetConnectionSource(Connection connectionId)
        {
            if (!connectionId.IsValid) return null;

            var error = GPSNatives.Instance.GetConnectionSource(connectionId, out int nodeId);
            if (error != (int)GPSError.None) return null;

            return new MapNode(nodeId);
        }

        public static MapNode? GetConnectionTarget(Connection connectionId)
        {
            if (!connectionId.IsValid) return null;

            var error = GPSNatives.Instance.GetConnectionTarget(connectionId, out int nodeId);
            if (error != (int)GPSError.None) return null;

            return new MapNode(nodeId);
        }

        public static ConnectionInfo? GetConnectionInfo(Connection connectionId)
        {
            var source = GetConnectionSource(connectionId);
            var target = GetConnectionTarget(connectionId);

            if (source == null || target == null) return null;

            return new ConnectionInfo(connectionId, source.Value, target.Value);
        }

        public static int GetMapNodeConnectionCount(MapNode nodeId)
        {
            if (!nodeId.IsValid) return 0;

            var error = GPSNatives.Instance.GetMapNodeConnectionCount(nodeId, out int count);
            if (error != (int)GPSError.None) return 0;

            return count;
        }

        public static List<Connection> GetMapNodeConnections(MapNode nodeId)
        {
            var connections = new List<Connection>();
            if (!nodeId.IsValid) return connections;

            int count = GetMapNodeConnectionCount(nodeId);
            for (int i = 0; i < count; i++)
            {
                var error = GPSNatives.Instance.GetMapNodeConnection(nodeId, i, out int connectionId);
                if (error == (int)GPSError.None)
                {
                    connections.Add(new Connection(connectionId));
                }
            }

            return connections;
        }

        public static Connection GetConnectionBetweenNodes(MapNode source, MapNode target)
        {
            if (!source.IsValid || !target.IsValid) return Connection.Invalid;

            var error = GPSNatives.Instance.GetConnectionBetweenMapNodes(source, target, out int connectionId);
            if (error != (int)GPSError.None) return Connection.Invalid;

            return new Connection(connectionId);
        }

        // ============================================
        // Distance & Angle Functions
        // ============================================

        public static float GetDistanceBetweenNodes(MapNode first, MapNode second)
        {
            if (!first.IsValid || !second.IsValid) return 0f;

            var error = GPSNatives.Instance.GetDistanceBetweenMapNodes(first, second, out float distance);
            if (error != (int)GPSError.None) return 0f;

            return distance;
        }

        public static float GetAngleBetweenNodes(MapNode first, MapNode second)
        {
            if (!first.IsValid || !second.IsValid) return 0f;

            var error = GPSNatives.Instance.GetAngleBetweenMapNodes(first, second, out float angle);
            if (error != (int)GPSError.None) return 0f;

            return angle;
        }

        public static float GetNodeDistanceFromPoint(MapNode nodeId, Vector3 point)
        {
            if (!nodeId.IsValid) return 0f;

            var error = GPSNatives.Instance.GetMapNodeDistanceFromPoint(nodeId, point.X, point.Y, point.Z, out float distance);
            if (error != (int)GPSError.None) return 0f;

            return distance;
        }

        public static float GetNodeAngleFromPoint(MapNode nodeId, Vector2 point)
        {
            if (!nodeId.IsValid) return 0f;

            var error = GPSNatives.Instance.GetMapNodeAngleFromPoint(nodeId, point.X, point.Y, out float angle);
            if (error != (int)GPSError.None) return 0f;

            return angle;
        }

        public static MapNode GetClosestNodeToPoint(Vector3 point, MapNode ignoredNode = default)
        {
            int ignored = ignoredNode.IsValid ? ignoredNode.Value : -1;
            var error = GPSNatives.Instance.GetClosestMapNodeToPoint(point.X, point.Y, point.Z, out int nodeId, ignored);

            if (error != (int)GPSError.None) return MapNode.Invalid;
            return new MapNode(nodeId);
        }

        // ============================================
        // Utility Functions
        // ============================================

        public static int GetHighestMapNodeId()
        {
            return GPSNatives.Instance.GetHighestMapNodeID();
        }

        public static MapNode GetRandomMapNode()
        {
            var error = GPSNatives.Instance.GetRandomMapNode(out int nodeId);
            if (error != (int)GPSError.None) return MapNode.Invalid;
            return new MapNode(nodeId);
        }

        public static bool SaveMapNodesToFile(string filename)
        {
            var error = GPSNatives.Instance.SaveMapNodesToFile(filename);
            return error == (int)GPSError.None;
        }

        // ============================================
        // Path Finding Functions
        // ============================================

        public static Path FindPath(MapNode source, MapNode target)
        {
            if (!source.IsValid || !target.IsValid) return Path.Invalid;

            var error = GPSNatives.Instance.FindPath(source, target, out int pathId);
            if (error != (int)GPSError.None)
            {
                Console.WriteLine($"[GPS] FindPath failed: {(GPSError)error}");
                return Path.Invalid;
            }

            return new Path(pathId);
        }

        public static void FindPathThreaded(MapNode source, MapNode target, Action<Path> callback)
        {
            if (!source.IsValid || !target.IsValid)
            {
                callback?.Invoke(Path.Invalid);
                return;
            }

            int callbackId = _callbackCounter++;
            _pathCallbacks[callbackId] = callback;

            var error = GPSNatives.Instance.FindPathThreaded(
                source,
                target,
                "GPS_OnPathFound",
                "ii",
                callbackId,
                0 // placeholder for path result
            );

            if (error != (int)GPSError.None)
            {
                Console.WriteLine($"[GPS] FindPathThreaded failed: {(GPSError)error}");
                _pathCallbacks.Remove(callbackId);
                callback?.Invoke(Path.Invalid);
            }
        }

        // Internal callback handler (akan dipanggil dari PAWN)
        public static void OnPathFound(int callbackId, Path pathId)
        {
            if (_pathCallbacks.TryGetValue(callbackId, out var callback))
            {
                callback?.Invoke(pathId);
                _pathCallbacks.Remove(callbackId);
            }
        }

        // ============================================
        // Path Functions
        // ============================================

        public static bool IsValidPath(Path pathId)
        {
            return GPSNatives.Instance.IsValidPath(pathId);
        }

        public static int GetPathSize(Path pathId)
        {
            if (!pathId.IsValid) return 0;

            var error = GPSNatives.Instance.GetPathSize(pathId, out int size);
            if (error != (int)GPSError.None) return 0;

            return size;
        }

        public static float GetPathLength(Path pathId)
        {
            if (!pathId.IsValid) return 0f;

            var error = GPSNatives.Instance.GetPathLength(pathId, out float length);
            if (error != (int)GPSError.None) return 0f;

            return length;
        }

        public static MapNode GetPathNode(Path pathId, int index)
        {
            if (!pathId.IsValid) return MapNode.Invalid;

            var error = GPSNatives.Instance.GetPathNode(pathId, index, out int nodeId);
            if (error != (int)GPSError.None) return MapNode.Invalid;

            return new MapNode(nodeId);
        }

        public static int GetPathNodeIndex(Path pathId, MapNode nodeId)
        {
            if (!pathId.IsValid || !nodeId.IsValid) return -1;

            var error = GPSNatives.Instance.GetPathNodeIndex(pathId, nodeId, out int index);
            if (error != (int)GPSError.None) return -1;

            return index;
        }

        public static List<MapNode> GetPathNodes(Path pathId)
        {
            var nodes = new List<MapNode>();
            if (!pathId.IsValid) return nodes;

            int size = GetPathSize(pathId);
            for (int i = 0; i < size; i++)
            {
                var node = GetPathNode(pathId, i);
                if (node.IsValid)
                {
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        public static PathInfo? GetPathInfo(Path pathId)
        {
            if (!pathId.IsValid) return null;

            var pathInfo = new PathInfo(pathId)
            {
                Nodes = GetPathNodes(pathId),
                Length = GetPathLength(pathId)
            };

            return pathInfo;
        }

        public static bool DestroyPath(Path pathId)
        {
            if (!pathId.IsValid) return false;

            var error = GPSNatives.Instance.DestroyPath(pathId);
            return error == (int)GPSError.None;
        }

        // ============================================
        // Helper Functions
        // ============================================

        public static List<Vector3> GetPathPositions(Path pathId)
        {
            var positions = new List<Vector3>();
            if (!pathId.IsValid) return positions;

            var nodes = GetPathNodes(pathId);
            foreach (var node in nodes)
            {
                var pos = GetMapNodePosition(node);
                if (pos.HasValue)
                {
                    positions.Add(pos.Value);
                }
            }

            return positions;
        }

        public static void ClearAllCallbacks()
        {
            _pathCallbacks.Clear();
        }
    }
}