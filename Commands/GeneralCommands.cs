using ProjectSMP.Core;
using ProjectSMP.Features.Chat;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Commands
{
    public class GeneralCommands
    {
        [Command("b")]
        public static void LocalOOC(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /b [Text]");
                return;
            }

            var msg = text;
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            if (msg.Length > 64)
            {
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.LocalOOC),
                    $"{Utilities.ReturnName(player)}: (( {msg.Substring(0, 64)} ..");
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.LocalOOC),
                    $".. {msg.Substring(64)} ))");
            }
            else
            {
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.LocalOOC),
                    $"{Utilities.ReturnName(player)} says: (( {msg} ))");
            }
        }

        [Command("ooc", Shortcut = "o")]
        public static void GlobalOOC(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /o(oc) [Text]");
                return;
            }

            if (text.Length > 90)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error> {FFFFFF} Teks yang kamu masukan terlalu panjang, maksimal 90 karakter!");
                return;
            }

            var msg = text;
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            string formatted;
            if (player.AdminOnDuty)
            {
                formatted = $"(( {{ff0000}}Admin {player.Ucp} [{player.Id}]: {{ffffff}}{msg} {{E0FFFF}}))";
            }
            else
            {
                formatted = $"(( {player.Ucp} [{player.Id}]: {{ffffff}}{msg} ))";
            }

            foreach (var p in BasePlayer.All)
            {
                if (p is Player target && target.IsCharLoaded && target.Settings.ToggleChatOOC)
                {
                    target.SendClientMessage(new Color(ChatColors.GlobalOOC), formatted);
                }
            }
        }

        [Command("ab")]
        public static void AboveHead(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /ab [Text]");
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan '/ab off' untuk menonaktifkan atau menghapus tag ab.");
                return;
            }

            if (text.Length > 128)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Teks maksimal hanya bisa sampai 128 karakter.");
                return;
            }

            if (text.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                if (!ChatService.Is3DLabelActive(player, false))
                {
                    player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Kamu belum mengaktifkan teks '{ffea00}ab{FFFFFF}'.");
                    return;
                }

                ChatService.Remove3DLabel(player, false);
                player.SendClientMessage(Color.White, "{ebe6ae}<AB>{FFFFFF} Kamu telah menghapus teks '{ffea00}ab{FFFFFF}'.");
                return;
            }

            var msg = ChatService.MessageFix(text);
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            var labelText = $"* {Utilities.ReturnName(player)} *\n(( OOC : {msg} ))";
            ChatService.CreateOrUpdate3DLabel(player, false, labelText, new Color(ChatColors.Purple));
            player.SendClientMessage(Color.White, "{ebe6ae}<AB>{FFFFFF} Teks telah ditempatkan di lokasimu, untuk menghapusnya gunakan '{ffea00}/ab off{FFFFFF}'.");
        }

        [Command("low", Shortcut = "l")]
        public static void Low(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /l(ow) [Text]");
                return;
            }
            ChatService.TalkMessage(ChatDistance.Low, player, "whispers", text);
        }

        [Command("shout", Shortcut = "s")]
        public static void Shout(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /s(hout) [Text]");
                return;
            }
            ChatService.TalkMessage(ChatDistance.Shout, player, "shouts", text);
            player.ApplyAnimation("ON_LOOKERS", "shout_01", 4.0f, false, false, false, false, 0);
        }

        [Command("whisper", Shortcut = "w")]
        public static void Whisper(Player player, string targetInput, string text)
        {
            if (string.IsNullOrWhiteSpace(targetInput) || string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /w(hisper) [PlayerId/PartOfName] [Text]");
                return;
            }

            var target = Utilities.GetPlayerFromPartOfName(player, targetInput);
            if (target == null) return;

            if (target.Id == player.Id)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Kamu tidak dapat melakukan Whisper kepada dirimu sendiri.");
                return;
            }

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Player tersebut tidak ada di kota saat ini.");
                return;
            }

            ChatService.ProcessWhisper(player, target, text);
        }

        [Command("me")]
        public static void Me(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /me [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.Me, ChatDistance.Normal);
        }

        [Command("lowerme", Shortcut = "lme")]
        public static void LowerMe(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /l(ower)me [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.LowerMe, ChatDistance.Low);
        }

        [Command("ame")]
        public static void Ame(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /ame [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.Ame, ChatDistance.Low);
        }

        [Command("do")]
        public static void Do(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /do [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.Do, ChatDistance.Normal);
        }

        [Command("lowerdo", Shortcut = "ldo")]
        public static void LowerDo(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /l(ower)do [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.LowerDo, ChatDistance.Low);
        }

        [Command("ado")]
        public static void Ado(Player player, string header, string description)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /ado [Text Header] [Text]");
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan '/ado off' untuk menonaktifkan atau menghapus tag ado.");
                return;
            }

            if (header.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                if (!ChatService.Is3DLabelActive(player, true))
                {
                    player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Kamu belum mengaktifkan teks '{ffea00}/ado{FFFFFF}'.");
                    return;
                }

                ChatService.Remove3DLabel(player, true);
                player.SendClientMessage(Color.White, "{ebe6ae}<ADO>{FFFFFF} Kamu telah menghapus teks '{ffea00}/ado{FFFFFF}'.");
                return;
            }

            var desc = ChatService.MessageFix(description ?? "");
            if (player.Settings.ToggleUppercase && desc.Length > 0)
                desc = char.ToUpper(desc[0]) + desc.Substring(1);

            var labelText = $"* [{header}] {desc} *\n(( {Utilities.ReturnName(player)} ))";
            ChatService.CreateOrUpdate3DLabel(player, true, labelText, new Color(ChatColors.Purple));
            player.SendClientMessage(Color.White, "{ebe6ae}<ADO>{FFFFFF} Teks telah ditempatkan di lokasimu, untuk menghapusnya gunakan '{ffea00}/ado off{FFFFFF}'.");
        }

        [Command("try")]
        public static void Try(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /try [Text Action]");
                return;
            }

            var msg = text;
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            var result = new Random().Next(2) == 0 ? "and success" : "but fail";

            if (msg.Length > 64)
            {
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.Purple),
                    $"* {Utilities.ReturnName(player)} {msg.Substring(0, 64)} ..");
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.Purple),
                    $".. {msg.Substring(64)}, {result}");
            }
            else
            {
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.Purple),
                    $"* {Utilities.ReturnName(player)} {msg}, {result}");
            }
        }

        [Command("pm")]
        public static void PrivateMessage(Player player, string targetInput, string text)
        {
            if (string.IsNullOrWhiteSpace(targetInput) || string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Command>{888888} Gunakan /pm [PlayerId/PartOfName] [Text]");
                return;
            }

            var target = Utilities.GetPlayerFromPartOfName(player, targetInput);
            if (target == null) return;

            if (target.Id == player.Id)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Kamu tidak dapat melakukan PM kepada dirimu sendiri.");
                return;
            }

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Player tersebut tidak ada di kota saat ini.");
                return;
            }

            if (!target.Settings.TogglePrivateMessage)
            {
                player.SendClientMessage(Color.White, "{C6E2FF}<Error>{FFFFFF} Player tersebut menonaktifkan private message.");
                return;
            }

            ChatService.ProcessPrivateMessage(player, target, text);
        }

        [Command("help")]
        public static void Help(Player player)
        {
            player.ShowList("General Help", "General Commands", "Chatting Commands")
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    if (e.ListItem == 0) ShowGeneralHelp(player);
                    else ShowChatHelp(player);
                });
        }

        [Command("clearchat", Shortcut = "clear")]
        public static void ClearChat(Player player)
        {
            for (var i = 0; i < 50; i++)
                player.SendClientMessage(Color.White, "");
        }

        [Command("stats")]
        public static void Stats(Player player)
        {
            var gender = player.Gender == 0 ? "Male" : "Female";
            var phoneStatus = player.Phone.Off == 0 ? "{FF0000}Offline{FFFFFF}" : "{91ff00}Online{FFFFFF}";
            var charStatus = player.VerifiedChar == 1 ? "{91ff00}Verified{FFFFFF}" : "{FF0000}Unverified{FFFFFF}";
            var admin = Utilities.GetAdminString(player);
            var warn = Utilities.GetWarningString(player);

            var stats = $@"{{FFFF00}}IC Information:
            {{FFFFFF}}Gender: [{{b8d2ec}}{gender}{{FFFFFF}}] | Birthdate: [{{b8d2ec}}{player.BirthDate}{{FFFFFF}}] | Money: [{{00f000}}{Utilities.GroupDigits(player.CharMoney)}{{FFFFFF}}] | Bank: [{{00f000}}0{{FFFFFF}}]
            {{FFFFFF}}Phone Status: [{phoneStatus}] | Phone Number: [{{ebeb00}}{player.Phone.Number}{{FFFFFF}}] | Phone Credit: [{{ebeb00}}{player.Phone.Credit}{{FFFFFF}}] | Mask ID: [{{b8d2ec}}{player.MaskId}{{FFFFFF}}]
            {{FFFFFF}}Jobs: [None{{FFFFFF}}] | Faction: [Civilian{{FFFFFF}}] | Family: [None]
            {{FFFFFF}}Working at: [None] [None (0){{FFFFFF}}] | Wealth: [None]

            {{FFFF00}}OOC Information:
            {{FFFFFF}}CitizenId: [{{77efc7}}{player.CitizenId}{{FFFFFF}}] | Level: [{{77efc7}}{player.Level}{{FFFFFF}}] | Paychecks: [{{b8d2ec}}{player.Paycheck}{{FFFFFF}}] | Time Played: [{{b8d2ec}}{player.Playtime.Hours} hour(s) {player.Playtime.Minutes} minute(s) {player.Playtime.Seconds} second(s){{FFFFFF}}]
            {{FFFFFF}}Character Story: [{charStatus}] | Staff: [{admin}] | Warns: [{warn}] | Prestige Coin: [0]
            {{FFFFFF}}World: [{{ebeb00}}{player.VirtualWorld}{{FFFFFF}}] | Interior: [{{ebeb00}}{player.Interior}{{FFFFFF}}] | MaxHP: [{{ab0000}}{player.Vitals.MaxHealth:F1}{{FFFFFF}}] | Health: [{{ab0000}}{player.Vitals.Health:F1}{{FFFFFF}}] | Armour: [{{9f9f9f}}{player.Vitals.Armour:F1}{{FFFFFF}}]";

            var title = $"{{6fe0ba}}{player.Username} Statistic {{c8c8c8}}(UCP: {player.Ucp})";
            player.ShowMessage(title, stats).WithButtons("Settings", "Close").Show();
        }

        private static void ShowGeneralHelp(Player player)
        {
            player.ShowTabList("General Player Commands", new[] { "Perintah", "Informasi" })
                .WithRows(
                    new[] { "/help", "Menampilkan daftar perintah bantuan" },
                    new[] { "/i(tems)", "Menampilkan daftar barang yang dimiliki character kamu" },
                    new[] { "/settings", "Mengubah pengaturan character kamu" },
                    new[] { "/hud", "Mengubah tampilan HUD character kamu" },
                    new[] { "/toggle", "Mengaktifkan atau menonaktifkan fitur tertentu" },
                    new[] { "/clear(chat)", "Menghapus semua pesan di chat" }
                )
                .WithButtons("Close", "")
                .Show();
        }

        private static void ShowChatHelp(Player player)
        {
            player.ShowTabList("Chatting Commands", new[] { "Perintah", "Informasi" })
                .WithRows(
                    new[] { "/b [text]", "Chat lokal yang digunakan sebagai komunikasi antar player" },
                    new[] { "/o [text]", "Chat global antar semua player yang online di server" },
                    new[] { "/ab [text]", "Menampilkan pesan label (Out Of Character)" },
                    new[] { "/l(ow) [text]", "Digunakan untuk berbicara dengan radius kecil" },
                    new[] { "/s(hout) [text]", "Digunakan untuk berteriak" },
                    new[] { "/w(hisper) [id] [text]", "Membisikkan sesuatu ke character yang ditentukan" },
                    new[] { "/me [text]", "Menampilkan aktivitas character kamu" },
                    new[] { "/l(ower)me [text]", "Menampilkan aktivitas character dengan radius kecil" },
                    new[] { "/ame [text]", "Menampilkan aktivitas di atas kepala character" },
                    new[] { "/do [text]", "Menampilkan keadaan atau situasi sekitar" },
                    new[] { "/l(ower)do [text]", "Menampilkan keadaan sekitar dengan radius kecil" },
                    new[] { "/ado [title] [text]", "Menampilkan pesan label keadaan character" },
                    new[] { "/try [text]", "Menampilkan percobaan tindakan character" },
                    new[] { "/pm [id] [text]", "Mengirim pesan pribadi ke player lain" }
                )
                .WithButtons("Close", "")
                .Show();
        }
    }
}