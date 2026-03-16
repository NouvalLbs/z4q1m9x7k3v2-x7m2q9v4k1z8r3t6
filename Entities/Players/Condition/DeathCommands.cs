using SampSharp.GameMode.SAMP.Commands;
namespace ProjectSMP.Entities.Players.Condition
{
    public class DeathCommands
    {
        [Command("death")]
        public static void Death(Player player)
        {
            ConditionService.HandleDeath(player);
        }
    }
}