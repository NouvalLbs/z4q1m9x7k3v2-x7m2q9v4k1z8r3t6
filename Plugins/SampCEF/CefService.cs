namespace ProjectSMP.Plugins.SampCEF
{
    public static class CefService
    {
        private const int CEF_STRING = 0;
        private const int CEF_INTEGER = 1;
        private const int CEF_FLOAT = 2;

        private const float DEFAULT_MAX_DIST = 50.0f;
        private const float DEFAULT_REF_DIST = 15.0f;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        public static void NotifyConnect(int playerId, string ip)
            => CefNatives.Instance.cef_on_player_connect(playerId, ip);

        public static void NotifyDisconnect(int playerId)
            => CefNatives.Instance.cef_on_player_disconnect(playerId);

        // ── Browser ────────────────────────────────────────────────────────────

        public static void CreateBrowser(int playerId, int browserId, string url, bool hidden = false, bool focused = true)
            => CefNatives.Instance.cef_create_browser(playerId, browserId, url, hidden, focused);

        public static void DestroyBrowser(int playerId, int browserId)
            => CefNatives.Instance.cef_destroy_browser(playerId, browserId);

        public static void HideBrowser(int playerId, int browserId, bool hide)
            => CefNatives.Instance.cef_hide_browser(playerId, browserId, hide);

        public static void FocusBrowser(int playerId, int browserId, bool focused)
            => CefNatives.Instance.cef_focus_browser(playerId, browserId, focused);

        public static void LoadUrl(int playerId, int browserId, string url)
            => CefNatives.Instance.cef_load_url(playerId, browserId, url);

        public static void AlwaysListenKeys(int playerId, int browserId, bool listen)
            => CefNatives.Instance.cef_always_listen_keys(playerId, browserId, listen);

        public static void ToggleDevTools(int playerId, int browserId, bool enabled)
            => CefNatives.Instance.cef_toggle_dev_tools(playerId, browserId, enabled);

        // ── 3D / Object ────────────────────────────────────────────────────────

        public static void CreateExtBrowser(int playerId, int browserId, string texture, string url, int scale = 1)
            => CefNatives.Instance.cef_create_ext_browser(playerId, browserId, texture, url, scale);

        public static void AppendToObject(int playerId, int browserId, int objectId)
            => CefNatives.Instance.cef_append_to_object(playerId, browserId, objectId);

        public static void RemoveFromObject(int playerId, int browserId, int objectId)
            => CefNatives.Instance.cef_remove_from_object(playerId, browserId, objectId);

        // ── Audio ──────────────────────────────────────────────────────────────

        public static void SetAudioSettings(int playerId, int browserId,
            float maxDistance = DEFAULT_MAX_DIST, float referenceDistance = DEFAULT_REF_DIST)
            => CefNatives.Instance.cef_set_audio_settings(playerId, browserId, maxDistance, referenceDistance);

        // ── Plugin Check ───────────────────────────────────────────────────────

        public static bool HasPlugin(int playerId)
            => CefNatives.Instance.cef_player_has_plugin(playerId) == 1;

        // ── Events ─────────────────────────────────────────────────────────────

        public static void Subscribe(string eventName, string callback)
            => CefNatives.Instance.cef_subscribe(eventName, callback);

        public static void EmitEvent(int playerId, string eventName)
            => CefEmit0.Instance.cef_emit_event(playerId, eventName);

        public static void EmitEvent(int playerId, string eventName, string v1)
            => CefEmit1S.Instance.cef_emit_event(playerId, eventName, CEF_STRING, v1);

        public static void EmitEvent(int playerId, string eventName, int v1)
            => CefEmit1I.Instance.cef_emit_event(playerId, eventName, CEF_INTEGER, v1);

        public static void EmitEvent(int playerId, string eventName, float v1)
            => CefEmit1F.Instance.cef_emit_event(playerId, eventName, CEF_FLOAT, v1);

        public static void EmitEvent(int playerId, string eventName, string v1, string v2)
            => CefEmit2SS.Instance.cef_emit_event(playerId, eventName, CEF_STRING, v1, CEF_STRING, v2);

        public static void EmitEvent(int playerId, string eventName, string v1, int v2)
            => CefEmit2SI.Instance.cef_emit_event(playerId, eventName, CEF_STRING, v1, CEF_INTEGER, v2);

        public static void EmitEvent(int playerId, string eventName, string v1, float v2)
            => CefEmit2SF.Instance.cef_emit_event(playerId, eventName, CEF_STRING, v1, CEF_FLOAT, v2);

        public static void EmitEvent(int playerId, string eventName, int v1, string v2)
            => CefEmit2IS.Instance.cef_emit_event(playerId, eventName, CEF_INTEGER, v1, CEF_STRING, v2);

        public static void EmitEvent(int playerId, string eventName, int v1, int v2)
            => CefEmit2II.Instance.cef_emit_event(playerId, eventName, CEF_INTEGER, v1, CEF_INTEGER, v2);

        public static void EmitEvent(int playerId, string eventName, int v1, float v2)
            => CefEmit2IF.Instance.cef_emit_event(playerId, eventName, CEF_INTEGER, v1, CEF_FLOAT, v2);
    }
}