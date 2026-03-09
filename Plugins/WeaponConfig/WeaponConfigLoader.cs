using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal static class WeaponConfigLoader
    {
        private const string FilePath = "Weapons.json";
        private static readonly JsonSerializerOptions Opts = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static (WeaponConfig cfg, WeaponEntry[] weapons) Load()
        {
            var def = BuildDefaultWeapons();

            if (!File.Exists(FilePath))
            {
                var root = new WeaponConfigRoot { Config = new WeaponConfig(), Weapons = new List<WeaponEntry>(def) };
                File.WriteAllText(FilePath, JsonSerializer.Serialize(root, Opts));
                Console.WriteLine("[WeaponConfig] Weapons.json tidak ditemukan, membuat default...");
                return (root.Config, def);
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<WeaponConfigRoot>(File.ReadAllText(FilePath), Opts) ?? new WeaponConfigRoot();
                var weapons = BuildDefaultWeapons();
                foreach (var w in parsed.Weapons)
                    if (w.Id >= 0 && w.Id < weapons.Length)
                        weapons[w.Id] = w;
                Console.WriteLine("[WeaponConfig] Weapons.json berhasil dimuat.");
                return (parsed.Config ?? new WeaponConfig(), weapons);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeaponConfig] Gagal memuat Weapons.json: {ex.Message}");
                return (new WeaponConfig(), def);
            }
        }

        private static WeaponEntry[] BuildDefaultWeapons()
        {
            // id: dmg, type, range, maxRate, affArmour, torsoOnly
            var d = new (float dmg, DamageType t, float r, int rate, bool arm, bool torso)[]
            {
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 0  Fist
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 1  Brass knuckles
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 2  Golf club
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 3  Nitestick
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 4  Knife
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 5  Bat
                (1.0f,   DamageType.Multiplier, 1.60f, 250, false, false), // 6  Shovel
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 7  Pool cue
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 8  Katana
                (1.0f,   DamageType.Multiplier, 1.76f,  30, false, false), // 9  Chainsaw
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 10 Dildo
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 11 Dildo2
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 12 Vibrator
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 13 Vibrator2
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 14 Flowers
                (1.0f,   DamageType.Multiplier, 1.76f, 250, false, false), // 15 Cane
                (82.5f,  DamageType.Multiplier,  40f,    0, false, false), // 16 Grenade
                (0.0f,   DamageType.Static,      40f,    0, false, false), // 17 Teargas
                (1.0f,   DamageType.Multiplier,  40f,    0, false, false), // 18 Molotov
                (9.9f,   DamageType.Static,      90f,   90,  true,  true), // 19 Vehicle M4
                (46.2f,  DamageType.Static,      75f,   20,  true,  true), // 20 Vehicle Minigun
                (82.5f,  DamageType.Multiplier,   0f,    0, false, false), // 21 Vehicle Rocket
                (8.25f,  DamageType.Static,      35f,  160,  true,  true), // 22 Colt45
                (13.2f,  DamageType.Static,      35f,  120,  true,  true), // 23 Silenced
                (46.2f,  DamageType.Static,      35f,  120,  true,  true), // 24 Deagle
                (3.3f,   DamageType.Static,      40f,  800,  true,  true), // 25 Shotgun
                (3.3f,   DamageType.Static,      35f,  120,  true,  true), // 26 Sawed-off
                (4.95f,  DamageType.Static,      40f,  120,  true,  true), // 27 SPAS
                (6.6f,   DamageType.Static,      35f,   50,  true,  true), // 28 UZI
                (8.25f,  DamageType.Static,      45f,   90,  true,  true), // 29 MP5
                (9.9f,   DamageType.Static,      70f,   90,  true,  true), // 30 AK47
                (9.9f,   DamageType.Static,      90f,   90,  true,  true), // 31 M4
                (6.6f,   DamageType.Static,      35f,   50,  true,  true), // 32 Tec9
                (24.75f, DamageType.Static,     100f,  800,  true,  true), // 33 Cuntgun
                (41.25f, DamageType.Static,     320f,  800,  true,  true), // 34 Sniper
                (82.5f,  DamageType.Multiplier,  55f,    0, false, false), // 35 RPG
                (82.5f,  DamageType.Multiplier,  55f,    0, false, false), // 36 HS-RPG
                (1.0f,   DamageType.Multiplier,  5.1f,   0, false, false), // 37 Flamethrower
                (46.2f,  DamageType.Static,      75f,   20,  true,  true), // 38 Minigun
                (82.5f,  DamageType.Multiplier,  40f,    0, false, false), // 39 Satchel
                (0.0f,   DamageType.Multiplier,  25f,    0, false, false), // 40 Detonator
                (0.33f,  DamageType.Static,      6.1f,  10, false, false), // 41 Spraycan
                (0.33f,  DamageType.Static,     10.1f,  10, false, false), // 42 Fire ext
                (0.0f,   DamageType.Multiplier, 100f,    0,  true, false), // 43 Camera
                (0.0f,   DamageType.Multiplier, 100f,    0,  true, false), // 44 NV goggles
                (0.0f,   DamageType.Multiplier, 100f,    0,  true, false), // 45 IR goggles
                (0.0f,   DamageType.Multiplier, 1.76f,   0,  true, false), // 46 Parachute
                (0.0f,   DamageType.Multiplier,  0f,     0,  true, false), // 47 Fake pistol
                (2.64f,  DamageType.Static,      0f,   400, false, false), // 48 Pistol whip
                (9.9f,   DamageType.Static,      0f,     0, false, false), // 49 Vehicle
                (330f,   DamageType.Static,      0f,     0, false, false), // 50 Heli blades
                (82.5f,  DamageType.Multiplier,  0f,     0, false, false), // 51 Explosion
                (1.0f,   DamageType.Multiplier,  0f,     0, false, false), // 52 Car park
                (1.0f,   DamageType.Multiplier,  0f,     0, false, false), // 53 Drowning
                (165f,   DamageType.Multiplier,  0f,     0, false, false), // 54 Splat
            };

            var arr = new WeaponEntry[d.Length];
            for (var i = 0; i < d.Length; i++)
            {
                var (dmg, t, r, rate, arm, torso) = d[i];
                arr[i] = new WeaponEntry { Id = i, Damage = dmg, Type = t, Range = r, MaxShootRate = rate, AffectsArmour = arm, TorsoOnly = torso };
            }
            return arr;
        }
    }
}