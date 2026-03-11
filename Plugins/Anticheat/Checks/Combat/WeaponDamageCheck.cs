using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class WeaponDamageCheck
{
    private static readonly Dictionary<int, (float Min, float Max)> _weaponDamage = new()
    {
        {22, (8.25f, 8.25f)}, {23, (13.2f, 13.2f)}, {24, (46.2f, 46.2f)},
        {25, (3.3f, 3.3f)}, {26, (3.3f, 3.3f)}, {27, (4.95f, 4.95f)},
        {28, (6.6f, 6.6f)}, {29, (8.25f, 8.25f)}, {32, (6.6f, 6.6f)},
        {30, (9.9f, 9.9f)}, {31, (9.9f, 9.9f)},
        {33, (24.75f, 24.75f)}, {34, (41.25f, 41.25f)},
        {38, (46.2f, 46.2f)}
    };

    private static readonly Dictionary<int, float> _headshotMultiplier = new()
    {
        {22, 3f}, {23, 3f}, {24, 3f}, {25, 3f}, {26, 3f}, {27, 3f},
        {28, 3f}, {29, 3f}, {32, 3f}, {30, 3f}, {31, 3f},
        {33, 3f}, {34, 3f}
    };

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public WeaponDamageCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerTakeDamage(BasePlayer victim, DamageEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("WeaponDamageHack").Enabled) return;
        if (e.OtherPlayer is not BasePlayer issuer) return;

        int weaponId = (int)e.Weapon;
        float damage = e.Amount;

        if (!_weaponDamage.TryGetValue(weaponId, out var range)) return;

        var ist = _players.Get(issuer.Id);
        if (ist is null || ist.IsDead) return;

        long now = Environment.TickCount64;
        if (now - ist.SpawnTick < 3000) return;

        float maxDamage = range.Max;
        if (_headshotMultiplier.TryGetValue(weaponId, out float headMult))
            maxDamage *= headMult;

        float tolerance = 2f;
        if (damage > maxDamage + tolerance)
        {
            _warnings.AddWarning(issuer.Id, "WeaponDamageHack",
                $"wid={weaponId} dmg={damage:F1} max={maxDamage:F1}");
        }

        if (damage < range.Min - tolerance && damage > 0.1f)
        {
            _warnings.AddWarning(issuer.Id, "WeaponDamageHack",
                $"wid={weaponId} dmg={damage:F1} min={range.Min:F1} (too low)");
        }
    }
}