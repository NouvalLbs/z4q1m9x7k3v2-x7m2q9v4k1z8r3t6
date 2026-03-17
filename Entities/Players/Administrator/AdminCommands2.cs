using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;

namespace ProjectSMP.Entities.Players.Administrator
{
    public class AdminCommands2
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

        [Command("freeze")]
        public static void Freeze(Player player, string targetName)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            target.ToggleControllableSafe(false);
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah Freeze {{00FFFF}}{target.Ucp}{{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah Freeze pergerakan kamu");
        }

        [Command("unfreeze")]
        public static void Unfreeze(Player player, string targetName)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            target.ToggleControllableSafe(true);
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah Unfreeze {{00FFFF}}{target.Ucp}{{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah Unfreeze pergerakan kamu");
        }

        [Command("kick")]
        public static void Kick(Player player, string targetName, string reason)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            if (target.Admin > player.Admin)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu tidak dapat menendang admin dengan level lebih tinggi!");
                return;
            }

            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}<AdmCmd> {target.Username} telah dikeluarkan dari server oleh {player.Ucp}.");
            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}Alasan: {reason}");
            Utilities.KickEx(target, 500);
        }

        [Command("slap")]
        public static void Slap(Player player, string targetName)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            if (target.Admin > player.Admin)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu tidak dapat menampar admin dengan level lebih tinggi!");
                return;
            }

            if (target.InAnyVehicle)
                target.RemoveFromVehicle();

            var pos = target.Position;
            target.SetPositionSafe(pos.X, pos.Y, pos.Z + 10.0f);
            target.PlaySound(1130, Vector3.Zero);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah menampar {{00FFFF}}{target.Ucp}{{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah menampar kamu");
        }

        [Command("acuff")]
        public static void ACuff(Player player, string targetName)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            if (target.Admin > player.Admin)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu tidak dapat memborgol admin dengan level lebih tinggi!");
                return;
            }

            if (target.Cuffed)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut sudah dalam keadaan terborgol!");
                return;
            }

            target.Cuffed = true;
            target.SetSpecialActionSafe(SpecialAction.Cuffed);
            target.ToggleControllableSafe(false);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah memborgol {{00FFFF}}{target.Ucp}{{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah memborgol kamu");
        }

        [Command("auncuff")]
        public static void AUncuff(Player player, string targetName)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            if (!target.Cuffed)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut tidak dalam keadaan terborgol!");
                return;
            }

            target.Cuffed = false;
            target.SetSpecialActionSafe(SpecialAction.None);
            target.ToggleControllableSafe(true);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah melepas borgol {{00FFFF}}{target.Ucp}{{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah melepas borgol kamu");
        }

        [Command("setweather")]
        public static void SetWeather(Player player, int weatherId)
        {
            if (!CheckAdmin(player, 2)) return;

            Server.SetWeather(weatherId);
            foreach (var p in BasePlayer.All)
                p.SetWeather(weatherId);

            BasePlayer.SendClientMessageToAll(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Weather telah dirubah oleh {{ff0000}}{player.Ucp}{{FFFFFF}}");
        }

        [Command("peject")]
        public static void PEject(Player player, string targetName)
        {
            if (!CheckAdmin(player, 2)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            if (target.Admin > player.Admin)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu tidak dapat mengeluarkan admin dengan level lebih tinggi dari kendaraan!");
                return;
            }

            if (!target.InAnyVehicle)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut tidak berada di dalam kendaraan!");
                return;
            }

            target.RemoveFromVehicle();
            var pos = target.Position;
            target.SetPositionSafe(pos.X, pos.Y, pos.Z + 1.0f);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah mengeluarkan {{00FFFF}}{target.Ucp}{{FFFFFF}} dari kendaraan!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengeluarkan kamu dari kendaraan");
        }

        [Command("jetpack")]
        public static void Jetpack(Player player)
        {
            if (!CheckAdmin(player, 2)) return;

            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu belum login!");
                return;
            }

            if (player.SpecialAction == SpecialAction.Usejetpack)
            {
                player.SetSpecialActionSafe(SpecialAction.None);
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu telah melepas jetpack.");
            }
            else
            {
                player.SetSpecialActionSafe(SpecialAction.Usejetpack);
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu telah memakai jetpack.");
            }
        }

        [Command("noclip", Shortcut = "nc")]
        public static void NoClip(Player player)
        {
            if (!CheckAdmin(player, 1)) return;

            if (player.GetData("NoClipActive", false))
            {
                NoClipService.Stop(player);
                player.SetData("NoClipActive", false);
            }
            else
            {
                NoClipService.Start(player);
                player.SetData("NoClipActive", true);
            }
        }
    }
}