using ProjectSMP.Core;
using ProjectSMP.Features.DynamicPickups;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Features.Bank
{
    public static class BankDialogManager
    {
        public static void ShowBankInterface(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Kamu belum login.");
                return;
            }

            var pickupId = PickupService.GetPickupByCallback(player, "OnBankInteract");
            if (pickupId == -1) {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Kamu tidak berada di area Bank!");
                return;
            }

            if (BankService.GetAccountCount(player) < 1)
            {
                ShowCreateAccountDialog(player);
                return;
            }

            ShowBankMenu(player, 0);
        }

        private static void ShowCreateAccountDialog(Player player)
        {
            player.ShowList("Bank Teller Menu", "{FFFF00}> {FFFFFF}Buat rekening baru")
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    CreateNewAccountAsync(player);
                });
        }

        private static async void CreateNewAccountAsync(Player player)
        {
            if (await BankService.CreateAccountAsync(player, "Tabungan"))
            {
                var account = player.BankAccounts[0];
                player.SendClientMessage(Color.White,
                    $"{{C6E2FF}}<BANK>{{FFFFFF}} Akun bank berhasil dibuat! ({account.AccountName} - No.Rek: {account.AccountNumber})");
            }
            else
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Gagal membuat akun bank!");
            }
        }

        private static void ShowBankMenu(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            var title = $"{{FFFFFF}}Bank: {{FFFF00}}{account.AccountName} {{FFFFFF}}| No.Rek: {{FFFF00}}{account.AccountNumber}";
            player.ShowTabListNoHeader(title, 2)
                .WithItems(
                    new[] { "{FFFFFF}Status Rekening:", "{00FF00}Aktif" },
                    new[] { "{FFFFFF}Saldo Rekening:", $"{{00FF00}}{Utilities.GroupDigits(account.Balance)}" },
                    new[] { "{FFFFFF}Transaksi Terakhir:", $"{{FF0000}}{account.LastTransaction}" },
                    new[] { "{FFFF00}> {FFFFFF}Deposit Uang", "" },
                    new[] { "{FFFF00}> {FFFFFF}Withdraw Uang", "" },
                    new[] { "{FFFF00}> {FFFFFF}Transfer Uang", "" },
                    new[] { "{FFFF00}> {FFFFFF}Ambil Gaji (Paycheck)", "" })
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    switch (e.ListItem)
                    {
                        case 4: ShowDepositDialog(player, accountIndex); break;
                        case 5: ShowWithdrawDialog(player, accountIndex); break;
                        case 6: ShowTransferAccountDialog(player, accountIndex); break;
                        case 7:
                            player.SendClientMessage(Color.White, "{C6E2FF}<BANK>{FFFFFF} Fitur Paycheck akan segera tersedia.");
                            break;
                    }
                });
        }

        private static void ShowDepositDialog(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("Bank - Deposit Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n\n{{FFFFFF}}Masukkan jumlah uang yang ingin Anda deposit:")
                .WithButtons("Deposit", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowBankMenu(player, accountIndex);
                        return;
                    }

                    if (!int.TryParse(e.InputText, out var amount) || amount <= 0)
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Jumlah deposit harus lebih dari $0!");
                        ShowDepositDialog(player, accountIndex);
                        return;
                    }

                    if (BankService.Deposit(player, account, amount))
                    {
                        player.SendClientMessage(Color.White,
                            $"{{C6E2FF}}<BANK>{{FFFFFF}} Berhasil deposit {Utilities.GroupDigits(amount)} ke akun {account.AccountName}. Saldo: {Utilities.GroupDigits(account.Balance)}");
                    }
                    else
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Deposit gagal! Uang kamu tidak cukup.");
                    }
                });
        }

        private static void ShowWithdrawDialog(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("Bank - Tarik Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n\n{{FFFFFF}}Masukkan jumlah uang yang ingin Anda tarik:")
                .WithButtons("Tarik", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowBankMenu(player, accountIndex);
                        return;
                    }

                    if (!int.TryParse(e.InputText, out var amount) || amount <= 0)
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Jumlah penarikan harus lebih dari $0!");
                        ShowWithdrawDialog(player, accountIndex);
                        return;
                    }

                    if (BankService.Withdraw(player, account, amount))
                    {
                        player.SendClientMessage(Color.White,
                            $"{{C6E2FF}}<BANK>{{FFFFFF}} Berhasil menarik {Utilities.GroupDigits(amount)} dari akun {account.AccountName}. Saldo: {Utilities.GroupDigits(account.Balance)}");
                    }
                    else
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Penarikan gagal! Saldo tidak cukup.");
                    }
                });
        }

        private static void ShowTransferAccountDialog(Player player, int accountIndex)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("Bank - Transfer Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n\n{{FFFFFF}}Masukkan nomor rekening tujuan transfer:")
                .WithButtons("Lanjutkan", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowBankMenu(player, accountIndex);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(e.InputText) || e.InputText.Length < 5)
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Nomor rekening tidak valid!");
                        ShowTransferAccountDialog(player, accountIndex);
                        return;
                    }

                    ShowTransferAmountDialog(player, accountIndex, e.InputText);
                });
        }

        private static void ShowTransferAmountDialog(Player player, int accountIndex, string targetAccount)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            player.ShowInput("Bank - Transfer Uang",
                $"Saldo saat ini: {{00FF00}}{Utilities.GroupDigits(account.Balance)}\n{{FFFFFF}}Rekening tujuan: {{FFFF00}}{targetAccount}\n\n{{FFFFFF}}Masukkan jumlah uang yang ingin Anda transfer:")
                .WithButtons("Transfer", "Kembali")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowTransferAccountDialog(player, accountIndex);
                        return;
                    }

                    if (!int.TryParse(e.InputText, out var amount) || amount <= 0)
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Jumlah transfer harus lebih dari $0!");
                        ShowTransferAmountDialog(player, accountIndex, targetAccount);
                        return;
                    }

                    if (account.Balance < amount)
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Saldo Anda tidak cukup untuk transfer ini!");
                        ShowBankMenu(player, accountIndex);
                        return;
                    }

                    ShowTransferConfirmDialog(player, accountIndex, targetAccount, amount);
                });
        }

        private static void ShowTransferConfirmDialog(Player player, int accountIndex, string targetAccount, int amount)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            var message = $"Anda akan mentransfer:\n\n" +
                         $"{{FFFFFF}}Jumlah: {{00FF00}}{Utilities.GroupDigits(amount)}\n" +
                         $"{{FFFFFF}}Dari Rekening: {{FFFF00}}{account.AccountName} ({account.AccountNumber})\n" +
                         $"{{FFFFFF}}Ke Rekening: {{FFFF00}}{targetAccount}\n\n" +
                         $"{{FF0000}}Apakah Anda yakin ingin melanjutkan transfer ini?";

            player.ShowMessage("Bank - Konfirmasi Transfer", message)
                .WithButtons("Ya", "Tidak")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowBankMenu(player, accountIndex);
                        return;
                    }

                    ProcessTransferAsync(player, accountIndex, targetAccount, amount);
                });
        }

        private static async void ProcessTransferAsync(Player player, int accountIndex, string targetAccount, int amount)
        {
            var account = BankService.GetAccount(player, accountIndex);
            if (account == null) return;

            await BankService.TransferAsync(player, account, targetAccount, amount);
        }
    }
}