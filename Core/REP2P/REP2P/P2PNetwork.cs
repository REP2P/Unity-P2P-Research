using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace REP2P
{
    /// <summary>
    /// Class that handles the Connections/Data transfer between peers in a p2p mesh
    /// </summary>
    public static class P2PNetwork
    {
        /// <summary>
        /// The Network Transport
        /// </summary>
        public static P2PTransport instance { get; private set; }

        /// <summary>
        /// Message Received Event
        /// </summary>
        public static event P2PMessageReceivedDelegate messageReceived;
        /// <summary>
        /// All the Connections in the session
        /// </summary>
        public static readonly Dictionary<Guid, P2PConnection> connections = new Dictionary<Guid, P2PConnection>();
        /// <summary>
        /// Your local connection
        /// </summary>
        public static P2PConnection connection { get; private set; } = null;
        /// <summary>
        /// Allow Debug Logs
        /// </summary>
        public static bool debug { get; set; }
        /// <summary>
        /// Initialize the P2P Network
        /// </summary>
        /// <param name="transport">Your selected transport</param>
        public static void Initialize(P2PTransport transport)
        {
            instance = null;
            instance = transport;
            instance.clientConnected += Instance_ClientConnected;
            instance.clientDisconnected += Instance_ClientDisconnected;
            instance.created += Instance_Created;
            instance.joined += Instance_Joined;
            instance.disconnected += Instance_Disconnected;
            instance.dataReceived += Instance_DataReceived;
            CreateMessageHandlersDictionary();
        }

        /// <summary>
        /// Are you host?
        /// </summary>
        /// <returns>true if host</returns>
        public static bool isHost => instance.isHost;

        /// <summary>
        /// Are you host?
        /// </summary>
        /// <returns>true if host</returns>
        public static bool isConnected => instance.isConnected;

        private static Dictionary<int, P2PMessageHandler> messageHandlers;

        private static void CreateMessageHandlersDictionary()
        {
            MethodInfo[] methods = FindMessageHandlers();

            messageHandlers = new Dictionary<int, P2PMessageHandler>(methods.Length);
            foreach (MethodInfo method in methods)
            {
                P2PMessageHandlerAttribute attribute = method.GetCustomAttribute<P2PMessageHandlerAttribute>();

                if (!method.IsStatic)
                {
                    P2PLogger.Error($"Method {method.Name} from {method.DeclaringType} is not static, BLOCKED");
                    return;
                }

                Delegate p2pMessageHandler = Delegate.CreateDelegate(typeof(P2PMessageHandler), method, false);
                if (p2pMessageHandler != null)
                {
                    // It's a message handler for Server instances
                    if (messageHandlers.ContainsKey(attribute.messageId))
                    {
                        MethodInfo otherMethodWithId = messageHandlers[attribute.messageId].GetMethodInfo();
                        P2PLogger.Error($"{attribute.messageId} has a message handler already");
                    }
                    else
                        messageHandlers.Add(attribute.messageId, (P2PMessageHandler)p2pMessageHandler);
                }
            }
        }

        private static MethodInfo[] FindMessageHandlers()
        {
            string thisAssemblyName = Assembly.GetExecutingAssembly().GetName().FullName;
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a
                    .GetReferencedAssemblies()
                    .Any(n => n.FullName == thisAssemblyName)) // Get only assemblies that reference this assembly
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) // Include instance methods in the search so we can show the developer an error instead of silently not adding instance methods to the dictionary
                .Where(m => m.GetCustomAttributes(typeof(P2PMessageHandlerAttribute), false).Length > 0)
                .ToArray();
        }
        private static void Instance_Created()
        {
            P2PLogger.Log("Created Session!");
        }
        private static void Instance_Joined(P2PConnection connection)
        {
            connection.guid = Guid.NewGuid();
            P2PLogger.Log("Joined Session!");
        }
        private static void Instance_Disconnected()
        {
            connection = null;
            P2PLogger.Log("Disconnected!");
        }

        private static void Instance_ClientConnected(P2PConnection connection)
        {
            connection.guid = Guid.NewGuid();
            connections.Add(connection.guid, connection);
            P2PLogger.Log($"Client {connection.guid} Connected!");
        }

        private static void Instance_ClientDisconnected(P2PConnection connection)
        {
            connections.Remove(connection.guid);
            P2PLogger.Log($"Client {connection.guid} Disconnected!");
        }

        private static void Instance_DataReceived(P2PConnection sender, byte[] data)
        {
            NetDataReader reader = new NetDataReader(data);
            int messageId = reader.GetInt();
            messageReceived?.Invoke(sender, messageId, reader);
            if (debug) P2PLogger.Log($"Received Message {messageId} from Client {sender.guid}");
            reader.Clear();
        }
        /// <summary>
        /// Creates session
        /// </summary>
        /// <param name="maxConnections">Max Connections to the session</param>
        /// <param name="visibility">the privacy of that session</param>
        public static void InitializeSession(int maxConnections = 10, P2PVisibility visibility = P2PVisibility.Public) => instance.InitializeSession(maxConnections, visibility);
        /// <summary>
        /// Connects to a session
        /// </summary>
        /// <param name="address">session address</param>
        public static P2PConnectionError Connect(string address) {
            P2PConnection connection = null;
            var error = instance.Connect(address, out connection, out string connectError);
            if(string.IsNullOrEmpty(connectError) || error != P2PConnectionError.None) P2PLogger.Error(connectError);
            P2PNetwork.connection = connection;
            return error;
        }
        /// <summary>
        /// Send a message to every one in the session
        /// </summary>
        /// <param name="messageId">your message id</param>
        /// <param name="stream">your data stream</param>
        /// <param name="sendMode">how reliable</param>
        /// <param name="shouldRelease">dispose after use</param>
        public static void SendToAll(int messageId, NetDataWriter stream, P2PSendMode sendMode, bool shouldRelease = true)
        {
            if (debug) P2PLogger.Log($"Sending Message {messageId} to all clients");
            foreach (var connection in connections.Values)
            {
                connection.Send(messageId, stream, sendMode, shouldRelease);
            }
            if (shouldRelease) stream.Reset();
        }
        /// <summary>
        /// Send a message to a client
        /// </summary>
        ///         /// <param name="target">your target</param>
        /// <param name="messageId">your message id</param>
        /// <param name="stream">your data stream</param>
        /// <param name="sendMode">how reliable</param>
        /// <param name="shouldRelease">dispose after use</param>
        public static void Send(Guid target, int messageId, NetDataWriter stream, P2PSendMode sendMode, bool shouldRelease = true) => connections[target].Send(messageId, stream, sendMode, shouldRelease);
        /// <summary>
        /// Send a message to a client
        /// </summary>
        /// <param name="target">your target</param>
        /// <param name="messageId">your message id</param>
        /// <param name="stream">your data stream</param>
        /// <param name="sendMode">how reliable</param>
        /// <param name="shouldRelease">dispose after use</param>
        public static void Send(P2PConnection target, int messageId, NetDataWriter stream, P2PSendMode sendMode, bool shouldRelease = true) => target.Send(messageId, stream, sendMode, shouldRelease);

        /// <summary>
        /// Updates the Network, Put this in an update function or while loop
        /// </summary>
        public static void Poll() => instance.Poll();

        /// <summary>
        /// Disconnects from current session
        /// </summary>
        public static void Disconnect() => instance.Disconnect();
    }
}
