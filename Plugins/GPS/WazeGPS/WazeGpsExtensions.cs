using SampSharp.GameMode;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.GPS.WazeGPS
{
    public static class WazeGpsExtensions
    {
        public static void SetWazeDestination(this BasePlayer player, Vector3 destination, uint color = 0x8A44E4FF)
        {
            WazeGpsService.SetPlayerWaze(player, destination, color);
        }

        public static void SetWazeDestination(this BasePlayer player, float x, float y, float z, uint color = 0x8A44E4FF)
        {
            WazeGpsService.SetPlayerWaze(player, x, y, z, color);
        }

        public static void StopWaze(this BasePlayer player)
        {
            WazeGpsService.StopWazeGPS(player);
        }

        public static bool HasActiveWaze(this BasePlayer player)
        {
            return WazeGpsService.IsValidWazeGPS(player);
        }
    }
}