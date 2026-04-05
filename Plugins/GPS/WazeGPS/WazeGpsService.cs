using ProjectSMP.Plugins.GPS.ZoneGPS;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.GPS.WazeGPS
{
    public static class WazeGpsService
    {
        private const int MaxWazeDots = 100;
        private const int WazeUpdateTime = 1000;
        private const float ArrivalRange = 30.0f;
        private const float DotDistance = 12.5f;

        private static readonly Dictionary<int, WazeGpsData> _playerData = new();
        private static readonly Dictionary<int, Timer> _timers = new();

        public static bool SetPlayerWaze(BasePlayer player, Vector3 destination, uint color = 0x8A44E4FF)
        {
            return SetPlayerWaze(player, destination.X, destination.Y, destination.Z, color);
        }

        public static bool SetPlayerWaze(BasePlayer player, float x, float y, float z, uint color = 0x8A44E4FF)
        {
            if (!_playerData.ContainsKey(player.Id))
                _playerData[player.Id] = new WazeGpsData();

            var data = _playerData[player.Id];

            DestroyWazeRoutes(player.Id);

            data.Color = color;
            data.TargetPosition = new Vector3(x, y, z);

            if (data.TimerId == 0)
            {
                var timer = new Timer(WazeUpdateTime, true);
                timer.Tick += (sender, args) => UpdateWaze(player.Id);
                _timers[player.Id] = timer;
                data.TimerId = 1;
            }

            UpdateWaze(player.Id);
            return true;
        }

        public static bool StopWazeGPS(BasePlayer player)
        {
            if (!_playerData.ContainsKey(player.Id))
                return false;

            if (_timers.TryGetValue(player.Id, out var timer))
            {
                timer.Dispose();
                _timers.Remove(player.Id);
            }

            _playerData[player.Id].TimerId = 0;
            DestroyWazeRoutes(player.Id);
            return true;
        }

        public static bool IsValidWazeGPS(BasePlayer player)
        {
            return _playerData.ContainsKey(player.Id) && _playerData[player.Id].TimerId > 0;
        }

        public static void OnPlayerDisconnect(BasePlayer player)
        {
            if (_timers.TryGetValue(player.Id, out var timer))
            {
                timer.Dispose();
                _timers.Remove(player.Id);
            }

            if (_playerData.ContainsKey(player.Id))
            {
                DestroyWazeRoutes(player.Id);
                _playerData.Remove(player.Id);
            }

            ZoneGpsService.OnPlayerDisconnect(player);
        }

        private static void UpdateWaze(int playerId)
        {
            var player = BasePlayer.Find(playerId);
            if (player == null || !_playerData.ContainsKey(playerId))
            {
                if (player != null) StopWazeGPS(player);
                return;
            }

            var data = _playerData[playerId];

            if (player.Interior != 0)
            {
                StopWazeGPS(player);
                return;
            }

            var currentPos = player.Position;
            var distance = Vector3.Distance(currentPos, data.TargetPosition);

            if (distance <= ArrivalRange)
            {
                StopWazeGPS(player);
                return;
            }

            data.CurrentPosition = currentPos;

            var startResult = GpsService.GetClosestNodeToPoint(currentPos);
            if (startResult.Error != GpsError.None)
                return;

            var targetResult = GpsService.GetClosestNodeToPoint(data.TargetPosition);
            if (targetResult.Error != GpsError.None)
                return;

            var pathResult = GpsService.FindPath(startResult.NodeId, targetResult.NodeId);
            if (pathResult.Error != GpsError.None)
                return;

            ProcessWazeRoute(playerId, pathResult.PathId);
            GpsService.DestroyPath(pathResult.PathId);
        }

        private static void ProcessWazeRoute(int playerId, int pathId)
        {
            var player = BasePlayer.Find(playerId);
            if (player == null || !_playerData.ContainsKey(playerId))
                return;

            if (!GpsService.IsValidPath(pathId))
                return;

            var data = _playerData[playerId];
            if (data.TimerId == 0)
                return;

            var sizeResult = GpsService.GetPathSize(pathId);
            if (sizeResult.Error != GpsError.None || sizeResult.Size <= 1)
            {
                StopWazeGPS(player);
                return;
            }

            DestroyWazeRoutes(playerId);

            var currentPos = player.Position;
            var closestNodeResult = GpsService.GetClosestNodeToPoint(currentPos);
            if (closestNodeResult.Error != GpsError.None)
                return;

            var nodePosResult = GpsService.GetNodePosition(closestNodeResult.NodeId);
            if (nodePosResult.Error != GpsError.None)
                return;

            var x = nodePosResult.Position.X;
            var y = nodePosResult.Position.Y;

            var maxSize = Math.Min(MaxWazeDots, sizeResult.Size);

            for (int i = 0; i < maxSize; i++)
            {
                var nodeResult = GpsService.GetPathNode(pathId, i);
                if (nodeResult.Error != GpsError.None)
                    continue;

                var nodePos = GpsService.GetNodePosition(nodeResult.NodeId);
                if (nodePos.Error != GpsError.None)
                    continue;

                var indexResult = GpsService.GetPathNodeIndex(pathId, nodeResult.NodeId);
                if (indexResult.Error != GpsError.None)
                    continue;

                if (i == indexResult.Index)
                {
                    if (!CreateWazePointer(playerId, x, y, nodePos.Position.X, nodePos.Position.Y, data.Color))
                        break;
                }

                x = nodePos.Position.X + 0.5f;
                y = nodePos.Position.Y + 0.5f;
            }
        }

        private static bool CreateWazePointer(int playerId, float x1, float y1, float x2, float y2, uint color)
        {
            var player = BasePlayer.Find(playerId);
            if (player == null || !_playerData.ContainsKey(playerId))
                return false;

            var data = _playerData[playerId];
            var distance = CalculateDistance(x1, y1, 0, x2, y2, 0);
            var points = (int)(distance / DotDistance);

            for (int i = 1; i <= points; i++)
            {
                if (data.RouteCount >= MaxWazeDots)
                    return false;

                var x = x1 + (((x2 - x1) / points) * i);
                var y = y1 + (((y2 - y1) / points) * i);

                var halfDot = DotDistance / 2;
                var zoneId = ZoneGpsService.Create(
                    player,
                    x - halfDot - 5,
                    y - halfDot - 5,
                    x + halfDot + 5,
                    y + halfDot + 5
                );

                if (zoneId == -1)
                    return false;

                ZoneGpsService.Show(player, zoneId, color);
                data.ZoneIds.Add(zoneId);
                data.RouteCount++;
            }

            return true;
        }

        private static void DestroyWazeRoutes(int playerId)
        {
            var player = BasePlayer.Find(playerId);
            if (player == null || !_playerData.ContainsKey(playerId))
                return;

            var data = _playerData[playerId];

            foreach (var zoneId in data.ZoneIds)
                ZoneGpsService.Destroy(player, zoneId);

            data.ZoneIds.Clear();
            data.Routes.Clear();
            data.CreatedRoutes.Clear();
            data.RouteCount = 0;
        }

        private static float CalculateDistance(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var dz = z2 - z1;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static void Dispose()
        {
            foreach (var timer in _timers.Values)
                timer.Dispose();

            _timers.Clear();
            _playerData.Clear();
            ZoneGpsService.Dispose();
        }
    }
}