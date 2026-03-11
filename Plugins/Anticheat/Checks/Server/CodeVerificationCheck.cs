using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class CodeVerificationCheck
{
    private const long VerificationIntervalMs = 300000; // Verify every 5 minutes
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;
    private readonly Dictionary<string, string> _componentHashes = new();
    private long _lastVerificationTick;
    private int _verificationFailures;
    private readonly object _lock = new();

    public CodeVerificationCheck(AnticheatConfig c, AcLogger l)
    {
        _config = c;
        _logger = l;
        InitializeHashes();
    }

    private void InitializeHashes()
    {
        // Store initial configuration hashes
        lock (_lock)
        {
            _componentHashes["Config.Enabled"] = HashValue(_config.Enabled.ToString());
            _componentHashes["Config.MaxPing"] = HashValue(_config.MaxPing.ToString());
            _componentHashes["Config.MaxConnectsPerIp"] = HashValue(_config.MaxConnectsPerIp.ToString());
            _componentHashes["Config.MinReconnectSeconds"] = HashValue(_config.MinReconnectSeconds.ToString());
            _componentHashes["Config.CheckCount"] = HashValue(_config.Checks.Count.ToString());

            // Store checksums of enabled checks
            foreach (var (key, value) in _config.Checks)
            {
                _componentHashes[$"Check.{key}.Enabled"] = HashValue(value.Enabled.ToString());
                _componentHashes[$"Check.{key}.MaxWarnings"] = HashValue(value.MaxWarnings.ToString());
                _componentHashes[$"Check.{key}.Action"] = HashValue(value.Action.ToString());
            }
        }
    }

    public void Tick()
    {
        if (!_config.Enabled || !_config.GetCheck("CodeVerification").Enabled) return;

        long now = Environment.TickCount64;
        if (now - _lastVerificationTick < VerificationIntervalMs) return;

        _lastVerificationTick = now;
        VerifyIntegrity();
    }

    private void VerifyIntegrity()
    {
        lock (_lock)
        {
            List<string> violations = new();

            // Verify config hasn't been tampered with
            if (!VerifyConfigHash("Config.Enabled", _config.Enabled.ToString()))
                violations.Add("Config.Enabled modified");

            if (!VerifyConfigHash("Config.MaxPing", _config.MaxPing.ToString()))
                violations.Add("Config.MaxPing modified");

            if (!VerifyConfigHash("Config.MaxConnectsPerIp", _config.MaxConnectsPerIp.ToString()))
                violations.Add("Config.MaxConnectsPerIp modified");

            if (!VerifyConfigHash("Config.MinReconnectSeconds", _config.MinReconnectSeconds.ToString()))
                violations.Add("Config.MinReconnectSeconds modified");

            if (!VerifyConfigHash("Config.CheckCount", _config.Checks.Count.ToString()))
                violations.Add("Config.CheckCount modified");

            // Verify critical checks are still enabled
            string[] criticalChecks = { "FakeSpawn", "FakeKill", "WeaponHack", "MoneyHack", "TeleportOnfoot" };
            foreach (var checkName in criticalChecks)
            {
                if (_config.Checks.TryGetValue(checkName, out var check))
                {
                    if (!VerifyConfigHash($"Check.{checkName}.Enabled", check.Enabled.ToString()))
                        violations.Add($"{checkName} enabled state modified");
                }
            }

            // Check if anticheat was disabled externally
            if (!_config.Enabled && _componentHashes.ContainsKey("Config.Enabled"))
            {
                violations.Add("Anticheat disabled externally");
            }

            // Report violations
            if (violations.Count > 0)
            {
                _verificationFailures++;
                _logger.LogWarn($"Code verification failed ({_verificationFailures}): {string.Join(", ", violations)}");

                if (_verificationFailures >= 3)
                {
                    _logger.LogWarn("CRITICAL: Multiple code verification failures detected!");
                }
            }
            else
            {
                _verificationFailures = 0;
            }
        }
    }

    private bool VerifyConfigHash(string key, string currentValue)
    {
        if (!_componentHashes.TryGetValue(key, out string? storedHash))
            return true; // No stored hash = assume valid

        string currentHash = HashValue(currentValue);
        return storedHash == currentHash;
    }

    private static string HashValue(string value)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public void RefreshHashes()
    {
        InitializeHashes();
        _verificationFailures = 0;
        _logger.Log("Code verification hashes refreshed");
    }

    public Dictionary<string, bool> GetVerificationStatus()
    {
        lock (_lock)
        {
            var status = new Dictionary<string, bool>();

            foreach (var (key, storedHash) in _componentHashes)
            {
                string currentValue = key switch
                {
                    "Config.Enabled" => _config.Enabled.ToString(),
                    "Config.MaxPing" => _config.MaxPing.ToString(),
                    "Config.MaxConnectsPerIp" => _config.MaxConnectsPerIp.ToString(),
                    "Config.MinReconnectSeconds" => _config.MinReconnectSeconds.ToString(),
                    "Config.CheckCount" => _config.Checks.Count.ToString(),
                    _ when key.StartsWith("Check.") && key.EndsWith(".Enabled") =>
                        GetCheckValue(key, "Enabled"),
                    _ when key.StartsWith("Check.") && key.EndsWith(".MaxWarnings") =>
                        GetCheckValue(key, "MaxWarnings"),
                    _ when key.StartsWith("Check.") && key.EndsWith(".Action") =>
                        GetCheckValue(key, "Action"),
                    _ => ""
                };

                string currentHash = HashValue(currentValue);
                status[key] = storedHash == currentHash;
            }

            return status;
        }
    }

    private string GetCheckValue(string key, string property)
    {
        string checkName = key.Replace("Check.", "").Replace($".{property}", "");
        if (!_config.Checks.TryGetValue(checkName, out var check))
            return "";

        return property switch
        {
            "Enabled" => check.Enabled.ToString(),
            "MaxWarnings" => check.MaxWarnings.ToString(),
            "Action" => check.Action.ToString(),
            _ => ""
        };
    }

    public int GetFailureCount() => _verificationFailures;
}