using SampSharp.GameMode;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.GPS
{
    public static class GpsService
    {
        private static GpsNatives Native => GpsNatives.Instance;

        public static (GpsError Error, int NodeId) CreateNode(Vector3 position)
        {
            var error = (GpsError)Native.CreateMapNode(position.X, position.Y, position.Z, out int nodeId);
            return (error, nodeId);
        }

        public static (GpsError Error, int NodeId) CreateNode(float x, float y, float z)
        {
            var error = (GpsError)Native.CreateMapNode(x, y, z, out int nodeId);
            return (error, nodeId);
        }

        public static GpsError DestroyNode(int nodeId)
        {
            return (GpsError)Native.DestroyMapNode(nodeId);
        }

        public static bool IsValidNode(int nodeId)
        {
            return Native.IsValidMapNode(nodeId);
        }

        public static (GpsError Error, Vector3 Position) GetNodePosition(int nodeId)
        {
            var error = (GpsError)Native.GetMapNodePos(nodeId, out float x, out float y, out float z);
            return (error, new Vector3(x, y, z));
        }

        public static (GpsError Error, int ConnectionId) CreateConnection(int sourceNode, int targetNode)
        {
            var error = (GpsError)Native.CreateConnection(sourceNode, targetNode, out int connectionId);
            return (error, connectionId);
        }

        public static GpsError DestroyConnection(int connectionId)
        {
            return (GpsError)Native.DestroyConnection(connectionId);
        }

        public static (GpsError Error, int NodeId) GetConnectionSource(int connectionId)
        {
            var error = (GpsError)Native.GetConnectionSource(connectionId, out int nodeId);
            return (error, nodeId);
        }

        public static (GpsError Error, int NodeId) GetConnectionTarget(int connectionId)
        {
            var error = (GpsError)Native.GetConnectionTarget(connectionId, out int nodeId);
            return (error, nodeId);
        }

        public static (GpsError Error, int Count) GetNodeConnectionCount(int nodeId)
        {
            var error = (GpsError)Native.GetMapNodeConnectionCount(nodeId, out int count);
            return (error, count);
        }

        public static (GpsError Error, List<int> Connections) GetNodeConnections(int nodeId)
        {
            var countResult = GetNodeConnectionCount(nodeId);
            if (countResult.Error != GpsError.None || countResult.Count <= 0)
                return (countResult.Error, new List<int>());

            var connections = new List<int>();
            for (int i = 0; i < countResult.Count; i++)
            {
                var error = (GpsError)Native.GetMapNodeConnection(nodeId, i, out int connectionId);
                if (error == GpsError.None)
                    connections.Add(connectionId);
            }

            return (GpsError.None, connections);
        }

        public static (GpsError Error, int ConnectionId) GetConnectionBetweenNodes(int sourceNode, int targetNode)
        {
            var error = (GpsError)Native.GetConnectionBetweenMapNodes(sourceNode, targetNode, out int connectionId);
            return (error, connectionId);
        }

        public static (GpsError Error, float Distance) GetDistanceBetweenNodes(int firstNode, int secondNode)
        {
            var error = (GpsError)Native.GetDistanceBetweenMapNodes(firstNode, secondNode, out float distance);
            return (error, distance);
        }

        public static (GpsError Error, float Angle) GetAngleBetweenNodes(int firstNode, int secondNode)
        {
            var error = (GpsError)Native.GetAngleBetweenMapNodes(firstNode, secondNode, out float angle);
            return (error, angle);
        }

        public static (GpsError Error, float Distance) GetNodeDistanceFromPoint(int nodeId, Vector3 point)
        {
            var error = (GpsError)Native.GetMapNodeDistanceFromPoint(nodeId, point.X, point.Y, point.Z, out float distance);
            return (error, distance);
        }

        public static (GpsError Error, float Angle) GetNodeAngleFromPoint(int nodeId, float x, float y)
        {
            var error = (GpsError)Native.GetMapNodeAngleFromPoint(nodeId, x, y, out float angle);
            return (error, angle);
        }

        public static (GpsError Error, int NodeId) GetClosestNodeToPoint(Vector3 point, int ignoredNode = -1)
        {
            var error = (GpsError)Native.GetClosestMapNodeToPoint(point.X, point.Y, point.Z, out int nodeId, ignoredNode);
            return (error, nodeId);
        }

        public static int GetHighestNodeId()
        {
            return Native.GetHighestMapNodeID();
        }

        public static (GpsError Error, int NodeId) GetRandomNode()
        {
            var error = (GpsError)Native.GetRandomMapNode(out int nodeId);
            return (error, nodeId);
        }

        public static GpsError SaveNodesToFile(string filename)
        {
            return (GpsError)Native.SaveMapNodesToFile(filename);
        }

        public static (GpsError Error, int PathId) FindPath(int sourceNode, int targetNode)
        {
            var error = (GpsError)Native.FindPath(sourceNode, targetNode, out int pathId);
            return (error, pathId);
        }

        public static bool IsValidPath(int pathId)
        {
            return Native.IsValidPath(pathId);
        }

        public static (GpsError Error, int Size) GetPathSize(int pathId)
        {
            var error = (GpsError)Native.GetPathSize(pathId, out int size);
            return (error, size);
        }

        public static (GpsError Error, float Length) GetPathLength(int pathId)
        {
            var error = (GpsError)Native.GetPathLength(pathId, out float length);
            return (error, length);
        }

        public static (GpsError Error, int NodeId) GetPathNode(int pathId, int index)
        {
            var error = (GpsError)Native.GetPathNode(pathId, index, out int nodeId);
            return (error, nodeId);
        }

        public static (GpsError Error, List<int> Nodes) GetPathNodes(int pathId)
        {
            var sizeResult = GetPathSize(pathId);
            if (sizeResult.Error != GpsError.None || sizeResult.Size <= 0)
                return (sizeResult.Error, new List<int>());

            var nodes = new List<int>();
            for (int i = 0; i < sizeResult.Size; i++)
            {
                var result = GetPathNode(pathId, i);
                if (result.Error == GpsError.None)
                    nodes.Add(result.NodeId);
            }

            return (GpsError.None, nodes);
        }

        public static (GpsError Error, int Index) GetPathNodeIndex(int pathId, int nodeId)
        {
            var error = (GpsError)Native.GetPathNodeIndex(pathId, nodeId, out int index);
            return (error, index);
        }

        public static GpsError DestroyPath(int pathId)
        {
            return (GpsError)Native.DestroyPath(pathId);
        }
    }
}