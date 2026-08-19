using REP2P;
using System.Collections;
using System.IO;
using UnityEngine;

public class LocalTestManager : MonoBehaviour
{
    public Transform target;
    public Transform copycat;

    private void Update()
    {
        Vector3 position = target.position;
        Vector3 scale = target.localScale;

        using(MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms)) {
                bw.Write(position);
                NetworkPacket packet = new NetworkPacket(0, ms.ToArray());
                StartCoroutine(HandlePacketCopycat(packet.ToBytes()));
            }
        }
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(scale);
                NetworkPacket packet = new NetworkPacket(1, ms.ToArray());
                StartCoroutine(HandlePacketCopycat(packet.ToBytes()));
            }
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
        using (MemoryStream ms = new MemoryStream(packet.packetBytes))
        {
            using (BinaryReader reader = new BinaryReader(ms))
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
}
