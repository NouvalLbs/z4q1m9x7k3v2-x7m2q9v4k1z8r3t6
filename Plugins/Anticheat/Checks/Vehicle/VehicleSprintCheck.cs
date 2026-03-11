using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

/// <summary>
/// Detects players using sprint key while in vehicles.
/// This can cause visual glitches, animation bugs, or be used for exploits.
/// In normal gameplay, sprint key should have no effect in vehicles.
/// </summary>
public class VehicleSprintCheck
{
    private const int ConsecutiveSprintChecks = 3; // Must sprint 3 times consecutively
    private const long SprintResetMs = 2000; // Reset counter after 2 seconds

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public VehicleSprintCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("VehicleSprint").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        // Only check when player is in a vehicle
        var pState = player.State;
        if (pState != PlayerState.Driving && pState != PlayerState.Passenger)
        {
            // Reset counter when not in vehicle
            if (st.VehicleSprintCount > 0)
            {
                st.VehicleSprintCount = 0;
                st.LastVehicleSprintTick = 0;
            }
            return;
        }

        long now = Environment.TickCount64;

        // Skip during grace periods
        if (now - st.SpawnTick < 3000) return;
        if (now - st.EnterVehicleTick < 1000) return;

        // Get player keys
        player.GetKeys(out Keys keys, out _, out _);
        bool isSprinting = (keys & Keys.Sprint) != 0;

        if (isSprinting)
        {
            // Increment sprint counter
            st.VehicleSprintCount++;
            st.LastVehicleSprintTick = now;

            // Warn after consecutive sprint detections
            if (st.VehicleSprintCount >= ConsecutiveSprintChecks)
            {
                var vehicle = player.Vehicle;
                int vehicleId = vehicle?.Id ?? -1;
                int model = vehicle is not null ? (int)vehicle.Model : -1;

                _warnings.AddWarning(player.Id, "VehicleSprint",
                    $"count={st.VehicleSprintCount} state={pState} veh={vehicleId} model={model}");

                // Reset counter to avoid spam (will increment again if still sprinting)
                st.VehicleSprintCount = 0;
            }
        }
        else
        {
            // Reset counter if player stops sprinting or after timeout
            if (st.VehicleSprintCount > 0 && now - st.LastVehicleSprintTick > SprintResetMs)
            {
                st.VehicleSprintCount = 0;
            }
        }
    }

    public void OnPlayerEnterVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        // Reset counter when entering vehicle
        st.VehicleSprintCount = 0;
        st.LastVehicleSprintTick = 0;
    }

    public void OnPlayerExitVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        // Reset counter when exiting vehicle
        st.VehicleSprintCount = 0;
        st.LastVehicleSprintTick = 0;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.VehicleSprintCount = 0;
        st.LastVehicleSprintTick = 0;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.VehicleSprintCount = 0;
        st.LastVehicleSprintTick = 0;
    }
}