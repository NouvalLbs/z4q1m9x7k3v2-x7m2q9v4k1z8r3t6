using ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;
using ProjectSMP.Plugins.Anticheat.Checks.AntiFlood;
using ProjectSMP.Plugins.Anticheat.Checks.Combat;
using ProjectSMP.Plugins.Anticheat.Checks.Movement;
using ProjectSMP.Plugins.Anticheat.Checks.Player;
using ProjectSMP.Plugins.Anticheat.Checks.Server;
using ProjectSMP.Plugins.Anticheat.Checks.Spawn;
using ProjectSMP.Plugins.Anticheat.Checks.Vehicle;
using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.IO;
using System.Text.Json;
using System.Timers;

namespace ProjectSMP.Plugins.Anticheat;

public class AnticheatPlugin
{
    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly FloodRateLimiter _flood;
    private readonly AcLogger _logger;
    private readonly AnticheatConfig _config;

    private AirBreakCheck _airBreak = null!;
    private TeleportCheck _teleport = null!;
    private SpeedHackCheck _speedHack = null!;
    private FlyHackCheck _flyHack = null!;
    private HealthCheck _health = null!;
    private ArmourCheck _armour = null!;
    private MoneyCheck _money = null!;
    private WeaponCheck _weapon = null!;
    private AmmoCheck _ammo = null!;
    private GodModeCheck _godMode = null!;
    private SpecialActionCheck _specialAction = null!;
    private InvisibleCheck _invisible = null!;
    private FakeSpawnCheck _fakeSpawn = null!;
    private FakeKillCheck _fakeKill = null!;

    private RapidFireCheck _rapidFire = null!;
    private ProAimCheck _proAim = null!;
    private QuickTurnCheck _quickTurn = null!;
    private LagCompSpoofCheck _lagComp = null!;
    private CarShotCheck _carShot = null!;
    private FullAimingCheck _fullAiming = null!;
    private CjRunCheck _cjRun = null!;
    private AfkGhostCheck _afkGhost = null!;

    private CarJackCheck _carJack = null!;

    private ReconnectCheck _reconnect = null!;
    private PingCheck _ping = null!;
    private DialogHackCheck _dialogHack = null!;
    private VersionCheck _version = null!;
    private SandboxProtection _sandbox = null!;
    private RconProtection _rcon = null!;

    private TuningCrasherCheck _tuningCrasher = null!;
    private TuningHackCheck _tuningHack = null!;
    private InvalidSeatCrasherCheck _seatCrasher = null!;
    private DialogCrasherCheck _dialogCrasher = null!;
    private AttachedObjectCrasherCheck _attachCrasher = null!;
    private WeaponCrasherCheck _weaponCrasher = null!;

    private ConnectionFloodCheck _connFlood = null!;
    private CallbackFloodCheck _cbFlood = null!;
    private SeatFloodCheck _seatFlood = null!;
    private DosCheck _dos = null!;

    public PlayerStateManager Players => _players;
    public VehicleStateManager Vehicles => _vehicles;
    public WarningManager Warnings => _warnings;
    public AnticheatConfig Config => _config;
    public MoneyCheck Money => _money;
    public WeaponCheck Weapon => _weapon;
    public AmmoCheck Ammo => _ammo;
    public DialogHackCheck Dialog => _dialogHack;
    public CallbackFloodCheck CbFlood => _cbFlood;
    public AttachedObjectCrasherCheck AttachCrasher => _attachCrasher;
    public SpecialActionCheck SpecialAction => _specialAction;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AnticheatPlugin(
        PlayerStateManager players, VehicleStateManager vehicles,
        WarningManager warnings, FloodRateLimiter flood,
        AcLogger logger, AnticheatConfig config)
    {
        _players = players;
        _vehicles = vehicles;
        _warnings = warnings;
        _flood = flood;
        _logger = logger;
        _config = config;
    }

