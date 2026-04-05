using SampSharp.GameMode;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.GPS.WazeGPS
{
    internal class WazeGpsData
    {
        public int TimerId { get; set; }
        public uint Color { get; set; } = 0x8A44E4FF;
        public Vector3 TargetPosition { get; set; }
        public Vector3 CurrentPosition { get; set; }
        public int RouteCount { get; set; }
        public Dictionary<int, int> Routes { get; set; } = new();
        public HashSet<int> CreatedRoutes { get; set; } = new();
        public List<int> ZoneIds { get; set; } = new();

        public void Reset()
        {
            TimerId = 0;
            Color = 0x8A44E4FF;
            TargetPosition = Vector3.Zero;
            CurrentPosition = Vector3.Zero;
            RouteCount = 0;
            Routes.Clear();
            CreatedRoutes.Clear();
        }
    }
}