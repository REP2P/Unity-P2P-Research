using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;

namespace REP2P.Facepunch.Steamworks
{
    public class FacepunchSteamworksTransport : P2PTransport
    {
        public const int CHANNEL = 420;
        public bool isHost => isConnected && CurrentLobby.IsOwnedBy(SteamClient.SteamId);
        public static Lobby CurrentLobby;
        private bool _isConnected;
        public bool isConnected => _isConnected;

        public P2PConnection host => connections[CurrentLobby.Owner.Id];
        public readonly Dictionary<SteamId, FacepunchSteamworksConnection> connections = new Dictionary<SteamId, FacepunchSteamworksConnection>();
        public event Action created;
        public event Action<P2PConnection> joined;
        public event Action disconnected;
        public event Action connectionFailed;
        public event Action<P2PConnection> clientConnected;
        public event Action<P2PConnection> clientDisconnected;
        public event P2PDataReceivedDelegate dataReceived;
        private bool runCallbacks;
        public FacepunchSteamworksTransport(bool runCallbacks = false)
        {
            this.runCallbacks = runCallbacks;
            _isConnected = false;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEnter;
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberKicked += (lobby, friend, _) => OnLobbyMemberDisconnected(lobby, friend);
            SteamMatchmaking.OnLobbyMemberBanned += (lobby, friend, _) => OnLobbyMemberDisconnected(lobby, friend);

            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
            SteamNetworking.OnP2PSessionRequest += (id) => SteamNetworking.AcceptP2PSessionWithUser(id);
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            var steamID = friend.Id;
            if (friend.Id != SteamClient.SteamId) clientConnected?.Invoke(CreateConnection(steamID));
        }

        private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            var steamID = friend.Id;
            if (friend.Id != SteamClient.SteamId) {
                connections[steamID]._isConnected = false;
                clientDisconnected?.Invoke(connections[steamID]);
                connections.Remove(steamID);
            }
        }

        private void OnLobbyEnter(Lobby lobby)
        {
            _isConnected = true;
            CurrentLobby = lobby;
            int length = lobby.MemberCount;
            foreach(var friend in lobby.Members)
            {
                if (friend.Id != SteamClient.SteamId)
                {
                    clientConnected?.Invoke(CreateConnection(friend.Id));
                }
            }
            joined?.Invoke(CreateConnection(SteamClient.SteamId));
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (result == Result.OK)
            {
                CurrentLobby = lobby;
                created?.Invoke();
            }
            else
            {
                connectionFailed?.Invoke();
            }
        }

        public P2PConnectionError Connect(string address)
        {
            P2PLogger.Log("Connecting!");
            try
            {
                var result = SteamMatchmaking.JoinLobbyAsync(ulong.Parse(address));
                return P2PConnectionError.None;
            }
            catch (Exception ex)
            {
                P2PLogger.Exception(ex);
                return P2PConnectionError.Unknown;
            }
        }

        public async void InitializeSession(int maxConnections, P2PVisibility visibility = P2PVisibility.Public)
        {
            P2PLogger.Log("Starting Game!");
            var lobbyTask = SteamMatchmaking.CreateLobbyAsync(maxConnections);
            await lobbyTask;
            if (!lobbyTask.Result.HasValue) return;
            var lobby = lobbyTask.Result.Value;
            switch (visibility)
            {
                case P2PVisibility.Public:
                     lobby.SetPublic();
                    break;
                case P2PVisibility.Private:
                    lobby.SetPrivate();
                    break;
                case P2PVisibility.FriendsOnly:
                    lobby.SetFriendsOnly();
                    break;
                case P2PVisibility.Invisible:
                    lobby.SetInvisible();
                    break;
            }
        }

        public void Poll()
        {
            if(runCallbacks)SteamClient.RunCallbacks();
            ReceiveMessages();
        }
        public void ReceiveMessages()
        {
            while (SteamNetworking.IsP2PPacketAvailable())
            {
                var packet = SteamNetworking.ReadP2PPacket();
                if (packet.HasValue)
                {
                    var sender = packet.Value.SteamId;
                    var data = packet.Value.Data;
                    try
                    {
                        dataReceived(connections[sender], data);
                    }
                    catch (Exception ex)
                    {
                        P2PLogger.Error($"Error receiving data from {sender}: {ex.Message} {ex.StackTrace}");
                    }

                }
            }
        }

        public FacepunchSteamworksConnection CreateConnection(SteamId steamID)
        {
            P2PLogger.Log($"Creating Connection for {steamID.Value}");
            connections.Add(steamID, new FacepunchSteamworksConnection(steamID) { _isConnected = true });
            return connections[steamID];
        }
        public void Disconnect()
        {
            P2PLogger.Log("Disconnecting!");
            connections.Clear();
            CurrentLobby.Leave();
            _isConnected = false;
            disconnected?.Invoke();
        }

        public void SetSessionData(string key, string value)
        {
            CurrentLobby.SetData(key, value);
        }

        public string GetSessionData(string key)
        {
            return CurrentLobby.GetData(key);
        }
    }
}
