using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace REP2P.SteamworksNET
{
    public class SteamConnection : P2PConnection
    {
        public override int Id { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        public override bool IsHost => throw new NotImplementedException();

        public override bool IsConnected => throw new NotImplementedException();

        public CSteamID SteamID { get; protected set; }
        internal SteamConnection(CSteamID id) {
            SteamID = id;
        }
        protected override void Send(byte[] data, P2PSendMode sendMode)
        {
            SteamNetworkingIdentity id = new SteamNetworkingIdentity();
            id.SetSteamID(SteamID);
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
                P2PLogger.Error($"Error sending message to user {SteamID}: {result}");
            }
            pinnedArray.Free();
        }
    }
}
