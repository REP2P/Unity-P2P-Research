using System;
using System.Collections.Generic;
using LiteNetLib;

namespace REP2P.LiteNetLib
{
    [Obsolete("Use this to test P2P locally on your machine or LAN, this could provide a huge security risk if used remotelly")]
    public class LiteNetLibLayer : P2PLayer
    {
        public NetManager netManager;
        public EventBasedNetListener listener;
        public void Connect(string address, P2PConnect handle)
        {
            LiteNetLibConnect connect = (LiteNetLibConnect)handle;
            netManager.Connect(address, connect.port, connect.key);
        }

        public void Deinitialize()
        {
            Disconnect();
            netManager = null;
            listener = null;
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect(P2PConnection connection)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, P2PConnection> GetConnections()
        {
            throw new NotImplementedException();
        }

        public P2PConnection GetOwner()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            listener = new EventBasedNetListener();
            netManager = new NetManager(listener);
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsHost()
        {
        }

        public void SendPacketToAll(P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable)
        {
            throw new NotImplementedException();
        }

        public void SendPacketToTarget(string connectionId, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable)
        {
        }

        public void Start(int maxConnections, P2PStart handle)
        {
            LiteNetLibStart start = (LiteNetLibStart)handle;
            netManager.Start(start.port);
        }

        public void Tick()
        {
            netManager.PollEvents();
        }
    }
}
