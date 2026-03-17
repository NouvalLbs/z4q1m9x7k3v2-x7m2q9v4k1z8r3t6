using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator
{
    public class AdminCommands4
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

        [Command("ban")]
        public static async void Ban(Player player, string targetName, int days, string reason)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            if (target.Admin > player.Admin)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu tidak dapat ban admin dengan level lebih tinggi!");
                return;
            }

            var banTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var banExpire = days > 0 ? banTime + (days * 86400) : 0;

            target.BanInfo.Time = (int)banTime;
            target.BanInfo.Expire = (int)banExpire;
            target.BanInfo.Reason = reason;
            target.BanInfo.Admin = player.Ucp;

            var query = @"UPDATE `players` SET `ban` = 1, `bantime` = @BanTime, `banexpire` = @BanExpire, 
                         `banreason` = @Reason, `banadmin` = @Admin WHERE `citizenId` = @CitizenId";
            await DatabaseManager.ExecuteAsync(query, new
            {
                BanTime = banTime,
                BanExpire = banExpire,
                Reason = reason,
                Admin = player.Ucp,
                CitizenId = target.CitizenId
            });

            var msg = days > 0
                ? $"{{992712}}<AdmCmd> {target.Username} telah di-ban dari server oleh {player.Ucp} selama {days} hari."
                : $"{{992712}}<AdmCmd> {target.Username} telah di-ban secara permanent dari server oleh {player.Ucp}.";

            BasePlayer.SendClientMessageToAll(Color.White, msg);
            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}Alasan: {reason}");
            Utilities.KickEx(target, 500);
        }

        [Command("oban")]
        public static async void OBan(Player player, string username, int days, string reason)
        {
            if (!CheckAdmin(player, 4)) return;

            var checkQuery = "SELECT * FROM `players` WHERE `username` = @Username LIMIT 1";
            var result = await DatabaseManager.QueryFirstAsync<dynamic>(checkQuery, new { Username = username });

            if (result == null)
            {
                player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Player dengan nama {username} tidak ditemukan di database!");
                return;
            }

            string citizenId = result.citizenId;
            foreach (var p in BasePlayer.All.OfType<Player>())
            {
                if (p.IsCharLoaded && p.CitizenId == citizenId)
                {
                    player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player sedang online, gunakan /ban.");
                    return;
                }
            }

            int adminLevel = result.admin;
            if (adminLevel > player.Admin)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Kamu tidak dapat ban admin dengan level lebih tinggi!");
                return;
            }

            var banTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var banExpire = days > 0 ? banTime + (days * 86400) : 0;

            var query = @"UPDATE `players` SET `ban` = 1, `bantime` = @BanTime, `banexpire` = @BanExpire, 
                         `banreason` = @Reason, `banadmin` = @Admin WHERE `citizenId` = @CitizenId";
            await DatabaseManager.ExecuteAsync(query, new
            {
                BanTime = banTime,
                BanExpire = banExpire,
                Reason = reason,
                Admin = player.Ucp,
                CitizenId = citizenId
            });

            var msg = days > 0
                ? $"{{992712}}<AdmCmd> {username} telah di-offline ban dari server oleh {player.Ucp} selama {days} hari."
                : $"{{992712}}<AdmCmd> {username} telah di-offline ban secara permanent dari server oleh {player.Ucp}.";

            BasePlayer.SendClientMessageToAll(Color.White, msg);
            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}Alasan: {reason}");
        }

        [Command("unban")]
        public static async void Unban(Player player, string username)
        {
            if (!CheckAdmin(player, 3)) return;

            var checkQuery = "SELECT * FROM `players` WHERE `username` = @Username LIMIT 1";
            var result = await DatabaseManager.QueryFirstAsync<dynamic>(checkQuery, new { Username = username });

            if (result == null)
            {
                player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Player dengan nama {username} tidak ditemukan di database!");
                return;
            }

            int isBanned = result.ban;
            if (isBanned == 0)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut tidak dalam status banned!");
                return;
            }

            var query = @"UPDATE `players` SET `ban` = 0, `bantime` = 0, `banexpire` = 0, 
                         `banreason` = '', `banadmin` = '' WHERE `username` = @Username";
            await DatabaseManager.ExecuteAsync(query, new { Username = username });

            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}<AdmCmd> {username} telah di-unban dari server oleh {player.Ucp}.");
        }

        [Command("astats")]
        public static void AStats(Player player, string targetName)
        {
            if (!CheckAdmin(player, 2)) return;

            var target = Utilities.GetPlayerFromPartOfName(player, targetName);
            if (target == null) return;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player target belum spawn!");
                return;
            }

            var stats = BuildStatsString(target);
            var title = $"{{FF6347}}Admin Stats: {{6fe0ba}}{target.Username} {{c8c8c8}}(UCP: {target.Ucp})";

            player.ShowMessage(title, stats).Show();
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah melihat statistik dari {{00FFFF}}{target.Ucp}{{FFFFFF}}!");
        }

        private static string BuildStatsString(Player p)
        {
            var gender = p.Gender == 0 ? "Male" : "Female";
            var phoneStatus = p.Phone.Off == 0 ? "{91ff00}Online{FFFFFF}" : "{FF0000}Offline{FFFFFF}";
            var charStatus = p.VerifiedChar == 1 ? "{91ff00}Verified{FFFFFF}" : "{FF0000}Unverified{FFFFFF}";
            var admin = Utilities.GetAdminString(p);
            var warn = Utilities.GetWarningString(p);

            return $@"{{FFFF00}}IC Information:
            {{FFFFFF}}Gender: [{{b8d2ec}}{gender}{{FFFFFF}}] | Birthdate: [{{b8d2ec}}{p.BirthDate}{{FFFFFF}}] | Money: [{{00f000}}{Utilities.GroupDigits(p.CharMoney)}{{FFFFFF}}]
            {{FFFFFF}}Phone Status: [{phoneStatus}] | Phone Number: [{{ebeb00}}{p.Phone}{{FFFFFF}}] | Mask ID: [{{b8d2ec}}{p.MaskId}{{FFFFFF}}]

            {{FFFF00}}OOC Information:
            {{FFFFFF}}CitizenId: [{{77efc7}}{p.CitizenId}{{FFFFFF}}] | Level: [{{77efc7}}{p.Level}{{FFFFFF}}] | Paychecks: [{{b8d2ec}}{p.Paycheck}{{FFFFFF}}]
            {{FFFFFF}}Character Story: [{charStatus}] | Staff: [{admin}] | Warns: [{warn}]
            {{FFFFFF}}World: [{{ebeb00}}{p.VirtualWorld}{{FFFFFF}}] | Interior: [{{ebeb00}}{p.Interior}{{FFFFFF}}] | Health: [{{ab0000}}{p.Vitals.Health:F1}{{FFFFFF}}] | Armour: [{{9f9f9f}}{p.Vitals.Armour:F1}{{FFFFFF}}]";
        }
    }
}