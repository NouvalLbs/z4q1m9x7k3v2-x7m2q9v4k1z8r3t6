using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Spawn;

/// <summary>Check 27 — pemain spawn padahal tidak mati/tidak valid</summary>
public class FakeSpawnCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public FakeSpawnCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    /// <summary>Dipanggil dari OnPlayerSpawn.</summary>
    public void OnPlayerSpawned(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || !_config.Enabled) return;
        if (!_config.GetCheck("FakeSpawn").Enabled) return;

        long now = Environment.TickCount64;

        // Spawn valid jika:
        //   1. Pemain baru connect (SpawnTick = 0)
        //   2. Pemain memang mati (IsDead = true)
        //   3. Server paksa spawn (SpawnSetFlag > 0)
        //   4. Request class baru saja (PendingClassResult = true)
        bool validSpawn = st.IsDead
                       || st.SpawnTick == 0
                       || st.SpawnSetFlag > 0
                       || st.PendingClassResult
                       || now - st.SpawnTick > 30_000; // toleransi 30 detik

        if (!validSpawn)
        {
            _warnings.AddWarning(player.Id, "FakeSpawn",
                $"isDead={st.IsDead} setFlag={st.SpawnSetFlag}");
        }

        // Reset flags
        st.IsDead = false;
        st.SpawnSetFlag = 0;
        st.PendingClassResult = false;
        st.SpawnTick = now;
        st.Health = player.Health;
        st.Armour = player.Armour;
        st.Money = player.Money;

        var pos = player.Position;
        st.SpawnX = pos.X;
        st.SpawnY = pos.Y;
        st.SpawnZ = pos.Z;
    }
}