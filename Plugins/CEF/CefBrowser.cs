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

            _n.CefCreateBrowser(playerId, browserId, url, hidden, focused);
        }

        public void Destroy()
            => _n.CefDestroyBrowser(PlayerId, BrowserId);

        public void Hide()
        {
            IsHidden = true;
            _n.CefHideBrowser(PlayerId, BrowserId, true);
        }

        public void Show()
        {
            IsHidden = false;
            _n.CefHideBrowser(PlayerId, BrowserId, false);
        }

        public void SetVisible(bool visible)
        {
            IsHidden = !visible;
            _n.CefHideBrowser(PlayerId, BrowserId, !visible);
        }

        public void Focus(bool focused)
            => _n.CefFocusBrowser(PlayerId, BrowserId, focused);

        public void LoadUrl(string url)
        {
            Url = url;
            _n.CefLoadUrl(PlayerId, BrowserId, url);
        }

        public void EmitEvent(string eventName, params CefValue[] args)
            => _n.CefEmitEvent(PlayerId, eventName, args.Flatten());

        public void AppendToObject(int objectId)
            => _n.CefAppendToObject(PlayerId, BrowserId, objectId);

        public void RemoveFromObject(int objectId)
            => _n.CefRemoveFromObject(PlayerId, BrowserId, objectId);

        public void ToggleDevTools(bool enabled)
            => _n.CefToggleDevTools(PlayerId, BrowserId, enabled);

        public void SetAudioSettings(float maxDistance = DefaultMaxDist, float referenceDistance = DefaultRefDist)
            => _n.CefSetAudioSettings(PlayerId, BrowserId, maxDistance, referenceDistance);

        public void AlwaysListenKeys(bool listen)
            => _n.CefAlwaysListenKeys(PlayerId, BrowserId, listen);
    }
}