using ProjectSMP.Core;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Plugins.GarageBlocker;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Threading.Tasks;

namespace ProjectSMP
{
    public class GameMode : BaseMode {
        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            // Initialize Primary Config
            ConfigManager.Load();
            ConfigManager.ApplyGameConfig(this);

            // Initialize Localization
            LocalizationManager.Load();

            // Initialize PreviewModelDialog
            PreviewModelDialog.Init();

            // Initialize Weapon Config
            var (wcCfg, wcWeapons) = WeaponConfigLoader.Load();
            WeaponConfigService.Init(wcCfg, wcWeapons);
            WeaponConfigHealthBar.Init();

            // Initialize Garage Blocker
            GarageBlockerService.Init();

            // Initialize Database Manager
            Task.Run(DatabaseManager.InitAsync).GetAwaiter().GetResult();
        }

        private void OnVehicleSpawned(object sender, EventArgs e)
        {
            if (sender is BaseVehicle vehicle)
                WeaponConfigService.OnVehicleSpawn(vehicle.Id);
        }

        private void OnVehicleDeath(object sender, PlayerEventArgs e)
        {
            if (sender is BaseVehicle vehicle)
                WeaponConfigService.OnVehicleDeath(vehicle.Id);
        }

        protected override void OnExited(EventArgs e) {
            WeaponConfigHealthBar.Dispose();
            PreviewModelDialog.Dispose();
            base.OnExited(e);
        }
    }
}