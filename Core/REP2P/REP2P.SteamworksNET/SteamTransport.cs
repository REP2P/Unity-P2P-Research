using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace REP2P.SteamworksNET
{
    // https://github.com/RugbugRedfern/Mycelium-Networking-For-Content-Warning/blob/master/MyceliumNetworkingForCW/MyceliumNetwork.cs
    public class SteamTransport : P2PTransport
    {
        public const int CHANNEL = 420;
        public bool IsHost => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public event EventHandler Listened;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler ConnectionFailed;
        public event EventHandler<P2PConnection> ClientConnected;
        public event EventHandler<P2PConnection> ClientDisconnected;

        public Dictionary<CSteamID, SteamConnection> connections = new Dictionary<CSteamID, SteamConnection>();

        public void Connect(string address, out P2PConnection connection, out string connectError)
        {
            var result = SteamMatchmaking.JoinLobby((CSteamID)ulong.Parse(address));
            connection = new SteamConnection(SteamUser.GetSteamID());
            connectError = result.ToString();
        }

        public void Listen(int maxConnections, P2PVisibility visibility = P2PVisibility.Public)
        {
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

            for (int i = 0; i < count; i++)
            {
                IntPtr outMessage = outMessages[i];

                int messageSize = Marshal.SizeOf(outMessage);
                if (messageSize > P2PMessage.MaxSize)
                {
                    P2PLogger.Error($"Ignored message because its size was above the max ({messageSize}/{P2PMessage.MaxSize})");
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
                if (!connections.ContainsKey(sender))
                {
                    SteamNetworkingMessage_t.Release(outMessages[i]);
                    continue;
                }

                byte[] managedArray = new byte[steamNetworkingMessage.m_cbSize];
                Marshal.Copy(steamNetworkingMessage.m_pData, managedArray, 0, steamNetworkingMessage.m_cbSize);

                var message = new P2PMessage(managedArray);

                try
                {
                    HandleMessage(message, sender);
                }
                catch (Exception ex)
                {
                    P2PLogger.Error($"Error executing RPC from {sender} ({destination}): {ex.Message} {ex.StackTrace}");
                }

                SteamNetworkingMessage_t.Release(outMessages[i]);
            }
        }
    }
}
