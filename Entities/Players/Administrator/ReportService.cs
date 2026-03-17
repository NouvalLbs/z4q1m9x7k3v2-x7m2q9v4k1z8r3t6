using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using ProjectSMP.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSMP.Entities.Players.Administrator.Data;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class ReportService
    {
        private const int MaxReports = 50;
        private const int ReportCooldown = 60;
        private static readonly List<Report> Reports = new();
        private static int NextId = 0;

        public static int AddReport(Player player, string text)
        {
            var report = new Report
            {
                Id = NextId++,
                PlayerId = player.Id,
                PlayerName = player.Username,
                Text = text,
                Timestamp = Environment.TickCount / 1000,
                Handled = false
            };

            Reports.Add(report);

            if (Reports.Count > MaxReports)
                Reports.RemoveAt(0);

            NotifyAdmins(report);
            return report.Id;
        }

        public static bool CanReport(Player player)
        {
            var lastTime = player.GetData("LastReportTime", 0);
            var currentTime = Environment.TickCount / 1000;
            return (currentTime - lastTime) >= ReportCooldown;
        }

        public static int GetCooldown(Player player)
        {
            var lastTime = player.GetData("LastReportTime", 0);
            var currentTime = Environment.TickCount / 1000;
            return Math.Max(0, ReportCooldown - (currentTime - lastTime));
        }

        public static void SetReportTime(Player player)
        {
            player.SetData("LastReportTime", Environment.TickCount / 1000);
        }

        public static Report GetReport(int id)
        {
            return Reports.FirstOrDefault(r => r.Id == id);
        }

        public static void DeleteReport(int id)
        {
            Reports.RemoveAll(r => r.Id == id);
        }

        public static void MarkHandled(int id, int adminId)
        {
            var report = GetReport(id);
            if (report != null)
            {
                report.Handled = true;
                report.AdminId = adminId;
            }
        }

        public static List<Report> GetPlayerReports(int playerId)
        {
            return Reports.Where(r => r.PlayerId == playerId).ToList();
        }

        public static List<Report> GetAllReports()
        {
            return Reports.ToList();
        }

        public static void RemovePlayerReports(int playerId)
        {
            var removed = Reports.RemoveAll(r => r.PlayerId == playerId);
            if (removed > 0)
            {
                foreach (var admin in BasePlayer.All.OfType<Player>().Where(p => p.Admin >= 1 && p.AdminOnDuty))
                {
                    admin.SendClientMessage(Color.White,
                        $"{{FF6347}}<Report System>{{FFFFFF}} Player disconnected. {removed} report(s) removed.");
                }
            }
        }

        public static string GetTimeElapsed(int timestamp)
        {
            var current = Environment.TickCount / 1000;
            var seconds = current - timestamp;

            if (seconds < 60) return $"{seconds} seconds ago";
            if (seconds < 3600) return $"{seconds / 60} minutes ago";
            if (seconds < 86400) return $"{seconds / 3600} hours ago";
            return $"{seconds / 86400} days ago";
        }

        private static void NotifyAdmins(Report report)
        {
            foreach (var admin in BasePlayer.All.OfType<Player>().Where(p => p.Admin >= 1 && p.AdminOnDuty))
            {
                admin.SendClientMessage(Color.White,
                    $"{{FF6347}}<Report>{{FFFFFF}} New report from {report.PlayerName} (Id: {report.PlayerId}): {report.Text}");
                admin.PlaySound(1058, Vector3.Zero);
            }
        }

        public static void Cleanup(Player player)
        {
            RemovePlayerReports(player.Id);
        }
    }
}