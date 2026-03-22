using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace ProjectSMP.Core.Discords
{
    public class DiscordCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ucp", "Open UCP Panel")]
        public async Task ShowUCP()
        {
            var embed = DiscordEmbeds.BuildUCPPanel();

            var component = new ComponentBuilder()
                .WithButton("📝 Register", "btn_register", ButtonStyle.Primary, row: 0)
                .WithButton("🔄 Resend Code", "btn_resend", ButtonStyle.Secondary, row: 0)
                .WithButton("🔑 Change Password", "btn_chgpass", ButtonStyle.Danger, row: 1)
                .WithButton("🔓 Reverif", "btn_reverif", ButtonStyle.Success, row: 1)
                .Build();

            await RespondAsync(embed: embed, components: component);
        }

        [ComponentInteraction("btn_register")]
        public async Task HandleRegister()
        {
            var user = Context.User;
            var discordId = user.Id.ToString();

            var existing = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT ucp FROM player_ucp WHERE discordId = @DiscordId LIMIT 1",
                new { DiscordId = discordId });

            if (existing != null)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError("Already Registered",
                    $"Akun Discord Anda sudah terdaftar dengan UCP: **{existing.ucp}**"), ephemeral: true);
                return;
            }

            await DatabaseManager.ExecuteAsync(
                "INSERT INTO discord_actions (user_id, discord_id, action, data) VALUES (@UserId, @DiscordId, 'register', '')",
                new { UserId = (long)user.Id, DiscordId = discordId });

            DiscordEventBus.Publish("discord.register", new { UserId = user.Id, DiscordId = discordId });

            await RespondAsync(embed: DiscordEmbeds.BuildSuccess("Registration Started",
                "Permintaan registrasi Anda telah dikirim! Admin akan segera memproses."), ephemeral: true);
        }

        [ComponentInteraction("btn_resend")]
        public async Task HandleResend()
        {
            var user = Context.User;
            var discordId = user.Id.ToString();

            var ucpData = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT ucp, verifycode FROM player_ucp WHERE discordId = @DiscordId AND password = '' LIMIT 1",
                new { DiscordId = discordId });

            if (ucpData == null)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError("Not Found",
                    "Akun Anda tidak ditemukan atau sudah terverifikasi."), ephemeral: true);
                return;
            }

            var code = ucpData.verifycode.ToString();
            var embed = DiscordEmbeds.BuildVerificationCode(ucpData.ucp, code);

            await RespondAsync(embed: embed, ephemeral: true);
        }

        [ComponentInteraction("btn_chgpass")]
        public async Task HandleChangePassword()
        {
            var user = Context.User;
            var discordId = user.Id.ToString();

            var ucpData = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT ucp FROM player_ucp WHERE discordId = @DiscordId AND password != '' LIMIT 1",
                new { DiscordId = discordId });

            if (ucpData == null)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError("Not Verified",
                    "Akun Anda belum terverifikasi. Silakan aktivasi terlebih dahulu."), ephemeral: true);
                return;
            }

            await DatabaseManager.ExecuteAsync(
                "INSERT INTO discord_actions (user_id, discord_id, action, data) VALUES (@UserId, @DiscordId, 'changepass', @Ucp)",
                new { UserId = (long)user.Id, DiscordId = discordId, Ucp = ucpData.ucp });

            DiscordEventBus.Publish("discord.changepass", new { UserId = user.Id, Ucp = ucpData.ucp });

            await RespondAsync(embed: DiscordEmbeds.BuildInfo("Password Reset",
                "Permintaan reset password telah dikirim. Ikuti instruksi yang diberikan."), ephemeral: true);
        }

        [ComponentInteraction("btn_reverif")]
        public async Task HandleReverif()
        {
            var user = Context.User;
            var discordId = user.Id.ToString();

            await DatabaseManager.ExecuteAsync(
                "INSERT INTO discord_actions (user_id, discord_id, action, data) VALUES (@UserId, @DiscordId, 'reverif', '')",
                new { UserId = (long)user.Id, DiscordId = discordId });

            DiscordEventBus.Publish("discord.reverif", new { UserId = user.Id, DiscordId = discordId });

            await RespondAsync(embed: DiscordEmbeds.BuildInfo("Reverification",
                "Permintaan reverifikasi telah dikirim. Silakan tunggu admin untuk memproses."), ephemeral: true);
        }
    }
}