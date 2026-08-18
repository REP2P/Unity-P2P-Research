namespace REP2P
{
    public interface NetworkLayer
    {
        void Initialize(Network network);
        void Deinitialize();
        void Tick();
        NetworkConnection GetOwner();
        NetworkConnection[] GetConnections();
        void CreateServer(NetworkCreateServer handle);
        void JoinServer(NetworkJoinServer handle);
        void Disconnect();
        void SendPacketToTarget(string connectionId, NetworkPacket message, PacketSendMode sendMode = PacketSendMode.Unreliable);
        void SendPacketToAll(NetworkPacket message, PacketSendMode sendMode = PacketSendMode.Unreliable);

        bool IsHost();
        bool IsConnected();
    }
}
