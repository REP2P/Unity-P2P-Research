using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    /// <summary>
    /// event args when you get a message
    /// </summary>
    public class P2PMessageReceivedEventArgs : EventArgs
    {      
        /// <summary>
        /// message sender
        /// </summary>
        public readonly P2PConnection sender;
        /// <summary>
        /// message id from message
        /// </summary>
        public readonly int messageId;
        /// <summary>
        /// message sent
        /// </summary>
        public readonly P2PMessage message;
        /// <summary>
        ///  event args when you get a message
        /// </summary>
        /// <param name="_sender">message sender</param>
        /// <param name="_messageId">message id</param>
        /// <param name="_message">message sent</param>
        public P2PMessageReceivedEventArgs(P2PConnection _sender, int _messageId, P2PMessage _message)
        {
            sender = _sender;
            messageId = _messageId;
            message = _message;
        }
    }
}
