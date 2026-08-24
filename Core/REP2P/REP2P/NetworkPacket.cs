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
                stream.Write(packet.packetId);
                stream.Write(packet.packetBytes);
                return stream.ToArray();
            }
        }

        public static NetworkPacket FromBytes(byte[] bytes)
        {
            using (BitStream stream = BitStream.Create(bytes))
            {
                return Create(stream.ReadInt(), stream.ReadBytes());
            }
        }
    }
}
