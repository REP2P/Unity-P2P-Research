using System;

namespace REP2P
{
    [Serializable]
    public struct P2PPacket
    {       
        public int packetId;
        public byte[] packetBytes;

        public static P2PPacket Create(int packetId, byte[] packetBytes)
        {
            return new P2PPacket()
            {
                packetId = packetId,
                packetBytes = packetBytes
            };
        }

        public static byte[] ToBytes(P2PPacket packet)
        {
            using (P2PStream stream = P2PStream.Create()) {
                stream.Write(packet.packetId);
                stream.Write(packet.packetBytes);
                return stream.ToArray();
            }
        }

        public static P2PPacket FromBytes(byte[] bytes)
        {
            using (P2PStream stream = P2PStream.Create(bytes))
            {
                return Create(stream.ReadInt(), stream.ReadBytes());
            }
        }
    }
}
