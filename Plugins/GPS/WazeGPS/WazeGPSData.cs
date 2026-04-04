using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.GPS.WazeGPS
{
    internal class WazeGPSData
    {
        public const int MaxWazeDots = 100;
        public const int UpdateInterval = 1000;

        public Timer UpdateTimer { get; set; }
        public uint Color { get; set; }
        public int DotCount { get; set; }
        public List<GangZone> RouteZones { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Vector3 LastTickPosition { get; set; }

        public WazeGPSData()
        {
            RouteZones = new List<GangZone>();
            Color = 0x8A44E4FF;
            Reset();
        }

        public void Reset()
        {
            UpdateTimer?.Dispose();
            UpdateTimer = null;
            DestroyRoutes();
            DotCount = 0;
            TargetPosition = Vector3.Zero;
            LastTickPosition = Vector3.Zero;
        }

        public void DestroyRoutes()
        {
            foreach (var zone in RouteZones)
            {
                zone?.Dispose();
            }
            RouteZones.Clear();
            DotCount = 0;
        }

        public bool IsActive => UpdateTimer != null;
    }
}