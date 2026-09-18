using System;

namespace REP2P.WebRTC
{
    public class WebRTCLayer : P2PLayer
    {
        public void Connect(string address, P2PConnect handle)
        {
            throw new NotImplementedException();
        }

        public void Deinitialize()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public string[] GetConnections()
        {
            throw new NotImplementedException();
        }

        public string GetHost()
        {
            throw new NotImplementedException();
        }

        public string GetLocalConnection()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsHost()
        {
            throw new NotImplementedException();
        }

        public void SendPacketToAll(P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable)
        {
            throw new NotImplementedException();
        }

        public void SendPacketToTarget(string connectionId, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable)
        {
            throw new NotImplementedException();
        }

        public void Start(int maxConnections, P2PStart handle)
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }
    }
}