    private void InitChecks()
    {
        _airBreak = new AirBreakCheck(_players, _warnings, _config);
        _teleport = new TeleportCheck(_players, _warnings, _config);
        _speedHack = new SpeedHackCheck(_players, _warnings, _config);
        _flyHack = new FlyHackCheck(_players, _warnings, _config);
        _health = new HealthCheck(_players, _warnings, _config);
        _armour = new ArmourCheck(_players, _warnings, _config);
        _money = new MoneyCheck(_players, _warnings, _config);
        _weapon = new WeaponCheck(_players, _warnings, _config);
        _ammo = new AmmoCheck(_players, _warnings, _config);
        _godMode = new GodModeCheck(_players, _warnings, _config);
        _specialAction = new SpecialActionCheck(_players, _warnings, _config);
        _invisible = new InvisibleCheck(_players, _warnings, _config);
        _fakeSpawn = new FakeSpawnCheck(_players, _warnings, _config);
        _fakeKill = new FakeKillCheck(_players, _warnings, _config);
        _rapidFire = new RapidFireCheck(_warnings, _config);
        _proAim = new ProAimCheck(_players, _warnings, _config);
        _quickTurn = new QuickTurnCheck(_players, _warnings, _config);
        _lagComp = new LagCompSpoofCheck(_players, _warnings, _config);
        _carShot = new CarShotCheck(_players, _warnings, _config);
        _fullAiming = new FullAimingCheck(_players, _warnings, _config);
        _cjRun = new CjRunCheck(_players, _warnings, _config);
        _afkGhost = new AfkGhostCheck(_players, _warnings, _config);
        _carJack = new CarJackCheck(_players, _warnings, _config);
        _reconnect = new ReconnectCheck(_warnings, _config, _logger);
        _ping = new PingCheck(_warnings, _config, _logger);
        _dialogHack = new DialogHackCheck(_players, _warnings, _config);
        _version = new VersionCheck(_config, _logger);
        _sandbox = new SandboxProtection(_config, _logger);
        _rcon = new RconProtection(_warnings, _config, _logger);
        _tuningCrasher = new TuningCrasherCheck(_warnings, _config);
        _tuningHack = new TuningHackCheck(_players, _vehicles, _warnings, _config);
        _seatCrasher = new InvalidSeatCrasherCheck(_warnings, _config);
        _dialogCrasher = new DialogCrasherCheck(_players, _warnings, _config);
        _attachCrasher = new AttachedObjectCrasherCheck(_warnings, _config);
        _weaponCrasher = new WeaponCrasherCheck(_players, _warnings, _config);
        _connFlood = new ConnectionFloodCheck(_config, _logger);
        _cbFlood = new CallbackFloodCheck(_flood, _warnings, _config);
        _seatFlood = new SeatFloodCheck(_warnings, _config);
        _dos = new DosCheck(_config, _logger);
    }

    public static AnticheatPlugin Create(string configPath = "anticheat.json")
    {
        var cfg = LoadConfig(configPath);
        var players = new PlayerStateManager();
        var vehicles = new VehicleStateManager();
        var logger = new AcLogger(cfg);
        var flood = new FloodRateLimiter();
        var warnings = new WarningManager(players, cfg, logger);
        return new AnticheatPlugin(players, vehicles, warnings, flood, logger, cfg);
    }

    public static AnticheatConfig LoadConfig(string path = "anticheat.json")
    {
        if (!File.Exists(path))
        {
            var def = new AnticheatConfig();
            File.WriteAllText(path, JsonSerializer.Serialize(def, _jsonOptions));
            return def;
        }
        return JsonSerializer.Deserialize<AnticheatConfig>(File.ReadAllText(path), _jsonOptions)
               ?? new AnticheatConfig();
    }

