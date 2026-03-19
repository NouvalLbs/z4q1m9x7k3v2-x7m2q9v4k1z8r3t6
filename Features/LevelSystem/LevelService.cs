using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Features.LevelSystem
{
    public static class LevelService
    {
        public static int GetPointsRequired(int level)
        {
            return 3 + ((level - 1) / 5 * 3);
        }

        public static int GetExpRequired(int level)
        {
            return GetPointsRequired(level) * 100;
        }

        public static bool AddExp(Player player, int expAmount)
        {
            var expRequired = GetExpRequired(player.Level);

            if (player.LevelPoints >= GetPointsRequired(player.Level))
                return false;

            player.LevelPointsExp += expAmount;

            if (player.LevelPointsExp >= expRequired)
                player.LevelPointsExp = expRequired;

            player.SendClientMessage(Color.Yellow, $"{{FFFF00}}✅ Kamu mendapatkan {expAmount} EXP!");
            return true;
        }

        public static string CreateProgressBar(int current, int max, bool completed)
        {
            if (completed)
                return "{00FF00}llllllllll";

            var progress = (int)((float)current / max * 10);
            var bar = "{00FF00}";

            for (var i = 0; i < 10; i++)
            {
                if (i < progress)
                    bar += "l";
                else
                {
                    if (i == progress) bar += "{FFFFFF}";
                    bar += "l";
                }
            }

            return bar;
        }

        public static bool CanLevelUp(Player player)
        {
            return player.LevelPoints >= GetPointsRequired(player.Level) &&
                   player.LevelPointsExp >= GetExpRequired(player.Level);
        }

        public static void LevelUp(Player player)
        {
            if (!CanLevelUp(player))
                return;

            player.Level++;
            player.LevelPoints = 0;
            player.LevelPointsExp = 0;

            player.SendClientMessage(Color.Yellow, $"{{00FF00}}🎉 Selamat! Kamu naik ke Level {player.Level}!");
        }
    }
}