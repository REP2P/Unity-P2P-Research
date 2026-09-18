using System;

namespace REP2P.LiteNetLib
{
    [Obsolete("Use this to test P2P locally on your machine or LAN, this could provide a huge security risk if used remotelly")]
    public struct LiteNetLibConnect : P2PConnect
    {
        public int port;
        public string key;
        public static LiteNetLibConnect CreateDefault()
        {
            return new LiteNetLibConnect() { port = 7777, key = LiteNetLibLayer.connectKey };
        }
    }
}
