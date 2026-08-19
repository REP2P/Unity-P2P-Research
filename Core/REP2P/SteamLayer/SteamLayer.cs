using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REP2P.Steam
{
    public class SteamLayer : NetworkLayer
    {
        private Network network;
        private Lobby? lobby;
        public async void CreateSession(CreateSessionHandle baseHandle)
        {
            SteamCreateSessionHandle handle = (SteamCreateSessionHandle)baseHandle;
            Task<Lobby?> task = SteamMatchmaking.CreateLobbyAsync(handle.maxMembers);
            await task;
            if (task.IsCompleted)
            {
                if (task.Result.HasValue)
                {
                    lobby = task.Result;
                    switch (handle.privacy)
                    {
                        case SteamLobbyPrivacy.Public:
                            lobby?.SetPublic();
                            break;
                        case SteamLobbyPrivacy.Private:
                            lobby?.SetPrivate();
                            break;
                        case SteamLobbyPrivacy.FriendsOnly:
                            lobby?.SetFriendsOnly();
                            break;
                        case SteamLobbyPrivacy.Invisible:
                            lobby?.SetInvisible();
                            break;
                    }
                    lobby?.SetJoinable(handle.isJoinable);
                }
            }
        }

        public void Deinitialize()
        {
            SteamNetworking.AllowP2PPacketRelay(false);
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberDataChanged -= OnLobbyMemberDataChanged;
            SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
            SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;
            Disconnect();
        }

        public void Disconnect()
        {
            isConnected = false;
            lobby?.Leave();
            connections.Clear();
        }

        public NetworkConnection[] GetConnections()
        {
            return connections.Values.ToArray();
        }

        private Dictionary<SteamId, NetworkConnection> connections = new Dictionary<SteamId, NetworkConnection>();

        public void Initialize(Network network)
        {
            connections = new Dictionary<SteamId, NetworkConnection>();
            this.network = network;
            SteamNetworking.AllowP2PPacketRelay(true);
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            isConnected = true;
            this.lobby = lobby;
            switch (result) {
                case Result.OK:
                    network.OnCreatedServer?.Invoke();
                    break;
            }
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            isConnected = true;
            this.lobby = lobby;
            network.OnConnected?.Invoke();
        }

        private void OnLobbyMemberDataChanged(Lobby lobby, Friend friend)
        {
            isConnected = true;
            this.lobby = lobby;
            if (connections.ContainsKey(friend.Id))
            {
                connections[friend.Id] = connections[friend.Id];
            }
        }

        private void OnP2PSessionRequest(SteamId id)
        {
            SteamNetworking.AcceptP2PSessionWithUser(id);
        }

        private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            isConnected = true;
            this.lobby = lobby;
            network.OnClientConnected?.Invoke(connections[friend.Id]);
            connections.Remove(friend.Id);
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            isConnected = true;
            this.lobby = lobby;
            NetworkConnection connection = new NetworkConnection { connectionId = friend.ToString(), isHost = lobby.IsOwnedBy(friend.Id) };
            connections.Add(friend.Id, connection);
            network.OnClientConnected?.Invoke(connection);
        }

        public async void JoinSession(JoinSessionHandle baseHandle)
        {
            SteamJoinSessionHandle handle = (SteamJoinSessionHandle)baseHandle;
            Task<Lobby?> task = SteamMatchmaking.JoinLobbyAsync(handle.lobby);
            await task;
            if(task.IsCompleted)
            {
                if (task.Result.HasValue)
                {
                    lobby = task.Result.Value;
                }
            }
        }

        public void SendPacketToAll(NetworkPacket message, PacketSendMode sendMode = PacketSendMode.Unreliable)
        {
            foreach (var connection in connections.Keys)
            {
                SendPacketToTarget(connection.ToString(), message, sendMode);
            }
        }

        public void SendPacketToTarget(string connectionId, NetworkPacket message, PacketSendMode sendMode = PacketSendMode.Unreliable)
        {
            SteamId steamId = ulong.Parse(connectionId);
            byte[] data = message.ToBytes();
            SteamNetworking.SendP2PPacket(steamId, data, sendType:ToP2PSend(sendMode));
        }
        public P2PSend ToP2PSend(PacketSendMode sendMode)
        {
            switch (sendMode)
            {
                case PacketSendMode.Unreliable:
                    return P2PSend.Unreliable;
                case PacketSendMode.Reliable:
                    return P2PSend.Reliable;
                default:
                    return P2PSend.Unreliable;
            }
        }
        public void Tick()
        {
            while (SteamNetworking.IsP2PPacketAvailable())
            {
                var packet = SteamNetworking.ReadP2PPacket();
                if (packet.HasValue)
                {
                    HandleMessageFrom(packet.Value.SteamId, packet.Value.Data);
                }
            }
        }

        private void HandleMessageFrom(SteamId steamId, byte[] data)
        {
            network.HandleMessageFrom(connections[steamId], NetworkPacket.FromBytes(data));
        }

        public NetworkConnection GetOwner()
        {
            return connections[lobby.Value.Owner.Id];
        }
        private bool isConnected;
        public bool IsHost() => lobby.Value.IsOwnedBy(SteamClient.SteamId);

        public bool IsConnected() => isConnected;

        public SessionState GetSessionState()
        {
            return new SessionState() { connectionCount = lobby.Value.MemberCount, maxConnections = lobby.Value.MaxMembers };
        }

        public void Disconnect(NetworkConnection connection)
        {
            throw new Exception("Valve does not support remotely disconnecting other members");
        }
    }
}
