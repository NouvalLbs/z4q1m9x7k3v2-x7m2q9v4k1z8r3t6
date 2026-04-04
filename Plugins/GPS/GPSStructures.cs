using SampSharp.GameMode;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.GPS
{
    public class MapNodeInfo
    {
        public MapNode NodeId { get; set; }
        public Vector3 Position { get; set; }

        public MapNodeInfo(MapNode nodeId, Vector3 position)
        {
            NodeId = nodeId;
            Position = position;
        }
    }

    public class ConnectionInfo
    {
        public Connection ConnectionId { get; set; }
        public MapNode Source { get; set; }
        public MapNode Target { get; set; }

        public ConnectionInfo(Connection connectionId, MapNode source, MapNode target)
        {
            ConnectionId = connectionId;
            Source = source;
            Target = target;
        }
    }

    public class PathInfo
    {
        public Path PathId { get; set; }
        public List<MapNode> Nodes { get; set; }
        public float Length { get; set; }

        public PathInfo(Path pathId)
        {
            PathId = pathId;
            Nodes = new List<MapNode>();
            Length = 0f;
        }

        public int Size => Nodes.Count;
    }

    public class RouteProgressEventArgs : EventArgs
    {
        public Path PathId { get; set; }
        public int CurrentNodeIndex { get; set; }
        public MapNode CurrentNode { get; set; }
        public MapNode NextNode { get; set; }
        public float DistanceToNext { get; set; }
        public bool IsLastNode { get; set; }

        public RouteProgressEventArgs(Path pathId, int currentIndex, MapNode current, MapNode next, float distance, bool isLast)
        {
            PathId = pathId;
            CurrentNodeIndex = currentIndex;
            CurrentNode = current;
            NextNode = next;
            DistanceToNext = distance;
            IsLastNode = isLast;
        }
    }
}