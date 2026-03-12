using SampSharp.GameMode.Display;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Threading.Tasks;

namespace ProjectSMP.Core
{
    public static class DialogManager
    {
        public static MessageDialogBuilder ShowMessage(this BasePlayer player, string title, string message)
            => new MessageDialogBuilder(player, title, message);

        public static InputDialogBuilder ShowInput(this BasePlayer player, string title, string message)
            => new InputDialogBuilder(player, title, message);

        public static ListDialogBuilder ShowList(this BasePlayer player, string title, params string[] items)
            => new ListDialogBuilder(player, title, items);

        public static TabListDialogBuilder ShowTabList(this BasePlayer player, string title, string[] headers)
            => new TabListDialogBuilder(player, title, headers);

        public class MessageDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string _message;
            private string _btnLeft = "OK";
            private string _btnRight = "";

            internal MessageDialogBuilder(BasePlayer player, string title, string message)
            {
                _player = player;
                _title = title;
                _message = message;
            }

            public MessageDialogBuilder WithButtons(string left, string right = "")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new MessageDialog(_title, _message, _btnLeft, _btnRight);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new MessageDialog(_title, _message, _btnLeft, _btnRight);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }

        public class InputDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string _message;
            private bool _isPassword;
            private string _btnLeft = "OK";
            private string _btnRight = "Batal";

            internal InputDialogBuilder(BasePlayer player, string title, string message)
            {
                _player = player;
                _title = title;
                _message = message;
            }

            public InputDialogBuilder AsPassword()
            {
                _isPassword = true;
                return this;
            }

            public InputDialogBuilder WithButtons(string left, string right = "Batal")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new InputDialog(_title, _message, _isPassword, _btnLeft, _btnRight);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new InputDialog(_title, _message, _isPassword, _btnLeft, _btnRight);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }

        public class ListDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string[] _items;
            private string _btnLeft = "Pilih";
            private string _btnRight = "Batal";

            internal ListDialogBuilder(BasePlayer player, string title, string[] items)
            {
                _player = player;
                _title = title;
                _items = items;
            }

            public ListDialogBuilder WithButtons(string left, string right = "Batal")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new ListDialog(_title, _btnLeft, _btnRight);
                foreach (var item in _items)
                    dialog.AddItem(item);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new ListDialog(_title, _btnLeft, _btnRight);
                foreach (var item in _items)
                    dialog.AddItem(item);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }

        public class TabListDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string[] _headers;
            private string[][] _rows = Array.Empty<string[]>();
            private string _btnLeft = "Pilih";
            private string _btnRight = "Batal";

            internal TabListDialogBuilder(BasePlayer player, string title, string[] headers)
            {
                _player = player;
                _title = title;
                _headers = headers;
            }

            public TabListDialogBuilder WithRows(params string[][] rows)
            {
                _rows = rows;
                return this;
            }

            public TabListDialogBuilder WithButtons(string left, string right = "Batal")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new TablistDialog(_title, _headers, _btnLeft, _btnRight);
                foreach (var row in _rows)
                    dialog.Add(row);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new TablistDialog(_title, _headers, _btnLeft, _btnRight);
                foreach (var row in _rows)
                    dialog.Add(row);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }
    }
}