    public void RegisterEvents(BaseMode gameMode)
    {
        InitChecks();

        gameMode.PlayerConnected += OnPlayerConnected;
        gameMode.PlayerDisconnected += OnPlayerDisconnected;
        gameMode.PlayerUpdate += OnPlayerUpdate;
        gameMode.PlayerSpawned += OnPlayerSpawned;
        gameMode.PlayerDied += OnPlayerDied;
        gameMode.PlayerTakeDamage += OnPlayerTakeDamage;
        gameMode.PlayerWeaponShot += OnPlayerWeaponShot;
        gameMode.PlayerEnterVehicle += OnPlayerEnterVehicle;
        gameMode.PlayerExitVehicle += OnPlayerExitVehicle;
        gameMode.PlayerStateChanged += OnPlayerStateChanged;
        gameMode.PlayerText += OnPlayerText;
        gameMode.VehicleMod += OnVehicleMod;
        gameMode.PlayerEnterExitModShop += OnPlayerEnterExitModShop;
        gameMode.DialogResponse += OnDialogResponse;
        gameMode.RconLoginAttempt += OnRconLoginAttempt;
        gameMode.PlayerPickUpPickup += OnPlayerPickUpPickup;
        gameMode.PlayerRequestClass += OnPlayerRequestClass;
        gameMode.VehiclePaintjobApplied += OnVehiclePaintjob;
        gameMode.VehicleResprayed += OnVehicleRespray;
        gameMode.VehicleDied += OnVehicleDied;
        gameMode.VehicleDamageStatusUpdated += OnVehicleDamageStatusUpdated;

        _warnings.PunishmentRequired += OnPunishment;

        var timer = new Timer(5000);
        timer.Elapsed += (_, _) => { _afkGhost.Tick(); _ping.Tick(); };
        timer.AutoReset = true;
        timer.Start();
    }

    private void OnPlayerConnected(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer player) return;

        if (!_connFlood.OnPlayerConnected(player)) return;
        _sandbox.OnPlayerConnected(player);
        _version.OnPlayerConnected(player);
        _reconnect.OnPlayerConnected(player);

