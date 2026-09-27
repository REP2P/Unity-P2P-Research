using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    public abstract class P2PConnection
    {
        public abstract int Id { get; protected set; }
        public abstract bool IsHost { get; }
        public abstract bool IsConnected { get; }

        public void Send(P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true)
        {
            message.ToArray();
            message.Dispose();
        }
        protected abstract void Send(byte[] data, P2PSendMode sendMode);
    }
}
