using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Features.Bank.Commands
{
    public class BankCommands {
        [Command("bank")]
        public static void AccessBank(Player player) {
            BankDialogManager.ShowBankInterface(player);
        }
    }
}
