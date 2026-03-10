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
    private VehicleTeleportCheck _vehicleTeleport = null!;
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
        _vehicleTeleport = new VehicleTeleportCheck(_players, _warnings, _config);
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

    public void RegisterEvents(BaseMode gm)
    {
        InitChecks();

        gm.PlayerConnected += OnPlayerConnected;
        gm.PlayerDisconnected += OnPlayerDisconnected;
        gm.PlayerUpdate += OnPlayerUpdate;
        gm.PlayerSpawned += OnPlayerSpawned;
        gm.PlayerDied += OnPlayerDied;
        gm.PlayerTakeDamage += OnPlayerTakeDamage;
        gm.PlayerWeaponShot += OnPlayerWeaponShot;
        gm.PlayerEnterVehicle += OnPlayerEnterVehicle;
        gm.PlayerExitVehicle += OnPlayerExitVehicle;
        gm.PlayerStateChanged += OnPlayerStateChanged;
        gm.PlayerText += OnPlayerText;
        gm.PlayerCommandText += OnPlayerCommandText;
        gm.PlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
        gm.PlayerLeaveCheckpoint += OnPlayerLeaveCheckpoint;
        gm.PlayerEnterRaceCheckpoint += OnPlayerEnterRaceCheckpoint;
        gm.PlayerLeaveRaceCheckpoint += OnPlayerLeaveRaceCheckpoint;
        gm.PlayerRequestClass += OnPlayerRequestClass;
        gm.PlayerRequestSpawn += OnPlayerRequestSpawn;
        gm.PlayerPickUpPickup += OnPlayerPickUpPickup;
        gm.PlayerSelectedMenuRow += OnPlayerSelectedMenuRow;
        gm.PlayerExitedMenu += OnPlayerExitedMenu;
        gm.PlayerClickMap += OnPlayerClickMap;
        gm.PlayerClickPlayer += OnPlayerClickPlayer;
        gm.PlayerClickTextDraw += OnPlayerClickTextDraw;
        gm.PlayerClickPlayerTextDraw += OnPlayerClickPlayerTextDraw;
        gm.PlayerSelectObject += OnPlayerSelectObject;
        gm.VehicleMod += OnVehicleMod;
        gm.PlayerEnterExitModShop += OnPlayerEnterExitModShop;
        gm.VehiclePaintjobApplied += OnVehiclePaintjob;
        gm.VehicleResprayed += OnVehicleRespray;
        gm.VehicleDied += OnVehicleDied;
        gm.VehicleDamageStatusUpdated += OnVehicleDamageStatusUpdated;
        gm.VehicleSirenStateChanged += OnVehicleSirenStateChanged;
        gm.DialogResponse += OnDialogResponse;
        gm.RconLoginAttempt += OnRconLoginAttempt;

        _warnings.PunishmentRequired += OnPunishment;

        var timer = new Timer(5000);
        timer.Elapsed += (_, _) => { _afkGhost.Tick(); _ping.Tick(); };
        timer.AutoReset = true;
        timer.Start();
    }

    private void OnPlayerConnected(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_connFlood.OnPlayerConnected(p)) return;
        _sandbox.OnPlayerConnected(p);
        _version.OnPlayerConnected(p);
        _reconnect.OnPlayerConnected(p);
        var st = _players.GetOrCreate(p.Id);
        st.IsOnline = true;
        st.IpAddress = p.IP;
        _logger.Log($"Player {p.Id} connected from {p.IP}");
    }

    private void OnPlayerDisconnected(object? sender, DisconnectEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _connFlood.OnPlayerDisconnected(p.IP);
        _reconnect.OnPlayerDisconnected(p);
        _sandbox.OnPlayerDisconnected(p);
        _rcon.OnPlayerDisconnected(p.Id);
        _rapidFire.OnPlayerDisconnected(p.Id);
        _ping.OnPlayerDisconnected(p.Id);
        _seatFlood.OnPlayerDisconnected(p.Id);
        _dos.OnPlayerDisconnected(p.Id);
        _flood.ClearPlayer(p.Id);
        _players.Remove(p.Id);
    }

    private void OnPlayerUpdate(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_config.Enabled) return;
        if (!_dos.OnPlayerUpdate(p)) return;
        _airBreak.OnPlayerUpdate(p);
        _teleport.OnPlayerUpdate(p);
        _speedHack.OnPlayerUpdate(p);
        _flyHack.OnPlayerUpdate(p);
        _health.OnPlayerUpdate(p);
        _armour.OnPlayerUpdate(p);
        _money.OnPlayerUpdate(p);
        _weapon.OnPlayerUpdate(p);
        _ammo.OnPlayerUpdate(p);
        _godMode.OnPlayerUpdate(p);
        _quickTurn.OnPlayerUpdate(p);
        _fullAiming.OnPlayerUpdate(p);
        _cjRun.OnPlayerUpdate(p);
        _afkGhost.OnPlayerUpdate(p);
        _weaponCrasher.OnPlayerUpdate(p);
        _specialAction.OnPlayerUpdate(p);
        _invisible.OnPlayerUpdate(p);
    }

    private void OnPlayerSpawned(object? sender, SpawnEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 19)) return;
        _fakeSpawn.OnPlayerSpawned(p);
        _health.OnPlayerSpawned(p);
        _armour.OnPlayerSpawned(p);
        _money.OnPlayerSpawned(p);
        _weapon.OnPlayerSpawned(p);
    }

    private void OnPlayerDied(object? sender, DeathEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _fakeKill.OnPlayerDied(p, e);
    }

    private void OnPlayerTakeDamage(object? sender, DamageEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _godMode.OnPlayerTakeDamage(p, e);
        _lagComp.OnPlayerTakeDamage(p, e);
    }

    private void OnPlayerWeaponShot(object? sender, WeaponShotEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        var st = _players.Get(p.Id);
        if (st is not null) st.ShotTick = Environment.TickCount64;
        _rapidFire.OnPlayerWeaponShot(p, e);
        _proAim.OnPlayerWeaponShot(p, e);
        _carShot.OnPlayerWeaponShot(p, e);
    }

    private void OnPlayerEnterVehicle(object? sender, EnterVehicleEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_seatFlood.OnPlayerEnterVehicle(p)) return;
        if (!_cbFlood.Check(p, 6)) return;
        _vehicleTeleport.OnPlayerEnterVehicle(p, e);
        if (!_carJack.OnPlayerEnterVehicle(p, e)) return;
        _seatCrasher.OnPlayerEnterVehicle(p, e);
        var st = _players.Get(p.Id);
        if (st is not null) st.EnterVehicleTick = Environment.TickCount64;
    }

    private void OnPlayerExitVehicle(object? sender, PlayerVehicleEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 7)) return;
        var st = _players.Get(p.Id);
        if (st is null) return;
        st.RemoveFromVehicleTick = Environment.TickCount64;
        st.VehicleId = -1;
    }

    private void OnPlayerStateChanged(object? sender, StateEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 11)) return;
        _vehicleTeleport.OnPlayerStateChanged(p, e);
        var st = _players.Get(p.Id);
        if (st is null) return;
        if (e.NewState == PlayerState.OnFoot && e.OldState == PlayerState.Driving)
            st.RemoveFromVehicleTick = Environment.TickCount64;
        if (e.NewState == PlayerState.Driving)
            st.EnterVehicleTick = Environment.TickCount64;
    }

    private void OnPlayerText(object? sender, TextEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 16)) e.SendToPlayers = false;
    }

    private void OnPlayerCommandText(object? sender, CommandTextEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 5);
    }

    private void OnPlayerEnterCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 17);
    }

    private void OnPlayerLeaveCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 18);
    }

    private void OnPlayerEnterRaceCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 21);
    }

    private void OnPlayerLeaveRaceCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 22);
    }

    private void OnPlayerRequestSpawn(object? sender, RequestSpawnEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 19);
    }

    private void OnPlayerPickUpPickup(object? sender, PickUpPickupEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 8)) return;
        _vehicleTeleport.OnPlayerPickUpPickup(p, e);
    }

    private void OnPlayerRequestClass(object? sender, RequestClassEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 9);
    }

    private void OnPlayerSelectedMenuRow(object? sender, MenuRowEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 10);
    }

    private void OnPlayerExitedMenu(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 20);
    }

    private void OnPlayerClickMap(object? sender, PositionEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 2);
    }

    private void OnPlayerClickPlayer(object? sender, ClickPlayerEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 3);
    }

    private void OnPlayerClickTextDraw(object? sender, ClickTextDrawEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 4);
    }

    private void OnPlayerClickPlayerTextDraw(object? sender, ClickPlayerTextDrawEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 23);
    }

    private void OnPlayerSelectObject(object? sender, SelectObjectEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 26);
    }

    private void OnVehicleMod(object? sender, VehicleModEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 12)) return;
        bool valid = _tuningCrasher.OnVehicleMod(v, p, e.ComponentId);
        if (valid) _tuningHack.OnVehicleMod(v, p, e.ComponentId);
    }

    private void OnPlayerEnterExitModShop(object? sender, EnterModShopEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 1)) return;
        var st = _players.Get(p.Id);
        if (st is null) return;
        st.IsInModShop = e.EnterExit == EnterExit.Entered;
    }

    private void OnVehiclePaintjob(object? sender, VehiclePaintjobEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 13)) return;
        var vst = _vehicles.GetOrCreate(v.Id);
        vst.PaintJob = e.PaintjobId;
    }

    private void OnVehicleRespray(object? sender, VehicleResprayedEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 14);
    }

    private void OnVehicleDied(object? sender, PlayerEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 15);
    }

    private void OnVehicleDamageStatusUpdated(object? sender, PlayerEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 24);
    }

    private void OnVehicleSirenStateChanged(object? sender, SirenStateEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 25);
    }

    private void OnDialogResponse(object? sender, DialogResponseEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 0)) return;
        bool valid = _dialogCrasher.OnDialogResponse(p, e);
        if (valid) _dialogHack.OnDialogResponse(p, e);
    }

    private void OnRconLoginAttempt(object? sender, RconLoginAttemptEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _rcon.OnRconLoginAttempt(p, e.Password, e.SuccessfulLogin);
    }

    private void OnPunishment(int playerId, string checkName, PunishAction action)
    {
        var p = BasePlayer.Find(playerId);
        if (p is null) return;
        switch (action)
        {
            case PunishAction.Kick: _logger.LogKick(playerId, checkName); p.Kick(); break;
            case PunishAction.Ban: _logger.LogBan(playerId, checkName); p.Ban(); break;
        }
    }
}