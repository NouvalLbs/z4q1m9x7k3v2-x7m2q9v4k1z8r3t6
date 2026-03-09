#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public static class WeaponConfigWrappers
    {
        public static PlayerState GetPlayerState(Player p)
        {
            if (WeaponConfigService.IsPlayerDying(p))
                return PlayerState.Wasted;
            return p.State;
        }

        public static float GetPlayerHealth(Player p)
            => WeaponConfigService.GetWcHealth(p);

        public static float GetPlayerArmour(Player p)
            => WeaponConfigService.GetWcArmour(p);

        public static void SetPlayerHealth(Player p, float health)
            => WeaponConfigService.SetPlayerHealth(p, health);

        public static void SetPlayerArmour(Player p, float armour)
            => WeaponConfigService.SetPlayerArmour(p, armour);

        public static int GetPlayerVirtualWorld(Player p)
            => WeaponConfigService.GetIntendedVirtualWorld(p);

        public static void SetPlayerVirtualWorld(Player p, int world)
        {
            p.VirtualWorld = world;
        }

        public static void SendDeathMessage(Player? killer, Player killee, int weapon)
            => WeaponConfigService.SendWcDeathMessage(killer, killee, weapon);

        public static string GetWeaponName(int weaponId)
            => WeaponConfigService.GetWeaponName(weaponId);

        public static void SetPlayerTeam(Player p, int team)
            => WeaponConfigService.SetPlayerTeam(p, team);

        public static int GetPlayerTeam(Player p)
        {
            if (!p.IsConnected) return 255;
            return p.Team;
        }

        public static void ApplyAnimation(Player p, string animLib, string animName,
            float delta = 4.0f, bool loop = false, bool lockX = false, bool lockY = false,
            bool freeze = false, int time = 0, bool forceSync = false)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            p.ApplyAnimation(animLib, animName, delta, loop, lockX, lockY, freeze, time, forceSync);
        }

        public static void ClearAnimations(Player p, bool forceSync = true)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            p.ClearAnimations(forceSync);
        }

        public static void TogglePlayerControllable(Player p, bool toggle)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            p.ToggleControllable(toggle);
        }

        public static void SetPlayerPos(Player p, Vector3 pos)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            p.Position = pos;
        }

        public static void SetPlayerVelocity(Player p, Vector3 velocity)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            p.Velocity = velocity;
        }

        public static bool IsPlayerSpawned(Player p)
            => WeaponConfigService.IsPlayerSpawned(p);

        public static bool IsPlayerPaused(Player p)
            => WeaponConfigService.IsPlayerPaused(p);

        public static void DamagePlayer(Player target, float amount,
            Player? issuer = null, int weapon = 55, int bodypart = 0,
            bool ignoreArmour = false)
            => WeaponConfigService.DamagePlayer(target, amount, issuer,
                weapon, bodypart, ignoreArmour);

        public static void HealPlayer(Player p, float amount)
            => WeaponConfigService.HealPlayer(p, amount);

        public static void ResyncPlayer(Player p)
            => WeaponConfigService.ResyncPlayer(p);

        public static void SetPlayerMaxHealth(Player p, float max)
            => WeaponConfigService.SetPlayerMaxHealth(p, max);

        public static void SetPlayerMaxArmour(Player p, float max)
            => WeaponConfigService.SetPlayerMaxArmour(p, max);

        public static float GetPlayerMaxHealth(Player p)
            => WeaponConfigService.GetPlayerMaxHealth(p);

        public static float GetPlayerMaxArmour(Player p)
            => WeaponConfigService.GetPlayerMaxArmour(p);

        public static void TogglePlayerSpectating(Player p, bool toggle)
        {
            if (toggle)
            {
                p.ToggleSpectating(true);
            }
            else
            {
                WeaponConfigService.OnStopSpectating(p);
                p.ToggleSpectating(false);
            }
        }

        public static void SpawnPlayer(Player p)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            p.Spawn();
        }
    }
}