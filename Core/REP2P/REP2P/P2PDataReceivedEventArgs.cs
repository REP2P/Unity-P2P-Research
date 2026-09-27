using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    public class P2PDataReceivedEventArgs : EventArgs
    {      
        public readonly P2PConnection sender;
        public readonly byte[] data;

        public P2PDataReceivedEventArgs(P2PConnection _sender, byte[] _data)
        {
            sender = _sender;
            data = _data;
        }
    }
}
