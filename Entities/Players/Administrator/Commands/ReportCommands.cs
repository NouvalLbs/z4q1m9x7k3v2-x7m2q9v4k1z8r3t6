using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class ReportCommands
    {
        [Command("report")]
        public static void Report(Player player, string text)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<Report>{FFFFFF} Kamu belum login!");
                return;
            }

            if (!ReportService.CanReport(player))
            {
                var remaining = ReportService.GetCooldown(player);
                player.SendClientMessage(Color.White, $"{{FF6347}}<Report>{{FFFFFF}} Kamu harus menunggu {remaining} detik sebelum membuat report baru.");
                return;
            }

            var reportId = ReportService.AddReport(player, text);
            ReportService.SetReportTime(player);

            player.SendClientMessage(Color.White, "{FF6347}<Report>{FFFFFF} Report kamu telah dikirim ke admin. Gunakan /reports untuk melihat status report kamu.");
        }

        [Command("reports")]
        public static void Reports(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{FF6347}<Report>{FFFFFF} Kamu belum login!");
                return;
            }

            if (player.Admin >= 1 && player.AdminOnDuty)
            {
                ShowAdminReports(player, 0);
            }
            else
            {
                ShowPlayerReports(player, 0);
            }
        }

        [Command("areport")]
        public static void AReport(Player player, int reportId, string response)
        {
            if (player.Admin < 1 || !player.AdminOnDuty)
            {
                player.SendClientMessage(Color.White, "{b9b9b9}Command '/areport' tidak ada, gunakan '/help'.");
                return;
            }

            var report = ReportService.GetReport(reportId);
            if (report == null)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Report dengan ID tersebut tidak ditemukan.");
                return;
            }

            var target = BasePlayer.Find(report.PlayerId) as Player;
            if (target != null && target.IsConnected)
            {
                target.SendClientMessage(Color.White, $"{{FF6347}}<Report Answer>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah menjawab report kamu:");
                target.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} {response}");
            }
            else
            {
                player.SendClientMessage(Color.White, "{FF6347}<Report>{FFFFFF} Player sudah tidak online, tetapi report telah ditandai sebagai sudah dijawab.");
            }

            ReportService.MarkHandled(reportId, player.Id);
            player.SendClientMessage(Color.White, $"{{FF6347}}<Report>{{FFFFFF}} Kamu telah menjawab report #{reportId} dari {report.PlayerName}.");
        }

        [Command("delreport")]
        public static void DelReport(Player player, int reportId)
        {
            if (player.Admin < 1 || !player.AdminOnDuty)
            {
                player.SendClientMessage(Color.White, "{b9b9b9}Command '/delreport' tidak ada, gunakan '/help'.");
                return;
            }

            var report = ReportService.GetReport(reportId);
            if (report == null)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Report dengan ID tersebut tidak ditemukan.");
                return;
            }

            var target = BasePlayer.Find(report.PlayerId) as Player;
            ReportService.DeleteReport(reportId);

            player.SendClientMessage(Color.White, $"{{FF6347}}<Report>{{FFFFFF}} Kamu telah menghapus report #{reportId}.");

            if (target != null && target.IsConnected)
            {
                target.SendClientMessage(Color.White, $"{{FF6347}}<Report>{{FFFFFF}} Report kamu telah ditutup oleh admin {{00FFFF}}{player.Ucp}{{FFFFFF}}.");
            }
        }

        private static void ShowPlayerReports(Player player, int page)
        {
            var reports = ReportService.GetPlayerReports(player.Id);
            if (reports.Count == 0)
            {
                player.SendClientMessage(Color.White, "{FF6347}<Report>{FFFFFF} Kamu tidak memiliki report aktif.");
                return;
            }

            var items = new System.Collections.Generic.List<string>();
            foreach (var r in reports)
            {
                var status = r.Handled ? "{00FF00}Answered" : "{FF6347}Pending";
                items.Add($"{r.Id}\t{status}\t{ReportService.GetTimeElapsed(r.Timestamp)}\t{r.Text}");
            }

            player.ShowTabList("Your Reports", new[] { "ReportId", "Status", "Time", "Report Text" })
                .WithRows(items.ToArray())
                .Show();
        }

        private static void ShowAdminReports(Player player, int page)
        {
            var reports = ReportService.GetAllReports();
            if (reports.Count == 0)
            {
                player.SendClientMessage(Color.White, "{FF6347}<Report>{FFFFFF} Tidak ada report aktif saat ini.");
                return;
            }

            var items = new System.Collections.Generic.List<string>();
            foreach (var r in reports)
            {
                var status = r.Handled ? "{00FF00}Answered" : "{FF6347}Pending";
                items.Add($"{r.Id} ({status})\t{r.PlayerName} (Id: {r.PlayerId})\t{ReportService.GetTimeElapsed(r.Timestamp)}\t{r.Text}");
            }

            player.ShowTabList("All Reports", new[] { "ReportId", "Player", "Time", "Report Text" })
                .WithRows(items.ToArray())
                .Show();
        }
    }
}