        var st = _players.GetOrCreate(player.Id);
        st.IsOnline = true;
        st.IpAddress = player.IP;
        _logger.Log($"Player {player.Id} connected from {player.IP}");
    }

    private void OnPlayerDisconnected(object? sender, DisconnectEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        _connFlood.OnPlayerDisconnected(player.IP);
        _reconnect.OnPlayerDisconnected(player);
        _sandbox.OnPlayerDisconnected(player);
        _rcon.OnPlayerDisconnected(player.Id);
        _rapidFire.OnPlayerDisconnected(player.Id);
        _ping.OnPlayerDisconnected(player.Id);
        _seatFlood.OnPlayerDisconnected(player.Id);
        _dos.OnPlayerDisconnected(player.Id);
        _flood.ClearPlayer(player.Id);
        _players.Remove(player.Id);
    }

    private void OnPlayerUpdate(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer player) return;
        if (!_config.Enabled) return;

        if (!_dos.OnPlayerUpdate(player)) return;

        _airBreak.OnPlayerUpdate(player);
        _teleport.OnPlayerUpdate(player);
        _speedHack.OnPlayerUpdate(player);
        _flyHack.OnPlayerUpdate(player);
        _health.OnPlayerUpdate(player);
        _armour.OnPlayerUpdate(player);
        _money.OnPlayerUpdate(player);
        _weapon.OnPlayerUpdate(player);
        _ammo.OnPlayerUpdate(player);
        _godMode.OnPlayerUpdate(player);
        _quickTurn.OnPlayerUpdate(player);
        _fullAiming.OnPlayerUpdate(player);
        _cjRun.OnPlayerUpdate(player);
        _afkGhost.OnPlayerUpdate(player);
        _weaponCrasher.OnPlayerUpdate(player);
        _specialAction.OnPlayerUpdate(player);
        _invisible.OnPlayerUpdate(player);
    }

    private void OnPlayerSpawned(object? sender, SpawnEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        if (!_cbFlood.Check(player, 19)) return;
        _fakeSpawn.OnPlayerSpawned(player);
        _health.OnPlayerSpawned(player);
        _armour.OnPlayerSpawned(player);
        _money.OnPlayerSpawned(player);
        _weapon.OnPlayerSpawned(player);
    }

    private void OnPlayerDied(object? sender, DeathEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        _fakeKill.OnPlayerDied(player, e);
    }

    private void OnPlayerTakeDamage(object? sender, DamageEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        _godMode.OnPlayerTakeDamage(player, e);
        _lagComp.OnPlayerTakeDamage(player, e);
    }

    private void OnPlayerWeaponShot(object? sender, WeaponShotEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        _rapidFire.OnPlayerWeaponShot(player, e);
        _proAim.OnPlayerWeaponShot(player, e);
        _carShot.OnPlayerWeaponShot(player, e);
    }

    private void OnPlayerEnterVehicle(object? sender, EnterVehicleEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        if (!_seatFlood.OnPlayerEnterVehicle(player)) return;
        if (!_cbFlood.Check(player, 6)) return;
        if (!_carJack.OnPlayerEnterVehicle(player, e)) return;
        _seatCrasher.OnPlayerEnterVehicle(player, e);

        var st = _players.Get(player.Id);
        if (st is not null) st.EnterVehicleTick = Environment.TickCount64;
    }

    private void OnPlayerExitVehicle(object? sender, PlayerVehicleEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        if (!_cbFlood.Check(player, 7)) return;
        var st = _players.Get(player.Id);
        if (st is null) return;
        st.RemoveFromVehicleTick = Environment.TickCount64;
        st.VehicleId = -1;
    }

    private void OnPlayerStateChanged(object? sender, StateEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        if (!_cbFlood.Check(player, 11)) return;
        var st = _players.Get(player.Id);
        if (st is null) return;

        if (e.NewState == PlayerState.OnFoot && e.OldState == PlayerState.Driving)
            st.RemoveFromVehicleTick = Environment.TickCount64;

        if (e.NewState == PlayerState.Driving)
            st.EnterVehicleTick = Environment.TickCount64;
    }

    private void OnPlayerText(object? sender, TextEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        if (!_cbFlood.Check(player, 16)) e.SendToPlayers = false;
    }

    private void OnVehicleMod(object? sender, VehicleModEventArgs e)
    {
        if (sender is not BaseVehicle vehicle) return;
        if (e.Player is not BasePlayer player) return;
        if (!_cbFlood.Check(player, 12)) return;
        bool valid = _tuningCrasher.OnVehicleMod(vehicle, player, e.ComponentId);
        if (valid) _tuningHack.OnVehicleMod(vehicle, player, e.ComponentId);
    }

    private void OnPlayerEnterExitModShop(object? sender, EnterModShopEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        if (!_cbFlood.Check(player, 1)) return;
        var st = _players.Get(player.Id);
        if (st is null) return;
        st.IsInModShop = e.EnterExit == EnterExit.Entered;
    }

    private void OnDialogResponse(object? sender, DialogResponseEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        if (!_cbFlood.Check(player, 0)) return;
        bool valid = _dialogCrasher.OnDialogResponse(player, e);
        if (valid) _dialogHack.OnDialogResponse(player, e);
    }

    private void OnRconLoginAttempt(object? sender, RconLoginAttemptEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        _rcon.OnRconLoginAttempt(player, e.Password, e.SuccessfulLogin);
    }

    private void OnPlayerPickUpPickup(object? sender, PickUpPickupEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        _cbFlood.Check(player, 8);
    }

    private void OnPlayerRequestClass(object? sender, RequestClassEventArgs e)
    {
        if (sender is not BasePlayer player) return;
        _cbFlood.Check(player, 9);
    }

    private void OnVehiclePaintjob(object? sender, VehiclePaintjobEventArgs e)
    {
        if (sender is not BaseVehicle vehicle) return;
        if (e.Player is not BasePlayer player) return;
        if (!_cbFlood.Check(player, 13)) return;
        var vst = _vehicles.GetOrCreate(vehicle.Id);
        vst.PaintJob = e.PaintjobId;
    }

    private void OnVehicleRespray(object? sender, VehicleResprayedEventArgs e)
    {
        if (sender is not BaseVehicle vehicle) return;
        if (e.Player is not BasePlayer player) return;
        _cbFlood.Check(player, 14);
    }

    private void OnVehicleDied(object? sender, PlayerEventArgs e)
    {
        if (sender is not BaseVehicle vehicle) return;
        if (e.Player is not BasePlayer player) return;
        _cbFlood.Check(player, 15);
    }

    private void OnVehicleDamageStatusUpdated(object? sender, PlayerEventArgs e)
    {
        if (sender is not BaseVehicle vehicle) return;
        if (e.Player is not BasePlayer player) return;
        _cbFlood.Check(player, 24);
    }

    private void OnPunishment(int playerId, string checkName, PunishAction action)
    {
        var player = BasePlayer.Find(playerId);
        if (player is null) return;
        switch (action)
        {
            case PunishAction.Kick: _logger.LogKick(playerId, checkName); player.Kick(); break;
            case PunishAction.Ban: _logger.LogBan(playerId, checkName); player.Ban(); break;
        }
    }
}