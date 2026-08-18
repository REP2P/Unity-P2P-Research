using System;

namespace REP2P
{
    public class Network
    {
        public NetworkLayer Layer { get; private set; }
        public bool IsHost => Layer.IsHost();
        public bool IsConnected => Layer.IsConnected();
        public NetworkConnection Owner => Layer.GetOwner();
        public NetworkConnection[] Connections => Layer.GetConnections();
        public Action OnCreatedServer;
        public Action OnConnected;
        public Action OnDisconnected;
        public Action<NetworkConnection> OnClientConnected;
        public Action<NetworkConnection> OnClientDisconnected;
        private PacketHandler packetHandlerSubscription;

        public void Initialize(NetworkLayer layer)
        {
            Layer = layer;
            Layer.Initialize(this);
        }
        public void DeInitialize() => Layer.Deinitialize();
        public void Update() => Layer.Tick();

        public void SubscribePacketHandler(PacketHandler packetHandler) => packetHandlerSubscription += packetHandler;
        public void UnSubscribePacketHandler(PacketHandler packetHandler) => packetHandlerSubscription -= packetHandler;
        public void CreateServer(NetworkCreateServer handle) => Layer.CreateServer(handle);
        public void JoinServer(NetworkJoinServer handle) => Layer.JoinServer(handle);
        public void Disconnect() => Layer.Disconnect();
        public void Send(NetworkConnection connection, NetworkPacket packet, PacketSendMode sendMode = PacketSendMode.Unreliable) => Send(connection.connectionId, packet, sendMode);
        public void Send(string connectionId, NetworkPacket packet, PacketSendMode sendMode = PacketSendMode.Unreliable) => Layer.SendPacketToTarget(connectionId, packet, sendMode);
        public void SendToAll(NetworkPacket packet, PacketSendMode sendMode = PacketSendMode.Unreliable) => Layer.SendPacketToAll(packet, sendMode);
        public void HandleMessageFrom(NetworkConnection sender, NetworkPacket message) => packetHandlerSubscription?.Invoke(sender, message);
    }
}
