using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Data;
using ProjectSMP.Extensions;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class SendToCommands
    {
        private static bool CheckAdmin(Player player, int level)
        {
            if (player.Admin < level)
            {
                player.SendClientMessage(Color.White, "{b9b9b9}Command tidak ada, gunakan '/help'.");
                return false;
            }
            if (!player.AdminOnDuty)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Command tidak dapat digunakan ketika kamu tidak duty.");
                return false;
            }
            return true;
        }

        [Command("sendto")]
        public static void SendTo(Player player, string targetName, string cityName = "")
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

            if (target.JailInfo.Jailed > 0)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target sedang berada di jail!");
                return;
            }

            player.SetData("SendTo_Target", target.Id);

            if (string.IsNullOrEmpty(cityName))
            {
                ShowCityDialog(player);
                return;
            }

            var cityId = cityName.ToLower() switch
            {
                "ls" => 1,
                "sf" => 2,
                "lv" => 3,
                _ => 0
            };

            if (cityId == 0)
            {
                ShowCityDialog(player);
                return;
            }

            player.SetData("SendTo_City", cityId);
            ShowLocationDialog(player);
        }

        private static void ShowCityDialog(Player player)
        {
            player.ShowList(
                "Select City",
                "Los Santos (LS)",
                "San Fierro (SF)",
                "Las Venturas (LV)")
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        player.SetData("SendTo_Target", -1);
                        player.SetData("SendTo_City", 0);
                        return;
                    }

                    var cityId = e.ListItem + 1;
                    player.SetData("SendTo_City", cityId);
                    ShowLocationDialog(player);
                });
        }

        private static void ShowLocationDialog(Player player)
        {
            var cityId = player.GetData("SendTo_City", 0);
            var targetId = player.GetData("SendTo_Target", -1);

            if (cityId == 0 || targetId == -1) return;

            var target = BasePlayer.Find(targetId) as Player;
            if (target == null || !target.IsConnected)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Player target telah disconnect.");
                player.SetData("SendTo_Target", -1);
                player.SetData("SendTo_City", 0);
                return;
            }

            var locations = System.Linq.Enumerable.Where(SendToData.All, loc => loc.City == cityId).ToArray();
            var items = new string[locations.Length];
            for (var i = 0; i < locations.Length; i++)
                items[i] = locations[i].Name;

            player.ShowList($"Locations - City {cityId}", items)
                .WithButtons("Select", "Back")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowCityDialog(player);
                        return;
                    }

                    var loc = locations[e.ListItem];
                    var tgt = BasePlayer.Find(targetId) as Player;
                    if (tgt == null || !tgt.IsConnected)
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Player target telah disconnect.");
                        player.SetData("SendTo_Target", -1);
                        player.SetData("SendTo_City", 0);
                        return;
                    }

                    tgt.SetInteriorSafe(loc.Interior);
                    tgt.SetVirtualWorldSafe(loc.VirtualWorld);
                    tgt.SetPositionSafe(loc.X, loc.Y, loc.Z);
                    tgt.PutCameraBehindPlayer();

                    player.SendClientMessage(Color.White,
                        $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah mengirim {{00FFFF}}{tgt.Ucp}{{FFFFFF}} ke {{00FFFF}}{loc.Name}{{FFFFFF}}!");
                    tgt.SendClientMessage(Color.White,
                        $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengirim kamu ke {{00FFFF}}{loc.Name}{{FFFFFF}}");

                    player.SetData("SendTo_Target", -1);
                    player.SetData("SendTo_City", 0);
                });
        }
    }
}