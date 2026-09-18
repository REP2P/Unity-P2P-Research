using System;

namespace REP2P.LiteNetLib
{
    [Obsolete("Use this to test P2P locally on your machine or LAN, this could provide a huge security risk if used remotelly")]
    public struct LiteNetLibStart : P2PStart
    {
        public int port;

        public static LiteNetLibStart CreateDefault()
        {
            return new LiteNetLibStart() { port = 7777 };
        }
    }
}
