using Steamworks;
using Steamworks.Data;

namespace REP2P.Facepunch.Steamworks
{
    public class FacepunchSteamworksTransport : P2PTransport
    {
        public const int CHANNEL = 420;
        public bool isHost => isConnected && CurrentLobby.IsOwnedBy(SteamClient.SteamId);
        public static Lobby CurrentLobby;
        public bool isConnected => CurrentLobby.Id.IsValid;

        public P2PConnection host => GetConnection(CurrentLobby.Owner.Id);
        public readonly Dictionary<SteamId, FacepunchSteamworksConnection> connections = new();
        public event Action created;
        public event Action<P2PConnection> joined;
        public event Action disconnected;
        public event Action connectionFailed;
        public event Action<P2PConnection> clientConnected;
        public event Action<P2PConnection> clientDisconnected;
        public event P2PDataReceivedDelegate dataReceived;

        public FacepunchSteamworksTransport()
        {
            SteamMatchmaking.OnLobbyEntered += OnLobbyEnter;
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
            SteamNetworking.OnP2PSessionRequest += (id) => SteamNetworking.AcceptP2PSessionWithUser(id);
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            var steamID = friend.Id;
            clientConnected?.Invoke(CreateConnection(steamID));
        }

        private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            var steamID = friend.Id;
            clientDisconnected?.Invoke(GetConnection(steamID));
        }

        private void OnLobbyEnter(Lobby lobby)
        {
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

        public P2PConnectionError Connect(string address, out P2PConnection connection, out string connectError)
        {
            P2PLogger.Log("Connecting!");
            try
            {
                var result = SteamMatchmaking.JoinLobbyAsync(ulong.Parse(address));
                connection = CreateConnection(SteamClient.SteamId);
                connectError = "";
                return P2PConnectionError.None;
            }
            catch (Exception ex)
            {
                connection = null;
                connectError = ex.Message;
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
            SteamClient.RunCallbacks();
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
                        dataReceived(GetConnection(sender), data);
                    }
                    catch (Exception ex)
                    {
                        P2PLogger.Error($"Error receiving data from {sender}: {ex.Message} {ex.StackTrace}");
                    }

                }
            }
        }

        public FacepunchSteamworksConnection GetConnection(SteamId id) => (FacepunchSteamworksConnection)P2PNetwork.connections.Values.First(x => (x as FacepunchSteamworksConnection).steamId == id);
        public FacepunchSteamworksConnection CreateConnection(SteamId steamID)
        {
            P2PLogger.Log($"Creating Connection for {steamID.Value}");
            return new FacepunchSteamworksConnection(steamID);
        }
        public void Disconnect()
        {
            P2PLogger.Log("Disconnecting!");
            CurrentLobby.Leave();
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
