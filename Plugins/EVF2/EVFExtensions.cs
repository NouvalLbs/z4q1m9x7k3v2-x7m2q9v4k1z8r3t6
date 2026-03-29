#nullable enable
using System.Linq;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.EVF2
{
    public static class EVFExtensions
    {
        public const int DoorDriver = 1;
        public const int DoorPassenger = 2;
        public const int DoorBackLeft = 3;
        public const int DoorBackRight = 4;

        public static bool GetWindowState(int vehicleId, int doorId)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return false;
            v.GetWindowsParameters(out bool d, out bool p, out bool bl, out bool br);
            return doorId switch { DoorDriver => d, DoorPassenger => p, DoorBackLeft => bl, DoorBackRight => br, _ => false };
        }

        public static bool SetWindowState(int vehicleId, int doorId, bool state)
        {
            if (!EVFService.IsValidVehicleDoor(vehicleId, doorId)) return false;
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return false;
            v.GetWindowsParameters(out bool d, out bool p, out bool bl, out bool br);
            switch (doorId)
            {
                case DoorDriver: d = state; break;
                case DoorPassenger: p = state; break;
                case DoorBackLeft: bl = state; break;
                case DoorBackRight: br = state; break;
            }
            v.SetWindowsParameters(d, p, bl, br);
            return true;
        }

        public static bool GetCarDoorState(int vehicleId, int doorId)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return false;
            v.GetDoorsParameters(out bool d, out bool p, out bool bl, out bool br);
            return doorId switch { DoorDriver => d, DoorPassenger => p, DoorBackLeft => bl, DoorBackRight => br, _ => false };
        }

        public static bool SetCarDoorState(int vehicleId, int doorId, bool state)
        {
            if (!EVFService.IsValidVehicleDoor(vehicleId, doorId)) return false;
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return false;
            v.GetDoorsParameters(out bool d, out bool p, out bool bl, out bool br);
            switch (doorId)
            {
                case DoorDriver: d = state; break;
                case DoorPassenger: p = state; break;
                case DoorBackLeft: bl = state; break;
                case DoorBackRight: br = state; break;
            }
            v.SetDoorsParameters(d, p, bl, br);
            return true;
        }

        public static bool IsVehicleSeatOccupied(int vehicleId, int seatId)
            => BasePlayer.All.Any(p => p.Vehicle?.Id == vehicleId && p.VehicleSeat == seatId);

        public static int GetVehicleNextSeat(int vehicleId, int startSeat = 1)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return -1;
            int seats = EVFService.GetModelSeats(v.Model);
            for (int i = startSeat; i < seats; i++)
                if (!IsVehicleSeatOccupied(vehicleId, i)) return i;
            return -1;
        }

        public static int GetVehiclePassenger(int vehicleId)
        {
            var found = BasePlayer.All.FirstOrDefault(pl =>
                pl.State == PlayerState.Passenger && pl.Vehicle?.Id == vehicleId);
            return found?.Id ?? -1;
        }

        public static void ImportVehicle(int vehicleId, Vector3 pos, float angle,
            int color1, int color2, int worldId, int interiorId, bool unoccupiedDamage)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return;
            EVFService.RegisterVehicle(vehicleId, v.Model, pos, angle, color1, color2, worldId, interiorId, unoccupiedDamage);
        }

        public static void HandleHorn(BasePlayer player)
        {
            if (player.State != PlayerState.Driving) return;
            int vId = player.Vehicle?.Id ?? -1;
            if (vId < 0) return;
            int horn = EVFService.GetHorn(vId);
            if (horn == 0) return;
            var pos = player.Position;
            foreach (var p in BasePlayer.All)
                if (p.Position.DistanceTo(pos) <= 35f)
                    p.PlaySound(horn, pos);
        }
    }
}