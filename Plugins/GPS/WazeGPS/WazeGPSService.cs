using SampSharp.GameMode;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.GPS.WazeGPS
{
    public static class WazeGPSService
    {
        private static readonly Dictionary<int, WazeGPSData> _playerData = new Dictionary<int, WazeGPSData>();

        public static void Initialize()
        {
            Console.WriteLine("[WazeGPS] Service initialized");
        }

        public static void Dispose()
        {
            foreach (var data in _playerData.Values)
            {
                data.Reset();
            }
            _playerData.Clear();
            Console.WriteLine("[WazeGPS] Service disposed");
        }

        internal static WazeGPSData GetOrCreateData(BasePlayer player)
        {
            if (!_playerData.ContainsKey(player.Id))
            {
                _playerData[player.Id] = new WazeGPSData();
            }
            return _playerData[player.Id];
        }

        public static void SetPlayerWaze(BasePlayer player, Vector3 destination, uint color = 0x8A44E4FF)
        {
            var data = GetOrCreateData(player);
            data.DestroyRoutes();

            data.Color = color;
            data.TargetPosition = destination;

            if (data.UpdateTimer == null)
            {
                data.UpdateTimer = new Timer(WazeGPSData.UpdateInterval, true);
                data.UpdateTimer.Tick += (sender, args) => UpdateWaze(player);
            }

            UpdateWaze(player);
        }

        public static void StopWazeGPS(BasePlayer player)
        {
            if (!_playerData.ContainsKey(player.Id)) return;

            var data = _playerData[player.Id];
            data.Reset();
        }

        public static bool IsValidWazeGPS(BasePlayer player)
        {
            return _playerData.ContainsKey(player.Id) && _playerData[player.Id].IsActive;
        }

        internal static void OnPlayerDisconnect(BasePlayer player)
        {
            if (_playerData.ContainsKey(player.Id))
            {
                _playerData[player.Id].Reset();
                _playerData.Remove(player.Id);
            }
        }

        private static void UpdateWaze(BasePlayer player)
        {
            if (!_playerData.ContainsKey(player.Id)) return;

            var data = _playerData[player.Id];

            if (player.Interior != 0)
            {
                StopWazeGPS(player);
                return;
            }

            if (Vector3.Distance(player.Position, data.TargetPosition) <= 30.0f)
            {
                StopWazeGPS(player);
                return;
            }

            data.LastTickPosition = player.Position;

            var startNode = GPSService.GetClosestNodeToPoint(data.LastTickPosition);
            var targetNode = GPSService.GetClosestNodeToPoint(data.TargetPosition);

            if (!startNode.IsValid || !targetNode.IsValid) return;

            GPSService.FindPathThreaded(startNode, targetNode, pathId => OnPathFound(player, pathId));
        }

        private static void OnPathFound(BasePlayer player, Path pathId)
        {
            if (player == null || !player.IsConnected) return;
            if (!_playerData.ContainsKey(player.Id)) return;

            var data = _playerData[player.Id];
            if (!data.IsActive) return;

            if (!pathId.IsValid) return;

            int size = GPSService.GetPathSize(pathId);
            if (size <= 1)
            {
                StopWazeGPS(player);
                return;
            }

            data.DestroyRoutes();

            var nodes = GPSService.GetPathNodes(pathId);
            var startPos = player.Position;

            int maxDots = Math.Min(WazeGPSData.MaxWazeDots, size);

            Vector3 prevPos = startPos;

            for (int i = 0; i < maxDots && i < nodes.Count; i++)
            {
                var nodePos = GPSService.GetMapNodePosition(nodes[i]);
                if (!nodePos.HasValue) continue;

                CreateWazePointer(player, data, prevPos, nodePos.Value);

                prevPos = new Vector3(nodePos.Value.X + 0.5f, nodePos.Value.Y + 0.5f, nodePos.Value.Z);
            }

            GPSService.DestroyPath(pathId);
        }

        private static void CreateWazePointer(BasePlayer player, WazeGPSData data, Vector3 start, Vector3 end)
        {
            const float dotSize = 12.5f;

            float distance = Vector3.Distance(new Vector3(start.X, start.Y, 0), new Vector3(end.X, end.Y, 0));
            int points = (int)Math.Round(distance / dotSize);

            for (int i = 1; i <= points; i++)
            {
                if (data.DotCount >= WazeGPSData.MaxWazeDots) return;

                float x = start.X + ((end.X - start.X) / points) * i;
                float y = start.Y + ((end.Y - start.Y) / points) * i;

                float halfSize = dotSize / 2;

                var zone = new GangZone(
                    x - halfSize - 5,
                    y - halfSize - 5,
                    x + halfSize + 5,
                    y + halfSize + 5
                );

                zone.Color = Color.FromInteger((int)data.Color, ColorFormat.ARGB);
                zone.Show(player);

                data.RouteZones.Add(zone);
                data.DotCount++;
            }
        }
    }
}