using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Linq;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class FakePickupCheck
{
    private const float MaxPickupDistance = 3.5f; // Max distance to pick up
    private const long MinPickupInterval = 100; // Min 100ms between pickups

    private readonly PlayerStateManager _players;
    private readonly PickupStateManager _pickups;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public FakePickupCheck(PlayerStateManager p, PickupStateManager pk, WarningManager w, AnticheatConfig c)
        => (_players, _pickups, _warnings, _config) = (p, pk, w, c);

    public void OnPlayerPickUpPickup(BasePlayer player, PickUpPickupEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("FakePickup").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        int pickupId = e.Pickup.Id;

        // Check 1: Verify pickup exists in registered pickups
        var pickup = _pickups.Get(pickupId);
        if (pickup is null)
        {
            _warnings.AddWarning(player.Id, "FakePickup",
                $"unregistered id={pickupId}");
            return;
        }

        // Check 2: Verify distance to pickup
        var pos = player.Position;
        float dist = VectorMath.Dist(pos.X, pos.Y, pos.Z, pickup.X, pickup.Y, pickup.Z);

        if (dist > MaxPickupDistance)
        {
            _warnings.AddWarning(player.Id, "FakePickup",
                $"too far id={pickupId} dist={dist:F1} max={MaxPickupDistance}");
            return;
        }

        // Check 3: Pickup spam detection
        long timeSinceLastPickup = now - st.LastPickupTick;
        if (timeSinceLastPickup < MinPickupInterval)
        {
            _warnings.AddWarning(player.Id, "FakePickup",
                $"spam id={pickupId} interval={timeSinceLastPickup}ms");
            return;
        }

        // Check 4: Verify same pickup not picked up twice rapidly
        if (st.LastPickupId == pickupId && timeSinceLastPickup < 1000)
        {
            _warnings.AddWarning(player.Id, "FakePickup",
                $"duplicate id={pickupId} interval={timeSinceLastPickup}ms");
        }

        st.LastPickupTick = now;
        st.LastPickupId = pickupId;
    }

    public void OnPickupCreated(int pickupId, float x, float y, float z, int type, int weapon, int amount)
    {
        _pickups.Register(pickupId, x, y, z, type, weapon, amount);
    }

    public void OnPickupDestroyed(int pickupId)
    {
        _pickups.Remove(pickupId);
    }

    public bool IsValidPickup(int pickupId)
        => _pickups.Get(pickupId) is not null;

    public int GetPickupCount()
        => _pickups.All.Count();
}