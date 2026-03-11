#nullable enable
using ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;
using ProjectSMP.Plugins.Anticheat.Checks.AntiFlood;
using ProjectSMP.Plugins.Anticheat.Checks.AntiNop;
using ProjectSMP.Plugins.Anticheat.Checks.Combat;
using ProjectSMP.Plugins.Anticheat.Checks.Movement;
using ProjectSMP.Plugins.Anticheat.Checks.Player;
using ProjectSMP.Plugins.Anticheat.Checks.Server;
using ProjectSMP.Plugins.Anticheat.Checks.Spawn;
using ProjectSMP.Plugins.Anticheat.Checks.Vehicle;
using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Events;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Statistics;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Timers;

namespace ProjectSMP.Plugins.Anticheat;

public class AnticheatPlugin : IDisposable
{
    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly PickupStateManager _pickups;
    private readonly WarningManager _warnings;
    private readonly FloodRateLimiter _flood;
    private readonly AcLogger _logger;
    private readonly AnticheatConfig _config;
    private readonly AnticheatEvents _events;
    private FileSystemWatcher? _watcher;
    private Timer? _timer;

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
    private SilentAimCheck _silentAim = null!;
    private QuickTurnCheck _quickTurn = null!;
    private LagCompSpoofCheck _lagComp = null!;
    private CarShotCheck _carShot = null!;
    private FullAimingCheck _fullAiming = null!;
    private CjRunCheck _cjRun = null!;
    private AfkGhostCheck _afkGhost = null!;
    private CarJackCheck _carJack = null!;
    private VehicleTeleportCheck _vehicleTeleport = null!;
    private TuningHackCheck _tuningHack = null!;
    private ReconnectCheck _reconnect = null!;
    private PingCheck _ping = null!;
    private DialogHackCheck _dialogHack = null!;
    private VersionCheck _version = null!;
    private SandboxProtection _sandbox = null!;
    private RconProtection _rcon = null!;
    private TuningCrasherCheck _tuningCrasher = null!;
    private InvalidSeatCrasherCheck _seatCrasher = null!;
    private DialogCrasherCheck _dialogCrasher = null!;
    private AttachedObjectCrasherCheck _attachCrasher = null!;
    private WeaponCrasherCheck _weaponCrasher = null!;
    private ConnectionFloodCheck _connFlood = null!;
    private CallbackFloodCheck _cbFlood = null!;
    private SeatFloodCheck _seatFlood = null!;
    private DosCheck _dos = null!;
    private ParkourModCheck _parkourMod = null!;
    private UnFreezeCheck _unFreeze = null!;
    private FakeNpcCheck _fakeNpc = null!;
    private JetpackCheck _jetpack = null!;
    private AnimationHackCheck _animationHack = null!;
    private NitroHackCheck _nitroHack = null!;
    private VehicleModHackCheck _vehicleModHack = null!;
    private VehicleHealthCheck _vehicleHealth = null!;
    private PaintJobCheck _paintJob = null!;
    private InteriorWeaponCheck _interiorWeapon = null!;
    private CheckpointTeleportCheck _checkpointTeleport = null!;
    private FakePickupCheck _fakePickup = null!;
    private MacroDetectionCheck _macroDetection = null!;
    private DriveOnWaterCheck _driveOnWater = null!;
    private WallClipCheck _wallClip = null!;
    private VehicleFlipCheck _vehicleFlip = null!;
    private InfiniteRunCheck _infiniteRun = null!;
    private VehicleSprintCheck _vehicleSprint = null!;
    private ClassSelectionCheck _classSelection = null!;
    private GravityHackCheck _gravityHack = null!;
    private CarwarpCheck _carwarp = null!;
    private BlacklistCheck _blacklist = null!;
    private CodeVerificationCheck _codeVerification = null!;
    private readonly AntiCheatStats _stats;
    private WeaponDamageCheck _weaponDamage = null!;
    private WeaponSwitchCheck _weaponSwitch = null!;
    private ObjectCrasherCheck _objectCrasher = null!;
    private TextDrawCrasherCheck _textDrawCrasher = null!;
    private Text3DCrasherCheck _text3DCrasher = null!;
    private MenuCrasherCheck _menuCrasher = null!;

    // ── Anti-NOP ─────────────────────────────────────────────────────────
    private NopGiveWeaponCheck _nopGiveWeapon = null!;
    private NopSetAmmoCheck _nopSetAmmo = null!;
    private NopSetInteriorCheck _nopSetInterior = null!;
    private NopSetHealthCheck _nopSetHealth = null!;
    private NopSetVehicleHealthCheck _nopSetVehicleHealth = null!;
    private NopSetArmourCheck _nopSetArmour = null!;
    private NopSetSpecialActionCheck _nopSetSpecialAction = null!;
    private NopPutPlayerInVehicleCheck _nopPutInVehicle = null!;
    private NopToggleSpectatingCheck _nopToggleSpectating = null!;
    private NopSpawnPlayerCheck _nopSpawnPlayer = null!;
    private NopSetPlayerPosCheck _nopSetPos = null!;
    private NopRemoveFromVehicleCheck _nopRemoveFromVehicle = null!;

