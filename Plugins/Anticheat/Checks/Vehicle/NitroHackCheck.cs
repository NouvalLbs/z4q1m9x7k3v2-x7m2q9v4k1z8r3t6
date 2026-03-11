using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class NitroHackCheck
{
    private const int Nitro2x = 1008;
    private const int Nitro5x = 1009;
    private const int Nitro10x = 1010;

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NitroHackCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NitroHack").Enabled) return;
        if (player.State != PlayerState.Driving) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        var vehicle = player.Vehicle;
        if (vehicle is null) return;

        var vst = _vehicles.Get(vehicle.Id);
        if (vst is null) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.EnterVehicleTick < 2000) return;

        // Check if vehicle has nitro component
        int model = (int)vehicle.Model;
        bool hasNitroComponent = vst.HasNitro;

        // Detect nitro usage via velocity spikes
        var vel = vehicle.Velocity;
        float speed = VectorMath.Speed(vel.X, vel.Y, vel.Z);

        // Check for nitro boost signature (rapid acceleration)
        if (speed > st.LastVehicleSpeed + 0.3f && speed > 0.5f)
        {
            long timeSinceLastBoost = now - st.LastNitroBoostTick;

            // Nitro boost detected
            if (timeSinceLastBoost > 100) // Ignore rapid velocity checks
            {
                if (!hasNitroComponent && !IsAllowedNitroVehicle(model))
                {
                    _warnings.AddWarning(player.Id, "NitroHack",
                        $"no component veh={vehicle.Id} model={model} spd={speed:F2}");
                }
                else if (hasNitroComponent)
                {
                    // Check nitro consumption
                    st.NitroUseCount++;

                    int maxUses = vst.NitroType switch
                    {
                        Nitro2x => 2,
                        Nitro5x => 5,
                        Nitro10x => 10,
                        _ => 0
                    };

                    if (maxUses > 0 && st.NitroUseCount > maxUses + 2) // +2 tolerance
                    {
                        _warnings.AddWarning(player.Id, "NitroHack",
                            $"infinite uses={st.NitroUseCount} max={maxUses} type={vst.NitroType}");
                    }
                }

                st.LastNitroBoostTick = now;
            }
        }

        st.LastVehicleSpeed = speed;
    }

    public void OnVehicleModAdded(int vehicleId, int componentId)
    {
        if (componentId is not (Nitro2x or Nitro5x or Nitro10x)) return;

        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.HasNitro = true;
        vst.NitroType = componentId;

        // Reset nitro use counter for driver
        foreach (var (pid, pst) in _players.All)
        {
            if (pst.VehicleId == vehicleId)
                pst.NitroUseCount = 0;
        }
    }

    public void OnPlayerEnterVehicle(int playerId, int vehicleId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.NitroUseCount = 0;
        st.LastVehicleSpeed = 0f;
    }

    public void OnPlayerExitVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.NitroUseCount = 0;
        st.LastVehicleSpeed = 0f;
    }

    private static bool IsAllowedNitroVehicle(int model)
    {
        // Some vehicles naturally have boost-like behavior
        // RC vehicles, trains, special vehicles
        return VehicleData.IsRC(model)
            || VehicleData.IsTrain(model)
            || model is 441 or 464 or 465 or 501 or 564 or 594; // RC Baron, RC Tiger, etc.
    }
}