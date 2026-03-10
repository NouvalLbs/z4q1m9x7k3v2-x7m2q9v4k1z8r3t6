using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using System.Threading.Tasks;

namespace ProjectSMP.Plugins.Anticheat.Core;

public abstract class BaseCheck
{
    protected readonly WarningManager Warnings;
    protected readonly AnticheatConfig Config;

    protected BaseCheck(WarningManager warnings, AnticheatConfig config)
    {
        Warnings = warnings;
        Config = config;
    }

    public abstract string Name { get; }
    protected bool IsEnabled => Config.Enabled && Config.GetCheck(Name).Enabled;
}

public abstract class PlayerCheck : BaseCheck
{
    protected PlayerCheck(WarningManager warnings, AnticheatConfig config)
        : base(warnings, config) { }

    public abstract Task<CheckResult> ExecuteAsync(int playerId);
}

public abstract class VehicleCheck : BaseCheck
{
    protected VehicleCheck(WarningManager warnings, AnticheatConfig config)
        : base(warnings, config) { }

    public abstract Task<CheckResult> ExecuteAsync(int vehicleId, int driverId);
}