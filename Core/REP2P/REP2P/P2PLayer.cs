using System.Collections.Generic;

namespace REP2P
{
    public interface P2PLayer
    {
        void Initialize();
        void Deinitialize();
        void Tick();
        P2PConnection GetOwner();
        Dictionary<string, P2PConnection> GetConnections();
        void Start(int maxConnections, P2PStart handle);
        void Connect(string address, P2PConnect handle);
        void Disconnect();
        void SendPacketToTarget(string connectionId, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable);
        void SendPacketToAll(P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable);

        bool IsHost();
        bool IsConnected();
        void Disconnect(P2PConnection connection);
    }
}
