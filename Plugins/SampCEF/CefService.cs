using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.SampCEF
{
    public static class CefService
    {
        private const float DefaultMaxDist = 50.0f;
        private const float DefaultRefDist = 15.0f;

        private static CefNatives N => CefNatives.Instance;

        public static void NotifyConnect(int playerId, string ip)
        {
            try
            {
                N.OnPlayerConnect(playerId, ip);
                Console.WriteLine($"[CEF] cef_on_player_connect OK - player:{playerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CEF] cef_on_player_connect FAILED: {ex.Message}");
            }
        }

        public static void NotifyDisconnect(int playerId)
        {
            try { N.OnPlayerDisconnect(playerId); }
            catch (Exception ex) { Console.WriteLine($"[CEF] cef_on_player_disconnect FAILED: {ex.Message}"); }
        }
        public static bool PlayerHasPlugin(int playerId) => N.PlayerHasPlugin(playerId) != 0;

        public static void CreateBrowser(int playerId, int browserId, string url, bool hidden = false, bool focused = true)
            => N.CreateBrowser(playerId, browserId, url, hidden, focused);

        public static void DestroyBrowser(int playerId, int browserId)
            => N.DestroyBrowser(playerId, browserId);

        public static void HideBrowser(int playerId, int browserId, bool hide)
            => N.HideBrowser(playerId, browserId, hide);

        public static void FocusBrowser(int playerId, int browserId, bool focused)
            => N.FocusBrowser(playerId, browserId, focused);

        public static void LoadUrl(int playerId, int browserId, string url)
            => N.LoadUrl(playerId, browserId, url);

        public static void CreateExtBrowser(int playerId, int browserId, string texture, string url, int scale = 1)
            => N.CreateExtBrowser(playerId, browserId, texture, url, scale);

        public static void AppendToObject(int playerId, int browserId, int objectId)
            => N.AppendToObject(playerId, browserId, objectId);

        public static void RemoveFromObject(int playerId, int browserId, int objectId)
            => N.RemoveFromObject(playerId, browserId, objectId);

        public static void ToggleDevTools(int playerId, int browserId, bool enabled)
            => N.ToggleDevTools(playerId, browserId, enabled);

        public static void AlwaysListenKeys(int playerId, int browserId, bool listen)
            => N.AlwaysListenKeys(playerId, browserId, listen);

        public static void SetAudioSettings(int playerId, int browserId, float maxDistance = DefaultMaxDist, float refDistance = DefaultRefDist)
            => N.SetAudioSettings(playerId, browserId, maxDistance, refDistance);

        public static void Subscribe(string eventName, string callback)
            => N.Subscribe(eventName, callback);

        public static void EmitEvent(int playerId, string eventName, params CefArg[] args)
        {
            var varArgs = new List<object>();

            foreach (var arg in args)
            {
                varArgs.Add((int)arg.Type);
                varArgs.Add(arg.Value);
            }

            N.EmitEvent(playerId, eventName, varArgs.ToArray());
        }
    }
}