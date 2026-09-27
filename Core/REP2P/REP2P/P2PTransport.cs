using System;
using System.Collections.Generic;

namespace REP2P
{
    public interface P2PTransport
    {
        public event EventHandler Listened;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler ConnectionFailed;
        public event EventHandler<P2PConnection> ClientConnected;
        public event EventHandler<P2PConnection> ClientDisconnected;

        public bool IsHost { get; }
        public bool IsConnected { get; }
        public void Listen(int maxConnections, P2PVisibility visibility = P2PVisibility.Public);
        public void Connect(string address, out P2PConnection connection, out string connectError);

        public void Poll();
    }
}
