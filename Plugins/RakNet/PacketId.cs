namespace ProjectSMP.Plugins.RakNet
{
    /// <summary>
    /// SA-MP Packet IDs
    /// </summary>
    public static class PacketId
    {
        public const int VEHICLE_SYNC = 200;
        public const int RCON_COMMAND = 201;
        public const int RCON_RESPONSE = 202;
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

    /// <summary>
    /// SA-MP RPC IDs
    /// </summary>
    public static class RPCId
    {
        // Player RPCs
        public const int ClickPlayer = 23;
        public const int ClientJoin = 25;
        public const int EnterVehicle = 26;
        public const int ScriptCash = 31;
        public const int Death = 53;
        public const int DialogResponse = 62;
        public const int ClickTextDraw = 83;
        public const int ServerJoin = 137;
        public const int SetInterior = 156;

        // Vehicle RPCs
        public const int EnterEditObject = 27;
        public const int SetPlayerPos = 12;
        public const int SetPlayerPosFindZ = 145;
        public const int SetPlayerFacingAngle = 19;
        public const int SetSpawnInfo = 68;
        public const int CreateExplosion = 79;
        public const int ShowTextDraw = 134;
        public const int HideTextDraw = 135;
        public const int EditTextDraw = 105;
        public const int AddPlayerClass = 67;
        public const int AddStaticVehicle = 164;
        public const int InitGame = 139;

        // Chat RPCs
        public const int Chat = 101;
        public const int ServerMessage = 93;
        public const int SetPlayerColor = 72;
        public const int SetPlayerName = 11;
        public const int SetPlayerTeam = 69;
        public const int SetPlayerSkin = 153;
        public const int SetPlayerShopName = 33;
        public const int SetPlayerSkillLevel = 34;

        // World RPCs
        public const int SetPlayerWorldBounds = 17;
        public const int SetPlayerTime = 29;
        public const int SetPlayerWeather = 152;
        public const int SetPlayerDrunkLevel = 35;
        public const int CreatePickup = 95;
        public const int DestroyPickup = 63;
        public const int Create3DTextLabel = 36;
        public const int Delete3DTextLabel = 37;

        // Dialog RPCs
        public const int ShowDialog = 61;

        // Object RPCs
        public const int CreateObject = 44;
        public const int DestroyObject = 47;
        public const int SetObjectPos = 45;
        public const int SetObjectRot = 46;
        public const int AttachObjectToPlayer = 113;
    }
}