using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P.Layers.Steam
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
