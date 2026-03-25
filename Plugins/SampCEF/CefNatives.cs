using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.SampCEF
{
    public class CefNatives : NativeObjectSingleton<CefNatives>
    {
        [NativeMethod(Function = "cef_create_browser")]
        public virtual int CreateBrowser(int playerId, int browserId, string url, bool hidden, bool focused)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_destroy_browser")]
        public virtual int DestroyBrowser(int playerId, int browserId)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_on_player_connect")]
        public virtual int OnPlayerConnect(int playerId, string ip)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_on_player_disconnect")]
        public virtual int OnPlayerDisconnect(int playerId)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_subscribe")]
        public virtual int Subscribe(string eventName, string callback)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_hide_browser")]
        public virtual int HideBrowser(int playerId, int browserId, bool hide)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_create_ext_browser")]
        public virtual int CreateExtBrowser(int playerId, int browserId, string texture, string url, int scale)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_append_to_object")]
        public virtual int AppendToObject(int playerId, int browserId, int objectId)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_remove_from_object")]
        public virtual int RemoveFromObject(int playerId, int browserId, int objectId)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_player_has_plugin")]
        public virtual int PlayerHasPlugin(int playerId)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_toggle_dev_tools")]
        public virtual int ToggleDevTools(int playerId, int browserId, bool enabled)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_set_audio_settings")]
        public virtual int SetAudioSettings(int playerId, int browserId, float maxDistance, float refDistance)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_focus_browser")]
        public virtual int FocusBrowser(int playerId, int browserId, bool focused)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_always_listen_keys")]
        public virtual int AlwaysListenKeys(int playerId, int browserId, bool listen)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_load_url")]
        public virtual int LoadUrl(int playerId, int browserId, string url)
            => throw new NativeNotImplementedException();

        [NativeMethod(Function = "cef_emit_event")]
        public virtual int EmitEvent(int playerId, string eventName, params object[] args)
            => throw new NativeNotImplementedException();
    }
}