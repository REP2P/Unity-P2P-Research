using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    /// <summary>
    /// Method for when you receive a message
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    public delegate void P2PMessageHandler(P2PConnection sender, P2PMessage message);
}
