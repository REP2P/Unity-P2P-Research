namespace REP2P
{
    public interface P2PLayer
    {
        void Initialize(P2PNetwork network);
        void Deinitialize();
        void Tick();
        P2PConnection GetOwner();
        P2PConnection[] GetConnections();
        void CreateSession(P2PCreate handle);
        void JoinSession(P2PJoin handle);
        void Disconnect();
        void SendPacketToTarget(string connectionId, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable);
        void SendPacketToAll(P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable);

        bool IsHost();
        bool IsConnected();
        P2PSession GetSessionState();
        void Disconnect(P2PConnection connection);
    }
}
