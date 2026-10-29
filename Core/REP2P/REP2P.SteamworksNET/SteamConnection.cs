using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace REP2P.SteamworksNET
{
    public class SteamConnection : P2PConnection
    {
        public override bool isHost => SteamMatchmaking.GetLobbyOwner(SteamTransport.CurrentLobby) == steamId;

        public override bool isConnected => steamId.IsValid();

        public CSteamID steamId { get; protected set; }

        internal SteamConnection(CSteamID id) {
            steamId = id; 
            P2PLogger.Log($"Created Connection for {steamId.m_SteamID}");
        }
        protected override void Send(byte[] data, P2PSendMode sendMode)
        {
            SteamNetworkingIdentity id = new SteamNetworkingIdentity();
            id.SetSteamID(steamId);
            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pData = pinnedArray.AddrOfPinnedObject();
            int sendFlags = 0;
            switch (sendMode)
            {
                case P2PSendMode.Unreliable:
                    sendFlags = Constants.k_nSteamNetworkingSend_Unreliable;
                    break;
                case P2PSendMode.Reliable:
                    sendFlags = Constants.k_nSteamNetworkingSend_Reliable;
                    break;
            }
            var result = SteamNetworkingMessages.SendMessageToUser(ref id, pData, (uint)data.Length, sendFlags | Constants.k_nSteamNetworkingSend_AutoRestartBrokenSession, SteamTransport.CHANNEL);
            if (result != EResult.k_EResultOK)
            {
                P2PLogger.Error($"Error sending message to user {steamId}: {result}");
            }
            pinnedArray.Free();
        }
    }
}
