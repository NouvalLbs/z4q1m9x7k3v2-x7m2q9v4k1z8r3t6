using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.CEF
{
    public class CefNatives : NativeObjectSingleton<CefNatives>
    {
        [NativeMethod]
        public virtual int CefCreateBrowser(int player_id, int browser_id, string url, bool hidden, bool focused)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefDestroyBrowser(int player_id, int browser_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefOnPlayerConnect(int player_id, string ip)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefOnPlayerDisconnect(int player_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefEmitEvent(int player_id, string @event, params object[] args)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefSubscribe(string @event, string callback)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefHideBrowser(int player_id, int browser_id, bool hide)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefCreateExtBrowser(int player_id, int browser_id, string texture, string url, int scale)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefAppendToObject(int player_id, int browser_id, int object_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefRemoveFromObject(int player_id, int browser_id, int object_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefPlayerHasPlugin(int player_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefToggleDevTools(int player_id, int browser_id, bool enabled)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefSetAudioSettings(int player_id, int browser_id, float max_distance, float reference_distance)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefFocusBrowser(int player_id, int browser_id, bool focused)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefAlwaysListenKeys(int player_id, int browser_id, bool listen)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int CefLoadUrl(int player_id, int browser_id, string url)
            => throw new NativeNotImplementedException();
    }
}