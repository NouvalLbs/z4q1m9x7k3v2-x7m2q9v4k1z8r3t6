#nullable enable
using System.Collections.Generic;
using ProjectSMP.Entities;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;

namespace ProjectSMP.Features.Jobs.Core
{
    public static class SideJobVehicleManager
    {
        private static readonly Dictionary<int, Timer> _pending = new();

        public static void ScheduleRespawn(Vehicle vehicle, int delayMs = 3000)
        {
            if (vehicle == null || vehicle.IsDisposed) return;

            var vid = vehicle.Id;
            Cancel(vid);

            var t = new Timer(delayMs, false);
            t.Tick += (s, e) =>
            {
                t.Dispose();
                _pending.Remove(vid);

                var v = BaseVehicle.Find(vid) as Vehicle;
                if (v == null || v.IsDisposed) return;

                if (IsOccupied(vid))
                {
                    ScheduleRespawn(v, delayMs);
                    return;
                }

                v.RespawnAtSpawnPoint();
            };

            _pending[vid] = t;
        }

        public static void EjectAndScheduleRespawn(Player player, Vehicle vehicle, int delayMs = 3000)
        {
            if (player != null && !player.IsDisposed && player.IsConnected)
                player.RemoveFromVehicle();

            ScheduleRespawn(vehicle, delayMs);
        }

        public static bool IsPendingRespawn(int vehicleId) => _pending.ContainsKey(vehicleId);

        private static void Cancel(int vid)
        {
            if (!_pending.TryGetValue(vid, out var t)) return;
            t.Dispose();
            _pending.Remove(vid);
        }

        private static bool IsOccupied(int vid)
        {
            foreach (var p in BasePlayer.All)
                if (p.IsConnected && p.Vehicle?.Id == vid) return true;
            return false;
        }

        public static void Dispose()
        {
            foreach (var t in _pending.Values) t.Dispose();
            _pending.Clear();
        }
    }
}