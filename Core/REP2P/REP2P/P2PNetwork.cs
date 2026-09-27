using System;
using System.Collections.Generic;

namespace REP2P
{
    public static class P2PNetwork
    {
        public static void Initialize(P2PTransport transport)
        {
            instance = transport;
            instance.ClientConnected += (a, b) => Connections.Add(b.Id, b);
            instance.ClientDisconnected += (a, b) => Connections.Remove(b.Id);
        }

        private static P2PTransport instance;
        public static readonly Dictionary<int, P2PConnection> Connections = new Dictionary<int, P2PConnection>();
        public static void SendToAll(P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true)
        {
            foreach(var connection in Connections.Values)
            {
                connection.Send(message, sendMode, shouldRelease);
            }
            if(shouldRelease) message.Dispose();
        }
        public static void Send(int toClient, P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true) => Connections[toClient].Send(message, sendMode, shouldRelease);
        public static void Send(P2PConnection toClient, P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true) => Send(toClient.Id, message, sendMode, shouldRelease);
    }
}
