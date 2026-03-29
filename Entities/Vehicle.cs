#nullable enable
using ProjectSMP.Extensions;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.World;
using System;
using System.Linq;

namespace ProjectSMP.Entities
{
    [PooledType]
    public partial class Vehicle : BaseVehicle
    {
        public int DatabaseId { get; set; } = -1;
        public VehicleType VehicleType { get; set; } = VehicleType.None;

        public string PlateText { get; set; } = "";
        public bool IsLocked { get; set; }
        public bool IsEngineOn { get; set; }

        public float VehicleHealth { get; set; } = 1000f;
        public float Fuel { get; set; } = 100f;
        public float MaxFuel { get; set; } = 100f;

        public Vector3 SpawnPosition { get; set; }
        public float SpawnRotation { get; set; }
        public int SpawnInterior { get; set; }
        public int SpawnWorld { get; set; }

        public int CustomColor1 { get; set; } = -1;
        public int CustomColor2 { get; set; } = -1;
        public int CustomPaintjob { get; set; } = -1;

        public int[] DamageStatus { get; set; } = new int[4];
        public int[] ModComponents { get; set; } = new int[17];

        public TextLabel? VehicleLabel { get; set; }

        public DateTime LastUsed { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public override void OnDeath(PlayerEventArgs e)
        {
            base.OnDeath(e);
            WeaponConfigService.OnVehicleDeath(Id);
            OnVehicleDestroyed(e.Player as Player);
        }

        public override void OnStreamIn(PlayerEventArgs e)
        {
            base.OnStreamIn(e);
            OnVehicleStreamIn(e.Player as Player);
        }

        public override void OnStreamOut(PlayerEventArgs e)
        {
            base.OnStreamOut(e);
            OnVehicleStreamOut(e.Player as Player);
        }

        public override void OnPlayerEnter(EnterVehicleEventArgs e)
        {
            base.OnPlayerEnter(e);
            if (e.Player is Player player)
                OnPlayerEnterVehicle(player, e.IsPassenger);
        }

        public override void OnPlayerExit(PlayerVehicleEventArgs e)
        {
            base.OnPlayerExit(e);
            if (e.Player is Player player)
                OnPlayerExitVehicle(player);
        }

        public override void OnMod(VehicleModEventArgs e)
        {
            base.OnMod(e);
            SaveModComponent(e.ComponentId);
            if (e.Player is Player player)
                OnVehicleModified(player, e.ComponentId);
        }

        public override void OnPaintjobApplied(VehiclePaintjobEventArgs e)
        {
            base.OnPaintjobApplied(e);
            CustomPaintjob = e.PaintjobId;
            if (e.Player is Player player)
                OnVehiclePaintjobChanged(player, e.PaintjobId);
        }

        public override void OnUnoccupiedUpdate(UnoccupiedVehicleEventArgs e)
        {
            base.OnUnoccupiedUpdate(e);
            if (e.Player is Player player)
                OnVehicleUnoccupiedUpdate(player);
        }

        protected virtual void OnPlayerEnterVehicle(Player? player, bool isPassenger) { }
        protected virtual void OnPlayerExitVehicle(Player? player) { }
        protected virtual void OnVehicleDestroyed(Player? killer) { }
        protected virtual void OnVehicleStreamIn(Player? player) { }
        protected virtual void OnVehicleStreamOut(Player? player) { }
        protected virtual void OnVehicleModified(Player? player, int componentId) { }
        protected virtual void OnVehiclePaintjobChanged(Player? player, int paintjob) { }
        protected virtual void OnVehicleUnoccupiedUpdate(Player? player) { }

        public virtual void LockDoors(bool locked)
        {
            IsLocked = locked;
            GetParameters(out bool engine, out bool lights, out bool alarm, out bool _, out bool bonnet, out bool boot, out bool objective);
            SetParameters(engine, lights, alarm, locked, bonnet, boot, objective);
        }

        public virtual void ToggleEngine(bool state)
        {
            IsEngineOn = state;
            GetParameters(out bool _, out bool lights, out bool alarm, out bool doors, out bool bonnet, out bool boot, out bool objective);
            SetParameters(state, lights, alarm, doors, bonnet, boot, objective);
        }

        public virtual void ToggleLights(bool state)
        {
            GetParameters(out bool engine, out bool _, out bool alarm, out bool doors, out bool bonnet, out bool boot, out bool objective);
            SetParameters(engine, state, alarm, doors, bonnet, boot, objective);
        }

        public virtual bool GetEngineState()
        {
            GetParameters(out bool engine, out bool _, out bool _, out bool _, out bool _, out bool _, out bool _);
            return engine;
        }

        public virtual bool GetLightsState()
        {
            GetParameters(out bool _, out bool lights, out bool _, out bool _, out bool _, out bool _, out bool _);
            return lights;
        }

        public virtual bool GetDoorsLockedState()
        {
            GetParameters(out bool _, out bool _, out bool _, out bool doors, out bool _, out bool _, out bool _);
            return doors;
        }

        public virtual void RefillFuel(float amount)
        {
            Fuel = Math.Min(Fuel + amount, MaxFuel);
            UpdateVehicleLabel();
        }

        public virtual void ConsumeFuel(float amount)
        {
            Fuel = Math.Max(Fuel - amount, 0f);
            UpdateVehicleLabel();
        }

        public virtual bool HasFuel()
        {
            return Fuel > 0f;
        }

        public virtual void SaveSpawnPoint()
        {
            SpawnPosition = Position;
            SpawnRotation = Angle;
            SpawnInterior = 0;
            SpawnWorld = VirtualWorld;
        }

        public virtual void RespawnAtSpawnPoint()
        {
            this.SetPositionSafe(SpawnPosition);
            Angle = SpawnRotation;
            VirtualWorld = SpawnWorld;
            this.SetHealthSafe(VehicleHealth);
            ApplySpawnSettings();
            ApplyDamageStatus();
            ApplyModComponents();
        }

        protected virtual void ApplySpawnSettings()
        {
            if (CustomPaintjob >= 0)
                this.SetPaintjobSafe(CustomPaintjob);

            LockDoors(IsLocked);
            ToggleEngine(IsEngineOn);
        }

        public virtual void SaveDamageStatus()
        {
            GetDamageStatus(out int panels, out int doors, out int lights, out int tires);
            DamageStatus[0] = panels;
            DamageStatus[1] = doors;
            DamageStatus[2] = lights;
            DamageStatus[3] = tires;
        }

        public virtual void ApplyDamageStatus()
        {
            if (DamageStatus.Any(d => d != 0))
                this.UpdateDamageStatusSafe(DamageStatus[0], DamageStatus[1], DamageStatus[2], DamageStatus[3]);
        }

        public virtual void SaveModComponent(int componentId)
        {
            for (var i = 0; i < ModComponents.Length; i++)
            {
                if (ModComponents[i] == 0)
                {
                    ModComponents[i] = componentId;
                    break;
                }
            }
        }

        public virtual void ApplyModComponents()
        {
            foreach (var mod in ModComponents)
            {
                if (mod > 0)
                    AddComponent(mod);
            }
        }

        public virtual void CreateVehicleLabel()
        {
            DestroyVehicleLabel();
            var labelText = GetVehicleLabelText();
            if (!string.IsNullOrEmpty(labelText))
                VehicleLabel = new TextLabel(labelText, new Color(255, 255, 255), Position, 10f, 0, false);
        }

        public virtual void UpdateVehicleLabel()
        {
            if (VehicleLabel != null)
            {
                var labelText = GetVehicleLabelText();
                VehicleLabel.Text = labelText;
            }
            else
            {
                CreateVehicleLabel();
            }
        }

        public virtual void DestroyVehicleLabel()
        {
            if (VehicleLabel != null)
            {
                VehicleLabel.Dispose();
                VehicleLabel = null;
            }
        }

        protected virtual string GetVehicleLabelText()
        {
            return string.Empty;
        }

        public virtual void ResetVehicle()
        {
            this.RepairSafe();
            Fuel = MaxFuel;
            VehicleHealth = 1000f;
            IsEngineOn = false;
            IsLocked = false;
            DamageStatus = new int[4];
            LockDoors(false);
            ToggleEngine(false);
            UpdateVehicleLabel();
        }

        public virtual void Destroy()
        {
            DestroyVehicleLabel();
            WeaponConfigService.OnVehicleDestroy(Id);
            Dispose();
        }

        public static Vehicle CreateVehicle(VehicleModelType modelid, Vector3 position, float rotation, int color1, int color2, int respawnDelay = -1, bool addSiren = false)
        {
            var baseVehicle = Create(modelid, position, rotation, color1, color2, respawnDelay, addSiren);
            if (baseVehicle is not Vehicle vehicle) {
                baseVehicle.Dispose();
                throw new InvalidOperationException("Failed to create Vehicle instance");
            }

            vehicle.SpawnPosition = position;
            vehicle.SpawnRotation = rotation;
            vehicle.CustomColor1 = color1;
            vehicle.CustomColor2 = color2;
            vehicle.SaveSpawnPoint();
            WeaponConfigService.OnVehicleSpawn(vehicle.Id);
            vehicle.ApplySpawnSettings();
            vehicle.CreateVehicleLabel();
            return vehicle;
        }

        public static T CreateVehicle<T>(VehicleModelType modelid, Vector3 position, float rotation, int color1, int color2, int respawnDelay = -1, bool addSiren = false) where T : Vehicle, new()
        {
            var baseVehicle = Create(modelid, position, rotation, color1, color2, respawnDelay, addSiren);
            if (baseVehicle is not T vehicle) {
                baseVehicle.Dispose();
                throw new InvalidOperationException($"Failed to create {typeof(T).Name} instance");
            }

            vehicle.SpawnPosition = position;
            vehicle.SpawnRotation = rotation;
            vehicle.CustomColor1 = color1;
            vehicle.CustomColor2 = color2;
            vehicle.SaveSpawnPoint();
            WeaponConfigService.OnVehicleSpawn(vehicle.Id);
            vehicle.ApplySpawnSettings();
            vehicle.CreateVehicleLabel();
            return vehicle;
        }
    }

    public enum VehicleType
    {
        None,
        Private,
        Faction,
        Business,
        Workshop,
        Job,
        Rental,
        Admin,
        Dealership
    }
}