namespace REP2P.Steam
{
    public struct SteamCreateSessionHandle : P2PCreate
    {
        public int maxMembers;
        public bool isJoinable;
        public SteamLobbyPrivacy privacy;
        public static SteamCreateSessionHandle CreateDefault()
        {
            return new SteamCreateSessionHandle() { maxMembers = 100, isJoinable = true, privacy = SteamLobbyPrivacy .Public};
        }
    }
}
