#nullable enable
using System;
using System.Collections.Generic;
using ProjectSMP.Core;
using ProjectSMP.Entities;
using ProjectSMP.Entities.Players.Delay;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Bank.Paycheck;
using ProjectSMP.Features.ProgressBar;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;

namespace ProjectSMP.Features.Jobs.Side.Forklifter
{
    public static class ForklifterService
    {
        private const int MaxCycles = 6;
        private const int PaycheckAmount = 50000;
        private const int DelayMinutes = 30;
        private const int ProgressDuration = 6;
        private const float CheckpointSize = 4.0f;

        private static readonly HashSet<int> _vehicleIds = new();
        private static readonly Dictionary<int, ForklifterSession> _sessions = new();
        private static Timer? _pollTimer;
        private static readonly Random _rng = new();

        private static readonly Vector3[] LoadPositions =
        {
            new(2555.8987f, -2465.7002f, 13.4002f),
            new(2783.2563f, -2369.0671f, 13.4004f),
            new(2445.2659f, -2490.4629f, 13.4105f),
            new(2402.7097f, -2565.7771f, 13.3183f),
            new(2349.3796f, -2332.3906f, 13.2158f)
        };

        private static readonly Vector3[] UnloadPositions =
        {
            new(2778.0747f, -2414.1794f, 13.4041f),
            new(2793.6707f, -2458.9797f, 13.3978f),
            new(2779.5942f, -2459.3501f, 13.4039f),
            new(2787.3235f, -2490.9282f, 13.4180f)
        };

        private static readonly (float X, float Y, float Z, float A)[] SpawnPoints =
        {
            (2758.7400f, -2385.7976f, 13.4077f, 177.1400f),
            (2749.7400f, -2385.7957f, 13.4244f, 177.1422f),
            (2753.1106f, -2385.8047f, 13.4057f, 177.4073f),
            (2755.7400f, -2385.7966f, 13.4036f, 177.1400f)
        };

        public static void Initialize()
        {
            foreach (var (x, y, z, a) in SpawnPoints)
            {
                var v = Vehicle.CreateVehicle((VehicleModelType)530, new Vector3(x, y, z), a, -1, -1, 60);
                v.VehicleType = VehicleType.Job;
                _vehicleIds.Add(v.Id);
            }

            _pollTimer = new Timer(500, true);
            _pollTimer.Tick += OnTick;

            Console.WriteLine($"[+] Forklifter - {_vehicleIds.Count} vehicles spawned.");
        }

        public static void Dispose() => _pollTimer?.Dispose();

