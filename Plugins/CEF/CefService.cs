using ProjectSMP.Plugins.CEF;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.CEF
{
    public static class CefService
    {
        private static readonly CefNatives Natives = CefNatives.Instance;
        private static readonly Dictionary<(int, int), CefBrowser> _browsers = new();

        public static event Action<int, bool> OnInitialized = delegate { };
        public static event Action<int, int, int> OnBrowserCreated = delegate { };

        public static void OnPlayerConnect(int playerId, string ip)
            => Natives.CefOnPlayerConnect(playerId, ip);

        public static void OnPlayerDisconnect(int playerId)
        {
            foreach (var key in new List<(int, int)>(_browsers.Keys))
                if (key.Item1 == playerId) _browsers.Remove(key);

            Natives.CefOnPlayerDisconnect(playerId);
        }

        public static CefBrowser CreateBrowser(int playerId, int browserId, string url, bool hidden = false, bool focused = true)
        {
            var browser = new CefBrowser(playerId, browserId, url, hidden, focused);
            _browsers[(playerId, browserId)] = browser;
            return browser;
        }

        public static void DestroyBrowser(int playerId, int browserId)
        {
            if (!_browsers.TryGetValue((playerId, browserId), out var b)) return;
            b.Destroy();
            _browsers.Remove((playerId, browserId));
        }

        public static CefBrowser? GetBrowser(int playerId, int browserId)
            => _browsers.TryGetValue((playerId, browserId), out var b) ? b : null;

        public static void HideBrowser(int playerId, int browserId, bool hide)
            => Natives.CefHideBrowser(playerId, browserId, hide);

        public static void FocusBrowser(int playerId, int browserId, bool focused)
            => Natives.CefFocusBrowser(playerId, browserId, focused);

        public static void LoadUrl(int playerId, int browserId, string url)
            => Natives.CefLoadUrl(playerId, browserId, url);

        public static void AlwaysListenKeys(int playerId, int browserId, bool listen)
            => Natives.CefAlwaysListenKeys(playerId, browserId, listen);

        public static void ToggleDevTools(int playerId, int browserId, bool enabled)
            => Natives.CefToggleDevTools(playerId, browserId, enabled);

        public static bool PlayerHasPlugin(int playerId)
            => Natives.CefPlayerHasPlugin(playerId) == 1;

        public static void HandleCefInitialize(int playerId, int success)
            => OnInitialized.Invoke(playerId, success == 1);

        public static void HandleBrowserCreated(int playerId, int browserId, int statusCode)
            => OnBrowserCreated.Invoke(playerId, browserId, statusCode);
    }
}