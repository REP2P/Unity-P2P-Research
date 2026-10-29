using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace REP2P.SteamworksNET
{
    // The source code of https://github.com/RugbugRedfern/Mycelium-Networking-For-Content-Warning/ is used partially under the Attribution-NonCommercial-ShareAlike 4.0 International license
    public class SteamTransport : P2PTransport
    {
        public const int CHANNEL = 420;
        public bool isHost => isConnected && SteamMatchmaking.GetLobbyOwner(CurrentLobby) == SteamUser.GetSteamID();
        public static CSteamID CurrentLobby;
        public bool isConnected => CurrentLobby.IsValid();

        public P2PConnection host => GetConnection(SteamMatchmaking.GetLobbyOwner(CurrentLobby));

        public event Action created;
        public event Action<P2PConnection> joined;
        public event Action disconnected;
        public event Action connectionFailed;
        public event Action<P2PConnection> clientConnected;
        public event Action<P2PConnection> clientDisconnected;
        public event P2PDataReceivedDelegate dataReceived;

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
                    clientConnected?.Invoke(CreateConnection(steamID));
                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                    clientDisconnected?.Invoke(GetConnection(steamID));
                    break;
            }
        }

        private void OnLobbyEnter(LobbyEnter_t param)
        {
            CurrentLobby = (CSteamID)param.m_ulSteamIDLobby;
            int length = SteamMatchmaking.GetNumLobbyMembers(CurrentLobby);
            for (int i = 0; i < length; i++)
            {
                CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobby, i);
                if (memberId != SteamUser.GetSteamID())
                {
                    clientConnected?.Invoke(CreateConnection(memberId));
                }
            }
            joined?.Invoke(CreateConnection(SteamUser.GetSteamID()));
        }

        private void OnLobbyCreated(LobbyCreated_t param)
        {
            if (param.m_eResult == EResult.k_EResultOK)
            {
                CurrentLobby = (CSteamID)param.m_ulSteamIDLobby;
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
                var result = SteamMatchmaking.JoinLobby((CSteamID)ulong.Parse(address));
                connection = new SteamConnection(SteamUser.GetSteamID());    
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

        public void InitializeSession(int maxConnections, P2PVisibility visibility = P2PVisibility.Public)
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
            if (P2PNetwork.debug && count > 0) P2PLogger.Log($"Inbound {count} Messages from SteamNetworkingMessages");
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
                    dataReceived(GetConnection(sender), managedArray);
                }
                catch (Exception ex)
                {
                    P2PLogger.Error($"Error receiving data from {sender}: {ex.Message} {ex.StackTrace}");
                }

                SteamNetworkingMessage_t.Release(outMessages[i]);
            }
        }

        public SteamConnection GetConnection(CSteamID id) => (SteamConnection)P2PNetwork.connections.Values.First(x => (x as SteamConnection).steamId == id);
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
            disconnected?.Invoke();
        }

        public void SetSessionData(string key, string value)
        {
            SteamMatchmaking.SetLobbyData(CurrentLobby, key, value);
        }

        public string GetSessionData(string key)
        {
            return SteamMatchmaking.GetLobbyData(CurrentLobby, key);
        }
    }
}
