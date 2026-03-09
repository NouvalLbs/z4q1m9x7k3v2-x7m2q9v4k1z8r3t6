#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal readonly struct VendingMachineEntry
    {
        public readonly int Model;
        public readonly int Interior;
        public readonly Vector3 Position;
        public readonly Vector3 Rotation;

        public VendingMachineEntry(int model, int interior,
            float x, float y, float z,
            float rx, float ry, float rz)
        {
            Model = model;
            Interior = interior;
            Position = new Vector3(x, y, z);
            Rotation = new Vector3(rx, ry, rz);
        }
    }

    internal static class WeaponConfigVendingMachines
    {
        // SA:MP vending machine building radius for RemoveBuilding
        private const float RemoveRadius = 0.5f;
        // Distance from which "use" triggers
        private const float UseRadius = 2.0f;
        // SA:MP vending-machine cooldown mirrors the real building (~60 s)
        private const int CooldownMs = 60_000;
        // Default health given per use
        private const float DefaultHealth = 5f;

        private static bool _enabled;
        private static readonly List<GlobalObject> _objects = new();
        private static readonly Dictionary<int, int> _lastUsed = new(); // playerId → tick

        public static event EventHandler<VendingMachineArgs>? PlayerUseVendingMachine;

        // ── Vending machine world positions ─────────────────────────────
        // Source:  weapon-config.inc  s_VendingMachines[]
        // Model 1209 = Sprunk machine,  Model 1210 = Food/snack machine
        // TODO: populate the full list from the .inc if you need all GTA:SA positions.
        // A curated subset is included below; add more entries as needed.
        private static readonly VendingMachineEntry[] Machines =
        {
            // ── Interior 0 (exterior world) ──────────────────────────────
            new(1209, 0,  375.57f, -131.21f, 1001.51f, 0f, 0f, 270f),  // Roboi's Food Mart
            new(1209, 0,  489.65f, -78.98f,  998.76f,  0f, 0f, 180f),  // Binco clothing
            new(1209, 0, -2670.0f, 1399.4f,   7.19f,   0f, 0f, 180f),  // El Quebrados
            new(1210, 0,  376.62f, -132.59f, 1001.51f, 0f, 0f,  90f),  // Roboi's (food side)
            // ── Interior 6 (diner 24/7 style) ────────────────────────────
            new(1209, 6,   -8.02f,  -35.34f, 1003.57f, 0f, 0f,  90f),
            // ── Add additional entries here, sourced from weapon-config.inc ──
        };

        // ── Init / shutdown ─────────────────────────────────────────────

        public static void Init(bool enabled)
        {
            _enabled = enabled;
            if (!enabled) return;

            // Create replacement objects visible to all players in interior 0.
            // Objects in other interiors are handled per-player via RemoveBuilding only.
            foreach (var m in Machines)
            {
                if (m.Interior != 0) continue;
                var obj = new GlobalObject(m.Model, m.Position, m.Rotation);
                _objects.Add(obj);
            }
        }

        public static void Dispose()
        {
            foreach (var obj in _objects) obj.Dispose();
            _objects.Clear();
        }

        // ── Per-player lifecycle ─────────────────────────────────────────

        public static void OnConnect(BasePlayer player)
        {
            if (!_enabled) return;

            // Remove the real SA:MP buildings so the client can't interact with them.
            /* foreach (var m in Machines)
                player.RemoveBuilding(m.Model, m.Position, RemoveRadius); */
        }

        public static void OnDisconnect(BasePlayer player)
            => _lastUsed.Remove(player.Id);

        // ── Per-tick proximity check (call from WeaponConfigService.OnUpdate) ─

        public static void OnUpdate(Player player)
        {
            if (!_enabled) return;
            if (player.Interior != GetExpectedInterior(player)) return;

            var now = Environment.TickCount;
            var playerPos = player.Position;

            foreach (var m in Machines)
            {
                if (m.Interior != player.Interior) continue;
                if (Dist(playerPos, m.Position) > UseRadius) continue;

                // Enforce cooldown
                if (_lastUsed.TryGetValue(player.Id, out var last) &&
                    now - last < CooldownMs) continue;

                _lastUsed[player.Id] = now;

                var args = new VendingMachineArgs { Player = player, HealthGiven = DefaultHealth };
                PlayerUseVendingMachine?.Invoke(null, args);

                if (!args.Cancel && args.HealthGiven > 0)
                    WeaponConfigService.HealPlayer(player, args.HealthGiven);

                break; // only one machine per tick
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static int GetExpectedInterior(Player p) => p.Interior;

        private static float Dist(Vector3 a, Vector3 b)
        {
            var dx = a.X - b.X; var dy = a.Y - b.Y; var dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}