namespace REP2P.LiteNetLib
{
    public struct LiteNetLibConnect : P2PConnect
    {
        public int port;
        public string key;
        public static LiteNetLibConnect CreateDefault()
        {
            return new LiteNetLibConnect() { port = 7777, key = "P2P" };
        }
    }
}
