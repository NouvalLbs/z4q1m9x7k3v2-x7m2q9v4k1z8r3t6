#nullable enable
using System;
using ProjectSMP.Core;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Entities.Vehicles.Engine
{
    public static class EngineService
    {
        private static readonly Random _rng = new();
        public static void OnPlayerStateChanged(Player player, PlayerState newState)
        {
            if (newState != PlayerState.Driving) return;
            var vehicle = player.Vehicle as Vehicle;
            if (vehicle == null) return;
            if (!vehicle.GetEngineState())
                player.SendClientMessage(Color.White,
                    $"{Msg.Vehicles} Mesin masih mati, ketik {{FFFF00}}/engine{{FFFFFF}} atau {{FFFF00}}[Y]{{FFFFFF}} untuk menghidupkannya.");
        }

        public static void ToggleEngine(Player player)
        {
            if (player.State != PlayerState.Driving) return;

            var vehicle = player.Vehicle as Vehicle;
            if (vehicle == null) return;

            bool newState = !vehicle.GetEngineState();
            if (newState)
            {
                var name = Utilities.ReturnName(player);
                player.SendClientMessage(Color.White, $"{{D0AEEB}}** {name} mencoba menghidupkan mesin kendaraan.");

                var delay = _rng.Next(2000, 4001);
                var t = new Timer(delay, false);
                t.Tick += (s, e) =>
                {
                    t.Dispose();
                    if (player.IsDisposed || player.State != PlayerState.Driving) return;
                    vehicle.ToggleEngine(true);
                };
            }
            else
            {
                vehicle.ToggleEngine(false);
            }
        }
    }
}