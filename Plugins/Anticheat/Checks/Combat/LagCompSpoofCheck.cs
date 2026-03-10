using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class LagCompSpoofCheck
{
    private static readonly Dictionary<int, float> _maxRange = new()
    {
        {22,35f},{23,35f},{24,45f},
        {25,15f},{26,15f},{27,25f},
        {28,40f},{29,45f},{32,40f},
        {30,70f},{31,70f},
        {33,100f},{34,350f},
        {35,200f},{36,200f}
    };

    private const float LagcompTolerance = 50f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public LagCompSpoofCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerTakeDamage(BasePlayer victim, DamageEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("LagCompSpoof").Enabled) return;
        if (e.OtherPlayer is not BasePlayer issuer) return;

        var ist = _players.Get(issuer.Id);
        var vst = _players.Get(victim.Id);
        if (ist is null || vst is null) return;

        int wid = (int)e.Weapon;
        if (!_maxRange.TryGetValue(wid, out float maxRange)) return;

        float dist = VectorMath.Dist(ist.X, ist.Y, ist.Z, vst.X, vst.Y, vst.Z);
        float limit = maxRange + LagcompTolerance;

        if (dist > limit)
            _warnings.AddWarning(issuer.Id, "LagCompSpoof",
                $"dist={dist:F1} limit={limit:F1} wid={wid}");
    }
}