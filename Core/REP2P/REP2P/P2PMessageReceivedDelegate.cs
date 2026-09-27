using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    /// <summary>
    /// event args when you get a message
    /// </summary>
    public delegate void P2PMessageReceivedDelegate(P2PConnection sender, int messageId, P2PMessage message);

}
