using ProjectSMP.Core;
using ProjectSMP.Extensions;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class ReportCommands
    {
        [Command("report")]
        public static void Report(Player player, string text)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Report} Kamu belum login!");
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Report_G} Gunakan /report [text]");
                return;
            }

            if (!ReportService.CanReport(player))
            {
                var remaining = ReportService.GetCooldown(player);
                player.SendClientMessage(Color.White, $"{Msg.Report} Kamu harus menunggu {remaining} detik sebelum membuat report baru.");
                return;
            }

            var reportId = ReportService.AddReport(player, text);
            ReportService.SetReportTime(player);

            player.SendClientMessage(Color.White, $"{Msg.Report} Report kamu telah dikirim ke admin. Gunakan /reports untuk melihat status report kamu.");
        }

        [Command("reports")]
        public static void Reports(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Report} Kamu belum login!");
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

            if (string.IsNullOrWhiteSpace(response))
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd_G} Gunakan /areport [ReportID] [Response]");
                return;
            }

            var report = ReportService.GetReport(reportId);
            if (report == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Report dengan ID tersebut tidak ditemukan.");
                return;
            }

            var target = BasePlayer.Find(report.PlayerId) as Player;
            if (target != null && target.IsConnected)
            {
                target.SendClientMessage(Color.White, $"{Msg.Report_A} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah menjawab report kamu:");
                target.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} {response}");
            }
            else
            {
                player.SendClientMessage(Color.White, $"{Msg.Report} Player sudah tidak online, tetapi report telah ditandai sebagai sudah dijawab.");
            }

            ReportService.MarkHandled(reportId, player.Id, player.Ucp);
            player.SendClientMessage(Color.White, $"{Msg.Report} Kamu telah menjawab report #{reportId} dari {report.PlayerName}.");
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
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Report dengan ID tersebut tidak ditemukan.");
                return;
            }

            var target = BasePlayer.Find(report.PlayerId) as Player;
            ReportService.DeleteReport(reportId);

            player.SendClientMessage(Color.White, $"{Msg.Report} Kamu telah menghapus report #{reportId}.");

            if (target != null && target.IsConnected)
            {
                target.SendClientMessage(Color.White, $"{Msg.Report} Report kamu telah ditutup oleh admin {{00FFFF}}{player.Ucp}{{FFFFFF}}.");
            }
        }

        private static void ShowPlayerReports(Player player, int page)
        {
            var allReports = ReportService.GetPlayerReports(player.Id);
            var totalReports = allReports.Count;

            if (totalReports == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Report} Kamu tidak memiliki report aktif.");
                return;
            }

            var maxPages = (int)Math.Ceiling((double)totalReports / ReportService.ReportsPerPageValue);
            if (page >= maxPages) page = maxPages - 1;
            if (page < 0) page = 0;

            var pageReports = allReports.Skip(page * ReportService.ReportsPerPageValue).Take(ReportService.ReportsPerPageValue).ToList();

            var rows = new List<string[]>();
            var reportIds = new List<int>();

            foreach (var r in pageReports)
            {
                var status = r.Handled ? "{00FF00}Answered" : "{FF6347}Pending";
                rows.Add(new[] { r.Id.ToString(), status, ReportService.GetTimeElapsed(r.Timestamp), r.Text });
                reportIds.Add(r.Id);
            }

            if (maxPages > 1)
            {
                if (page > 0)
                    rows.Add(new[] { "-1", "{00FFFF}< Previous", "", "" });
                if (page < maxPages - 1)
                    rows.Add(new[] { "-2", "{00FFFF}> Next", "", "" });
            }

            player.SetData("ReportPage", page);
            player.SetData("PageReportIds", reportIds);

            var title = maxPages > 1 ? $"Your Reports (Page {page + 1}/{maxPages})" : "Your Reports";

            player.ShowTabList(title, new[] { "#", "Status", "Time", "Text" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    HandlePlayerReportSelection(player, e.ListItem);
                });
        }

        private static void ShowAdminReports(Player player, int page)
        {
            var allReports = ReportService.GetAllReports();
            var totalReports = allReports.Count;

            if (totalReports == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Report} Tidak ada report aktif saat ini.");
                return;
            }

            var maxPages = (int)Math.Ceiling((double)totalReports / ReportService.ReportsPerPageValue);
            if (page >= maxPages) page = maxPages - 1;
            if (page < 0) page = 0;

            var pageReports = allReports.Skip(page * ReportService.ReportsPerPageValue).Take(ReportService.ReportsPerPageValue).ToList();

            var rows = new List<string[]>();
            var reportIds = new List<int>();

            foreach (var r in pageReports)
            {
                var status = r.Handled ? "{00FF00}Answered" : "{FF6347}Pending";
                var reportIdText = $"{r.Id} {{FF6347}}(( {status} {{FF6347}}))";
                var playerText = $"{r.PlayerName}[P{r.PlayerId}]";
                rows.Add(new[] { reportIdText, playerText, ReportService.GetTimeElapsed(r.Timestamp), r.Text });
                reportIds.Add(r.Id);
            }

            if (maxPages > 1)
            {
                if (page > 0)
                    rows.Add(new[] { "-1", "{00FFFF}< Previous", "", "" });
                if (page < maxPages - 1)
                    rows.Add(new[] { "-2", "{00FFFF}> Next", "", "" });
            }

            player.SetData("ReportPage", page);
            player.SetData("PageReportIds", reportIds);

            var title = maxPages > 1 ? $"All Reports (Page {page + 1}/{maxPages})" : "All Reports";

            player.ShowTabList(title, new[] { "#", "Player", "Time", "Text" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    HandleAdminReportSelection(player, e.ListItem);
                });
        }

        private static void HandlePlayerReportSelection(Player player, int listItem)
        {
            var page = player.GetData("ReportPage", 0);
            var reportIds = player.GetData<List<int>>("PageReportIds", null);

            if (reportIds == null) return;

            if (listItem >= reportIds.Count)
            {
                var navItem = listItem - reportIds.Count;
                if (navItem == 0 && page > 0)
                {
                    ShowPlayerReports(player, page - 1);
                }
                else if (navItem == 1 || (navItem == 0 && page == 0))
                {
                    ShowPlayerReports(player, page + 1);
                }
                return;
            }

            var reportId = reportIds[listItem];
            var report = ReportService.GetReport(reportId);

            if (report == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.Report} Report tersebut sudah tidak ada.");
                return;
            }

            ShowPlayerReportDetails(player, report);
        }

        private static void HandleAdminReportSelection(Player player, int listItem)
        {
            var page = player.GetData("ReportPage", 0);
            var reportIds = player.GetData<List<int>>("PageReportIds", null);

            if (reportIds == null) return;

            if (listItem >= reportIds.Count)
            {
                var navItem = listItem - reportIds.Count;
                if (navItem == 0 && page > 0)
                {
                    ShowAdminReports(player, page - 1);
                }
                else if (navItem == 1 || (navItem == 0 && page == 0))
                {
                    ShowAdminReports(player, page + 1);
                }
                return;
            }

            var reportId = reportIds[listItem];
            var report = ReportService.GetReport(reportId);

            if (report == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.Report} Report tersebut sudah tidak ada.");
                return;
            }

            player.SetData("SelectedReport", reportId);
            ShowAdminReportOptions(player, report);
        }

        private static void ShowPlayerReportDetails(Player player, Data.Report report)
        {
            var status = report.Handled ? "{00FF00}Answered" : "{FF6347}Pending";
            var handledInfo = "";

            if (report.Handled) {
                handledInfo = $"{{a9c4e4}}Handled by: {{ff0000}}{report.AdminName}[P{report.AdminId}]{{a9c4e4}}\n";
            }

            var details = TextFormatter.Build(
                handledInfo,
                $"Submitted: {{ffefad}}{ReportService.GetTimeElapsed(report.Timestamp)}{{a9c4e4}}\n\n",
                $"Your Report:\n{report.Text}\n\n",
                $"{{FFFF00}}Tip: Admin akan menjawab report kamu segera."
            );

            player.ShowMessage($"{{FF6347}}Report #{report.Id} (( {status}{{FF6347}} ))", details)
                .WithButtons("Back", "")
                .Show(e => ShowPlayerReports(player, player.GetData("ReportPage", 0)));
        }

        private static void ShowAdminReportOptions(Player player, Data.Report report)
        {
            var title = $"Report #{report.Id} - {report.PlayerName}";

            player.ShowList(title, "View Details", "Answer Report", "Teleport to Player", "Teleport Player to Me", "Delete Report")
                .WithButtons("Select", "Back")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        player.SetData("SelectedReport", -1);
                        ShowAdminReports(player, player.GetData("ReportPage", 0));
                        return;
                    }

                    HandleAdminReportOption(player, e.ListItem);
                });
        }

        private static void HandleAdminReportOption(Player player, int option)
        {
            var reportId = player.GetData("SelectedReport", -1);
            var report = ReportService.GetReport(reportId);

            if (report == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.Report} Report tersebut sudah tidak ada.");
                return;
            }

            var target = BasePlayer.Find(report.PlayerId) as Player;

            switch (option)
            {
                case 0:
                    ShowAdminReportDetails(player, report);
                    break;

                case 1:
                    if (target == null || !target.IsConnected)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Report} Player sudah tidak online.");
                        ShowAdminReportOptions(player, report);
                        return;
                    }
                    ShowAnswerReportDialog(player, report);
                    break;

                case 2:
                    if (target == null || !target.IsConnected)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Report} Player sudah tidak online.");
                        ShowAdminReportOptions(player, report);
                        return;
                    }
                    TeleportHelper.TeleportToPlayer(player, target);
                    player.SendClientMessage(Color.White, $"{Msg.Report} Kamu telah diteleport ke player {report.PlayerName}.");
                    target.SendClientMessage(Color.White, $"{Msg.Report} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah teleport ke lokasi kamu untuk menindaklanjuti report kamu.");
                    break;

                case 3:
                    if (target == null || !target.IsConnected)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Report} Player sudah tidak online.");
                        ShowAdminReportOptions(player, report);
                        return;
                    }
                    TeleportHelper.TeleportToPlayer(target, player, reverse: true);
                    player.SendClientMessage(Color.White, $"{Msg.Report} Kamu telah menarik player {report.PlayerName} ke lokasi kamu.");
                    target.SendClientMessage(Color.White, $"{Msg.Report} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah menarik kamu ke lokasi mereka untuk menindaklanjuti report kamu.");
                    break;

                case 4:
                    ReportService.DeleteReport(reportId);
                    player.SendClientMessage(Color.White, $"{Msg.Report} Kamu telah menghapus report #{reportId}.");
                    if (target != null && target.IsConnected)
                    {
                        target.SendClientMessage(Color.White, $"{Msg.Report} Report kamu telah ditutup oleh admin {{00FFFF}}{player.Ucp}{{FFFFFF}}.");
                    }
                    player.SetData("SelectedReport", -1);
                    ShowAdminReports(player, player.GetData("ReportPage", 0));
                    break;
            }
        }

        private static void ShowAdminReportDetails(Player player, Data.Report report)
        {
            var status = report.Handled ? "{00FF00}Answered" : "{FF6347}Pending";
            var handledBy = report.Handled ? $"Handled by: {{ff0000}}{report.AdminName}[P{report.AdminId}]{{a9c4e4}}\n" : "";

            var details = TextFormatter.Build(
                $"{{a9c4e4}}Created by: {{ffeea8}}{report.PlayerName}[P{report.PlayerId}]{{a9c4e4}}\n",
                handledBy,
                $"Submitted: {{ffefad}}{ReportService.GetTimeElapsed(report.Timestamp)}{{a9c4e4}}\n\n",
                $"Report Text:\n{report.Text}"
            );

            player.ShowMessage($"{{FF6347}}Report #{report.Id} (( {status}{{FF6347}} ))", details)
                .WithButtons("Back", "")
                .Show(e => ShowAdminReportOptions(player, report));
        }

        private static void ShowAnswerReportDialog(Player player, Data.Report report)
        {
            player.ShowInput("Answer Report", "Enter your response to this report:")
                .WithButtons("Send", "Back")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowAdminReportOptions(player, report);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(e.InputText))
                    {
                        ShowAnswerReportDialog(player, report);
                        return;
                    }

                    var target = BasePlayer.Find(report.PlayerId) as Player;
                    if (target == null || !target.IsConnected)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Report} Player sudah tidak online, tetapi report telah ditandai sebagai sudah dijawab.");
                    }
                    else
                    {
                        target.SendClientMessage(Color.White, $"{Msg.Report_A} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah menjawab report kamu:");
                        target.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} {e.InputText}");
                    }

                    ReportService.MarkHandled(report.Id, player.Id, player.Ucp);
                    player.SendClientMessage(Color.White, $"{Msg.Report} Kamu telah menjawab report #{report.Id} dari {report.PlayerName}.");
                    player.SetData("SelectedReport", -1);
                    ShowAdminReports(player, player.GetData("ReportPage", 0));
                });
        }
    }
}