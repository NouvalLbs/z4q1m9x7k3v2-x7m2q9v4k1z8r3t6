using ProjectSMP.Entities.Players.Condition;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Commands {
    public class DeathCommands {
        [Command("death")]
        public static void Death(Player player) {
            ConditionService.HandleDeath(player);
        }
    }
}