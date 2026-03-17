using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class JailService
    {
        private const int MaxJailTime = 3600;
        private static readonly Vector3 JailPosition = new(265.0534f, 77.6823f, 1001.0391f);
        private static readonly HashSet<int> JailedPlayers = new();
        private static Timer UpdateTimer;

        public static void Initialize()
        {
            UpdateTimer = new Timer(1000, true);
            UpdateTimer.Tick += OnUpdate;
        }

        public static void Dispose()
        {
            UpdateTimer?.Dispose();
        }

        public static bool JailPlayer(Player target, int seconds, string reason)
        {
            if (seconds < 1 || seconds > MaxJailTime) return false;

            var pos = target.Position;
            target.SavedPosX = pos.X;
            target.SavedPosY = pos.Y;
            target.SavedPosZ = pos.Z;
            target.SavedInterior = target.Interior;
            target.SavedWorld = target.VirtualWorld;

            target.JailTime = seconds;
            target.JailTimestamp = Environment.TickCount / 1000;
            target.JailReason = reason;

            if (target.IsInAnyVehicle)
                target.RemoveFromVehicle();

            if (target.Cuffed)
            {
                target.Cuffed = false;
                target.SetSpecialAction(SpecialAction.None);
            }

            target.SetInteriorSafe(6);
            target.SetVirtualWorldSafe(target.Id);
            target.SetPositionSafe(JailPosition);
            target.Angle = 270.0f;
            target.SetCameraBehindPlayer();

            target.ToggleControllableSafe(false);
            var timer = new Timer(2000, false);
            timer.Tick += (s, e) => { target.ToggleControllableSafe(true); timer.Dispose(); };

            CreateJailTextDraw(target);
            UpdateJailTextDraw(target, seconds);
            JailedPlayers.Add(target.Id);

            return true;
        }

        public static void UnjailPlayer(Player target)
        {
            target.JailTime = 0;
            target.JailTimestamp = 0;
            target.JailReason = "";

            if (target.JailTextDraw != null)
            {
                target.JailTextDraw.Hide();
                target.JailTextDraw.Dispose();
                target.JailTextDraw = null;
            }

            target.SetInteriorSafe(target.SavedInterior);
            target.SetVirtualWorldSafe(target.SavedWorld);
            target.SetPositionSafe(target.SavedPosX, target.SavedPosY, target.SavedPosZ);
            target.SetCameraBehindPlayer();

            JailedPlayers.Remove(target.Id);
        }

        public static void OnPlayerSpawn(Player player)
        {
            if (player.JailTime > 0)
            {
                player.JailTimestamp = Environment.TickCount / 1000;
                player.SetInteriorSafe(6);
                player.SetVirtualWorldSafe(player.Id);
                player.SetPositionSafe(JailPosition);
                player.Angle = 270.0f;
                player.SetCameraBehindPlayer();

                CreateJailTextDraw(player);
                UpdateJailTextDraw(player, player.JailTime);

                player.SendClientMessage(Color.White,
                    $"{{992712}}Kamu masih memiliki waktu jail tersisa sebanyak {player.JailTime} detik.");
                player.SendClientMessage(Color.White, $"{{992712}}Alasan: {player.JailReason}");
            }
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            foreach (var player in BasePlayer.All.OfType<Player>())
            {
                if (!player.IsCharLoaded || player.JailTime <= 0) continue;

                var currentTime = Environment.TickCount / 1000;
                var remaining = player.JailTime - (currentTime - player.JailTimestamp);

                if (remaining <= 0)
                {
                    UnjailPlayer(player);
                    player.SendClientMessage(Color.White, "{992712}Waktu jail kamu telah berakhir, kamu telah dibebaskan.");
                }
                else
                {
                    UpdateJailTextDraw(player, remaining);

                    var pos = player.Position;
                    if (player.Interior != 6 || player.VirtualWorld != player.Id ||
                        pos.DistanceTo(JailPosition) > 15.0f)
                    {
                        player.SetInteriorSafe(6);
                        player.SetVirtualWorldSafe(player.Id);
                        player.SetPositionSafe(JailPosition);
                        player.Angle = 270.0f;
                        player.SendClientMessage(Color.White, "{992712}Jangan coba-coba untuk keluar dari jail!");
                    }
                }
            }
        }

        private static void CreateJailTextDraw(Player player)
        {
            if (player.JailTextDraw != null)
                player.JailTextDraw.Dispose();

            player.JailTextDraw = new PlayerTextDraw(player, new Vector2(320.0f, 380.0f), "Waktu Jail: ~r~0 ~w~detik")
            {
                Alignment = TextDrawAlignment.Center,
                BackColor = new Color(0, 0, 0, 170),
                Font = TextDrawFont.Slim,
                LetterSize = new Vector2(0.5f, 1.5f),
                ForeColor = Color.White,
                Outline = 1,
                Proportional = true,
                Shadow = 1,
                UseBox = true,
                BoxColor = new Color(0, 0, 0, 136),
                Width = 160.0f
            };
        }

        private static void UpdateJailTextDraw(Player player, int seconds)
        {
            if (player.JailTextDraw == null) return;
            player.JailTextDraw.Text = $"Waktu Jail: ~r~{seconds} ~w~detik";
            player.JailTextDraw.Show();
        }

        public static void Cleanup(Player player)
        {
            JailedPlayers.Remove(player.Id);
            if (player.JailTextDraw != null)
            {
                player.JailTextDraw.Dispose();
                player.JailTextDraw = null;
            }
        }
    }
}