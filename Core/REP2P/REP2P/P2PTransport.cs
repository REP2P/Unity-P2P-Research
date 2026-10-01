using System;
using System.Collections.Generic;

namespace REP2P
{
    /// <summary>
    /// P2P Transport template
    /// </summary>
    public interface P2PTransport
    {
        /// <summary>
        /// invokes when a session starts
        /// </summary>
        public event Action Created;
        /// <summary>
        /// invokes when connected
        /// </summary>
        public event Action<P2PConnection> Joined;
        /// <summary>
        /// invokes when disconnected
        /// </summary>
        public event Action Disconnected;
        /// <summary>
        /// invokes when connection fails
        /// </summary>
        public event Action ConnectionFailed;  
        /// <summary>
        /// invokes when remote client connected
        /// </summary>
        public event Action<P2PConnection> ClientConnected;
        /// <summary>
        /// invokes when remote client disconnected
        /// </summary>
        public event Action<P2PConnection> ClientDisconnected;
        /// <summary>
        /// invokes when data received
        /// </summary>
        public event P2PDataReceivedDelegate DataReceived;

        /// <summary>
        /// are you host?
        /// </summary>
        public bool IsHost { get; }
        /// <summary>
        /// are you connected
        /// </summary>
        public bool IsConnected { get; }
        /// <summary>
        /// host's connection
        /// </summary>
        public P2PConnection Host { get; }
        /// <summary>
        /// starts the game
        /// </summary>
        /// <param name="maxConnections">max connections</param>
        /// <param name="visibility">how visible</param>
        public void CreateSession(int maxConnections, P2PVisibility visibility = P2PVisibility.Public);
        /// <summary>
        /// Joins a session
        /// </summary>
        /// <param name="address">end address</param>
        /// <param name="connection">your connection</param>
        /// <param name="connectError">your connection error</param>
        public void JoinSession(string address, out P2PConnection connection, out string connectError);

        /// <summary>
        /// Updates the network transport state
        /// </summary>
        public void Poll();
        /// <summary>
        /// Disconnects you from the session
        /// </summary>
        public void Disconnect();

        /// <summary>
        /// Sets a data for the session
        /// </summary>
        /// <param name="key">the key of the data</param>
        /// <param name="value">the value of the data</param>
        public void SetSessionData(string key, string value);
        /// <summary>
        /// Returns the data from the session
        /// </summary>
        /// <param name="key">the key of the data</param>
        public string GetSessionData(string key);
    }
}
