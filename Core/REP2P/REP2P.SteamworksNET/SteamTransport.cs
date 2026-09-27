using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace REP2P.SteamworksNET
{
    // https://github.com/RugbugRedfern/Mycelium-Networking-For-Content-Warning/blob/master/MyceliumNetworkingForCW/MyceliumNetwork.cs
    public class SteamTransport : P2PTransport
    {
        public const int CHANNEL = 420;
        public bool IsHost => IsConnected && SteamMatchmaking.GetLobbyOwner(CurrentLobby) == SteamUser.GetSteamID();
        public static CSteamID CurrentLobby;
        public bool IsConnected => CurrentLobby.IsValid();

        public P2PConnection Host => GetConnection(SteamMatchmaking.GetLobbyOwner(CurrentLobby));

        public event Action StartedGame;
        public event Action Connected;
        public event Action Disconnected;
        public event Action ConnectionFailed;
        public event Action<P2PConnection> ClientConnected;
        public event Action<P2PConnection> ClientDisconnected;
        public event P2PDataReceivedDelegate DataReceived;

        protected Callback<LobbyEnter_t> lobbyEntered;
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;
        protected Callback<LobbyDataUpdate_t> lobbyDataUpdate;

        public SteamTransport()
        {
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        }

        private void OnLobbyChatUpdate(LobbyChatUpdate_t param)
        {
            var steamID = new CSteamID(param.m_ulSteamIDUserChanged);

            switch ((EChatMemberStateChange)param.m_rgfChatMemberStateChange)
            {
                case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    ClientConnected?.Invoke(CreateConnection(steamID));
                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                    ClientDisconnected?.Invoke(GetConnection(steamID));
                    break;
            }
        }

        private void OnLobbyEnter(LobbyEnter_t param)
        {
            CurrentLobby = (CSteamID)param.m_ulSteamIDLobby;

            Connected?.Invoke();
        }

        private void OnLobbyCreated(LobbyCreated_t param)
        {
            if (param.m_eResult == EResult.k_EResultOK)
            {
                CurrentLobby = (CSteamID)param.m_ulSteamIDLobby;
                StartedGame?.Invoke();
            }
            else
            {
                ConnectionFailed?.Invoke();
            }  
        }

        public void Connect(string address, out P2PConnection connection, out string connectError)
        {
            P2PLogger.Log("Connecting!");
            try
            {
                var result = SteamMatchmaking.JoinLobby((CSteamID)ulong.Parse(address));
                connection = new SteamConnection(SteamUser.GetSteamID());
                connectError = "";
            }
            catch (Exception ex)
            {
                connection = null;
                connectError = ex.Message;
            }
        }

        public void StartGame(int maxConnections, P2PVisibility visibility = P2PVisibility.Public)
        {
            P2PLogger.Log("Starting Game!");
            ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic;
            switch (visibility)
            {
                case P2PVisibility.Public:
                    lobbyType = ELobbyType.k_ELobbyTypePublic;
                    break;
                case P2PVisibility.Private:
                    lobbyType = ELobbyType.k_ELobbyTypePrivate;
                    break;
                case P2PVisibility.FriendsOnly:
                    lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
                    break;
            }
            SteamMatchmaking.CreateLobby(lobbyType, maxConnections);
        }

        public void Poll()
        {
            SteamAPI.RunCallbacks();
            ReceiveMessages();
        }
        const int MAX_MESSAGES = 500;
        static IntPtr[] outMessages = new IntPtr[MAX_MESSAGES];
        public void ReceiveMessages()
        {
            int count = SteamNetworkingMessages.ReceiveMessagesOnChannel(CHANNEL, outMessages, MAX_MESSAGES);
            if (P2PNetwork.Debug && count > 0) P2PLogger.Log($"Inbound {count} Messages from SteamNetworkingMessages");
            for (int i = 0; i < count; i++)
            {
                IntPtr outMessage = outMessages[i];

                int messageSize = Marshal.SizeOf(outMessage);
                if (messageSize > Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend)
                {
                    P2PLogger.Error($"Ignored message because its size was above the max ({messageSize}/{Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend})");
                    SteamNetworkingMessage_t.Release(outMessages[i]);
                    continue;
                }

                SteamNetworkingMessage_t steamNetworkingMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(outMessage);

                CSteamID sender = steamNetworkingMessage.m_identityPeer.GetSteamID();

                // Ignore them if their steam id is null (happens sometimes idk)
                if (sender == null)
                {
                    SteamNetworkingMessage_t.Release(outMessages[i]);
                    continue;
                }

                // Ignore them if they're not in the lobby
                if (GetConnection(sender) == null)
                {
                    SteamNetworkingMessage_t.Release(outMessages[i]);
                    continue;
                }

                byte[] managedArray = new byte[steamNetworkingMessage.m_cbSize];
                Marshal.Copy(steamNetworkingMessage.m_pData, managedArray, 0, steamNetworkingMessage.m_cbSize);
                try
                {
                    DataReceived(GetConnection(sender), managedArray);
                }
                catch (Exception ex)
                {
                    P2PLogger.Error($"Error receiving data from {sender}: {ex.Message} {ex.StackTrace}");
                }

                SteamNetworkingMessage_t.Release(outMessages[i]);
            }
        }

        public SteamConnection GetConnection(CSteamID id) => (SteamConnection)P2PNetwork.Connections.Values.First(x => (x as SteamConnection).SteamID == id);
        public SteamConnection CreateConnection(CSteamID steamID)
        {
            P2PLogger.Log($"Creating Connection for {steamID.m_SteamID}");
            return new SteamConnection(steamID);
        }
        public void Disconnect()
        {
            P2PLogger.Log("Disconnecting!");
            SteamMatchmaking.LeaveLobby(CurrentLobby);
            CurrentLobby = CSteamID.Nil;
            Disconnected?.Invoke();
        }
    }
}
