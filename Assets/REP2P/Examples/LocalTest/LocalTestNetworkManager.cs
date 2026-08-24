using REP2P;
using System.Collections;
using System.IO;
using UnityEngine;
using BitStream = REP2P.BitStream;

public class LocalTestNetworkManager : MonoBehaviour
{
    public Transform target;
    public Transform copycat;

    private void Update()
    {
        Vector3 position = target.position;
        Vector3 scale = target.localScale;

        using(BitStream stream = BitStream.Create())
        {
            stream.Write(position);
            NetworkPacket packet = NetworkPacket.Create(0, stream.ToArray());
            StartCoroutine(HandlePacketCopycat(NetworkPacket.ToBytes(packet)));
        }
        using (BitStream stream = BitStream.Create())
        {
            stream.Write(scale);
            NetworkPacket packet = NetworkPacket.Create(1, stream.ToArray());
            StartCoroutine(HandlePacketCopycat(NetworkPacket.ToBytes(packet)));
        }
    }
    public float minLatency = 0;
    public float maxLatency = 0;
    private IEnumerator HandlePacketCopycat(byte[] bytes)
    {
        yield return new WaitForSeconds(Random.Range(minLatency, maxLatency)); 
        HandlePacketCopycatInternal(bytes);
    }

    private void HandlePacketCopycatInternal(byte[] bytes)
    {
        NetworkPacket packet = NetworkPacket.FromBytes(bytes);
        using (BitStream reader = BitStream.Create(packet.packetBytes))
        {
            switch (packet.packetId)
            {
                case 0:
                    copycat.position = reader.ReadVector3();
                    break;
                case 1:
                    copycat.localScale = reader.ReadVector3();
                    break;
            }
        }
    }
}
