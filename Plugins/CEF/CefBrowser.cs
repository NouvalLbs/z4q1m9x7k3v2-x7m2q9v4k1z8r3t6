namespace ProjectSMP.Plugins.CEF
{
    public sealed class CefBrowser
    {
        private const float DefaultMaxDist = 50.0f;
        private const float DefaultRefDist = 15.0f;

        private readonly CefNatives _n = CefNatives.Instance;

        public int PlayerId { get; }
        public int BrowserId { get; }
        public string Url { get; private set; }
        public bool IsHidden { get; private set; }

        public CefBrowser(int playerId, int browserId, string url, bool hidden = false, bool focused = true)
        {
            PlayerId = playerId;
            BrowserId = browserId;
            Url = url;
            IsHidden = hidden;

            _n.cef_create_browser(playerId, browserId, url, hidden, focused);
        }

        public void Destroy()
            => _n.cef_destroy_browser(PlayerId, BrowserId);

        public void Hide()
        {
            IsHidden = true;
            _n.cef_hide_browser(PlayerId, BrowserId, true);
        }

        public void Show()
        {
            IsHidden = false;
            _n.cef_hide_browser(PlayerId, BrowserId, false);
        }

        public void SetVisible(bool visible)
        {
            IsHidden = !visible;
            _n.cef_hide_browser(PlayerId, BrowserId, !visible);
        }

        public void Focus(bool focused)
            => _n.cef_focus_browser(PlayerId, BrowserId, focused);

        public void LoadUrl(string url)
        {
            Url = url;
            _n.cef_load_url(PlayerId, BrowserId, url);
        }

        public void EmitEvent(string eventName, params CefValue[] args)
            => _n.cef_emit_event(PlayerId, eventName, args.Flatten());

        public void AppendToObject(int objectId)
            => _n.cef_append_to_object(PlayerId, BrowserId, objectId);

        public void RemoveFromObject(int objectId)
            => _n.cef_remove_from_object(PlayerId, BrowserId, objectId);

        public void ToggleDevTools(bool enabled)
            => _n.cef_toggle_dev_tools(PlayerId, BrowserId, enabled);

        public void SetAudioSettings(float maxDistance = DefaultMaxDist, float referenceDistance = DefaultRefDist)
            => _n.cef_set_audio_settings(PlayerId, BrowserId, maxDistance, referenceDistance);

        public void AlwaysListenKeys(bool listen)
            => _n.cef_always_listen_keys(PlayerId, BrowserId, listen);
    }
}