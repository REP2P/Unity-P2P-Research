namespace REP2P.LiteNetLib
{
    public struct LiteNetLibStart : P2PStart
    {
        public int port;

        public static LiteNetLibStart CreateDefault()
        {
            return new LiteNetLibStart() { port = 7777 };
        }
    }
}
