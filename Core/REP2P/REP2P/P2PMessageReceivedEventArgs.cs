using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    public class P2PMessageReceivedEventArgs : EventArgs
    {      
        public readonly P2PConnection sender;
        public readonly int messageId;
        public readonly P2PMessage message;

        public P2PMessageReceivedEventArgs(P2PConnection _sender, int _messageId, P2PMessage _message)
        {
            sender = _sender;
            messageId = _messageId;
            message = _message;
        }
    }
}
