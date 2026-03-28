using ProjectSMP.Core;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Features.Jobs.DynamicJob
{
    public static class JobDialogManager
    {
        public static void ShowJobInterface(Player player, DynamicJobData jobData)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            var hasJob = JobService.HasJob(player, jobData.JobName);

            if (hasJob)
            {
                player.ShowList($"{jobData.Name}", "{FF0000}> {FFFFFF}Quit Job")
                    .WithButtons("Select", "Close")
                    .Show(e =>
                    {
                        if (e.DialogButton != DialogButton.Left) return;
                        ShowQuitConfirmDialog(player, jobData);
                    });
            }
            else
            {
                player.ShowList($"{jobData.Name}", "{00FF00}> {FFFFFF}Get Job")
                    .WithButtons("Select", "Close")
                    .Show(e =>
                    {
                        if (e.DialogButton != DialogButton.Left) return;
                        HandleGetJob(player, jobData);
                    });
            }
        }

        private static void HandleGetJob(Player player, DynamicJobData jobData)
        {
            if (JobService.GetJobCount(player) >= 2)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu sudah memiliki 2 job! Quit salah satu job terlebih dahulu.");
                return;
            }

            if (JobService.AddJob(player, jobData.JobName))
            {
                player.SendClientMessage(Color.White, $"{Msg.Jobs} Kamu berhasil bergabung dengan job {{FFFF00}}{jobData.JobName}{{FFFFFF}}!");
            }
            else
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Gagal bergabung dengan job!");
            }
        }

        private static void ShowQuitConfirmDialog(Player player, DynamicJobData jobData)
        {
            var message = $"Apakah kamu yakin ingin keluar dari job {{FFFF00}}{jobData.JobName}{{FFFFFF}}?\n\n" +
                         $"{{FF0000}}Peringatan: Semua progress job kamu akan hilang!";

            player.ShowMessage("Konfirmasi Quit Job", message)
                .WithButtons("Ya, Quit", "Batal")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    if (JobService.RemoveJob(player, jobData.JobName))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Jobs} Kamu telah keluar dari job {{FFFF00}}{jobData.JobName}{{FFFFFF}}.");
                    }
                    else
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Gagal keluar dari job!");
                    }
                });
        }
    }
}