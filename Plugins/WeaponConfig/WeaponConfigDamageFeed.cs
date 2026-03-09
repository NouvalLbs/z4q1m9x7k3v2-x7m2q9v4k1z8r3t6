#nullable enable
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
        public int LastRenderTick;
        public CancellationTokenSource? HideCts;
        /// <summary>Player ID being spectated, or -1 if not spectating.</summary>
        public int SpectatingId = -1;
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

        // ── Init / lifecycle ────────────────────────────────────────────

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

        // ── Spectate support ────────────────────────────────────────────

        /// <summary>
        /// Call when a player begins spectating another.
        /// Their feed will mirror the target's damage feed.
        /// </summary>
        public static void SetSpectating(BasePlayer spectator, int targetPlayerId)
        {
            if (_feeds.TryGetValue(spectator.Id, out var s))
                s.SpectatingId = targetPlayerId;
        }

        /// <summary>Call when a player stops spectating.</summary>
        public static void ClearSpectating(BasePlayer spectator)
        {
            if (!_feeds.TryGetValue(spectator.Id, out var s)) return;
            s.SpectatingId = -1;
            s.TakenTD?.Hide();
            s.GivenTD?.Hide();
        }

        // ── Feed updates ────────────────────────────────────────────────

        public static void AddTaken(BasePlayer player, string issuerName, float amount, int weapon)
        {
            if (!_feeds.TryGetValue(player.Id, out var s) || !s.Enabled) return;

            var idx = s.TakenIdx % FeedHeight;
            s.Taken[idx] = new FeedEntry
            { Name = issuerName, Amount = amount, Weapon = weapon, Tick = Environment.TickCount };
            s.TakenIdx++;

            var now = Environment.TickCount;
            if (now - s.LastRenderTick >= MaxUpdateRateMs)
            {
                s.LastRenderTick = now;
                RenderFeed(s);
                PushFeedToSpectators(player.Id, s);
            }
            ScheduleHide(s, player);
        }

        public static void AddGiven(BasePlayer player, string targetName, float amount, int weapon)
        {
            if (!_feeds.TryGetValue(player.Id, out var s) || !s.Enabled) return;

            var idx = s.GivenIdx % FeedHeight;
            s.Given[idx] = new FeedEntry
            { Name = targetName, Amount = amount, Weapon = weapon, Tick = Environment.TickCount };
            s.GivenIdx++;

            var now = Environment.TickCount;
            if (now - s.LastRenderTick >= MaxUpdateRateMs)
            {
                s.LastRenderTick = now;
                RenderFeed(s);
                PushFeedToSpectators(player.Id, s);
            }
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

        // ── Private helpers ─────────────────────────────────────────────

        private static void RenderFeed(PlayerFeedState s)
        {
            s.TakenTD!.Text = BuildText(s.Taken, s.TakenIdx);
            s.GivenTD!.Text = BuildText(s.Given, s.GivenIdx);
            s.TakenTD.Show();
            s.GivenTD.Show();
        }

        /// <summary>
        /// For every spectator watching <paramref name="targetId"/>,
        /// push the target's feed data to that spectator's TextDraws.
        /// </summary>
        private static void PushFeedToSpectators(int targetId, PlayerFeedState targetState)
        {
            foreach (var (spectId, spectState) in _feeds)
            {
                if (spectState.SpectatingId != targetId) continue;
                if (BasePlayer.Find(spectId) is not BasePlayer spec) continue;

                spectState.TakenTD!.Text = BuildText(targetState.Taken, targetState.TakenIdx);
                spectState.GivenTD!.Text = BuildText(targetState.Given, targetState.GivenIdx);
                spectState.TakenTD.Show();
                spectState.GivenTD.Show();
                ScheduleHide(spectState, spec);
            }
        }

        private static string BuildText(FeedEntry[] entries, int headIdx)
        {
            var lines = new string[FeedHeight];
            var now = Environment.TickCount;

            for (var i = 0; i < FeedHeight; i++)
            {
                var idx = ((headIdx - i - 1) % FeedHeight + FeedHeight) % FeedHeight;
                var entry = entries[idx];
                lines[FeedHeight - 1 - i] = entry == null || now - entry.Tick > _hideDelayMs
                    ? " "
                    : $"{entry.Name}: -{entry.Amount:F1}";
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