using ProjectSMP.Plugins.Anticheat.Configuration;
using System;
using System.IO;
using System.Threading;

namespace ProjectSMP.Plugins.Anticheat.Utilities;

public class AcLogger
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public AcLogger(AnticheatConfig config)
    {
        _path = config.LogPath;
        var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (dir is not null) Directory.CreateDirectory(dir);
    }

    public void Log(string msg) => Write("INFO", msg);
    public void LogWarn(string msg) => Write("WARN", msg);
    public void LogKick(int pid, string check) => Write("KICK", $"P:{pid} | {check}");
    public void LogBan(int pid, string check) => Write("BAN", $"P:{pid} | {check}");

    public void LogCheat(int pid, string check, int count, string details = "") =>
        Write("CHEAT", $"P:{pid} | {check} | W:{count}" + (details.Length > 0 ? $" | {details}" : ""));

    private void Write(string level, string msg)
    {
        _lock.Wait();
        try { File.AppendAllText(_path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {msg}\n"); }
        finally { _lock.Release(); }
    }
}