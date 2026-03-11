using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class VehicleHealthCheck
{
    private const float AllowedGain = 50f; // Normal repair tolerance
    private const float PayNSprayGain = 500f; // Pay N Spray full repair
    private const float MinDamageForCheck = 100f; // Only check if vehicle was damaged
    private const float MaxVehicleHealth = 1000f;

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public VehicleHealthCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("VehicleHealthHack").Enabled) return;
        if (player.State != PlayerState.Driving) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        var vehicle = player.Vehicle;
        if (vehicle is null) return;

        var vst = _vehicles.GetOrCreate(vehicle.Id);
        long now = Environment.TickCount64;

        // Skip checks during grace periods
        if (now - st.SpawnTick < 3000) { vst.LastHealth = vehicle.Health; return; }
        if (now - st.EnterVehicleTick < 2000) { vst.LastHealth = vehicle.Health; return; }

        float currentHealth = vehicle.Health;
        float lastHealth = vst.LastHealth;

        // Initialize tracking on first check
        if (lastHealth == 0f)
        {
            vst.LastHealth = currentHealth;
            return;
        }

        float healthChange = currentHealth - lastHealth;

        // Only check for suspicious health INCREASES
        if (healthChange <= AllowedGain)
        {
            vst.LastHealth = currentHealth;
            return;
        }

        // Check if health increase is legitimate
        bool serverSet = now - vst.NopSetHealthDeadline < 2000;
        if (serverSet)
        {
            vst.LastHealth = currentHealth;
            return;
        }

        // Check if in Pay N Spray
        var pos = vehicle.Position;
        bool inPayNSpray = VehicleData.IsInPayNSpray(pos.X, pos.Y, pos.Z);

        if (inPayNSpray)
        {
            // Pay N Spray is allowed, but validate the repair amount
            if (healthChange > PayNSprayGain + 100f) // Tolerance for Pay N Spray
            {
                _warnings.AddWarning(player.Id, "VehicleHealthHack",
                    $"excessive paynspray gain={healthChange:F1} veh={vehicle.Id}");
            }
            vst.LastHealth = currentHealth;
            vst.LastPayNSprayTick = now;
            return;
        }

        // Check if just left Pay N Spray (grace period)
        if (now - vst.LastPayNSprayTick < 3000)
        {
            vst.LastHealth = currentHealth;
            return;
        }

        // Check for health hack - instant repair
        if (healthChange > AllowedGain)
        {
            // Instant full repair
            if (currentHealth >= MaxVehicleHealth - 10f && lastHealth < MaxVehicleHealth - MinDamageForCheck)
            {
                _warnings.AddWarning(player.Id, "VehicleHealthHack",
                    $"instant repair prev={lastHealth:F1} curr={currentHealth:F1} veh={vehicle.Id}");
            }
            // Partial instant repair (still suspicious)
            else if (healthChange > 150f)
            {
                _warnings.AddWarning(player.Id, "VehicleHealthHack",
                    $"rapid repair gain={healthChange:F1} veh={vehicle.Id}");
            }
        }

        // Check for health above maximum
        if (currentHealth > MaxVehicleHealth + 10f)
        {
            _warnings.AddWarning(player.Id, "VehicleHealthHack",
                $"above max health={currentHealth:F1} veh={vehicle.Id}");
        }

        vst.LastHealth = currentHealth;
    }

    public void OnPlayerEnterVehicle(int playerId, int vehicleId)
    {
        var vehicle = BaseVehicle.Find(vehicleId);
        if (vehicle is null) return;

        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.LastHealth = vehicle.Health;
        vst.LastPayNSprayTick = 0;
    }

    public void OnPlayerExitVehicle(int playerId, int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is not null)
        {
            vst.LastHealth = 0f; // Reset tracking when player exits
        }
    }

    public void OnVehicleRespawned(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is null) return;

        vst.LastHealth = 1000f; // Vehicles spawn at full health
        vst.LastPayNSprayTick = 0;
    }

    public void OnVehicleDestroyed(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is not null)
        {
            vst.LastHealth = 0f;
        }
    }
}