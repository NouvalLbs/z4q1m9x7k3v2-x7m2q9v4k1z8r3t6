using ProjectSMP.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSMP.Features.Bank
{
    internal static class BankService
    {
        private const string Table = "players_banks";

        public static async Task LoadAsync(Player player)
        {
            var rows = await DatabaseManager.QueryAsync<PlayerBankAccount>(
                $"SELECT id AS Id, CitizenId, account_number AS AccountNumber, " +
                $"account_name AS AccountName, creation_date AS CreationDate, " +
                $"last_transaction AS LastTransaction, balance AS Balance, is_active AS IsActive " +
                $"FROM `{Table}` WHERE CitizenId = @CitizenId",
                new { player.CitizenId });

            if (player.IsDisposed) return;
            player.BankAccounts = new List<PlayerBankAccount>(rows);
        }

        public static async Task SaveAccountAsync(PlayerBankAccount account)
        {
            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET balance=@Balance, last_transaction=CURDATE(), " +
                $"is_active=@IsActive WHERE id=@Id",
                new { account.Balance, account.IsActive, account.Id });
        }
    }
}