    public PlayerStateManager Players => _players;
    public WarningManager Warnings => _warnings;
    public AnticheatConfig Config => _config;
    public AnticheatEvents Events => _events;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AnticheatPlugin(PlayerStateManager players, VehicleStateManager vehicles, PickupStateManager pickups, WarningManager warnings, FloodRateLimiter flood, AcLogger logger, AnticheatConfig config) {
        _players = players;
        _vehicles = vehicles;
        _pickups = pickups;
        _warnings = warnings;
        _flood = flood;
        _logger = logger;
        _config = config;
        _events = new AnticheatEvents();
        _stats = new AntiCheatStats(); // ADD THIS
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
        _silentAim = new SilentAimCheck(_players, _warnings, _config);
        _quickTurn = new QuickTurnCheck(_players, _warnings, _config);
        _lagComp = new LagCompSpoofCheck(_players, _warnings, _config);
        _carShot = new CarShotCheck(_players, _warnings, _config);
        _fullAiming = new FullAimingCheck(_players, _warnings, _config);
        _cjRun = new CjRunCheck(_players, _warnings, _config);
        _afkGhost = new AfkGhostCheck(_players, _warnings, _config);
        _carJack = new CarJackCheck(_players, _warnings, _config);
        _vehicleTeleport = new VehicleTeleportCheck(_players, _pickups, _warnings, _config);
        _tuningHack = new TuningHackCheck(_players, _warnings, _config);
        _reconnect = new ReconnectCheck(_warnings, _config, _logger);
        _ping = new PingCheck(_warnings, _config, _logger);
        _dialogHack = new DialogHackCheck(_players, _warnings, _config);
        _version = new VersionCheck(_config, _logger);
        _sandbox = new SandboxProtection(_config, _logger);
        _rcon = new RconProtection(_warnings, _config, _logger);
        _tuningCrasher = new TuningCrasherCheck(_warnings, _config);
        _seatCrasher = new InvalidSeatCrasherCheck(_warnings, _config);
        _dialogCrasher = new DialogCrasherCheck(_players, _warnings, _config);
        _attachCrasher = new AttachedObjectCrasherCheck(_warnings, _config);
        _weaponCrasher = new WeaponCrasherCheck(_players, _warnings, _config);
        _connFlood = new ConnectionFloodCheck(_config, _logger);
        _cbFlood = new CallbackFloodCheck(_flood, _warnings, _config);
        _seatFlood = new SeatFloodCheck(_warnings, _config);
        _dos = new DosCheck(_config, _logger);
        _parkourMod = new ParkourModCheck(_players, _warnings, _config);
        _unFreeze = new UnFreezeCheck(_players, _warnings, _config);
        _fakeNpc = new FakeNpcCheck(_config, _logger);
        _jetpack = new JetpackCheck(_players, _warnings, _config);
        _animationHack = new AnimationHackCheck(_players, _warnings, _config);
        _nitroHack = new NitroHackCheck(_players, _vehicles, _warnings, _config);
        _vehicleModHack = new VehicleModHackCheck(_players, _vehicles, _warnings, _config);
        _vehicleHealth = new VehicleHealthCheck(_players, _vehicles, _warnings, _config);
        _paintJob = new PaintJobCheck(_players, _vehicles, _warnings, _config);
        _interiorWeapon = new InteriorWeaponCheck(_players, _warnings, _config);
        _macroDetection = new MacroDetectionCheck(_players, _warnings, _config);
        _driveOnWater = new DriveOnWaterCheck(_players, _warnings, _config);
        _wallClip = new WallClipCheck(_players, _warnings, _config);
        _vehicleFlip = new VehicleFlipCheck(_players, _vehicles, _warnings, _config);
        _infiniteRun = new InfiniteRunCheck(_players, _warnings, _config);
        _vehicleSprint = new VehicleSprintCheck(_players, _warnings, _config);
        _classSelection = new ClassSelectionCheck(_players, _warnings, _config);
        _gravityHack = new GravityHackCheck(_players, _warnings, _config);
        _carwarp = new CarwarpCheck(_players, _vehicles, _warnings, _config);
        _blacklist = new BlacklistCheck(_players, _vehicles, _warnings, _config);
        _codeVerification = new CodeVerificationCheck(_config, _logger);
        _weaponDamage = new WeaponDamageCheck(_players, _warnings, _config);
        _weaponSwitch = new WeaponSwitchCheck(_players, _warnings, _config);
        _objectCrasher = new ObjectCrasherCheck(_players, _warnings, _config);
        _textDrawCrasher = new TextDrawCrasherCheck(_players, _warnings, _config);
        _text3DCrasher = new Text3DCrasherCheck(_players, _warnings, _config);
        _menuCrasher = new MenuCrasherCheck(_players, _warnings, _config);

        _nopGiveWeapon = new NopGiveWeaponCheck(_players, _warnings, _config);
        _nopSetAmmo = new NopSetAmmoCheck(_players, _warnings, _config);
        _nopSetInterior = new NopSetInteriorCheck(_players, _warnings, _config);
        _nopSetHealth = new NopSetHealthCheck(_players, _warnings, _config);
        _nopSetVehicleHealth = new NopSetVehicleHealthCheck(_players, _vehicles, _warnings, _config);
        _nopSetArmour = new NopSetArmourCheck(_players, _warnings, _config);
        _nopSetSpecialAction = new NopSetSpecialActionCheck(_players, _warnings, _config);
        _nopPutInVehicle = new NopPutPlayerInVehicleCheck(_players, _warnings, _config);
        _nopToggleSpectating = new NopToggleSpectatingCheck(_players, _warnings, _config);
        _nopSpawnPlayer = new NopSpawnPlayerCheck(_players, _warnings, _config);
        _nopSetPos = new NopSetPlayerPosCheck(_players, _warnings, _config);
        _nopRemoveFromVehicle = new NopRemoveFromVehicleCheck(_players, _warnings, _config);
        _checkpointTeleport = new CheckpointTeleportCheck(_players, _warnings, _config);
        _fakePickup = new FakePickupCheck(_players, _pickups, _warnings, _config);
    }

