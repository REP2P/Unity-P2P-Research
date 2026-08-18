namespace REP2P.Steam
{
    public struct SteamCreateServer : NetworkCreateServer
    {
        public int maxMembers;
        public bool isJoinable;
        public SteamLobbyPrivacy privacy;
        public static SteamCreateServer CreateDefault()
        {
            return new SteamCreateServer() { maxMembers = 100, isJoinable = true, privacy = SteamLobbyPrivacy .Public};
        }
    }
}
