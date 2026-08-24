using System;

namespace REP2P
{
    [Serializable]
    public struct NetworkPacket
    {       
        public int packetId;
        public byte[] packetBytes;

        public static NetworkPacket Create(int packetId, byte[] packetBytes)
        {
            return new NetworkPacket()
            {
                packetId = packetId,
                packetBytes = packetBytes
            };
        }

        public static byte[] ToBytes(NetworkPacket packet)
        {
            using (BitStream stream = BitStream.Create()) {
                return stream.Write(packet.packetId).Write(packet.packetBytes).ToArray();
            }
        }

        public static NetworkPacket FromBytes(byte[] bytes)
        {
            using (BitStream stream = BitStream.Create(bytes))
            {
                return NetworkPacket.Create(stream.ReadInt(), stream.ReadBytes());
            }
        }
    }
}