    private void InitHotReload(string configPath)
    {
        string dir = Path.GetDirectoryName(Path.GetFullPath(configPath)) ?? ".";
        string file = Path.GetFileName(configPath);
        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        _watcher.Changed += (_, _) =>
        {
            try
            {
                var fresh = LoadConfig(configPath);
                _config.Enabled = fresh.Enabled;
                _config.MaxPing = fresh.MaxPing;
                _config.MaxConnectsPerIp = fresh.MaxConnectsPerIp;
                _config.MinReconnectSeconds = fresh.MinReconnectSeconds;
                _config.SpeedHackVehResetDelay = fresh.SpeedHackVehResetDelay;
                foreach (var (k, v) in fresh.Checks) _config.Checks[k] = v;
                _logger.Log("anticheat.json reloaded.");
            }
            catch (Exception ex) { _logger.LogWarn($"Hot-reload failed: {ex.Message}"); }
        };
    }

    public static AnticheatPlugin Create(string configPath = "anticheat.json")
    {
        var cfg = LoadConfig(configPath);
        var players = new PlayerStateManager();
        var vehicles = new VehicleStateManager();
        var pickups = new PickupStateManager();
        var logger = new AcLogger(cfg);
        var flood = new FloodRateLimiter();
        var warnings = new WarningManager(players, cfg, logger);
        var plugin = new AnticheatPlugin(players, vehicles, pickups, warnings, flood, logger, cfg);
        plugin.InitHotReload(configPath);
        return plugin;
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

    // ── Game Script Hooks ────────────────────────────────────────────────

    public void OnGivePlayerMoney(int playerId, int amount)
        => _money.AllowMoneyGain(playerId, amount);

    public void OnTakePlayerMoney(int playerId, int amount)
        => _money.AllowMoneyLoss(playerId, amount);

    public void OnResetPlayerMoney(int playerId)
        => _money.OnResetPlayerMoney(playerId);

    public void OnGivePlayerWeapon(int playerId, int weaponId, int ammo)
    {
        _weapon.OnWeaponGiven(playerId, weaponId, ammo);
        _ammo.OnAmmoGiven(playerId, weaponId, ammo);
        _nopGiveWeapon.OnWeaponGiven(playerId, weaponId, ammo);
    }

    public void OnSetPlayerAmmo(int playerId, int weaponId, int ammo)
        => _nopSetAmmo.OnSetPlayerAmmo(playerId, weaponId, ammo);

    public void OnSetPlayerInterior(int playerId, int interiorId) {
        var p = BasePlayer.Find(playerId);
        if (p is null) return;

        int oldInterior = p.Interior;
        _nopSetInterior.OnSetPlayerInterior(playerId, interiorId);
        _interiorWeapon.OnPlayerInteriorChanged(playerId, interiorId, oldInterior);
    }

    public void OnResetPlayerWeapons(int playerId)
    {
        _weapon.OnWeaponsReset(playerId);
        _nopGiveWeapon.OnWeaponsReset(playerId);
        _nopSetAmmo.OnWeaponsReset(playerId);
    }

    public void OnShowPlayerDialog(int playerId, int dialogId)
        => _dialogHack.OnDialogShown(playerId, dialogId);

    public void OnSetPlayerSpecialAction(int playerId, int action) {
        _specialAction.OnSpecialActionSet(playerId, action);
        _nopSetSpecialAction.OnSetPlayerSpecialAction(playerId, action);
    }

    public void OnSetPlayerHealth(int playerId, float health)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SetHealth = (int)health;
        st.SetHealthTick = Environment.TickCount64;
        _nopSetHealth.OnSetPlayerHealth(playerId, health);
    }

    public void OnSetVehicleHealth(int vehicleId, float health)
        => _nopSetVehicleHealth.OnSetVehicleHealth(vehicleId, health);

