#nullable enable
using ProjectSMP.Plugins.Anticheat.State;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Managers;

public class VehicleStateManager
{
    private readonly ConcurrentDictionary<int, VehicleAcState> _states = new();

    public VehicleAcState GetOrCreate(int vehicleId) =>
        _states.GetOrAdd(vehicleId, _ => new VehicleAcState());

    public VehicleAcState? Get(int vehicleId) =>
        _states.TryGetValue(vehicleId, out var s) ? s : null;

    public void Remove(int vehicleId) => _states.TryRemove(vehicleId, out _);

    public bool Exists(int vehicleId) => _states.ContainsKey(vehicleId);

    public IEnumerable<KeyValuePair<int, VehicleAcState>> All => _states;
}