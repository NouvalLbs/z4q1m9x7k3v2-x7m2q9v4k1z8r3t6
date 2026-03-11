using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;
using System.Linq;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class TextDrawCrasherCheck
{
    private const int MaxTextDraws = 2048;
    private const int MaxTextDrawsPerSec = 100;
    private const long TextDrawSpamWindowMs = 1000;
    private const int MaxTextLength = 1024;
    private const int MaxFormatStringLength = 512;

    private static readonly char[] _crashChars = { '~', '%', '\n', '\r', '\0' };
    private static readonly string[] _crashStrings =
    {
        "~n~~n~~n~~n~~n~~n~~n~~n~~n~~n~",
        "~r~~r~~r~~r~~r~~r~~r~~r~~r~~r~",
        "%s%s%s%s%s%s%s%s%s%s",
        "~h~~h~~h~~h~~h~~h~~h~~h~~h~~h~"
    };

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public TextDrawCrasherCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public bool ValidateTextDraw(BasePlayer player, string text)
    {
        if (!_config.Enabled || !_config.GetCheck("TextDrawCrasher").Enabled) return true;

        var st = _players.Get(player.Id);
        if (st is null) return true;

        if (text.Length > MaxTextLength)
        {
            _warnings.AddWarning(player.Id, "TextDrawCrasher",
                $"text too long len={text.Length}");
            return false;
        }

        int colorCodeCount = text.Count(c => c == '~');
        if (colorCodeCount > 50)
        {
            _warnings.AddWarning(player.Id, "TextDrawCrasher",
                $"excessive color codes count={colorCodeCount}");
            return false;
        }

        int formatCount = text.Count(c => c == '%');
        if (formatCount > 10)
        {
            _warnings.AddWarning(player.Id, "TextDrawCrasher",
                $"excessive format strings count={formatCount}");
            return false;
        }

        foreach (string crashStr in _crashStrings)
        {
            if (text.Contains(crashStr))
            {
                _warnings.AddWarning(player.Id, "TextDrawCrasher",
                    $"crash string detected");
                return false;
            }
        }

        if (text.Contains('\0'))
        {
            _warnings.AddWarning(player.Id, "TextDrawCrasher",
                $"null byte detected");
            return false;
        }

        long now = Environment.TickCount64;
        st.TextDrawCreateHistory.Enqueue(now);
        while (st.TextDrawCreateHistory.Count > 0 &&
               now - st.TextDrawCreateHistory.Peek() > TextDrawSpamWindowMs)
        {
            st.TextDrawCreateHistory.Dequeue();
        }

        if (st.TextDrawCreateHistory.Count > MaxTextDrawsPerSec)
        {
            _warnings.AddWarning(player.Id, "TextDrawCrasher",
                $"spam count={st.TextDrawCreateHistory.Count}");
            return false;
        }

        return true;
    }
}