#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
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

        public static string GetRejectedHit(Player p, int index)
            => WeaponConfigService.GetRejectedHitFormatted(p, index);

        public static string GetPreviousHit(Player p, int index)
            => WeaponConfigService.GetPreviousHitFormatted(p, index);

        public static void SetMaxShootRateSamples(int samples)
            => WeaponConfigService.SetMaxShootRateSamples(samples);

        public static int GetMaxShootRateSamples()
            => WeaponConfigService.GetMaxShootRateSamples();

        public static void SetMaxHitRateSamples(int samples)
            => WeaponConfigService.SetMaxHitRateSamples(samples);

        public static int GetMaxHitRateSamples()
            => WeaponConfigService.GetMaxHitRateSamples();

        public static void SetPlayerStreamDistance(float distance)
            => WeaponConfigService.SetPlayerStreamDistance(distance);

        public static float GetPlayerStreamDistance()
            => WeaponConfigService.GetPlayerStreamDistance();

        public static void SetMaxDistFromShot(float distance)
            => WeaponConfigService.SetMaxDistFromShot(distance);

        public static float GetMaxDistFromShot()
            => WeaponConfigService.GetMaxDistFromShot();

        public static void SetMaxDistFromOrigin(float distance)
            => WeaponConfigService.SetMaxDistFromOrigin(distance);

        public static float GetMaxDistFromOrigin()
            => WeaponConfigService.GetMaxDistFromOrigin();

        public static void SetShotTimeout(int ms)
            => WeaponConfigService.SetShotTimeout(ms);

        public static int GetShotTimeout()
            => WeaponConfigService.GetShotTimeout();

        public static void SetDeathSkipTimeout(int ms)
            => WeaponConfigService.SetDeathSkipTimeout(ms);

        public static int GetDeathSkipTimeout()
            => WeaponConfigService.GetDeathSkipTimeout();

        public static void SetMaxPreviousHits(int count)
            => WeaponConfigService.SetMaxPreviousHits(count);

        public static int GetMaxPreviousHits()
            => WeaponConfigService.GetMaxPreviousHits();

        public static bool IsBulletWeapon(int weaponId)
            => WeaponConfigService.IsBulletWeapon(weaponId);

        public static bool IsMeleeWeapon(int weaponId)
            => WeaponConfigService.IsMeleeWeapon(weaponId);

        public static bool IsShotgunWeapon(int weaponId)
            => WeaponConfigService.IsShotgunWeapon(weaponId);

        public static bool IsHighRateWeapon(int weaponId)
            => WeaponConfigService.IsHighRateWeapon(weaponId);

        public static void SetWeaponRangeDamage(int weaponId, DamageType damageType, params float[] values)
            => WeaponConfigService.SetWeaponRangeDamage(weaponId, damageType, values);

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

        public static void ForceClassSelection(Player p)
            => WeaponConfigService.ForcePlayerClassSelection(p);

        public static bool IsInClassSelection(Player p)
            => WeaponConfigService.IsPlayerInClassSelection(p);

        public static void SetPlayerPosFindZ(Player p, float x, float y, float z)
        {
            if (p.IsDisposed || WeaponConfigService.IsPlayerDying(p)) return;

            if (p.Position.Z > z)
                p.Position = new SampSharp.GameMode.Vector3(x, y, p.Position.Z);
            else
                p.Position = new SampSharp.GameMode.Vector3(x, y, z);
        }

        public static void SetWeaponName(int weaponId, string name)
            => WeaponConfigService.SetWeaponName(weaponId, name);

        public static void SetDisableSyncBugs(bool toggle)
            => WeaponConfigService.SetDisableSyncBugs(toggle);

        public static void SetKnifeSync(bool toggle)
            => WeaponConfigService.SetKnifeSync(toggle);

        public static void SetSpawnInfo(Player p, int team, int skin, float x, float y, float z,
            float rotation, Weapon weapon1 = Weapon.None, int ammo1 = 0,
            Weapon weapon2 = Weapon.None, int ammo2 = 0,
            Weapon weapon3 = Weapon.None, int ammo3 = 0)
        {
            WeaponConfigService.AddPlayerClassEx(team, skin,
                new SampSharp.GameMode.Vector3(x, y, z), rotation,
                weapon1, ammo1, weapon2, ammo2, weapon3, ammo3);

            p.SetSpawnInfo(team, skin, new SampSharp.GameMode.Vector3(x, y, z), rotation,
                weapon1, ammo1, weapon2, ammo2, weapon3, ammo3);
        }

        public static void PlayerSpectatePlayer(Player spectator, Player target, SpectateMode mode = SpectateMode.Normal)
        {
            spectator.SpectatePlayer(target, mode);
            WeaponConfigService.OnStartSpectating(spectator, target);
        }

        public static void StopSpectating(Player p)
        {
            WeaponConfigService.OnStopSpectating(p);
            p.ToggleSpectating(false);
        }

        public static bool IsPlayerInCheckpoint(Player p)
        {
            if (!WeaponConfigService.IsPlayerSpawned(p)) return false;
            return p.InCheckpoint;
        }

        public static bool IsPlayerInRaceCheckpoint(Player p)
        {
            if (!WeaponConfigService.IsPlayerSpawned(p)) return false;
            return p.InRaceCheckpoint;
        }

        public static void SetPlayerSpecialAction(Player p, SpecialAction action)
        {
            if (!WeaponConfigService.IsPlayerSpawned(p)) return;
            p.SpecialAction = action;
        }

        public static void SetCbugDeathDelay(bool toggle)
            => WeaponConfigService.SetCbugDeathDelay(toggle);

        public static int CreateVehicle(int modelid, float x, float y, float z, float rotation, int color1, int color2, int respawnDelay, bool addSiren = false)
        {
            var vehicle = BaseVehicle.Create(
                (VehicleModelType)modelid,
                new Vector3(x, y, z), rotation, color1, color2, respawnDelay, addSiren);

            if (vehicle != null)
            {
                WeaponConfigService.OnVehicleSpawn(vehicle.Id);
                vehicle.Spawned += (s, e) => WeaponConfigService.OnVehicleSpawn(vehicle.Id);
                vehicle.Death += (s, e) => WeaponConfigService.OnVehicleDeath(vehicle.Id);
            }

            return vehicle?.Id ?? -1;
        }

        public static void DestroyVehicle(int vehicleid)
        {
            var vehicle = BaseVehicle.Find(vehicleid);
            if (vehicle != null)
            {
                WeaponConfigService.OnVehicleDestroy(vehicleid);
                vehicle.Dispose();
            }
        }

        public static int AddStaticVehicle(int modelid, float x, float y, float z, float rotation,
            int color1, int color2)
        {
            var vehicle = BaseVehicle.Create(
                (VehicleModelType)modelid,
                new SampSharp.GameMode.Vector3(x, y, z), rotation, color1, color2);

            if (vehicle != null)
            {
                WeaponConfigService.OnVehicleSpawn(vehicle.Id);
                vehicle.Spawned += (s, e) => WeaponConfigService.OnVehicleSpawn(vehicle.Id);
                vehicle.Death += (s, e) => WeaponConfigService.OnVehicleDeath(vehicle.Id);
            }

            return vehicle?.Id ?? -1;
        }

        public static int AddStaticVehicleEx(int modelid, float x, float y, float z, float rotation,
            int color1, int color2, int respawnDelay, bool addSiren = false)
        {
            var vehicle = BaseVehicle.Create(
                (VehicleModelType)modelid,
                new SampSharp.GameMode.Vector3(x, y, z), rotation, color1, color2, respawnDelay, addSiren);

            if (vehicle != null)
            {
                WeaponConfigService.OnVehicleSpawn(vehicle.Id);
                vehicle.Spawned += (s, e) => WeaponConfigService.OnVehicleSpawn(vehicle.Id);
                vehicle.Death += (s, e) => WeaponConfigService.OnVehicleDeath(vehicle.Id);
            }

            return vehicle?.Id ?? -1;
        }

        public static int AddPlayerClass(int modelid, float spawnX, float spawnY, float spawnZ,
            float zAngle, Weapon weapon1 = Weapon.None, int weapon1Ammo = 0,
            Weapon weapon2 = Weapon.None, int weapon2Ammo = 0,
            Weapon weapon3 = Weapon.None, int weapon3Ammo = 0)
        {
            return WeaponConfigService.AddPlayerClass(modelid,
                new SampSharp.GameMode.Vector3(spawnX, spawnY, spawnZ), zAngle,
                weapon1, weapon1Ammo, weapon2, weapon2Ammo, weapon3, weapon3Ammo);
        }

        public static int AddPlayerClassEx(int teamid, int modelid, float spawnX, float spawnY,
            float spawnZ, float zAngle, Weapon weapon1 = Weapon.None, int weapon1Ammo = 0,
            Weapon weapon2 = Weapon.None, int weapon2Ammo = 0,
            Weapon weapon3 = Weapon.None, int weapon3Ammo = 0)
        {
            return WeaponConfigService.AddPlayerClassEx(teamid, modelid,
                new SampSharp.GameMode.Vector3(spawnX, spawnY, spawnZ), zAngle,
                weapon1, weapon1Ammo, weapon2, weapon2Ammo, weapon3, weapon3Ammo);
        }

        public static SpawnClassInfo? GetSpawnClass(int classid)
            => WeaponConfigService.GetSpawnClass(classid);

        public static int AverageShootRate(Player p, int shots)
            => WeaponConfigService.AverageShootRate(p, shots);

        public static int AverageShootRate(Player p, int shots, out bool multipleWeapons)
            => WeaponConfigService.AverageShootRate(p, shots, out multipleWeapons);

        public static int AverageHitRate(Player p, int hits)
            => WeaponConfigService.AverageHitRate(p, hits);

        public static int AverageHitRate(Player p, int hits, out bool multipleWeapons)
            => WeaponConfigService.AverageHitRate(p, hits, out multipleWeapons);

        public static float GetLastDamageHealth(Player p)
            => WeaponConfigService.GetLastDamageHealth(p);

        public static float GetLastDamageArmour(Player p)
            => WeaponConfigService.GetLastDamageArmour(p);

        public static void SetLagCompMode(LagCompMode mode)
            => WeaponConfigService.SetLagCompMode(mode);

        public static LagCompMode GetLagCompMode()
            => WeaponConfigService.GetLagCompMode();
    }
}