using REP2P;
using System.Collections;
using UnityEngine;

public class LocalTestNetworkManager : MonoBehaviour
{
    public Transform target;
    public Transform copycat;

    private void Update()
    {
        Vector3 position = target.position;
        Vector3 scale = target.localScale;

        using(P2PStream stream = P2PStream.Create())
        {
            stream.Write(position);
            StartCoroutine(HandlePacketCopycat(stream.ToPacket(0)));
        }
        using (P2PStream stream = P2PStream.Create())
        {
            stream.Write(scale);
            StartCoroutine(HandlePacketCopycat(stream.ToPacket(1)));
        }
    }
    public float minLatency = 0;
    public float maxLatency = 0;
    private IEnumerator HandlePacketCopycat(P2PPacket packet)
    {
        byte[] bytes = P2PPacket.ToBytes(packet);
        yield return new WaitForSeconds(Random.Range(minLatency, maxLatency)); 
        HandlePacketCopycatInternal(bytes);
    }

    private void HandlePacketCopycatInternal(byte[] bytes)
    {
        P2PPacket packet = P2PPacket.FromBytes(bytes);
        using (P2PStream reader = P2PStream.Create(packet.packetBytes))
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
