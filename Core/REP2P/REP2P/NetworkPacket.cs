using System.Collections.Generic;
using System.IO;

namespace REP2P
{
    public struct NetworkPacket
    {
        public int packetId;
        public byte[] packetBytes;

        public NetworkPacket(int packetId, byte[] packetBytes)
        {
            this.packetId = packetId;
            this.packetBytes = packetBytes;
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
                    return new NetworkPacket(reader.ReadInt32(), reader.ReadBytes(bytes.Length));
                }
            }
        }
    }
}
