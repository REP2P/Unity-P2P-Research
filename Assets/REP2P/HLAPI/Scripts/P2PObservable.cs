using LiteNetLib.Utils;
using REP2P;

public interface P2PObservable
{
    public void OnP2PSerialize(NetDataWriter writer, P2PPacketInfo info);
    public void OnP2PDeserialize(NetDataReader reader, P2PPacketInfo info);
}
