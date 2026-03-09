using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal sealed class FeedEntry
    {
        public string Name { get; set; } = "";
        public float Amount { get; set; }
        public int Weapon { get; set; }
        public int Tick { get; set; }
    }

    internal sealed class PlayerFeedState
    {
        public PlayerTextDraw? TakenTD;
        public PlayerTextDraw? GivenTD;
        public bool Enabled = true;
        public readonly FeedEntry[] Taken = new FeedEntry[5];
        public readonly FeedEntry[] Given = new FeedEntry[5];
        public int TakenIdx;
        public int GivenIdx;
        public CancellationTokenSource? HideCts;
    }

    internal static class WeaponConfigDamageFeed
    {
        private const int FeedHeight = 5;
        private const int MaxUpdateRateMs = 250;
        private const float TakenX = 551f;
        private const float GivenX = 551f;
        private const float StartY = 340f;
        private const float LineH = 10f;

        private static int _hideDelayMs = 3000;

        private static readonly Color TakenColor = new(0x33, 0xCC, 0xFF, 0xFF);
        private static readonly Color GivenColor = new(0x30, 0xFF, 0x50, 0xFF);
        private static readonly Color Black = new(0x00, 0x00, 0x00, 0xFF);

        private static readonly Dictionary<int, PlayerFeedState> _feeds = new();
        private static bool _globalEnabled = true;

        public static void Init(bool globalEnabled, int hideDelayMs = 3000)
        {
            _globalEnabled = globalEnabled;
            _hideDelayMs = hideDelayMs;
        }

        public static void OnConnect(BasePlayer player)
        {
            var state = new PlayerFeedState { Enabled = _globalEnabled };

            state.TakenTD = new PlayerTextDraw(player, new Vector2(TakenX, StartY), "_")
            {
                Font = TextDrawFont.Slim,
                LetterSize = new Vector2(0.14f, 0.9f),
                Alignment = TextDrawAlignment.Right,
                ForeColor = TakenColor,
                Shadow = 0,
                Outline = 1,
                BackColor = Black,
                Proportional = true,
                Width = 640f,
                Height = 480f
            };

            state.GivenTD = new PlayerTextDraw(player,
                new Vector2(GivenX, StartY + LineH * FeedHeight + 2f), "_")
            {
                Font = TextDrawFont.Slim,
                LetterSize = new Vector2(0.14f, 0.9f),
                Alignment = TextDrawAlignment.Right,
                ForeColor = GivenColor,
                Shadow = 0,
                Outline = 1,
                BackColor = Black,
                Proportional = true,
                Width = 640f,
                Height = 480f
            };

            _feeds[player.Id] = state;
        }

        public static void OnDisconnect(BasePlayer player)
        {
            if (!_feeds.TryGetValue(player.Id, out var s)) return;
            s.HideCts?.Cancel();
            s.TakenTD?.Dispose();
            s.GivenTD?.Dispose();
            _feeds.Remove(player.Id);
        }

        public static void AddTaken(BasePlayer player, string issuerName, float amount, int weapon)
        {
            if (!_feeds.TryGetValue(player.Id, out var s) || !s.Enabled) return;
            var idx = s.TakenIdx % FeedHeight;
            s.Taken[idx] = new FeedEntry { Name = issuerName, Amount = amount, Weapon = weapon, Tick = Environment.TickCount };
            s.TakenIdx++;
            RenderFeed(s);
            ScheduleHide(s, player);
        }

        public static void AddGiven(BasePlayer player, string targetName, float amount, int weapon)
        {
            if (!_feeds.TryGetValue(player.Id, out var s) || !s.Enabled) return;
            var idx = s.GivenIdx % FeedHeight;
            s.Given[idx] = new FeedEntry { Name = targetName, Amount = amount, Weapon = weapon, Tick = Environment.TickCount };
            s.GivenIdx++;
            RenderFeed(s);
            ScheduleHide(s, player);
        }

        public static void SetEnabled(BasePlayer player, bool enable)
        {
            if (!_feeds.TryGetValue(player.Id, out var s)) return;
            s.Enabled = enable;
            if (!enable)
            {
                s.HideCts?.Cancel();
                s.TakenTD?.Hide();
                s.GivenTD?.Hide();
            }
        }

        public static bool IsEnabled(BasePlayer player)
            => _feeds.TryGetValue(player.Id, out var s) && s.Enabled;

        private static void RenderFeed(PlayerFeedState s)
        {
            s.TakenTD!.Text = BuildText(s.Taken, s.TakenIdx);
            s.GivenTD!.Text = BuildText(s.Given, s.GivenIdx);
            s.TakenTD.Show();
            s.GivenTD.Show();
        }

        private static string BuildText(FeedEntry[] entries, int headIdx)
        {
            var lines = new string[FeedHeight];
            var now = Environment.TickCount;

            for (var i = 0; i < FeedHeight; i++)
            {
                var idx = ((headIdx - i - 1) % FeedHeight + FeedHeight) % FeedHeight;
                var entry = entries[idx];

                if (entry == null || now - entry.Tick > _hideDelayMs)
                {
                    lines[FeedHeight - 1 - i] = " ";
                    continue;
                }

                lines[FeedHeight - 1 - i] = $"{entry.Name}: -{entry.Amount:F1}";
            }

            return string.Join("~n~", lines);
        }

        private static void ScheduleHide(PlayerFeedState s, BasePlayer player)
        {
            s.HideCts?.Cancel();
            var cts = new CancellationTokenSource();
            s.HideCts = cts;
            _ = HideAfterAsync(s, player, cts.Token);
        }

        private static async Task HideAfterAsync(PlayerFeedState s, BasePlayer player,
            CancellationToken ct)
        {
            try
            {
                await Task.Delay(_hideDelayMs + 200, ct);
                if (player.IsDisposed) return;
                s.TakenTD?.Hide();
                s.GivenTD?.Hide();
            }
            catch (OperationCanceledException) { }
        }
    }
}