    public void OnSetPlayerArmour(int playerId, float armour)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SetArmour = (int)armour;
        st.SetArmourTick = Environment.TickCount64;
        _nopSetArmour.OnSetPlayerArmour(playerId, armour);
    }

    public void OnSetPlayerPos(int playerId, float x, float y, float z) {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SetPosTick = Environment.TickCount64;
        _nopSetPos.OnSetPlayerPos(playerId, x, y, z);
    }

    public void OnRemovePlayerFromVehicle(int playerId)
        => _nopRemoveFromVehicle.OnRemovePlayerFromVehicle(playerId);

    public void OnPutPlayerInVehicle(int playerId, int vehicleId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.VehicleId = vehicleId;
        st.PutInVehicleTick = Environment.TickCount64;
        _nopPutInVehicle.OnPutPlayerInVehicle(playerId, vehicleId);
    }

    public void OnTogglePlayerSpectating(int playerId, bool toggle) {
        var st = _players.Get(playerId);
        if (st is not null) {
            st.SpectateTick = Environment.TickCount64;
            st.IsSpectating = toggle;
        }
        _nopToggleSpectating.OnTogglePlayerSpectating(playerId, toggle);
    }

    public void OnPlayerVelocitySet(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.PlayerVelocityTick = Environment.TickCount64;
    }

    public void OnVehicleVelocitySet(int vehicleId)
    {
        long now = Environment.TickCount64;
        foreach (var (_, st) in _players.All)
            if (st.VehicleId == vehicleId) st.VehicleVelocityTick = now;
    }

    public void OnSetAttachedObject(int playerId, int slot, int modelId)
    {
        var p = BasePlayer.Find(playerId);
        if (p is not null) _attachCrasher.ValidateAttachedObject(p, slot, modelId);
    }

    public void OnRegisterPickup(int pickupId, float x, float y, float z, int type = 0, int weapon = 0, int amount = 0) {
        _pickups.Register(pickupId, x, y, z, type, weapon, amount);
        _fakePickup.OnPickupCreated(pickupId, x, y, z, type, weapon, amount);
    }

    public void OnDestroyPickup(int pickupId) {
        _pickups.Remove(pickupId);
        _fakePickup.OnPickupDestroyed(pickupId);
    }

    public void OnSpawnPlayer(int playerId) {
        var st = _players.Get(playerId);
        if (st is not null) st.SpawnSetFlag = 1;
        _nopSpawnPlayer.OnSpawnPlayer(playerId);
    }

    public void OnTogglePlayerControllable(int playerId, bool toggle)
        => _unFreeze.OnPlayerFrozen(playerId, !toggle);

    public void OnSetPlayerSpawnInfo(int playerId, int weapon1, int ammo1, int weapon2, int ammo2, int weapon3, int ammo3)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SpawnWeapon1 = weapon1; st.SpawnAmmo1 = ammo1;
        st.SpawnWeapon2 = weapon2; st.SpawnAmmo2 = ammo2;
        st.SpawnWeapon3 = weapon3; st.SpawnAmmo3 = ammo3;
    }

    // ── RegisterEvents ───────────────────────────────────────────────────

    public void RegisterEvents(BaseMode gm)
    {
        InitChecks();
        _events.Wire(_warnings);

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
        gm.PlayerKeyStateChanged += OnPlayerKeyStateChange;
        gm.VehicleMod += OnVehicleMod;
        gm.PlayerEnterExitModShop += OnPlayerEnterExitModShop;
        gm.VehiclePaintjobApplied += OnVehiclePaintjob;
        gm.VehicleResprayed += OnVehicleRespray;
        gm.VehicleDied += OnVehicleDied;
        gm.VehicleDamageStatusUpdated += OnVehicleDamageStatusUpdated;
        gm.VehicleSirenStateChange += OnVehicleSirenStateChange;
        gm.DialogResponse += OnDialogResponse;
        gm.RconLoginAttempt += OnRconLoginAttempt;

        _warnings.CheatDetected += (s, e) => _stats.RecordDetection(e.PlayerId, e.CheckName, e.Details);
        _warnings.PunishmentRequired += (pid, check, action) =>
        {
            if (action == PunishAction.Kick) _stats.RecordKick(pid, check);
            else if (action == PunishAction.Ban) _stats.RecordBan(pid, check);
        };

        _timer = new Timer(5000);
        _timer.Elapsed += (_, _) => {
            _afkGhost.Tick();
            _ping.Tick();
            _codeVerification.Tick();
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    // ── Event Handlers ───────────────────────────────────────────────────

    private void OnPlayerConnected(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_connFlood.OnPlayerConnected(p)) return;
        _sandbox.OnPlayerConnected(p);
        if (!_fakeNpc.OnPlayerConnected(p)) return;
        _version.OnPlayerConnected(p);
        _reconnect.OnPlayerConnected(p);
        _classSelection.OnPlayerConnected(p.Id);
        _stats.RecordPlayerChecked(p.Id);
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
        _silentAim.OnPlayerDisconnected(p.Id);
        _ping.OnPlayerDisconnected(p.Id);
        _seatFlood.OnPlayerDisconnected(p.Id);
        _dos.OnPlayerDisconnected(p.Id);
        _flood.ClearPlayer(p.Id);
        _nopSpawnPlayer.OnPlayerDisconnected(p.Id);
        _nopRemoveFromVehicle.OnPlayerDisconnected(p.Id);
        _macroDetection.OnPlayerDisconnected(p.Id);
        _objectCrasher.OnPlayerDisconnected(p.Id);
        _text3DCrasher.OnPlayerDisconnected(p.Id);
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
        _parkourMod.OnPlayerUpdate(p);
        _unFreeze.OnPlayerUpdate(p);
        _jetpack.OnPlayerUpdate(p);
        _animationHack.OnPlayerUpdate(p);
        _nitroHack.OnPlayerUpdate(p);
        _interiorWeapon.OnPlayerUpdate(p);
        _vehicleHealth.OnPlayerUpdate(p);
        _driveOnWater.OnPlayerUpdate(p);
        _wallClip.OnPlayerUpdate(p);
        _vehicleFlip.OnPlayerUpdate(p);
        _infiniteRun.OnPlayerUpdate(p);
        _vehicleSprint.OnPlayerUpdate(p);
        _gravityHack.OnPlayerUpdate(p);
        _blacklist.OnPlayerUpdate(p);
        _weaponSwitch.OnPlayerUpdate(p);

        _nopGiveWeapon.OnPlayerUpdate(p);
        _nopSetAmmo.OnPlayerUpdate(p);
        _nopSetInterior.OnPlayerUpdate(p);
        _nopSetHealth.OnPlayerUpdate(p);
        _nopSetVehicleHealth.OnPlayerUpdate(p);
        _nopSetArmour.OnPlayerUpdate(p);
        _nopSetSpecialAction.OnPlayerUpdate(p);
        _nopPutInVehicle.OnPlayerUpdate(p);
        _nopToggleSpectating.OnPlayerUpdate(p);
        _nopSpawnPlayer.OnPlayerUpdate(p);
        _nopSetPos.OnPlayerUpdate(p);
        _nopRemoveFromVehicle.OnPlayerUpdate(p);
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
        _jetpack.OnPlayerSpawned(p.Id);
        _animationHack.OnPlayerSpawned(p.Id);
        _wallClip.OnPlayerSpawned(p.Id);
        _infiniteRun.OnPlayerSpawned(p.Id);
        _vehicleSprint.OnPlayerSpawned(p.Id);
        _silentAim.OnPlayerSpawned(p.Id);
        _classSelection.OnPlayerSpawned(p.Id);
        _gravityHack.OnPlayerSpawned(p.Id);

        _nopGiveWeapon.OnPlayerSpawned(p.Id);
        _nopSetAmmo.OnPlayerSpawned(p.Id);
        _nopSetInterior.OnPlayerSpawned(p.Id);
        _nopSetHealth.OnPlayerSpawned(p.Id);
        _nopSetArmour.OnPlayerSpawned(p.Id);
        _nopSetSpecialAction.OnPlayerSpawned(p.Id);
        _nopPutInVehicle.OnPlayerSpawned(p.Id);
        _nopToggleSpectating.OnPlayerSpawned(p.Id);
        _nopSpawnPlayer.OnPlayerSpawned(p.Id);
        _nopSetPos.OnPlayerSpawned(p.Id);
        _nopRemoveFromVehicle.OnPlayerSpawned(p.Id);
    }

    private void OnPlayerDied(object? sender, DeathEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _godMode.OnPlayerDied(p);
        _fakeKill.OnPlayerDied(p, e);
        _jetpack.OnPlayerDied(p.Id);
        _animationHack.OnPlayerDied(p.Id);
        _infiniteRun.OnPlayerDied(p.Id);
        _vehicleSprint.OnPlayerDied(p.Id);
        _silentAim.OnPlayerDied(p.Id);
        _classSelection.OnPlayerDied(p.Id);
        _gravityHack.OnPlayerDied(p.Id);

        _nopSetHealth.OnPlayerDied(p.Id);
        _nopSetArmour.OnPlayerDied(p.Id);
        _nopSetSpecialAction.OnPlayerDied(p.Id);
        _nopPutInVehicle.OnPlayerDied(p.Id);
        _nopToggleSpectating.OnPlayerDied(p.Id);
        _nopSetPos.OnPlayerDied(p.Id);
        _nopRemoveFromVehicle.OnPlayerDied(p.Id);
    }

    private void OnPlayerTakeDamage(object? sender, DamageEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _godMode.OnPlayerTakeDamage(p, e);
        _lagComp.OnPlayerTakeDamage(p, e);
        _weaponDamage.OnPlayerTakeDamage(p, e);
    }

    private void OnPlayerWeaponShot(object? sender, WeaponShotEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _macroDetection.OnPlayerWeaponShot(p, e);
        _ammo.OnPlayerWeaponShot(p, (int)e.Weapon);
        _rapidFire.OnPlayerWeaponShot(p, e);
        _proAim.OnPlayerWeaponShot(p, e);
        _silentAim.OnPlayerWeaponShot(p, e);
        _carShot.OnPlayerWeaponShot(p, e);
        _interiorWeapon.OnPlayerWeaponShot(p, e);
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
        if (st is null) return;
        st.VehicleId = e.Vehicle.Id;
        st.EnterVehicleTick = Environment.TickCount64;
        _nopSetPos.OnPlayerEnterVehicle(p.Id);
        _blacklist.OnPlayerEnterVehicle(p, e.Vehicle.Id);
        _nitroHack.OnPlayerEnterVehicle(p.Id, e.Vehicle.Id);
        _driveOnWater.OnPlayerEnterVehicle(p.Id);
        _vehicleHealth.OnPlayerEnterVehicle(p.Id, e.Vehicle.Id);
        _vehicleFlip.OnPlayerEnterVehicle(p.Id, e.Vehicle.Id);
        _vehicleSprint.OnPlayerEnterVehicle(p.Id);
        _infiniteRun.OnPlayerEnterVehicle(p.Id);
        _gravityHack.OnPlayerEnterVehicle(p.Id);
    }

    private void OnPlayerExitVehicle(object? sender, PlayerVehicleEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 7)) return;
        var st = _players.Get(p.Id);
        if (st is null) return;
        _nopPutInVehicle.OnPlayerExitVehicle(p.Id);
        st.RemoveFromVehicleTick = Environment.TickCount64;
        _nopSetPos.OnPlayerExitVehicle(p.Id);
        _nitroHack.OnPlayerExitVehicle(p.Id);
        _driveOnWater.OnPlayerExitVehicle(p.Id);
        _vehicleHealth.OnPlayerExitVehicle(p.Id, vehicle.Id);
        _vehicleSprint.OnPlayerExitVehicle(p.Id);
        _nopRemoveFromVehicle.OnPlayerExitVehicle(p.Id);
        st.VehicleId = -1;
    }

    private void OnPlayerStateChanged(object? sender, StateEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 11)) return;
        _vehicleTeleport.OnPlayerStateChanged(p, e);
        _carwarp.OnPlayerStateChanged(p, e);
        _blacklist.OnPlayerStateChanged(p, e);
        var st = _players.Get(p.Id);
        if (st is null) return;
        if (e.NewState == PlayerState.OnFoot && e.OldState == PlayerState.Driving)
        {
            st.RemoveFromVehicleTick = Environment.TickCount64;
            st.VehicleId = -1;
        }
        if (e.NewState == PlayerState.Driving)
        {
            st.EnterVehicleTick = Environment.TickCount64;
            if (p.Vehicle is not null) st.VehicleId = p.Vehicle.Id;
        }
    }

    private void OnPlayerText(object? sender, TextEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _macroDetection.OnPlayerText(p, e.Text);
        if (!_cbFlood.Check(p, 16)) e.SendToPlayers = false;
    }

    private void OnPlayerCommandText(object? sender, CommandTextEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 5);
        _macroDetection.OnPlayerCommandText(p, e.Text);
    }

    private void OnPlayerEnterCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 17);
        _checkpointTeleport.OnPlayerEnterCheckpoint(p);
    }

    private void OnPlayerLeaveCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 18);
        _checkpointTeleport.OnCheckpointDisabled(p.Id);
    }

    private void OnPlayerEnterRaceCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 21);
        _checkpointTeleport.OnPlayerEnterRaceCheckpoint(p);
    }

    private void OnPlayerLeaveRaceCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 22);
        _checkpointTeleport.OnRaceCheckpointDisabled(p.Id);
    }

    private void OnPlayerRequestSpawn(object? sender, RequestSpawnEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 19);
        _classSelection.OnPlayerRequestSpawn(p, e);
    }

    private void OnPlayerPickUpPickup(object? sender, PickUpPickupEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 8)) return;
        _fakePickup.OnPlayerPickUpPickup(p, e);
        _vehicleTeleport.OnPlayerPickUpPickup(p, e);
        var pickup = _pickups.Get(e.Pickup.Id);
        if (pickup is null) return;

        switch (pickup.Type)
        {
            case 1:
                _money.AllowMoneyGain(p.Id, pickup.Amount);
                break;
            case 2:
                var hst = _players.Get(p.Id);
                if (hst is not null) hst.SetHealthTick = Environment.TickCount64;
                break;
            case 3:
                var ast = _players.Get(p.Id);
                if (ast is not null) ast.SetArmourTick = Environment.TickCount64;
                break;
            case 4:
                _weapon.OnWeaponGiven(p.Id, pickup.Weapon, WeaponData.PickupAmmo[pickup.Weapon]);
                _ammo.OnAmmoGiven(p.Id, pickup.Weapon, WeaponData.PickupAmmo[pickup.Weapon]);
                break;
        }
    }

    private void OnPlayerRequestClass(object? sender, RequestClassEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 9);
        _classSelection.OnPlayerRequestClass(p, e);
    }

    private void OnPlayerSelectedMenuRow(object? sender, MenuRowEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 10)) return;
        _menuCrasher.OnPlayerMenuResponse(p, e);
    }

    private void OnPlayerExitedMenu(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 20)) return;
        _menuCrasher.OnPlayerExitMenu(p.Id);
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

    private void OnPlayerKeyStateChange(object? sender, KeyStateChangedEventArgs e) {
        if (sender is not BasePlayer p) return;
        _macroDetection.OnPlayerKeyStateChange(p, e.NewKeys, e.OldKeys);

        var st = _players.Get(p.Id);
        if (st is not null && e.NewKeys != e.OldKeys)
        {
            st.LastKeys = e.NewKeys;
            st.LastKeyChangeTick = Environment.TickCount64;
        }
    }

    private void OnVehicleMod(object? sender, VehicleModEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 12)) return;

        bool valid = _tuningCrasher.OnVehicleMod(v, p, e.ComponentId);
        if (valid)
        {
            _tuningHack.OnVehicleMod(v, p, e.ComponentId);
            _vehicleModHack.OnVehicleComponentAdded(v, p, e.ComponentId);
            _blacklist.OnVehicleModAdded(v, p, e.ComponentId);
            _nitroHack.OnVehicleModAdded(v.Id, e.ComponentId);
        }
    }

    private void OnPlayerEnterExitModShop(object? sender, EnterModShopEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 1)) return;
        var st = _players.Get(p.Id);
        if (st is null) return;
        st.IsInModShop = e.EnterExit == EnterExit.Entered;
    }

    private void OnVehiclePaintjob(object? sender, VehiclePaintjobEventArgs e) {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 13)) return;
        _paintJob.OnVehiclePaintjob(v, p, e.PaintjobId);
    }

    private void OnVehicleRespray(object? sender, VehicleResprayedEventArgs e)
    {
        if (sender is not BaseVehicle) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 14);
    }

    private void OnVehicleDied(object? sender, PlayerEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 15);
        _vehicles.Remove(v.Id);
        _nopSetVehicleHealth.OnVehicleDestroyed(v.Id);
        _vehicleModHack.OnVehicleDestroyed(v.Id);
        _vehicleHealth.OnVehicleDestroyed(v.Id);
        _paintJob.OnVehicleDestroyed(v.Id);
        _carwarp.OnVehicleDestroyed(v.Id);
    }

    private void OnVehicleDamageStatusUpdated(object? sender, PlayerEventArgs e)
    {
        if (sender is not BaseVehicle) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 24);
    }

    private void OnVehicleSirenStateChange(object? sender, SirenStateEventArgs e)
    {
        if (sender is not BaseVehicle) return;
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

    private void OnPunishment(int playerId, string checkName, PunishAction action) {
        var p = BasePlayer.Find(playerId);
        if (p is null) return;

        // Check whitelist
        if (IsWhitelisted(playerId)) {
            _logger.Log($"Punishment skipped for whitelisted player {playerId}");
            return;
        }

        var checkCfg = _config.GetCheck(checkName);
        string message = string.IsNullOrEmpty(checkCfg.CustomMessage)
            ? $"Anticheat: {checkName}"
            : checkCfg.CustomMessage;

        switch (action) {
            case PunishAction.Kick:
                _logger.LogKick(playerId, checkName);
                _warnings.TrackKick(playerId); // Track for auto-ban

                if (checkCfg.KickDelay > 0) {
                    System.Threading.Tasks.Task.Delay(checkCfg.KickDelay).ContinueWith(_ => {
                        var player = BasePlayer.Find(playerId);
                        player?.Kick(message);
                    });
                } else {
                    p.Kick(message);
                }
                break;

            case PunishAction.Ban:
                _logger.LogBan(playerId, checkName);
                p.Ban(message);
                break;
        }
    }

    public void OnApplyAnimation(int playerId, string animLib, string animName) {
        _animationHack.OnAnimationApplied(playerId, animLib, animName);
        _blacklist.OnAnimationApplied(playerId, animLib, animName);
    }

    public void OnAddVehicleComponent(int vehicleId, int componentId)
    => _vehicleModHack.OnServerAddComponent(vehicleId, componentId);

    public void OnRemoveVehicleComponent(int vehicleId, int componentId)
        => _vehicleModHack.OnServerRemoveComponent(vehicleId, componentId);

    public void OnVehicleRespawn(int vehicleId) {
        _vehicleModHack.OnVehicleRespawned(vehicleId);
        _vehicleFlip.OnVehicleRespawned(vehicleId);
        _vehicleHealth.OnVehicleRespawned(vehicleId);
        _paintJob.OnVehicleRespawned(vehicleId);
    }

    public bool VehicleHasComponent(int vehicleId, int componentId)
        => _vehicleModHack.HasComponent(vehicleId, componentId);

    public bool IsInteriorForbidden(int interiorId)
        => _interiorWeapon.IsForbiddenInterior(interiorId);

    public void AddForbiddenInterior(int interiorId)
        => _interiorWeapon.AddForbiddenInterior(interiorId);

    public void RemoveForbiddenInterior(int interiorId)
        => _interiorWeapon.RemoveForbiddenInterior(interiorId);

    public void OnSetPlayerCheckpoint(int playerId, float x, float y, float z, float size)
        => _checkpointTeleport.OnCheckpointSet(playerId, x, y, z, size);

    public void OnSetPlayerRaceCheckpoint(int playerId, float x, float y, float z, float nextX, float nextY, float nextZ, float size)
        => _checkpointTeleport.OnRaceCheckpointSet(playerId, x, y, z, nextX, nextY, nextZ, size);

    public void OnDisablePlayerCheckpoint(int playerId)
        => _checkpointTeleport.OnCheckpointDisabled(playerId);

    public void OnDisablePlayerRaceCheckpoint(int playerId)
        => _checkpointTeleport.OnRaceCheckpointDisabled(playerId);

    public bool IsPickupValid(int pickupId)
        => _fakePickup.IsValidPickup(pickupId);

    public int GetRegisteredPickupCount()
        => _fakePickup.GetPickupCount();

    public void OnAddPlayerClass(int classId)
        => _classSelection.RegisterClass(classId);

    public void OnClearPlayerClasses()
        => _classSelection.ClearRegisteredClasses();

    public bool IsValidClass(int classId)
        => _classSelection.IsValidClass(classId);

    public void SetEnabled(bool enabled)
    {
        _config.Enabled = enabled;
        _events.RaiseAnticheatToggled(enabled);
        _logger.Log($"Anticheat {(enabled ? "enabled" : "disabled")}");
    }

    public void SetCheckEnabled(string checkName, bool enabled)
    {
        var check = _config.GetCheck(checkName);
        check.Enabled = enabled;
        _events.RaiseCheckToggled(checkName, enabled);
        _logger.Log($"Check '{checkName}' {(enabled ? "enabled" : "disabled")}");
    }

    public bool IsCheckEnabled(string checkName)
        => _config.GetCheck(checkName).Enabled;

    public void ReloadConfig()
    {
        var fresh = LoadConfig();
        _config.Enabled = fresh.Enabled;
        _config.MaxPing = fresh.MaxPing;
        _config.LogPath = fresh.LogPath;
        _config.SpeedHackVehResetDelay = fresh.SpeedHackVehResetDelay;
        _config.MaxConnectsPerIp = fresh.MaxConnectsPerIp;
        _config.MinReconnectSeconds = fresh.MinReconnectSeconds;
        foreach (var (k, v) in fresh.Checks) _config.Checks[k] = v;
        _events.RaiseConfigReloaded();
        _logger.Log("Config reloaded via API");
    }

    public string[] GetAllCheckNames()
        => _config.Checks.Keys.ToArray();

    public CheckConfig GetCheckConfig(string checkName)
        => _config.GetCheck(checkName);

    public void OnSetVehiclePos(int vehicleId, float x, float y, float z)
        => _carwarp.OnServerSetVehiclePos(vehicleId, x, y, z);

    public void OnVehicleSpawn(int vehicleId, float x, float y, float z)
        => _carwarp.OnVehicleSpawned(vehicleId, x, y, z);

    public void RefreshCodeVerification()
        => _codeVerification.RefreshHashes();

    public Dictionary<string, bool> GetCodeVerificationStatus()
        => _codeVerification.GetVerificationStatus();

    public int GetVerificationFailures()
        => _codeVerification.GetFailureCount();

    public void AddWhitelistedIP(string ip) {
        if (!_config.WhitelistedIPs.Contains(ip)) {
            _config.WhitelistedIPs.Add(ip);
            _logger.Log($"IP whitelisted: {ip}");
        }
    }

    public void RemoveWhitelistedIP(string ip) {
        if (_config.WhitelistedIPs.Remove(ip))
            _logger.Log($"IP removed from whitelist: {ip}");
    }

    public void AddWhitelistedPlayer(int playerId) {
        if (!_config.WhitelistedPlayerIds.Contains(playerId)) {
            _config.WhitelistedPlayerIds.Add(playerId);
            _logger.Log($"Player whitelisted: {playerId}");
        }
    }

    public void RemoveWhitelistedPlayer(int playerId) {
        if (_config.WhitelistedPlayerIds.Remove(playerId))
            _logger.Log($"Player removed from whitelist: {playerId}");
    }

    public bool IsWhitelisted(int playerId) {
        var st = _players.Get(playerId);
        return _config.IsWhitelisted(playerId) ||
               (st != null && _config.IsWhitelisted(st.IpAddress));
    }

    public int GetPlayerKickCount(int playerId)
        => _warnings.GetTotalKicks(playerId);

    public void UpdateCheckConfig(string checkName, Action<CheckConfig> configure) {
        var check = _config.GetCheck(checkName);
        configure(check);
        _logger.Log($"Check '{checkName}' configuration updated");
    }

    public AntiCheatStats GetStatistics() => _stats;

    public string GenerateStatsReport() => _stats.GenerateReport();

    public PlayerCheatHistory? GetPlayerHistory(int playerId)
        => _stats.GetPlayerHistory(playerId);

    public void ResetStatistics() => _stats.Reset();
    public void AddBlacklistedWeapon(int weaponId) => _blacklist.AddBlacklistedWeapon(weaponId);
    public void RemoveBlacklistedWeapon(int weaponId) => _blacklist.RemoveBlacklistedWeapon(weaponId);
    public void AddBlacklistedSkin(int skin) => _blacklist.AddBlacklistedSkin(skin);
    public void RemoveBlacklistedSkin(int skin) => _blacklist.RemoveBlacklistedSkin(skin);
    public void AddBlacklistedVehicleMod(int componentId) => _blacklist.AddBlacklistedVehicleMod(componentId);
    public void RemoveBlacklistedVehicleMod(int componentId) => _blacklist.RemoveBlacklistedVehicleMod(componentId);
    public void AddBlacklistedVehicle(int model) => _blacklist.AddBlacklistedVehicle(model);
    public void RemoveBlacklistedVehicle(int model) => _blacklist.RemoveBlacklistedVehicle(model);
    public void AddBlacklistedSpecialAction(int action) => _blacklist.AddBlacklistedSpecialAction(action);
    public void RemoveBlacklistedSpecialAction(int action) => _blacklist.RemoveBlacklistedSpecialAction(action);
    public void AddBlacklistedAnimation(string animLib, string animName) => _blacklist.AddBlacklistedAnimation(animLib, animName);
    public void RemoveBlacklistedAnimation(string animLib, string animName) => _blacklist.RemoveBlacklistedAnimation(animLib, animName);

    public bool IsWeaponBlacklisted(int weaponId) => _blacklist.IsWeaponBlacklisted(weaponId);
    public bool IsSkinBlacklisted(int skin) => _blacklist.IsSkinBlacklisted(skin);
    public bool IsVehicleModBlacklisted(int componentId) => _blacklist.IsVehicleModBlacklisted(componentId);
    public bool IsVehicleBlacklisted(int model) => _blacklist.IsVehicleBlacklisted(model);

    public void OnSetVehiclePaintjob(int vehicleId, int paintjobId)
    => _paintJob.OnServerSetPaintjob(vehicleId, paintjobId);

    public bool IsValidPaintjob(int model, int paintjobId)
        => PaintJobCheck.IsValidPaintjob(model, paintjobId);

    public bool VehicleSupportsPaintjob(int model)
        => PaintJobCheck.VehicleSupportsPaintjob(model);

    public bool ValidateObjectCreate(int playerId, int modelId, float x, float y, float z, float drawDistance)
    {
        var player = BasePlayer.Find(playerId);
        if (player is null) return false;

        bool valid = _objectCrasher.ValidateObjectCreate(player, modelId, x, y, z, drawDistance);
        if (valid) _objectCrasher.OnObjectCreated(playerId);
        return valid;
    }

    public void OnObjectDestroyed(int playerId)
        => _objectCrasher.OnObjectDestroyed(playerId);

    public bool ValidateTextDraw(int playerId, string text)
    {
        var player = BasePlayer.Find(playerId);
        if (player is null) return false;
        return _textDrawCrasher.ValidateTextDraw(player, text);
    }

    public bool Validate3DText(int playerId, string text, float x, float y, float z, float drawDistance, int color)
    {
        var player = BasePlayer.Find(playerId);
        if (player is null) return false;

        bool valid = _text3DCrasher.Validate3DText(player, text, x, y, z, drawDistance, color);
        if (valid) _text3DCrasher.On3DTextCreated(playerId);
        return valid;
    }

    public void On3DTextDestroyed(int playerId)
        => _text3DCrasher.On3DTextDestroyed(playerId);

    public void OnMenuCreated(int menuId, int rows, int columns)
    {
        if (_menuCrasher.ValidateMenuCreate(rows, columns))
            _menuCrasher.OnMenuCreated(menuId, rows, columns);
    }

    public void OnMenuDestroyed(int menuId)
        => _menuCrasher.OnMenuDestroyed(menuId);

    public void OnPlayerShowMenu(int playerId, int menuId)
        => _menuCrasher.OnPlayerShowMenu(playerId, menuId);

    public bool ValidateMenuCreate(int rows, int columns)
        => _menuCrasher.ValidateMenuCreate(rows, columns);

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _watcher?.Dispose();
        _logger.Dispose();
    }
}