using System;
using System.Collections.Generic;

namespace REP2P
{
    public static class P2PNetwork
    {
        public static P2PTransport instance { get; private set; }
        private static int _nextClientId = 0;
        private static int nextClientId { get => _nextClientId++; }
        public static event EventHandler<P2PMessageReceivedEventArgs> MessageReceived;
        public static readonly Dictionary<int, P2PConnection> Connections = new Dictionary<int, P2PConnection>();
        public static bool Debug { get; set; }
        public static void Initialize(P2PTransport transport)
        {
            _nextClientId = 0;
            instance = transport;
            instance.ClientConnected += Instance_ClientConnected;
            instance.ClientDisconnected += Instance_ClientDisconnected;
            instance.Connected += Instance_Connected;
            instance.Disconnected += Instance_Disconnected;
            instance.DataReceived += Instance_DataReceived;
        }

        private static void Instance_Connected(object sender, EventArgs e)
        {
            P2PLogger.Log("Connected!");
        }
        private static void Instance_Disconnected(object sender, EventArgs e)
        {
            P2PLogger.Log("Disconnected!");
        }

        private static void Instance_ClientConnected(object sender, P2PConnection e)
        {
            e.Id = nextClientId;
            Connections.Add(e.Id, e);
            P2PLogger.Log($"Client {e.Id} Connected!");
        }

        private static void Instance_ClientDisconnected(object sender, P2PConnection e)
        {
            Connections.Remove(e.Id);
            P2PLogger.Log($"Client {e.Id} Disconnected!");
        }

        private static void Instance_DataReceived(object sender, P2PDataReceivedEventArgs e)
        {
            P2PMessage message = new P2PMessage(e.data);
            MessageReceived?.Invoke(sender, new P2PMessageReceivedEventArgs(e.sender, message.messageId, message));
            message.Dispose();
            if (Debug) P2PLogger.Log($"Received Message {message.messageId} from Client {e.sender.Id}");
        }

        public static void SendToAll(P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true)
        {
            if (Debug) P2PLogger.Log($"Sending Message {message.messageId} to all clients");
            foreach (var connection in Connections.Values)
            {
                connection.Send(message, sendMode, shouldRelease);
            }
            if(shouldRelease) message.Dispose();
        }
        public static void Send(int toClient, P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true) => Connections[toClient].Send(message, sendMode, shouldRelease);
        public static void Send(P2PConnection toClient, P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true) => Send(toClient.Id, message, sendMode, shouldRelease);
    }
}
