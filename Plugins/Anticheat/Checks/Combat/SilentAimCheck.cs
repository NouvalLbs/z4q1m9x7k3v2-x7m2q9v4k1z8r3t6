using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.State;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class SilentAimCheck
{
    private const float MaxAngleDifference = 25f;
    private const float MaxVerticalAngleDiff = 30f;
    private const float MinDistanceForCheck = 5f;
    private const float MaxSilentAimDistance = 150f;

    private static readonly HashSet<int> _validWeapons = new()
    {
        22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 38
    };

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public SilentAimCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerWeaponShot(BasePlayer player, WeaponShotEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("SilentAim").Enabled) return;
        if (e.BulletHitType != BulletHitType.Player) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        int weaponId = (int)e.Weapon;
        if (!_validWeapons.Contains(weaponId)) return;

        var hitPlayer = BasePlayer.Find((int)e.HitId);
        if (hitPlayer is null) return;

        var hitSt = _players.Get(hitPlayer.Id);
        if (hitSt is null) return;

        var shooterPos = player.Position;
        var targetPos = hitPlayer.Position;

        float distance = VectorMath.Dist(
            shooterPos.X, shooterPos.Y, shooterPos.Z,
            targetPos.X, targetPos.Y, targetPos.Z
        );

        float expectedAngle = MathF.Atan2(
            targetPos.Y - shooterPos.Y,
            targetPos.X - shooterPos.X
        ) * (180f / MathF.PI);

        float playerAngle = player.Angle;
        float angleDiff = AngleHelper.Diff(expectedAngle, playerAngle);

        if (player.State == PlayerState.OnFoot) {
            CheckOnFootSilentAim(player, st, angleDiff, distance, weaponId, targetPos, shooterPos);
        } else if (player.State == PlayerState.Driving) {
            CheckVehicleSilentAim(player, st, angleDiff, distance, weaponId);
        }
    }

    private void CheckOnFootSilentAim(BasePlayer player, PlayerAcState st, float angleDiff, float distance, int weaponId, SampSharp.GameMode.Vector3 targetPos, SampSharp.GameMode.Vector3 shooterPos)
    {
        player.GetKeys(out Keys keys, out _, out _);
        bool isAiming = (keys & Keys.Aim) != 0 || (keys & Keys.SecondaryAttack) != 0;

        if (angleDiff > MaxAngleDifference)
        {
            float verticalAngle = MathF.Atan2(
                targetPos.Z - shooterPos.Z,
                VectorMath.Dist2D(shooterPos.X, shooterPos.Y, targetPos.X, targetPos.Y)
            ) * (180f / MathF.PI);

            float absVertical = MathF.Abs(verticalAngle);

            if (absVertical > MaxVerticalAngleDiff && isAiming)
            {
                _warnings.AddWarning(player.Id, "SilentAim",
                    $"angle={angleDiff:F1}° vert={absVertical:F1}° dist={distance:F1} wid={weaponId}");
            }
            else if (!isAiming)
            {
                _warnings.AddWarning(player.Id, "SilentAim",
                    $"no aim angle={angleDiff:F1}° dist={distance:F1} wid={weaponId}");
            }
        }
    }

    private void CheckVehicleSilentAim(BasePlayer player, PlayerAcState st, float angleDiff, float distance, int weaponId)
    {
        if (angleDiff > MaxAngleDifference * 1.5f)
        {
            _warnings.AddWarning(player.Id, "SilentAim",
                $"vehicle angle={angleDiff:F1}° dist={distance:F1} wid={weaponId}");
        }
    }
}