using SampSharp.GameMode.Display;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Core
{
    public static class DialogManager
    {
        public static void ShowMessage(BasePlayer player, string title, string message,
            string btnLeft = "OK", string btnRight = "",
            Action<DialogResponseEventArgs> onResponse = null)
        {
            var dialog = new MessageDialog(title, message, btnLeft, btnRight);
            if (onResponse != null)
                dialog.Response += (s, e) => onResponse(e);
            dialog.Show(player);
        }

        public static void ShowInput(BasePlayer player, string title, string message,
            bool isPassword = false, string btnLeft = "OK", string btnRight = "Batal",
            Action<DialogResponseEventArgs> onResponse = null)
        {
            var dialog = new InputDialog(title, message, isPassword, btnLeft, btnRight);
            if (onResponse != null)
                dialog.Response += (s, e) => onResponse(e);
            dialog.Show(player);
        }

        public static void ShowList(BasePlayer player, string title, string[] items,
            string btnLeft = "Pilih", string btnRight = "Batal",
            Action<DialogResponseEventArgs> onResponse = null)
        {
            var dialog = new ListDialog(title, btnLeft, btnRight);
            foreach (var item in items)
                dialog.AddItem(item);
            if (onResponse != null)
                dialog.Response += (s, e) => onResponse(e);
            dialog.Show(player);
        }

        public static void ShowTabList(BasePlayer player, string title, string[] headers,
            string[][] rows, string btnLeft = "Pilih", string btnRight = "Batal",
            Action<DialogResponseEventArgs> onResponse = null)
        {
            var dialog = new TablistDialog(title, headers, btnLeft, btnRight);
            foreach (var row in rows)
                dialog.Add(row);
            if (onResponse != null)
                dialog.Response += (s, e) => onResponse(e);
            dialog.Show(player);
        }
    }
}