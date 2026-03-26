using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.CEF
{
    public class CefNatives : NativeObjectSingleton<CefNatives>
    {
        [NativeMethod]
        public virtual int cef_create_browser(int player_id, int browser_id, string url, bool hidden, bool focused)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_destroy_browser(int player_id, int browser_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_on_player_connect(int player_id, string ip)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_on_player_disconnect(int player_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_emit_event(int player_id, string @event, params object[] args)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_subscribe(string @event, string callback)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_hide_browser(int player_id, int browser_id, bool hide)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_create_ext_browser(int player_id, int browser_id, string texture, string url, int scale)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_append_to_object(int player_id, int browser_id, int object_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_remove_from_object(int player_id, int browser_id, int object_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_player_has_plugin(int player_id)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_toggle_dev_tools(int player_id, int browser_id, bool enabled)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_set_audio_settings(int player_id, int browser_id, float max_distance, float reference_distance)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_focus_browser(int player_id, int browser_id, bool focused)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_always_listen_keys(int player_id, int browser_id, bool listen)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int cef_load_url(int player_id, int browser_id, string url)
            => throw new NativeNotImplementedException();
    }
}