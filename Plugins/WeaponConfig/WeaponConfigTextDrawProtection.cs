using SampSharp.GameMode.Display;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public static class WeaponConfigTextDrawProtection
    {
        public static bool CanDestroyTextDraw(TextDraw td)
        {
            return !WeaponConfigHealthBar.IsInternalTextDraw(td);
        }

        public static bool CanDestroyPlayerTextDraw(PlayerTextDraw ptd, BasePlayer player)
        {
            var playerId = player.Id;
            return !WeaponConfigHealthBar.IsInternalPlayerTextDraw(ptd, playerId) &&
                   !WeaponConfigDamageFeed.IsInternalPlayerTextDraw(ptd, playerId);
        }

        public static bool CanModifyTextDraw(TextDraw td)
        {
            return !WeaponConfigHealthBar.IsInternalTextDraw(td);
        }

        public static bool CanModifyPlayerTextDraw(PlayerTextDraw ptd, BasePlayer player)
        {
            var playerId = player.Id;
            return !WeaponConfigHealthBar.IsInternalPlayerTextDraw(ptd, playerId) &&
                   !WeaponConfigDamageFeed.IsInternalPlayerTextDraw(ptd, playerId);
        }
    }
}