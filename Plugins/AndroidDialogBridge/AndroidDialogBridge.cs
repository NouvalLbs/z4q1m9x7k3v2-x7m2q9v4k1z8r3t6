using ProjectSMP.Core;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.AndroidDialogBridge
{
    public class AndroidDialogNatives : NativeObjectSingleton<AndroidDialogNatives>
    {
        [NativeMethod]
        public virtual int CallRemoteFunction(string function, string format, params object[] args)
            => throw new NativeNotImplementedException();
    }

    public static class AndroidDialogBridge
    {
        private static AndroidDialogNatives N => AndroidDialogNatives.Instance;
        private static ConcurrentDictionary<int, Action<bool, int, string>> _callbacks = new();
        private static int _dialogIdCounter = 10000;

        public static bool ShouldUseBridge(Player player)
        {
            return ClientManager.IsAndroid(player);
        }

        public static void ShowDialog(int playerId, int style, string caption, string info,
            string button1, string button2, Action<bool, int, string> callback)
        {
            var dialogId = _dialogIdCounter++;
            _callbacks[dialogId] = callback;

            var data = new AndroidDialogData
            {
                PlayerId = playerId,
                DialogId = dialogId,
                Style = style,
                Caption = caption,
                Info = info,
                Button1 = button1,
                Button2 = button2
            };

            var json = JsonSerializer.Serialize(data);
            N.CallRemoteFunction("AndroidDialog_Show", "s", json);
        }

        public static void HandleResponse(int playerId, int dialogId, bool response, int listitem, string inputtext)
        {
            if (_callbacks.TryRemove(dialogId, out var callback))
            {
                callback?.Invoke(response, listitem, inputtext);
            }
        }

        public static void Cleanup(int playerId)
        {
            var toRemove = new System.Collections.Generic.List<int>();
            foreach (var kvp in _callbacks)
            {
                toRemove.Add(kvp.Key);
            }
            foreach (var id in toRemove)
            {
                _callbacks.TryRemove(id, out _);
            }
        }
    }
}