using ProjectSMP.Core;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using System.Collections.Generic;

namespace ProjectSMP.Features.Jobs.Commands
{
    public class JobCommands
    {
        [Command("quitjob")]
        public static void QuitJob(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            var jobs = JobService.GetAllJobs(player);
            if (jobs.Length == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu tidak memiliki job apapun!");
                return;
            }

            var items = new List<string>();
            foreach (var job in jobs)
            {
                items.Add($"{{FFFF00}}{job.JobName} {{888888}}(Joined: {job.RegisterDate})");
            }

            player.ShowList("Quit Job - Pilih Job", items.ToArray())
                .WithButtons("Quit", "Batal")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    var selectedJob = jobs[e.ListItem];
                    ShowQuitConfirmDialog(player, selectedJob.JobName);
                });
        }

        private static void ShowQuitConfirmDialog(Player player, string jobName)
        {
            var message = $"Apakah kamu yakin ingin keluar dari job {{FFFF00}}{jobName}{{FFFFFF}}?\n\n" +
                         $"{{FF0000}}Peringatan: Semua progress job kamu akan hilang!";

            player.ShowMessage("Konfirmasi Quit Job", message)
                .WithButtons("Ya, Quit", "Batal")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    if (JobService.RemoveJob(player, jobName))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Jobs} Kamu telah keluar dari job {{FFFF00}}{jobName}{{FFFFFF}}.");
                    }
                    else
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Gagal keluar dari job!");
                    }
                });
        }
    }
}