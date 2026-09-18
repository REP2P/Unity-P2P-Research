using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;

namespace REP2P.LiteNetLib
{
    [Obsolete("Use this to test P2P locally on your machine or LAN, this could provide a huge security risk if used remotelly")]
    public class LiteNetLibLayer : P2PLayer, INetEventListener
    {
        public const string connectKey = "P2P";
        private NetPeer? server;
        public NetManager? netManager;
        public void Connect(string address, P2PConnect handle)
        {
            netManager?.Start();
            LiteNetLibConnect connect = (LiteNetLibConnect)handle;
            netManager?.Connect(address, connect.port, connect.key);
        }

        public void Deinitialize()
        {
            Disconnect();
            netManager = null;
        }

        public void Disconnect()
        {
            netManager?.DisconnectPeer(netManager.FirstPeer);
        }


        public string[] GetConnections()
        {
            List<NetPeer> peers = new List<NetPeer>();
            netManager?.GetPeers(peers, ConnectionState.Connected);
            return netManager != null ? peers.Select(x => x.Id.ToString()).ToArray() : Array.Empty<string>();
        }

        public string GetHost()
        {
            throw new NotImplementedException();
        }

        public string GetLocalConnection()
        {
            return netManager != null ? netManager.FirstPeer.Id.ToString() : "";
        }

        public void Initialize()
        {
            netManager = new NetManager(this);
            netManager.AutoRecycle = true;
        }

        public bool IsConnected()
        {
            return netManager != null && netManager.IsRunning;
        }

        public bool IsHost()
        {
            return netManager != null && netManager.IsRunning && server == netManager.FirstPeer;
        }

        public void SendPacketToAll(P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable)
        {
            netManager?.SendToAll(P2PPacket.ToBytes(packet), 0, ToDeliveryMethod(sendMode));
        }

        public void SendPacketToTarget(string connectionId, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable)
        {
            netManager
        }

        public DeliveryMethod ToDeliveryMethod(P2PSend send) => send == P2PSend.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;

        public void Start(int maxConnections, P2PStart handle)
        {
            LiteNetLibStart start = (LiteNetLibStart)handle;
            netManager.Start(start.port);
        }

        public void Tick()
        {
            netManager.PollEvents();
        }

        public void OnPeerConnected(NetPeer peer)
        {
            server = peer;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey("p2p");
        }
    }
}
