using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REP2P.Layers.Steam
{
    public class SteamLayer : NetworkLayer
    {
        private Network network;
        private Lobby lobby;
        public async void CreateServer(NetworkCreateServer baseHandle)
        {
            SteamCreateServer handle = (SteamCreateServer)baseHandle;
            Task<Lobby?> task = SteamMatchmaking.CreateLobbyAsync(handle.maxMembers);
            await task;
            if (task.IsCompleted)
            {
                if (task.Result.HasValue)
                {
                    lobby = task.Result.Value;
                    switch (handle.privacy)
                    {
                        case SteamLobbyPrivacy.Public:
                            lobby.SetPublic();
                            break;
                        case SteamLobbyPrivacy.Private:
                            lobby.SetPrivate();
                            break;
                        case SteamLobbyPrivacy.FriendsOnly:
                            lobby.SetFriendsOnly();
                            break;
                        case SteamLobbyPrivacy.Invisible:
                            lobby.SetInvisible();
                            break;
                    }
                    lobby.SetJoinable(handle.isJoinable);
                }
            }
        }

        public void Deinitialize()
        {
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
            SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;
        }

        public void Disconnect()
        {
            lobby.Leave();
            connections.Clear();
        }

        public NetworkConnection[] GetConnections()
        {
            return connections.Values.ToArray();
        }

        private Dictionary<SteamId, NetworkConnection> connections = new Dictionary<SteamId, NetworkConnection>();

        public void Initialize(Network network)
        {
            this.connections = new Dictionary<SteamId, NetworkConnection>();
            this.network = network;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
        }

        private void OnLobbyMemberDataChanged(Lobby lobby, Friend friend)
        {
            if(connections.ContainsKey(friend.Id))
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
            connections.Remove(friend.Id);
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            this.lobby = lobby;
            connections.Add(friend.Id, new NetworkConnection { connectionId = friend.ToString(), isHost = lobby.IsOwnedBy(friend.Id) });
        }

        public async void JoinServer(NetworkJoinServer baseHandle)
        {
            SteamJoinServer handle = (SteamJoinServer)baseHandle;
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
            return connections[lobby.Owner.Id];
        }
    }
}
