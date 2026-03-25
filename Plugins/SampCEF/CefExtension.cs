using SampSharp.Core.Callbacks;
using SampSharp.GameMode;
using SampSharp.GameMode.Controllers;
using System;

[assembly: SampSharp.GameMode.SampSharpExtension(typeof(ProjectSMP.Plugins.SampCEF.CefExtension))]

namespace ProjectSMP.Plugins.SampCEF
{
    public class CefExtension : Extension
    {
        public static CefExtension Instance { get; private set; } = null!;

        public event EventHandler<CefInitializeEventArgs>? Initialized;
        public event EventHandler<CefBrowserCreatedEventArgs>? BrowserCreated;

        public override void LoadServices(BaseMode gameMode)
        {
            Instance = this;
            gameMode.Services.AddService(this);
            base.LoadServices(gameMode);
        }

        public override void LoadControllers(BaseMode gameMode, ControllerCollection controllerCollection)
        {
            base.LoadControllers(gameMode, controllerCollection);
        }

        [Callback]
        public void OnCefInitialize(int playerId, int success)
        {
            Initialized?.Invoke(this, new CefInitializeEventArgs(playerId, success == 1));
        }

        [Callback]
        public void OnCefBrowserCreated(int playerId, int browserId, int statusCode)
        {
            BrowserCreated?.Invoke(this, new CefBrowserCreatedEventArgs(playerId, browserId, statusCode));
        }
    }
}