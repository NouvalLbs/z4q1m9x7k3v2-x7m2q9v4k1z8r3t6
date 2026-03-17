using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator
{
    public class AdminCommands
    {
        private static bool CheckAdmin(Player player, int level)
        {
            if (player.Admin < level)
            {
                player.SendClientMessage(Color.White, "{b9b9b9} Command tidak ada, gunakan '/help'.");
                return false;
            }
            if (!player.AdminOnDuty)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Command tidak dapat digunakan ketika kamu tidak duty.");
                return false;
            }
            return true;
        }

        [Command("aduty")]
        public static void ADuty(Player player)
        {
            if (player.Admin < 1)
            {
                player.SendClientMessage(Color.White, "{b9b9b9} Command '/aduty' tidak ada, gunakan '/help'.");
                return;
            }

            if (!player.AdminOnDuty)
            {
                player.Color = new Color(255, 0, 0, 0);
                player.AdminOnDuty = true;
                player.Name = player.Ucp;
                Utilities.SendStaffMessage(-1, "{FF6347}{0}{FFFFFF} telah on duty admin dengan nama {1}", 
                    player.Username, player.Ucp);
            }
            else
            {
                player.Color = Color.White;
                player.AdminOnDuty = false;
                player.Name = player.Username;
                Utilities.SendStaffMessage(-1, "{FF6347}{0}{FFFFFF} telah off duty admin.", player.Ucp);
            }
        }

        [Command("staff")]
        public static void Staff(Player player, string text)
        {
            if (player.Admin < 1)
            {
                player.SendClientMessage(Color.White, "{b9b9b9} Command '/staff' tidak ada, gunakan '/help'.");
                return;
            }

            var adminTier = Utilities.GetAdminStringChat(player);
            foreach (var p in BasePlayer.All.OfType<Player>().Where(p => p.Admin > 0))
            {
                p.SendClientMessage(0x15D4EDFF, $"{adminTier} {player.Ucp}(Id:{player.Id}): {text}");
            }
        }

        [Command("asay")]
        public static void ASay(Player player, string text)
        {
            if (!CheckAdmin(player, 1)) return;

            var adminTier = Utilities.GetAdminStringChat(player);
            BasePlayer.SendClientMessageToAll(0xFF6347AA, 
                $"(( {{FFFF00}}{adminTier} {player.Ucp}: {{00FF00}}{Utilities.ColouredText(text)} {{FF6347}}))");
        }

        [Command("coords")]
        public static void Coords(Player player, string name, int devMode = 0)
        {
            if (!CheckAdmin(player, 1)) return;

            var pos = player.Position;
            var coords = $"{name} | {pos.X}, {pos.Y}, {pos.Z}, {player.Angle}";

            if (devMode == 1)
            {
                player.SendClientMessage(Color.White, 
                    $"{{FF6347}}<AdmCmd>{{FFFFFF}} {pos.X}, {pos.Y}, {pos.Z}, {player.Angle} disalin ke console dengan nama {name}");
                System.Console.WriteLine(coords);
                return;
            }

            System.IO.File.AppendAllText("coords.txt", coords + "\n");
            player.SendClientMessage(Color.White, 
                $"{{FF6347}}<AdmCmd>{{FFFFFF}} {pos.X}, {pos.Y}, {pos.Z}, {player.Angle} disimpan dengan nama {name}");
        }

        [Command("tp")]
        public static void Teleport(Player player, float x, float y, float z, int interior = 0, int vw = 0)
        {
            if (!CheckAdmin(player, 1)) return;

            player.SetInteriorSafe(interior);
            player.SetVirtualWorldSafe(vw);
            player.SetPositionSafe(x, y, z);
        }

        [Command("goto")]
        public static void GoTo(Player player, string targetName)
        {
            if (!CheckAdmin(player, 1)) return;
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu belum login!");
                return;
            }

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            var pos = target.Position;
            player.SetInteriorSafe(target.Interior);
            player.SetVirtualWorldSafe(target.VirtualWorld);
            player.SetPositionSafe(pos.X + 1, pos.Y + 1, pos.Z);

            player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu telah diteleport!");
            target.SendClientMessage(Color.White, 
                $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah teleport ke lokasi kamu");
        }

        [Command("gethere")]
        public static void GetHere(Player player, string targetName)
        {
            if (!CheckAdmin(player, 1)) return;
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu belum login!");
                return;
            }

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            var pos = player.Position;
            target.SetInteriorSafe(player.Interior);
            target.SetVirtualWorldSafe(player.VirtualWorld);
            target.SetPositionSafe(pos.X + 1, pos.Y + 1, pos.Z);

            player.SendClientMessage(Color.White, 
                $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah menarik {{00FFFF}}{target.Ucp}{{FFFFFF}} ke lokasi kamu!");
            target.SendClientMessage(Color.White, 
                $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah menarik kamu ke lokasi mereka");
        }
    }
}