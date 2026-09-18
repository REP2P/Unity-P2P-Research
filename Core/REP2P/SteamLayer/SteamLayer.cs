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
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
            SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;
            Disconnect();
        }

        private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            throw new NotImplementedException();
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            throw new NotImplementedException();
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            this.lobby = lobby;
        }

        public void Disconnect()
        {
            lobby?.Leave();
            connections.Clear();
        }

        public string[] GetConnections() => connections.Values.ToArray();

        private Dictionary<SteamId, string> connections = new Dictionary<SteamId, string>();

        public void Initialize()
        {
            connections = new Dictionary<SteamId, string>();
            SteamNetworking.AllowP2PPacketRelay(true);
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
        }

        private void OnP2PSessionRequest(SteamId id)
        {
            SteamNetworking.AcceptP2PSessionWithUser(id);
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
                string id = connection.Value.ToString();
                SendPacketToTarget(id, packet, sendMode);
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

        public string GetHost()
        {
            return connections[lobby.Value.Owner.Id];
        }
        public bool IsHost() => lobby.Value.IsOwnedBy(SteamClient.SteamId.Value);

        public bool IsConnected() => lobby.HasValue && lobby.Value.Id.IsValid;

        public string GetLocalConnection() => SteamClient.SteamId.Value.ToString();
    }
}
