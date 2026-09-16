using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REP2P.Steam
{
    public class SteamLayer : P2PLayer
    {
        private Lobby? lobby;
        public async void Start(int maxMembers, P2PStart baseHandle)
        {
            SteamStart handle = (SteamStart)baseHandle;
            Task<Lobby?> task = SteamMatchmaking.CreateLobbyAsync(maxMembers);
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

        public P2PConnection[] GetConnections()
        {
            return connections.Values.ToArray();
        }

        private Dictionary<SteamId, P2PConnection> connections = new Dictionary<SteamId, P2PConnection>();

        public void Initialize()
        {
            connections = new Dictionary<SteamId, P2PConnection>();
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
                    P2PNetwork.OnCreatedSession?.Invoke();
                    break;
            }
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            isConnected = true;
            this.lobby = lobby;
            P2PNetwork.OnConnected?.Invoke();
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
            P2PNetwork.OnClientConnected?.Invoke(connections[friend.Id]);
            connections.Remove(friend.Id);
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            isConnected = true;
            this.lobby = lobby;
            P2PConnection connection = new P2PConnection { connectionId = friend.ToString(), isHost = lobby.IsOwnedBy(friend.Id) };
            connections.Add(friend.Id, connection);
            P2PNetwork.OnClientConnected?.Invoke(connection);
        }

        public async void Connect(string address, P2PConnect baseHandle)
        {
            SteamConnect handle = (SteamConnect)baseHandle;
            Task<Lobby?> task = SteamMatchmaking.JoinLobbyAsync(ulong.Parse(address));
            await task;
            if(task.IsCompleted)
            {
                if (task.Result.HasValue)
                {
                    lobby = task.Result.Value;
                }
            }
        }

        public void SendPacketToAll(P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable)
        {
            foreach (var connection in connections.Keys)
            {
                SendPacketToTarget(connection.ToString(), packet, sendMode);
            }
        }

        public void SendPacketToTarget(string connectionId, P2PPacket packet, P2PSend sendMode = P2PSend.Unreliable)
        {
            SteamId steamId = ulong.Parse(connectionId);
            byte[] data = P2PPacket.ToBytes(packet);
            SteamNetworking.SendP2PPacket(steamId, data, sendType:ToP2PSend(sendMode));
        }
        public Steamworks.P2PSend ToP2PSend(P2PSend sendMode)
        {
            switch (sendMode)
            {
                case P2PSend.Unreliable:
                    return Steamworks.P2PSend.Unreliable;
                case P2PSend.Reliable:
                    return Steamworks.P2PSend.Reliable;
                default:
                    return Steamworks.P2PSend.Unreliable;
            }
        }
        public void Tick()
        {
            SteamClient.RunCallbacks();
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
            P2PNetwork.HandleMessageFrom(connections[steamId], P2PPacket.FromBytes(data));
        }

        public P2PConnection GetOwner()
        {
            return connections[lobby.Value.Owner.Id];
        }
        private bool isConnected;
        public bool IsHost() => lobby.Value.IsOwnedBy(SteamClient.SteamId);

        public bool IsConnected() => isConnected;

        public P2PSession GetSessionState()
        {
            return new P2PSession() { connectionCount = lobby.Value.MemberCount, maxConnections = lobby.Value.MaxMembers };
        }

        public void Disconnect(P2PConnection connection)
        {
            throw new Exception("Valve does not support remotely disconnecting other members");
        }
    }
}
