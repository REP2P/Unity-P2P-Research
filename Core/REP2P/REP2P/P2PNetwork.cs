using System;

namespace REP2P
{
    public class P2PNetwork
    {
        public P2PLayer Layer { get; private set; }
        public bool IsHost => Layer.IsHost();
        public bool IsConnected => Layer.IsConnected();
        public P2PConnection Owner => Layer.GetOwner();
        public P2PConnection[] Connections => Layer.GetConnections();
        public P2PSession SessionState => Layer.GetSessionState();
        public Action OnCreatedSession;
        public Action OnConnected;
        public Action OnDisconnected;
        public Action<P2PConnection> OnClientConnected;
        public Action<P2PConnection> OnClientDisconnected;
        private P2PHandler packetHandlerSubscription;

        public void Initialize(P2PLayer layer)
        {
            Layer = layer;
            Layer.Initialize(this);
        }
        public void DeInitialize() => Layer.Deinitialize();
        public void Update() => Layer.Tick();

        public void SubscribePacketHandler(P2PHandler packetHandler) => packetHandlerSubscription += packetHandler;
        public void UnSubscribePacketHandler(P2PHandler packetHandler) => packetHandlerSubscription -= packetHandler;
        public void CreateSession(P2PCreate handle) => Layer.CreateSession(handle);
        public void JoinSession(P2PJoin handle) => Layer.JoinSession(handle);
        public void Disconnect() => Layer.Disconnect();
        public void Disconnect(P2PConnection connection) => Layer.Disconnect(connection);
        public void Send(P2PConnection connection, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable) => Send(connection.connectionId, packet, sendMode);
        public void Send(string connectionId, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable) => Layer.SendPacketToTarget(connectionId, packet, sendMode);
        public void SendToAll(P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable) => Layer.SendPacketToAll(packet, sendMode);
        public void HandleMessageFrom(P2PConnection sender, P2PPacket message) => packetHandlerSubscription?.Invoke(sender, message);
    }
}
