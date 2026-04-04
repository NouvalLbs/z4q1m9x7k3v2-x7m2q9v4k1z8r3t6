namespace ProjectSMP.Plugins.RakNet
{
    public static class PacketId
    {
        public const int VEHICLE_SYNC = 200;
        public const int RCON_COMMAND = 201;
        public const int RCON_RESPONCE = 202;
        public const int AIM_SYNC = 203;
        public const int WEAPONS_UPDATE = 204;
        public const int STATS_UPDATE = 205;
        public const int BULLET_SYNC = 206;
        public const int PLAYER_SYNC = 207;
        public const int MARKERS_SYNC = 208;
        public const int UNOCCUPIED_SYNC = 209;
        public const int TRAILER_SYNC = 210;
        public const int PASSENGER_SYNC = 211;
        public const int SPECTATOR_SYNC = 212;
    }

    public static class RPCId
    {
        public const int RPC_ClickPlayer = 23;
        public const int RPC_ClientJoin = 25;
        public const int RPC_EnterVehicle = 26;
        public const int RPC_ScriptCash = 31;
        public const int RPC_ServerJoin = 137;
        public const int RPC_Death = 53;
        public const int RPC_DialogResponse = 62;
        public const int RPC_ClickTextDraw = 83;
        public const int RPC_SetInterior = 156;
    }
}