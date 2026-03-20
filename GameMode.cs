using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator;
using ProjectSMP.Entities.Players.Condition;
using ProjectSMP.Entities.Players.Needs;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Dynamic.DynamicDoor;
using ProjectSMP.Features.Dynamic.DynamicPickups;
using ProjectSMP.Features.LevelSystem;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Plugins.Anticheat;
using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.GarageBlocker;
using ProjectSMP.Plugins.RealtimeClock;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Threading.Tasks;

namespace ProjectSMP
{
    public class GameMode : BaseMode {
        private AnticheatPlugin _anticheat = null!;

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            // Initialize Weapon Config
            var (wcCfg, wcWeapons) = WeaponConfigLoader.Load();
            WeaponConfigService.Init(wcCfg, wcWeapons);
            WeaponConfigHealthBar.Init();

            // Initialize Anticheat
            _anticheat = AnticheatPlugin.Create(configPath: "AntiCheat.json", weaponConfigMode: true);
            _anticheat.RegisterEvents(this);
            _anticheat.Warnings.PunishmentRequired += OnAnticheatPunishment;

            // Initialize Safe Extensions
            SafeServerExtensions.Initialize(_anticheat);
            SafeVehicleExtensions.Initialize(_anticheat);
            SafeNativeExtensions.Initialize(_anticheat);

            // Initialize Primary Config
            ConfigManager.Load();
            ConfigManager.ApplyGameConfig(this);

            // Initialize Localization
            LocalizationManager.Load();

            // Initialize PreviewModelDialog
            PreviewModelDialog.Init();

            // Initialize TextDrawManager
            TextDrawManager.Init();

            // Initialize Garage Blocker
            GarageBlockerService.Init();

            // Initialize Database Manager
            Task.Run(DatabaseManager.InitAsync).GetAwaiter().GetResult();

            // Initialize RealtimeClock
            RealtimeClockService.Init();
            RealtimeClockService.SetInterval(10000, restartTimer: false);
            RealtimeClockService.Sync(serverTime: true);
            
            // Initialize Needs Service
            NeedsService.Initialize();

            // Initialize Condition Service
            ConditionService.Initialize();

            // Initialize Jail Service
            JailService.Initialize();

            // Initialize Playing Time Service
            PlaytimeService.Initialize();

            // Initialize Dynamic Pickups
            PickupService.Initialize();
            var pickupDataList = Task.Run(PickupService.LoadDataAsync).GetAwaiter().GetResult();
            PickupService.CreatePickupObjects(pickupDataList);

            // Initialize Dynamic Doors
            DoorService.Initialize();
            var doorDataList = Task.Run(DoorService.LoadDataAsync).GetAwaiter().GetResult();
            DoorService.CreateDoorObjects(doorDataList);
        }

        private void OnAnticheatPunishment(int playerId, string checkName, PunishAction action)
        {
            var player = BasePlayer.Find(playerId);
            if (player is null) return;

            string message = $"{{FF0000}}[ANTICHEAT] {checkName}";

            switch (action)
            {
                case PunishAction.Kick:
                    player.SendClientMessage(Color.Red, message);
                    Console.WriteLine($"[AC-KICK] {player.Name} (ID:{playerId}) - {checkName}");

                    // Delay kick agar message terkirim
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        BasePlayer.Find(playerId)?.Kick();
                    });
                    break;

                case PunishAction.Ban:
                    player.SendClientMessage(Color.Red, message);
                    Console.WriteLine($"[AC-BAN] {player.Name} (ID:{playerId}) - {checkName}");

                    // Delay ban agar message terkirim
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        BasePlayer.Find(playerId)?.Ban();
                    });
                    break;
            }
        }

        protected override void OnPlayerCommandText(BasePlayer player, CommandTextEventArgs e)
        {
            base.OnPlayerCommandText(player, e);

            if (!e.Success && player is Player p) {
                p.SendClientMessage(Color.White, $"{{b9b9b9}}Command '{e.Text}' tidak ada, gunakan '/help'.");
                e.Success = true;
            }
        }

        protected override void OnVehicleSpawned(BaseVehicle vehicle, EventArgs e)
        {
            base.OnVehicleSpawned(vehicle, e);
            WeaponConfigService.OnVehicleSpawn(vehicle.Id);
        }

        protected override void OnVehicleDied(BaseVehicle vehicle, PlayerEventArgs e)
        {
            base.OnVehicleDied(vehicle, e);
            WeaponConfigService.OnVehicleDeath(vehicle.Id);
        }

        protected override void OnExited(EventArgs e) {
            WeaponConfigHealthBar.Dispose();
            PreviewModelDialog.Dispose();
            RealtimeClockService.Dispose();
            NeedsService.Dispose();
            ConditionService.Dispose();
            JailService.Dispose();
            PlaytimeService.Dispose();
            base.OnExited(e);
        }
    }
}