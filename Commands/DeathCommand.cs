using ProjectSMP.Entities.Players.Condition;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Commands
{
    public class DeathCommands
    {
        [Command("death")]
        public static void Death(Player player)
        {
            if (player.Condition.Injured < 1)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Kamu tidak terluka saat ini.");
                return;
            }

            if (player.Condition.DyingTime >= 3420)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Death>{FFFFFF} Kamu harus menunggu 3 menit untuk respawn di rumah sakit.");
                return;
            }

            ConditionService.HandleDeath(player);
        }
    }
}