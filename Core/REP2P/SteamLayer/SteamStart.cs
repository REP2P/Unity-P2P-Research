namespace REP2P.Steam
{
    public struct SteamStart : P2PStart
    {
        public bool isJoinable;
        public SteamLobbyPrivacy privacy;
        public static SteamStart CreateDefault()
        {
            return new SteamStart() { isJoinable = true, privacy = SteamLobbyPrivacy .Public};
        }
    }
}
