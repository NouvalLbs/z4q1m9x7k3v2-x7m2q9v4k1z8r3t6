using ProjectSMP.Core;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Plugins.GarageBlocker;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using System;

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

            // Initialize Database Manager
            _ = DatabaseManager.InitAsync();

            // Initialize Garage Blocker
            GarageBlockerService.Init();
        }

        protected override void OnExited(EventArgs e) {
            WeaponConfigHealthBar.Dispose();
            PreviewModelDialog.Dispose();
            base.OnExited(e);
        }
    }
}