using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.CEF
{
    public static class CefService
    {
        private static readonly CefNatives Natives = CefNatives.Instance;

        private static readonly Dictionary<(int playerId, int browserId), CefBrowser> _browsers = new();

        public static event Action<int, bool>      OnInitialized   = delegate { };
        public static event Action<int, int, int>  OnBrowserCreated = delegate { };

        public static CefBrowser CreateBrowser(int playerId, int browserId, string url, bool hidden = false, bool focused = true)
        {
            var browser = new CefBrowser(playerId, browserId, url, hidden, focused);
            _browsers[(playerId, browserId)] = browser;
            return browser;
        }

        public static CefBrowser CreateExtBrowser(int playerId, int browserId, string texture, string url, int scale = 1)
        {
            Natives.cef_create_ext_browser(playerId, browserId, texture, url, scale);
            var browser = new CefBrowser(playerId, browserId, url);
            _browsers[(playerId, browserId)] = browser;
            return browser;
        }

        public static void DestroyBrowser(int playerId, int browserId)
        {
            if (!_browsers.TryGetValue((playerId, browserId), out var b)) return;
            b.Destroy();
            _browsers.Remove((playerId, browserId));
        }

        public static void DestroyBrowser(CefBrowser browser)
            => DestroyBrowser(browser.PlayerId, browser.BrowserId);

        public static CefBrowser GetBrowser(int playerId, int browserId)
            => _browsers.GetValueOrDefault((playerId, browserId));

        public static bool TryGetBrowser(int playerId, int browserId, out CefBrowser browser)
            => _browsers.TryGetValue((playerId, browserId), out browser);

        public static void OnPlayerConnect(int playerId, string ip)
            => Natives.cef_on_player_connect(playerId, ip);

        public static void OnPlayerDisconnect(int playerId)
        {
            foreach (var key in new List<(int, int)>(_browsers.Keys))
                if (key.Item1 == playerId) _browsers.Remove(key);

            Natives.cef_on_player_disconnect(playerId);
        }

        public static bool PlayerHasPlugin(int playerId)
            => Natives.cef_player_has_plugin(playerId) == 1;

        public static void Subscribe(string eventName, string callback)
            => Natives.cef_subscribe(eventName, callback);

        public static void EmitEvent(int playerId, string eventName, params CefValue[] args)
            => Natives.cef_emit_event(playerId, eventName, args.Flatten());

        public static void HandleCefInitialize(int playerId, int success)
            => OnInitialized?.Invoke(playerId, success == 1);

        public static void HandleBrowserCreated(int playerId, int browserId, int statusCode)
            => OnBrowserCreated?.Invoke(playerId, browserId, statusCode);
    }
}