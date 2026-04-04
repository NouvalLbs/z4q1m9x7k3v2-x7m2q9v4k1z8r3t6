using SampSharp.Core.Natives.NativeObjects;
using System;

namespace ProjectSMP.Plugins.GPS
{
    public class GPSNatives : NativeObjectSingleton<GPSNatives>
    {
        // MapNode functions
        [NativeMethod]
        public virtual int CreateMapNode(float x, float y, float z, out int nodeid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int DestroyMapNode(int nodeid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual bool IsValidMapNode(int nodeid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetMapNodePos(int nodeid, out float x, out float y, out float z)
        {
            throw new NotImplementedException();
        }

        // Connection functions
        [NativeMethod]
        public virtual int CreateConnection(int source, int target, out int connectionid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int DestroyConnection(int connectionid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetConnectionSource(int connectionid, out int nodeid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetConnectionTarget(int connectionid, out int nodeid)
        {
            throw new NotImplementedException();
        }

        // MapNode connection queries
        [NativeMethod]
        public virtual int GetMapNodeConnectionCount(int nodeid, out int count)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetMapNodeConnection(int nodeid, int index, out int connectionid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetConnectionBetweenMapNodes(int source, int target, out int connectionid)
        {
            throw new NotImplementedException();
        }

        // Distance and angle functions
        [NativeMethod]
        public virtual int GetDistanceBetweenMapNodes(int first, int second, out float distance)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetAngleBetweenMapNodes(int first, int second, out float angle)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetMapNodeDistanceFromPoint(int nodeid, float x, float y, float z, out float distance)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetMapNodeAngleFromPoint(int nodeid, float x, float y, out float angle)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetClosestMapNodeToPoint(float x, float y, float z, out int nodeid, int ignorednode = -1)
        {
            throw new NotImplementedException();
        }

        // Utility functions
        [NativeMethod]
        public virtual int GetHighestMapNodeID()
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetRandomMapNode(out int nodeid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int SaveMapNodesToFile(string filename)
        {
            throw new NotImplementedException();
        }

        // Path finding functions
        [NativeMethod]
        public virtual int FindPath(int source, int target, out int pathid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int FindPathThreaded(int source, int target, string callback, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        // Path functions
        [NativeMethod]
        public virtual bool IsValidPath(int pathid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetPathSize(int pathid, out int size)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetPathLength(int pathid, out float length)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetPathNode(int pathid, int index, out int nodeid)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int GetPathNodeIndex(int pathid, int nodeid, out int index)
        {
            throw new NotImplementedException();
        }

        [NativeMethod]
        public virtual int DestroyPath(int pathid)
        {
            throw new NotImplementedException();
        }
    }
}