using ProjectSMP.Plugins.CEF;
using System;
using System.Collections.Generic;

public static class CefService
{
    private static readonly CefNatives Natives = CefNatives.Instance;
    private static readonly Dictionary<(int, int), CefBrowser> _browsers = new();

    public static event Action<int, bool> OnInitialized = delegate { };
    public static event Action<int, int, int> OnBrowserCreated = delegate { };

    // ─── C# → PAWN bridge (pakai CallRemoteFunction via SampSharp) ──────────

    public static void OnPlayerConnect(int playerId, string ip)
        => Natives.CallRemoteFunction("_CefPlayerConnect", playerId, ip);

    public static void OnPlayerDisconnect(int playerId)
    {
        foreach (var key in new List<(int, int)>(_browsers.Keys))
            if (key.Item1 == playerId) _browsers.Remove(key);

        Natives.CallRemoteFunction("_CefPlayerDisconnect", playerId);
    }

    public static CefBrowser CreateBrowser(int playerId, int browserId, string url, bool hidden = false, bool focused = true)
    {
        Natives.CallRemoteFunction("_CefCreateBrowser", playerId, browserId, url, hidden ? 1 : 0, focused ? 1 : 0);
        var browser = new CefBrowser(playerId, browserId, url, hidden);
        _browsers[(playerId, browserId)] = browser;
        return browser;
    }

    public static void DestroyBrowser(int playerId, int browserId)
    {
        if (!_browsers.TryGetValue((playerId, browserId), out var b)) return;
        Natives.CallRemoteFunction("_CefDestroyBrowser", playerId, browserId);
        _browsers.Remove((playerId, browserId));
    }

    public static void HideBrowser(int playerId, int browserId, bool hide)
        => Natives.CallRemoteFunction("_CefHideBrowser", playerId, browserId, hide ? 1 : 0);

    public static void FocusBrowser(int playerId, int browserId, bool focused)
        => Natives.CallRemoteFunction("_CefFocusBrowser", playerId, browserId, focused ? 1 : 0);

    public static void LoadUrl(int playerId, int browserId, string url)
        => Natives.CallRemoteFunction("_CefLoadUrl", playerId, browserId, url);

    public static void AlwaysListenKeys(int playerId, int browserId, bool listen)
        => Natives.CallRemoteFunction("_CefAlwaysListenKeys", playerId, browserId, listen ? 1 : 0);

    public static void ToggleDevTools(int playerId, int browserId, bool enabled)
        => Natives.CallRemoteFunction("_CefToggleDevTools", playerId, browserId, enabled ? 1 : 0);

    public static bool PlayerHasPlugin(int playerId)
        => Natives.cef_player_has_plugin(playerId) == 1;

    // ─── Callbacks dari PAWN bridge → C# ────────────────────────────────────

    public static void HandleCefInitialize(int playerId, int success)
        => OnInitialized.Invoke(playerId, success == 1);

    public static void HandleBrowserCreated(int playerId, int browserId, int statusCode)
        => OnBrowserCreated.Invoke(playerId, browserId, statusCode);
}