using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Entities.Players.Condition;
using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSMP.Entities.Players.Administrator
{
    public class AdminCommands3
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

        [Command("jail")]
        public static void Jail(Player player, string targetName, int seconds, string reason)
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
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu tidak dapat menjail admin dengan level lebih tinggi!");
                return;
            }

            if (target.JailTime > 0)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut sudah berada di jail!");
                return;
            }

            if (!JailService.JailPlayer(target, seconds, reason))
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Waktu jail harus antara 1 - 3600 detik!");
                return;
            }

            BasePlayer.SendClientMessageToAll(Color.White,
                $"{{992712}}<AdmCmd> {target.Username} telah dijail oleh {player.Ucp} selama {seconds} detik.");
            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}Alasan: {reason}");
        }

        [Command("unjail")]
        public static void Unjail(Player player, string targetName)
        {
            if (!CheckAdmin(player, 2)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            if (target.JailTime <= 0)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut tidak berada di jail!");
                return;
            }

            JailService.UnjailPlayer(target);
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah mengeluarkan {{00FFFF}}{target.Ucp}{{FFFFFF}} dari jail!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengeluarkan kamu dari jail");
        }

        [Command("revive")]
        public static void Revive(Player player, string targetName)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            if (target.Condition.Injured < 1)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut tidak dalam keadaan mati!");
                return;
            }

            var pos = target.Position;
            target.SetData("HospitalRespawn", false);
            ConditionService.HandleDeath(target);

            target.SetPositionSafe(pos);
            target.SetHealthSafe(target.Vitals.MaxHealth, 0);
            target.Vitals.Health = target.Vitals.MaxHealth;
            target.ClearAnimationsSafe();
            target.ToggleControllableSafe(true);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah melakukan revive terhadap {{00FFFF}}{target.Ucp}{{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah melakukan revive terhadap kamu");
        }

        [Command("setint")]
        public static void SetInt(Player player, string targetName, int interiorId)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            target.SetInteriorSafe(interiorId);
            player.SendClientMessage(Color.White,
                $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah mengubah interior {{00FFFF}}{target.Ucp}{{FFFFFF}} menjadi {{00FFFF}}{interiorId}{{FFFFFF}}!");
            target.SendClientMessage(Color.White,
                $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengubah interior kamu menjadi {{00FFFF}}{interiorId}{{FFFFFF}}");
        }

        [Command("setvw")]
        public static void SetVW(Player player, string targetName, int vwId)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            target.SetVirtualWorldSafe(vwId);
            player.SendClientMessage(Color.White,
                $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah mengubah virtual world {{00FFFF}}{target.Ucp}{{FFFFFF}} menjadi {{00FFFF}}{vwId}{{FFFFFF}}!");
            target.SendClientMessage(Color.White,
                $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengubah virtual world kamu menjadi {{00FFFF}}{vwId}{{FFFFFF}}");
        }

        [Command("getip")]
        public static void GetIP(Player player, string targetName)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            var ip = Utilities.ReturnIP(target);
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Informasi IP {{00FFFF}}{target.Username} (UCP: {target.Ucp}){{FFFFFF}}");
            player.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} IP Address: {{00FFFF}}{ip}");
        }

        [Command("aka")]
        public static async void AKA(Player player, string username)
        {
            if (!CheckAdmin(player, 3)) return;

            var query = "SELECT `ip`, `username`, `last_login` FROM `players` WHERE `username` = @Username LIMIT 1";
            var result = await DatabaseManager.QueryFirstAsync<dynamic>(query, new { Username = username });

            if (result == null)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player dengan username tersebut tidak ditemukan di database!");
                return;
            }

            string ip = result.ip;
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Data player {username}:");
            player.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} IP: {ip} | Last Login: {result.last_login}");

            var altQuery = "SELECT `username` FROM `players` WHERE `ip` = @IP AND `username` != @Username";
            var alts = await DatabaseManager.QueryAsync<dynamic>(altQuery, new { IP = ip, Username = username });

            if (!alts.Any())
            {
                player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Tidak ada akun lain yang menggunakan IP yang sama dengan {username}");
                return;
            }

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Akun lain yang menggunakan IP yang sama dengan {username}:");
            foreach (var alt in alts)
            {
                player.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} {alt.username}");
            }
        }

        [Command("akaip")]
        public static async void AKAIP(Player player, string ip)
        {
            if (!CheckAdmin(player, 3)) return;

            if (ip.Length < 7)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Format IP tidak valid!");
                return;
            }

            var query = "SELECT username FROM players WHERE ip = @IP";
            var results = await DatabaseManager.QueryAsync<dynamic>(query, new { IP = ip });

            if (!results.Any())
            {
                player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Tidak ada akun yang menggunakan IP {{00FFFF}}{ip}{{FFFFFF}}");
                return;
            }

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Daftar akun yang menggunakan IP {{00FFFF}}{ip}{{FFFFFF}}:");
            foreach (var result in results)
            {
                player.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} {result.username}");
            }
        }
    }
}