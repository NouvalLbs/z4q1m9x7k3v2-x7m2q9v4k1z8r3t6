using Discord;
using ProjectSMP.Core.Discords.Models;

namespace ProjectSMP.Core.Discords
{
    public static class DiscordEmbeds
    {
        public static Embed BuildUCPPanel(DiscordConfigs config)
        {
            return new EmbedBuilder()
                .WithTitle(config.UcpPanelTitle)
                .WithDescription(config.UcpPanelDescription)
                .WithColor(Color.Blue)
                .WithThumbnailUrl(config.UcpPanelThumbnailUrl)
                .WithFooter(config.UcpPanelFooterText, config.UcpPanelFooterIconUrl)
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildSuccess(string title, string message)
        {
            return new EmbedBuilder()
                .WithTitle($"✅ {title}")
                .WithDescription(message)
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildError(string title, string message)
        {
            return new EmbedBuilder()
                .WithTitle($"❌ {title}")
                .WithDescription(message)
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildInfo(string title, string message)
        {
            return new EmbedBuilder()
                .WithTitle($"ℹ️ {title}")
                .WithDescription(message)
                .WithColor(Color.Orange)
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildVerificationCode(string username, string code)
        {
            return new EmbedBuilder()
                .WithTitle("🔐 Verification Code")
                .WithDescription($"**Username:** {username}\n**Verification Code:** `{code}`\n\nGunakan code ini untuk aktivasi akun di server.")
                .WithColor(Color.Gold)
                .WithCurrentTimestamp()
                .Build();
        }
    }
}