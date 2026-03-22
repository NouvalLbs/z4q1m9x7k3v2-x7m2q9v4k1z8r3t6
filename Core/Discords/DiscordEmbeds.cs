using Discord;

namespace ProjectSMP.Core.Discords
{
    public static class DiscordEmbeds
    {
        public static Embed BuildUCPPanel()
        {
            return new EmbedBuilder()
                .WithTitle("🎮 State Side UCP Panel")
                .WithDescription("Halo! Selamat datang di server State Side Roleplay! Di sini, Anda akan mendaftar akun UCP (User Control Panel), melakukan verifikasi ulang akun, dan mengirim ulang kode.\n\nJangan ragu untuk bertanya jika Anda membutuhkan bantuan lebih lanjut.")
                .WithColor(Color.Blue)
                .WithThumbnailUrl("https://i.imgur.com/example.png")
                .WithFooter("State Side Roleplay", "https://i.imgur.com/footer.png")
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