using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class VehicleModHackCheck
{
    private const long ComponentAddGracePeriod = 2000; // 2 seconds after server adds
    private const int MaxComponentChangesPerMin = 15;

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public VehicleModHackCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnVehicleComponentAdded(BaseVehicle vehicle, BasePlayer player, int componentId)
    {
        if (!_config.Enabled || !_config.GetCheck("VehicleModHack").Enabled) return;

        var pst = _players.Get(player.Id);
        if (pst is null) return;

        var vst = _vehicles.GetOrCreate(vehicle.Id);
        long now = Environment.TickCount64;

        // Check 1: Component added outside mod shop
        bool inModShop = pst.IsInModShop;
        bool serverAuthorized = now - vst.LastServerModTick < ComponentAddGracePeriod;

        if (!inModShop && !serverAuthorized)
        {
            _warnings.AddWarning(player.Id, "VehicleModHack",
                $"outside shop comp={componentId} veh={vehicle.Id}");
            return;
        }

        // Check 2: Component spam detection
        vst.ComponentChangeHistory.Enqueue(now);
        while (vst.ComponentChangeHistory.Count > 0 &&
               now - vst.ComponentChangeHistory.Peek() > 60000) // 60 sec window
        {
            vst.ComponentChangeHistory.Dequeue();
        }

        if (vst.ComponentChangeHistory.Count > MaxComponentChangesPerMin)
        {
            _warnings.AddWarning(player.Id, "VehicleModHack",
                $"spam count={vst.ComponentChangeHistory.Count} comp={componentId}");
            return;
        }

        // Check 3: Track installed components
        int model = (int)vehicle.Model;
        if (TuningData.IsValidComponent(model, componentId))
        {
            vst.InstalledComponents.Add(componentId);
        }
    }

    public void OnVehicleComponentRemoved(BaseVehicle vehicle, int componentId)
    {
        var vst = _vehicles.Get(vehicle.Id);
        if (vst is null) return;
        vst.InstalledComponents.Remove(componentId);
    }

    public void OnServerAddComponent(int vehicleId, int componentId)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.LastServerModTick = Environment.TickCount64;
        vst.InstalledComponents.Add(componentId);
    }

    public void OnServerRemoveComponent(int vehicleId, int componentId)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.LastServerModTick = Environment.TickCount64;
        vst.InstalledComponents.Remove(componentId);
    }

    public void OnVehicleRespawned(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is null) return;

        vst.InstalledComponents.Clear();
        vst.ComponentChangeHistory.Clear();
        vst.HasNitro = false;
        vst.NitroType = -1;
    }

    public void OnVehicleDestroyed(int vehicleId)
    {
        _vehicles.Remove(vehicleId);
    }

    public bool HasComponent(int vehicleId, int componentId)
    {
        var vst = _vehicles.Get(vehicleId);
        return vst?.InstalledComponents.Contains(componentId) ?? false;
    }
}