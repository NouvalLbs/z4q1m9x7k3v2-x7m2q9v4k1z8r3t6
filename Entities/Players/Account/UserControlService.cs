
#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Account.Data;
using ProjectSMP.Entities.Players.Character;
using SampSharp.GameMode.Definitions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSMP.Entities.Players.Account
{
    internal static class UserControlService
    {
        private const string Table = "player_ucp";
        private const int MaxAttempts = 3;
        private const int LoginTimeoutMs = 60_000;
        private const int InitDelayMs = 1_500;

        private static readonly Dictionary<int, PlayerUcpData> _sessions = new();
        private static readonly Dictionary<int, CancellationTokenSource> _kickTimers = new();
        private static readonly Regex _passwordPattern =
            new(@"^[A-Za-z0-9\[\]()\._@#]+$", RegexOptions.Compiled);

        // ── Public API ────────────────────────────────────────────────────────

        public static async void InitAsync(Player player)
        {
            await Task.Delay(InitDelayMs);
            if (player.IsDisposed) return;

            ResetSession(player);

            if (player.Name.Contains('_'))
            {
                ShowInvalidNameDialog(player);
                ScheduleKick(player, 3_000);
                return;
            }

            var data = await DatabaseManager.QueryFirstAsync<PlayerUcpData>(
                $"SELECT `ucp` AS UCP, `password` AS Password, " +
                $"`discordId` AS DiscordId, `verifycode` AS VerifyCode " +
                $"FROM `{Table}` WHERE `ucp` = @Ucp LIMIT 1",
                new { Ucp = player.Name });

            if (player.IsDisposed) return;

            if (data is null)
            {
                ShowNotRegisteredDialog(player);
                ScheduleKick(player, 1_000);
                return;
            }

            var session = _sessions[player.Id];
            session.UCP = data.UCP;
            session.Password = data.Password;
            session.DiscordId = data.DiscordId;
            session.VerifyCode = data.VerifyCode;
            session.LoginAttempt = MaxAttempts;

            ScheduleKick(player, LoginTimeoutMs);

            if (string.IsNullOrEmpty(data.Password))
                ShowActivateDialog(player);
            else
                ShowLoginDialog(player);
        }

        public static void Cleanup(Player player)
        {
            _sessions.Remove(player.Id);
            CancelKickTimer(player);
        }

        public static PlayerUcpData? GetSession(Player player)
            => _sessions.TryGetValue(player.Id, out var s) ? s : null;

        // ── Dialog Presenters ─────────────────────────────────────────────────

        private static void ShowInvalidNameDialog(Player player)
        {
            DialogManager.ShowMessage(player,
                "UCP - Invalid Account",
                $"{{ffffff}}\nUCP: {{dec000}}{player.Name}\n" +
                $"{{ffffff}}Status UCP: {{de0000}}UCP Tidak Valid\n\n" +
                $"{{ffffff}}Silakan gunakan nama UCP Kamu, bukan nama Roleplay Kamu",
                "OK");
        }

        private static void ShowNotRegisteredDialog(Player player)
        {
            DialogManager.ShowMessage(player,
                "UCP - Not Registered",
                $"{{ffffff}}\nUCP: {{dec000}}{player.Name}\n" +
                $"{{ffffff}}Status UCP: {{de0000}}Belum Terdaftar\n\n" +
                $"{{ffffff}}Silakan daftarkan UCP Kamu terlebih dahulu.\n" +
                $"Untuk mendaftar UCP Kamu, silakan kunjungi Discord kami",
                "OK");
        }

        private static void ShowActivateDialog(Player player, string? error = null)
        {
            var session = _sessions[player.Id];
            var body =
                $"{{ffffff}}\nAkun Ini {{de0000}}Belum Aktif!{{ffffff}}\n" +
                $"UCP: {{dec000}}{session.UCP}\n\n" +
                $"{{ffffff}}Silakan masukkan PIN Kamu untuk mengaktifkan Akun" +
                (error is not null ? $"\n{{FF0000}}{error}" : "");

            DialogManager.ShowInput(player, "UCP - Account Activation", body,
                btnLeft: "Input", btnRight: "Cancel",
                onResponse: e =>
                {
                    if (e.DialogButton != DialogButton.Left) { KickDelayed(player); return; }
                    HandleActivate(player, e.InputText);
                });
        }

        private static void ShowRegisterDialog(Player player, string? error = null)
        {
            var body = error switch
            {
                null =>
                    "Sekarang, silakan masukkan password yang valid\n" +
                    "Simbol Kata Sandi yang Valid: A-Z, a-z, 0-9, _, [ ], () dan Panjang Minimum Kata Sandi adalah 6 karakter",
                "short" =>
                    "Sekarang, silakan masukkan password yang valid\n" +
                    "Simbol Kata Sandi yang Valid: A-Z, a-z, 0-9, _, [ ], ()\n" +
                    "{FF0000}Panjang Minimum Kata Sandi adalah 6 karakter",
                _ =>
                    "Sekarang, silakan masukkan password yang valid\n" +
                    "Simbol Kata Sandi yang Valid: A-Z, a-z, 0-9, _, [ ], () dan Panjang Minimum Kata Sandi adalah 6 karakter\n" +
                    "{FF0000}Password yang Kamu gunakan mengandung karakter yang tidak valid"
            };

            DialogManager.ShowInput(player, "UCP - Account Registration", body,
                isPassword: true, btnLeft: "Register", btnRight: "Abort",
                onResponse: e =>
                {
                    if (e.DialogButton != DialogButton.Left) { KickDelayed(player); return; }
                    HandleRegisterAsync(player, e.InputText);
                });
        }

        private static void ShowLoginDialog(Player player)
        {
            var session = _sessions[player.Id];
            var body =
                $"{{ffffff}}\nAkun Ini {{8feb34}}Aktif!{{ffffff}}\n" +
                $"UCP: {{34ebc0}}{session.UCP}\n\n" +
                $"{{ffffff}}Kamu masih memiliki {{dec000}}{session.LoginAttempt} percobaan\n" +
                $"{{ffffff}}Kamu memiliki 60 detik, jadi silakan masukkan password Kamu";

            DialogManager.ShowInput(player, "Welcome to PrestigeSMP", body,
                isPassword: true, btnLeft: "Login", btnRight: "Abort",
                onResponse: e =>
                {
                    if (e.DialogButton != DialogButton.Left) { KickDelayed(player); return; }
                    HandleLogin(player, e.InputText);
                });
        }

        // ── Dialog Handlers ───────────────────────────────────────────────────

        private static void HandleActivate(Player player, string pin)
        {
            var session = GetSession(player);
            if (session is null) return;

            if (string.IsNullOrWhiteSpace(pin) || !int.TryParse(pin, out var pinValue))
            {
                ShowActivateDialog(player, "PIN harus berisi 6 digit, bukan huruf");
                return;
            }

            if (pinValue != session.VerifyCode)
            {
                ShowActivateDialog(player, "PIN yang Kamu masukkan tidak benar");
                return;
            }

            ShowRegisterDialog(player);
        }

        private static async void HandleRegisterAsync(Player player, string password)
        {
            var session = GetSession(player);
            if (session is null) return;

            if (password.Length < 6)
            {
                ShowRegisterDialog(player, "short");
                return;
            }

            if (!_passwordPattern.IsMatch(password))
            {
                ShowRegisterDialog(player, "invalid");
                return;
            }

            var hashed = HashPassword(password);

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET `password` = @Password WHERE `ucp` = @Ucp",
                new { Password = hashed, Ucp = session.UCP });

            if (player.IsDisposed) return;

            session.Password = hashed;
            CancelKickTimer(player);
            CharacterService.CheckPlayerCharAsync(player);
        }

        private static void HandleLogin(Player player, string password)
        {
            var session = GetSession(player);
            if (session is null) return;

            if (HashPassword(password) != session.Password)
            {
                session.LoginAttempt--;

                if (session.LoginAttempt <= 0)
                {
                    KickDelayed(player);
                    return;
                }

                ShowLoginDialog(player);
                return;
            }

            CancelKickTimer(player);
            CharacterService.CheckPlayerCharAsync(player);
        }

        // ── Session ───────────────────────────────────────────────────────────

        private static void ResetSession(Player player)
        {
            _sessions[player.Id] = new PlayerUcpData();
            CancelKickTimer(player);
        }

        // ── Kick Helpers ──────────────────────────────────────────────────────

        private static void ScheduleKick(Player player, int delayMs)
        {
            CancelKickTimer(player);
            var cts = new CancellationTokenSource();
            _kickTimers[player.Id] = cts;
            KickAfterAsync(player, delayMs, cts.Token);
        }

        private static async void KickAfterAsync(Player player, int delayMs, CancellationToken ct)
        {
            try
            {
                await Task.Delay(delayMs, ct);
                if (!player.IsDisposed) player.Kick();
            }
            catch (OperationCanceledException) { }
        }

        private static void KickDelayed(Player player)
        {
            CancelKickTimer(player);
            ScheduleKick(player, 200);
        }

        private static void CancelKickTimer(Player player)
        {
            if (!_kickTimers.TryGetValue(player.Id, out var cts)) return;
            cts.Cancel();
            _kickTimers.Remove(player.Id);
        }

        // ── Utilities ─────────────────────────────────────────────────────────

        private static string HashPassword(string raw)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            var sb = new StringBuilder(64);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}