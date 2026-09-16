using System;

namespace REP2P
{
    public static class P2PNetwork
    {
        public static P2PLayer Layer { get; private set; }
        public static bool IsHost => Layer.IsHost();
        public static bool IsConnected => Layer.IsConnected();
        public static P2PConnection Owner => Layer.GetOwner();
        public static P2PConnection[] Connections => Layer.GetConnections();
        public static P2PSession SessionState => Layer.GetSessionState();
        public static Action OnCreatedSession;
        public static Action OnConnected;
        public static Action OnDisconnected;
        public static Action<P2PConnection> OnClientConnected;
        public static Action<P2PConnection> OnClientDisconnected;
        private static P2PHandler packetHandlerSubscription;

        public static void Initialize(P2PLayer layer)
        {
            Layer = layer;
            Layer.Initialize();
        }
        public static void DeInitialize() => Layer.Deinitialize();
        public static void Update() => Layer.Tick();

        public static void SubscribePacketHandler(P2PHandler packetHandler) => packetHandlerSubscription += packetHandler;
        public static void UnSubscribePacketHandler(P2PHandler packetHandler) => packetHandlerSubscription -= packetHandler;
        public static void Start(int maxConnections, P2PStart handle) => Layer.Start(maxConnections, handle);
        public static void Connect(string address, P2PConnect handle) => Layer.Connect(address, handle);
        public static void Disconnect() => Layer.Disconnect();
        public static void Disconnect(P2PConnection connection) => Layer.Disconnect(connection);
        public static void Send(P2PConnection connection, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable) => Send(connection.connectionId, packet, sendMode);
        public static void Send(string connectionId, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable) => Layer.SendPacketToTarget(connectionId, packet, sendMode);
        public static void SendToAll(P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable) => Layer.SendPacketToAll(packet, sendMode);
        public static void HandleMessageFrom(P2PConnection sender, P2PPacket message) => packetHandlerSubscription?.Invoke(sender, message);
    }
}
