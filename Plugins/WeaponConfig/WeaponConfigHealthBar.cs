#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal static class WeaponConfigHealthBar
    {
        private const float BarX = 502f;
        private const float BarY = 383.5f;
        private const float MaxWidth = 33.5f;
        private const float BarH = 6.5f;
        private const float BorderPad = 0.5f;
        private static bool _isInitialized = false;

        internal static bool IsInternalTextDraw(TextDraw td)
            => _isInitialized && (td == _border || td == _background);

        internal static bool IsInternalPlayerTextDraw(PlayerTextDraw ptd, int playerId)
            => _isInitialized && _bars.TryGetValue(playerId, out var bar) && ptd == bar;

        private static readonly Color FgColor = new(0xB4, 0x19, 0x1D, 0xFF);
        private static readonly Color BgColor = new(0x5A, 0x0C, 0x0E, 0xFF);
        private static readonly Color BorderColor = new(0x00, 0x00, 0x00, 0xFF);
        private static readonly Color Transparent = new(0x00, 0x00, 0x00, 0x00);

        private static TextDraw? _border;
        private static TextDraw? _background;

        private static readonly Dictionary<int, PlayerTextDraw> _bars = new();
        private static readonly Dictionary<int, bool> _enabled = new();

        public static void Init()
        {
            _border = new TextDraw(new Vector2(BarX - BorderPad, BarY - BorderPad), "LD_SPAC:WHITE")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 1f),
                ForeColor = BorderColor,
                UseBox = true,
                BoxColor = BorderColor,
                Width = BarX + MaxWidth + BorderPad,
                Height = BarH + BorderPad * 2,
                Shadow = 0,
                Outline = 0,
                Proportional = true
            };

            _background = new TextDraw(new Vector2(BarX, BarY), "LD_SPAC:WHITE")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 1f),
                ForeColor = BgColor,
                UseBox = true,
                BoxColor = BgColor,
                Width = BarX + MaxWidth,
                Height = BarH,
                Shadow = 0,
                Outline = 0,
                Proportional = true
            };

            _isInitialized = true;
        }

        public static void Dispose()
        {
            _border?.Dispose();
            _background?.Dispose();
            _isInitialized = false;
        }

        public static void OnConnect(BasePlayer player)
        {
            _enabled[player.Id] = true;

            var ptd = new PlayerTextDraw(player, new Vector2(BarX, BarY), "LD_SPAC:WHITE")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 1f),
                ForeColor = FgColor,
                UseBox = true,
                BoxColor = FgColor,
                Width = BarX + MaxWidth,
                Height = BarH,
                Shadow = 0,
                Outline = 0,
                Proportional = true
            };
            _bars[player.Id] = ptd;

            _border?.Show(player);
            _background?.Show(player);
        }

        public static void OnDisconnect(BasePlayer player)
        {
            if (_bars.TryGetValue(player.Id, out var ptd)) ptd.Dispose();
            _bars.Remove(player.Id);
            _enabled.Remove(player.Id);
        }

        public static void Update(BasePlayer player, float health, float maxHealth)
        {
            if (!_enabled.TryGetValue(player.Id, out var en) || !en) return;
            if (!_bars.TryGetValue(player.Id, out var ptd)) return;

            var pct = maxHealth > 0 ? MathF.Max(0, MathF.Min(health / maxHealth, 1f)) : 0f;
            var w = pct * MaxWidth;

            if (w <= 0)
            {
                ptd.BoxColor = Transparent;
                ptd.ForeColor = Transparent;
            }
            else
            {
                ptd.BoxColor = FgColor;
                ptd.ForeColor = FgColor;
                ptd.Width = BarX + w;
            }
            ptd.Show();
        }

        public static void Show(BasePlayer player)
        {
            if (!_enabled.TryGetValue(player.Id, out var en) || !en) return;
            _border?.Show(player);
            _background?.Show(player);
            if (_bars.TryGetValue(player.Id, out var ptd)) ptd.Show();
        }

        public static void Hide(BasePlayer player)
        {
            _border?.Hide(player);
            _background?.Hide(player);
            if (_bars.TryGetValue(player.Id, out var ptd)) ptd.Hide();
        }

        public static void SetEnabled(BasePlayer player, bool enable)
        {
            _enabled[player.Id] = enable;
            if (enable) Show(player);
            else Hide(player);
        }
    }
}