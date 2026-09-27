using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    /// <summary>
    /// Received the data event arg
    /// </summary>
    public class P2PDataReceivedEventArgs : EventArgs
    {      
        /// <summary>
        /// Who sent the data
        /// </summary>
        public readonly P2PConnection sender;
        /// <summary>
        /// The data sent
        /// </summary>
        public readonly byte[] data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_sender">who sent the data</param>
        /// <param name="_data">the data sent</param>
        public P2PDataReceivedEventArgs(P2PConnection _sender, byte[] _data)
        {
            sender = _sender;
            data = _data;
        }
    }
}
