namespace ProjectSMP.Entities.Players.Needs
{
    public static class NeedsService
    {
        public static void Initialize()
        {
            NeedsUpdateService.Initialize();
            NeedsEffectService.Initialize();
        }

        public static void Dispose()
        {
            NeedsUpdateService.Dispose();
            NeedsEffectService.Dispose();
        }

        public static void OnPlayerSpawn(Player player)
        {
            NeedsUpdateService.RegisterPlayer(player);
            NeedsEffectService.RegisterPlayer(player);
            NeedsHudManager.Initialize(player);
        }

        public static void OnPlayerDisconnect(Player player)
        {
            NeedsUpdateService.UnregisterPlayer(player);
            NeedsEffectService.UnregisterPlayer(player);
            NeedsHudManager.Cleanup(player);
        }

        public static void SetHunger(Player player, float value)
        {
            if (value > 100) value = 100;
            else if (value < 0) value = 0;

            player.Vitals.Hunger = value;
            NeedsHudManager.UpdateHud(player);
        }

        public static void SetEnergy(Player player, float value)
        {
            if (value > 100) value = 100;
            else if (value < 0) value = 0;

            player.Vitals.Energy = value;
            NeedsHudManager.UpdateHud(player);
        }

        public static void RefreshHud(Player player)
        {
            NeedsHudManager.RegenerateHud(player);
        }
    }
}