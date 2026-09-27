using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace REP2P
{
    public abstract class P2PConnection
    {
        public int Id { get; internal set; }
        public abstract bool IsHost { get; }
        public abstract bool IsConnected { get; }

        public void Send(P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true)
        {
            if (P2PNetwork.Debug) P2PLogger.Log($"Sending Message {message.messageId} to Client {Id}");
            Send(message.ToArray(), sendMode);
            message.Dispose();
        }
        protected abstract void Send(byte[] data, P2PSendMode sendMode);
    }
}
