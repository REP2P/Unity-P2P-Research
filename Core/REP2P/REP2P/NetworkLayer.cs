namespace REP2P
{
    public interface NetworkLayer
    {
        void Initialize(Network network);
        void Deinitialize();
        void Tick();
        NetworkConnection GetOwner();
        NetworkConnection[] GetConnections();
        void CreateSession(CreateSessionHandle handle);
        void JoinSession(JoinSessionHandle handle);
        void Disconnect();
        void SendPacketToTarget(string connectionId, NetworkPacket packet, PacketSendMode sendMode = PacketSendMode.Unreliable);
        void SendPacketToAll(NetworkPacket packet, PacketSendMode sendMode = PacketSendMode.Unreliable);

        bool IsHost();
        bool IsConnected();
        SessionState GetSessionState();
        void Disconnect(NetworkConnection connection);
    }
}