        public static void OnPlayerEnterVehicle(Player player, Vehicle? vehicle, bool isPassenger)
        {
            if (isPassenger || vehicle == null || !_vehicleIds.Contains(vehicle.Id)) return;
            if (!player.IsCharLoaded || _sessions.ContainsKey(player.Id)) return;

            player.ShowMessage("Side Job - Forklift", "Anda akan bekerja sebagai forklift?")
                .WithButtons("Start Job", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) {
                        player.RemoveFromVehicleSafe();
                        vehicle.RespawnAtSpawnPoint();
                        return;
                    }

                    if (DelayService.HasJobDelay(player, "forklifter")) {
                        var rem = DelayService.GetJobDelay(player, "forklifter");
                        player.SendClientMessage(Color.White,
                            $"{Msg.Jobs} Kamu harus menunggu {{FF6347}}{rem} menit{{FFFFFF}} sebelum bekerja sebagai Forklift lagi.");
                        player.RemoveFromVehicleSafe();
                        vehicle.RespawnAtSpawnPoint();
                        return;
                    }

                    StartJob(player);
                });
        }

        public static void OnPlayerExitVehicle(Player player, Vehicle? vehicle)
        {
            if (vehicle == null || !_vehicleIds.Contains(vehicle.Id)) return;
            if (!_sessions.ContainsKey(player.Id)) return;

            CancelJob(player);
            player.SendClientMessage(Color.White, $"{Msg.Jobs} Pekerjaan Forklift dibatalkan karena keluar dari kendaraan.");
        }

        private static void StartJob(Player player)
        {
            var session = new ForklifterSession
            {
                IsActive = true,
                Phase = ForklifterPhase.GoToLoad,
                CurrentLoadIndex = _rng.Next(LoadPositions.Length),
                CurrentUnloadIndex = _rng.Next(UnloadPositions.Length)
            };
            _sessions[player.Id] = session;

            player.SetCheckpoint(LoadPositions[session.CurrentLoadIndex], CheckpointSize);
            player.SendClientMessage(Color.White,
                $"{Msg.Jobs} Pekerjaan Forklift dimulai! Pergi ke {{FFFF00}}titik muat{{FFFFFF}}.");
        }

        private static void OnTick(object sender, EventArgs e)
        {
            foreach (var (id, session) in new Dictionary<int, ForklifterSession>(_sessions))
            {
                var player = BasePlayer.Find(id) as Player;
                if (player == null || !player.IsConnected || !player.IsCharLoaded)
                {
                    _sessions.Remove(id);
                    continue;
                }
                Process(player, session);
            }
        }

        private static void Process(Player player, ForklifterSession session)
        {
            switch (session.Phase)
            {
                case ForklifterPhase.GoToLoad:
                    if (!player.IsInCheckpointSafe()) return;
                    player.DisableCheckpoint();
                    session.Phase = ForklifterPhase.Loading;
                    ProgressBarService.StartProgress(player, ProgressDuration, "Loading_Cargo...");
                    break;

                case ForklifterPhase.Loading:
                    if (player.ProgressBarData.IsActive) return;
                    session.LoadCount++;
                    session.CurrentUnloadIndex = _rng.Next(UnloadPositions.Length);
                    session.Phase = ForklifterPhase.GoToUnload;
                    player.SetCheckpoint(UnloadPositions[session.CurrentUnloadIndex], CheckpointSize);
                    player.SendClientMessage(Color.White,
                        $"{Msg.Jobs} Kargo dimuat! Antarkan ke {{FFFF00}}titik unload{{FFFFFF}}. ({session.LoadCount}/{MaxCycles})");
                    break;

                case ForklifterPhase.GoToUnload:
                    if (!player.IsInCheckpointSafe()) return;
                    player.DisableCheckpoint();
                    session.Phase = ForklifterPhase.Unloading;
                    ProgressBarService.StartProgress(player, ProgressDuration, "Unloading_Cargo...");
                    break;

                case ForklifterPhase.Unloading:
                    if (player.ProgressBarData.IsActive) return;
                    session.UnloadCount++;

                    if (session.UnloadCount >= MaxCycles)
                    {
                        CompleteJob(player);
                        return;
                    }

                    session.CurrentLoadIndex = _rng.Next(LoadPositions.Length);
                    session.Phase = ForklifterPhase.GoToLoad;
                    player.SetCheckpoint(LoadPositions[session.CurrentLoadIndex], CheckpointSize);
                    player.SendClientMessage(Color.White,
                        $"{Msg.Jobs} Kargo diturunkan! Kembali ke {{FFFF00}}titik muat{{FFFFFF}}. ({session.UnloadCount}/{MaxCycles})");
                    break;
            }
        }

        private static void CompleteJob(Player player)
        {
            _sessions.Remove(player.Id);
            player.DisableCheckpoint();

            DelayService.SetJobDelay(player, "forklifter", DelayMinutes);
            PaycheckService.GivePaycheck(player, PaycheckAmount, "Side Job - Forklift");

            player.SendClientMessage(Color.White,
                $"{Msg.Jobs} Kerja bagus! Paycheck {{00FF00}}{Utilities.GroupDigits(PaycheckAmount)}{{FFFFFF}} ditambahkan dan delay {{FF6347}}{DelayMinutes} menit{{FFFFFF}} dimulai.");
        }

        private static void CancelJob(Player player)
        {
            _sessions.Remove(player.Id);
            player.DisableCheckpoint();

            if (player.ProgressBarData.IsActive)
                ProgressBarService.DestroyProgressBar(player);
        }
    }
}