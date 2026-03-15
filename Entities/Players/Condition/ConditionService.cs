using ProjectSMP.Entities.Players.Needs;
using ProjectSMP.Extensions;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Condition
{
    public static class ConditionService
    {
        private static SampSharp.GameMode.SAMP.Timer _timer;
        private static readonly Dictionary<int, bool> _hospitalRespawn = new();

        public static void Initialize()
        {
            _timer = new SampSharp.GameMode.SAMP.Timer(1000, true);
            _timer.Tick += OnTimerTick;

            WeaponConfigService.PlayerPrepareDeath += OnPlayerPrepareDeath;
            WeaponConfigService.PlayerDeathFinished += OnPlayerDeathFinished;
        }

        public static void Dispose()
        {
            if (_timer != null && _timer.IsRunning)
                _timer.Dispose();

            _hospitalRespawn.Clear();

            WeaponConfigService.PlayerPrepareDeath -= OnPlayerPrepareDeath;
            WeaponConfigService.PlayerDeathFinished -= OnPlayerDeathFinished;
        }

        public static void RegisterPlayer(Player player)
        {
            _hospitalRespawn[player.Id] = false;
        }

        public static void UnregisterPlayer(Player player)
        {
            _hospitalRespawn.Remove(player.Id);
            DeathScreenManager.Destroy(player);
        }

        public static void HandleDeath(Player player)
        {
            if (player.Condition.DyingTime <= 0 || player.Condition.DyingTime > 3420)
                player.Condition.DyingTime = 3600;

            _hospitalRespawn[player.Id] = true;
            player.SpawnPlayerSafe();
        }

        private static void OnTimerTick(object sender, System.EventArgs e)
        {
            foreach (var player in BasePlayer.All)
            {
                if (player is not Player p || !p.IsCharLoaded) continue;

                ProcessDeathHandler(p);
            }
        }

        private static void ProcessDeathHandler(Player player)
        {
            if (player.Condition.DyingTime > 0)
            {
                player.Condition.DyingTime--;
                if (player.Condition.DyingTime < 0)
                    player.Condition.DyingTime = 0;
            }

            var uptime = player.Condition.DyingTime;
            var dyingCount = GetDyingTime(uptime);
            var isInjured = player.Condition.Injured;

            if (isInjured == 1)
            {
                var minutes = dyingCount / 60;
                var seconds = dyingCount % 60;

                DeathScreenManager.UpdateTimer(player, minutes, seconds);

                if (uptime <= 3420)
                {
                    if (uptime == 3420)
                    {
                        player.SendClientMessage(Color.White, "{C6E2FF}<Death>{ffffff} Sekarang kamu bisa respawn, ketik {FFFF00}/death{ffffff} untuk respawn ke rumah sakit terdekat.");
                    }
                    DeathScreenManager.UpdateStatus(player, true);
                }

                if (uptime == 0)
                {
                    _hospitalRespawn[player.Id] = true;
                    player.SpawnPlayerSafe();
                }
            }
        }

        private static void OnPlayerPrepareDeath(object sender, PrepareDeathArgs e)
        {
            if (e.Player.Condition.Injured == 0)
            {
                SaveDeathPosition(e.Player);
            }

            e.Player.ToggleControllableSafe(false);
            e.Player.Condition.Injured = 1;

            NeedsHudManager.HideHud(e.Player);
            DeathScreenManager.Destroy(e.Player);
            DeathScreenManager.Create(e.Player);

            if (e.Player.Condition.DyingTime == 0)
                e.Player.Condition.DyingTime = 3600;

            e.RespawnTime = 60 * 60 * 1000;
        }

        private static void OnPlayerDeathFinished(object sender, DeathFinishedArgs e)
        {
            if (_hospitalRespawn.TryGetValue(e.Player.Id, out var isHospital) && isHospital)
            {
                AfterDeathInit(e.Player);
            }
            else
            {
                AfterReviveInit(e.Player);
            }
        }

        private static void SaveDeathPosition(Player player)
        {
            var pos = player.Position;
            player.CharSpawnPos.X = pos.X;
            player.CharSpawnPos.Y = pos.Y;
            player.CharSpawnPos.Z = pos.Z;
            player.CharSpawnPos.A = player.Angle;
            player.CharSpawnPos.Interior = player.Interior;
            player.CharSpawnPos.World = player.GetVirtualWorldSafe();
        }

        private static void AfterDeathInit(Player player)
        {
            player.Condition.Injured = 0;

            player.Vitals.Hunger = 50;
            player.Vitals.Energy = 50;

            player.SetHealthSafe(50, 0);

            for (var i = 0; i < 15; i++)
                player.SendClientMessage(Color.White, "");

            player.SendClientMessage(Color.White, "{C6E2FF}<Death>{FFFFFF} Kamu telah menyerah dan menerima kematianmu.");
            player.SendClientMessage(Color.White, "{F4C2C2}<Hospital>{FFFFFF} Kamu telah keluar dari rumah sakit, kamu membayar $10.50 kerumah sakit.");

            player.ToggleControllableSafe(true);
            player.SetVirtualWorldSafe(0);
            player.SetInteriorSafe(0);

            player.ClearAnimationsSafe();
            player.Condition.DyingTime = 0;

            NeedsHudManager.Initialize(player);
            DeathScreenManager.Destroy(player);
            _hospitalRespawn[player.Id] = false;
        }

        private static void AfterReviveInit(Player player)
        {
            player.Condition.Injured = 0;

            player.SetHealthSafe(100, 0);
            player.ToggleControllableSafe(true);

            player.SetVirtualWorldSafe(player.CharSpawnPos.World);
            player.SetInteriorSafe(player.CharSpawnPos.Interior);

            player.ClearAnimationsSafe();
            player.Condition.DyingTime = 0;

            NeedsHudManager.Initialize(player);
            DeathScreenManager.Destroy(player);
            _hospitalRespawn[player.Id] = false;
        }

        public static void RestoreDeathState(Player player)
        {
            if (player.Condition.Injured == 1 && player.Condition.DyingTime > 0)
            {
                player.ToggleControllableSafe(false);
                player.ResetWeaponsSafe();

                var animLib = "PED";
                var animName = "FLOOR_HIT";
                player.ApplyAnimationSafe(animLib, animName, 4.0f, false, false, false, true, 0, true);

                NeedsHudManager.HideHud(player);
                DeathScreenManager.Create(player);

                var dyingCount = GetDyingTime(player.Condition.DyingTime);
                var minutes = dyingCount / 60;
                var seconds = dyingCount % 60;
                DeathScreenManager.UpdateTimer(player, minutes, seconds);

                if (player.Condition.DyingTime <= 3420)
                {
                    DeathScreenManager.UpdateStatus(player, true);
                    player.SendClientMessage(Color.White, "{C6E2FF}<Death>{ffffff} Kamu masih dalam kondisi sekarat. Ketik {FFFF00}/death{ffffff} untuk respawn.");
                }
                else
                {
                    DeathScreenManager.UpdateStatus(player, false);
                }
            }
        }

        private static int GetDyingTime(int uptime)
        {
            return uptime / 60;
        }
    }
}