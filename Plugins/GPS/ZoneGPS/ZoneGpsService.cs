using ProjectSMP.Plugins.RakNet;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.GPS.ZoneGPS
{
    public static class ZoneGpsService
    {
        private const int MaxPlayerGangZones = 1024;
        private static readonly Dictionary<int, Dictionary<int, ZoneGpsData>> _playerZones = new();

        public static int Create(BasePlayer player, float minX, float minY, float maxX, float maxY)
        {
            if (!_playerZones.ContainsKey(player.Id))
                _playerZones[player.Id] = new Dictionary<int, ZoneGpsData>();

            var zones = _playerZones[player.Id];

            for (int i = 0; i < MaxPlayerGangZones; i++)
            {
                if (!zones.ContainsKey(i) || !zones[i].Used)
                {
                    zones[i] = new ZoneGpsData
                    {
                        MinX = minX,
                        MinY = minY,
                        MaxX = maxX,
                        MaxY = maxY,
                        Color = 0xFFFFFFAA,
                        Used = true,
                        Shown = false
                    };
                    return i;
                }
            }

            return -1;
        }

        public static bool Show(BasePlayer player, int zoneId, uint color)
        {
            if (!_playerZones.ContainsKey(player.Id) ||
                !_playerZones[player.Id].ContainsKey(zoneId) ||
                !_playerZones[player.Id][zoneId].Used)
                return false;

            var zone = _playerZones[player.Id][zoneId];
            zone.Color = color;
            zone.Shown = true;

            var abgrColor = ConvertToABGR(color);
            var bs = RakNetService.BS_New();

            RakNetService.BS_WriteUint16(bs, 1023 - zoneId);
            RakNetService.BS_WriteFloat(bs, zone.MinX);
            RakNetService.BS_WriteFloat(bs, zone.MinY);
            RakNetService.BS_WriteFloat(bs, zone.MaxX);
            RakNetService.BS_WriteFloat(bs, zone.MaxY);
            RakNetService.BS_WriteUint32(bs, unchecked((int)abgrColor));

            RakNetService.PR_SendRPC(bs, player.Id, 0x6C);
            RakNetService.BS_Delete(bs);

            return true;
        }

        public static bool Hide(BasePlayer player, int zoneId)
        {
            if (!_playerZones.ContainsKey(player.Id) ||
                !_playerZones[player.Id].ContainsKey(zoneId) ||
                !_playerZones[player.Id][zoneId].Used ||
                !_playerZones[player.Id][zoneId].Shown)
                return false;

            _playerZones[player.Id][zoneId].Shown = false;

            var bs = RakNetService.BS_New();
            RakNetService.BS_WriteUint16(bs, 1023 - zoneId);
            RakNetService.PR_SendRPC(bs, player.Id, 0x78);
            RakNetService.BS_Delete(bs);

            return true;
        }

        public static bool Destroy(BasePlayer player, int zoneId)
        {
            if (!_playerZones.ContainsKey(player.Id) ||
                !_playerZones[player.Id].ContainsKey(zoneId) ||
                !_playerZones[player.Id][zoneId].Used)
                return false;

            if (_playerZones[player.Id][zoneId].Shown)
                Hide(player, zoneId);

            _playerZones[player.Id].Remove(zoneId);
            return true;
        }

        public static void OnPlayerDisconnect(BasePlayer player)
        {
            if (_playerZones.ContainsKey(player.Id))
                _playerZones.Remove(player.Id);
        }

        private static uint ConvertToABGR(uint rgba)
        {
            return (((rgba << 16) | rgba & 0xFF00) << 8) |
                   (((rgba >> 16) | rgba & 0xFF0000) >> 8);
        }

        public static void Dispose()
        {
            _playerZones.Clear();
        }
    }
}