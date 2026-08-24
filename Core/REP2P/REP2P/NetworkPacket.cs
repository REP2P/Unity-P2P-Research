using System.Collections.Generic;
using System.IO;

namespace REP2P
{
    public struct NetworkPacket
    {
        public int packetId;
        public byte[] packetBytes;

        public static NetworkPacket Create(int packetId, byte[] packetBytes)
        {
            var instance = new NetworkPacket()
            {
                packetId = packetId,
                packetBytes = packetBytes
            };
            return instance;
        }

        public byte[] ToBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream)) { 
                    writer.Write(packetId);
                    writer.Write(packetBytes);
                    return stream.ToArray();
                }
            }
        }

        public static NetworkPacket FromBytes(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {                  
                    return Create(reader.ReadInt32(), reader.ReadBytes(bytes.Length));
                }
            }
        }
    }
}
