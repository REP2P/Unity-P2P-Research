using Steamworks;

namespace REP2P.Facepunch.Steamworks
{
    public class FacepunchSteamworksConnection : P2PConnection
    {
        public override bool isHost => FacepunchSteamworksTransport.CurrentLobby.IsOwnedBy(steamId);

        internal bool _isConnected;
        public override bool isConnected => _isConnected;

        public SteamId steamId { get; protected set; }

        internal FacepunchSteamworksConnection(SteamId id)
        {
            steamId = id;
            P2PLogger.Log($"Created Connection for {steamId.Value}");
        }

        protected override void Send(byte[] data, P2PSendMode sendMode)
        {
            SteamNetworking.SendP2PPacket(steamId, data, sendType: sendMode == P2PSendMode.Reliable ? P2PSend.Reliable : P2PSend.Unreliable);
        }
    }
}