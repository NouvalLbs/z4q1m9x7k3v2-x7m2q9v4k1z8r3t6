using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class PaintJobCheck
{
    private static readonly Dictionary<int, HashSet<int>> _validPaintjobs = new()
    {
        { 483, new HashSet<int> { 0, 1 } },
        { 534, new HashSet<int> { 0, 1, 2 } },
        { 535, new HashSet<int> { 0, 1, 2 } },
        { 536, new HashSet<int> { 0, 1, 2 } },
        { 558, new HashSet<int> { 0, 1, 2 } },
        { 559, new HashSet<int> { 0, 1, 2 } },
        { 560, new HashSet<int> { 0, 1, 2 } },
        { 561, new HashSet<int> { 0, 1, 2 } },
        { 562, new HashSet<int> { 0, 1, 2 } },
        { 565, new HashSet<int> { 0, 1, 2 } },
        { 567, new HashSet<int> { 0, 1, 2 } },
        { 575, new HashSet<int> { 0, 1 } },
        { 576, new HashSet<int> { 0, 1, 2 } }
    };

    private const int MaxPaintjobChangesPerMinute = 10;
    private const long PaintjobChangeWindowMs = 60000;

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public PaintJobCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnVehiclePaintjob(BaseVehicle vehicle, BasePlayer player, int paintjobId)
    {
        if (!_config.Enabled || !_config.GetCheck("PaintJobHack").Enabled) return;

        var pst = _players.Get(player.Id);
        if (pst is null) return;

        var vst = _vehicles.GetOrCreate(vehicle.Id);
        int model = (int)vehicle.Model;

        long now = Environment.TickCount64;
        if (now - pst.SpawnTick < 3000) { vst.PaintJob = paintjobId; return; }

        if (!IsValidPaintjob(model, paintjobId))
        {
            _warnings.AddWarning(player.Id, "PaintJobHack",
                $"invalid model={model} paintjob={paintjobId}");
            return;
        }

        if (!pst.IsInModShop && now - vst.LastServerPaintjobTick > 2000)
        {
            _warnings.AddWarning(player.Id, "PaintJobHack",
                $"outside shop model={model} paintjob={paintjobId}");
            return;
        }

        vst.PaintjobChangeHistory.Enqueue(now);
        while (vst.PaintjobChangeHistory.Count > 0 &&
               now - vst.PaintjobChangeHistory.Peek() > PaintjobChangeWindowMs)
        {
            vst.PaintjobChangeHistory.Dequeue();
        }

        if (vst.PaintjobChangeHistory.Count > MaxPaintjobChangesPerMinute)
        {
            _warnings.AddWarning(player.Id, "PaintJobHack",
                $"spam count={vst.PaintjobChangeHistory.Count} model={model}");
        }

        vst.PaintJob = paintjobId;
    }

    public void OnServerSetPaintjob(int vehicleId, int paintjobId)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.PaintJob = paintjobId;
        vst.LastServerPaintjobTick = Environment.TickCount64;
    }

    public void OnVehicleRespawned(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is null) return;

        vst.PaintJob = 3;
        vst.PaintjobChangeHistory.Clear();
        vst.LastServerPaintjobTick = 0;
    }

    public void OnVehicleDestroyed(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is null) return;

        vst.PaintJob = 3;
        vst.PaintjobChangeHistory.Clear();
    }

    public static bool IsValidPaintjob(int model, int paintjobId)
    {
        if (paintjobId == 3) return true;

        if (!_validPaintjobs.TryGetValue(model, out var validJobs))
            return false;

        return validJobs.Contains(paintjobId);
    }

    public static bool VehicleSupportsPaintjob(int model)
        => _validPaintjobs.ContainsKey(model);

    public static HashSet<int>? GetValidPaintjobs(int model)
        => _validPaintjobs.TryGetValue(model, out var jobs) ? jobs : null;
}