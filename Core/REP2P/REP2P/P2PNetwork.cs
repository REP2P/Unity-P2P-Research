using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// This file uses code from Riptide by Tom Weiland licensed under MIT
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

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
        private static int _nextClientId = 0;
        private static int nextClientId { get => _nextClientId++; }
        /// <summary>
        /// Message Received Event
        /// </summary>
        public static event EventHandler<P2PMessageReceivedEventArgs> MessageReceived;
        /// <summary>
        /// All the Connections in the session
        /// </summary>
        public static readonly Dictionary<int, P2PConnection> Connections = new Dictionary<int, P2PConnection>();
        /// <summary>
        /// Your local connection
        /// </summary>
        public static P2PConnection Connection { get; private set; } = null;
        /// <summary>
        /// Allow Debug Logs
        /// </summary>
        public static bool Debug { get; set; }
        /// <summary>
        /// Initialize the P2P Network
        /// </summary>
        /// <param name="transport">Your selected transport</param>
        public static void Initialize(P2PTransport transport)
        {
            instance = null;
            _nextClientId = 0;
            instance = transport;
            instance.ClientConnected += Instance_ClientConnected;
            instance.ClientDisconnected += Instance_ClientDisconnected;
            instance.Connected += Instance_Connected;
            instance.Disconnected += Instance_Disconnected;
            instance.DataReceived += Instance_DataReceived;
            CreateMessageHandlersDictionary();
        }

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
                    if (messageHandlers.ContainsKey(attribute.MessageId))
                    {
                        MethodInfo otherMethodWithId = messageHandlers[attribute.MessageId].GetMethodInfo();
                        P2PLogger.Error($"{attribute.MessageId} has a message handler already");
                    }
                    else
                        messageHandlers.Add(attribute.MessageId, (P2PMessageHandler)p2pMessageHandler);
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

        private static void Instance_Connected(object sender, EventArgs e)
        {
            P2PLogger.Log("Connected!");
        }
        private static void Instance_Disconnected(object sender, EventArgs e)
        {
            Connection = null;
            P2PLogger.Log("Disconnected!");
        }

        private static void Instance_ClientConnected(object sender, P2PConnection e)
        {
            e.Id = nextClientId;
            Connections.Add(e.Id, e);
            P2PLogger.Log($"Client {e.Id} Connected!");
        }

        private static void Instance_ClientDisconnected(object sender, P2PConnection e)
        {
            Connections.Remove(e.Id);
            P2PLogger.Log($"Client {e.Id} Disconnected!");
        }

        private static void Instance_DataReceived(object sender, P2PDataReceivedEventArgs e)
        {
            P2PMessage message = new P2PMessage(e.data);
            MessageReceived?.Invoke(sender, new P2PMessageReceivedEventArgs(e.sender, message.messageId, message));
            message.Dispose();
            if (Debug) P2PLogger.Log($"Received Message {message.messageId} from Client {e.sender.Id}");
        }
        /// <summary>
        /// Starts a session
        /// </summary>
        /// <param name="maxConnections">Max Connections to that session</param>
        /// <param name="visibility">the privacy of that session</param>
        public static void StartGame(int maxConnections = 10, P2PVisibility visibility = P2PVisibility.Public) => instance.StartGame(maxConnections, visibility);
        /// <summary>
        /// Connects to a session
        /// </summary>
        /// <param name="address">session address</param>
        public static void Connect(string address) {
            P2PConnection connection = null;
            instance.Connect(address, out connection, out string error);
            if(string.IsNullOrEmpty(error)) P2PLogger.Error(error);
            Connection = connection;
        }
        /// <summary>
        /// Send a message to every one in the session
        /// </summary>
        /// <param name="message">your message</param>
        /// <param name="sendMode">how reliable</param>
        /// <param name="shouldRelease">dispose after use</param>
        public static void SendToAll(P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true)
        {
            if (Debug) P2PLogger.Log($"Sending Message {message.messageId} to all clients");
            foreach (var connection in Connections.Values)
            {
                connection.Send(message, sendMode, shouldRelease);
            }
            if(shouldRelease) message.Dispose();
        }
        /// <summary>
        /// Send a message to a client
        /// </summary>
        /// <param name="target">your target</param>
        /// <param name="message">your message</param>
        /// <param name="sendMode">how reliable</param>
        /// <param name="shouldRelease">dispose after use</param>
        public static void Send(int target, P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true) => Connections[target].Send(message, sendMode, shouldRelease);
        /// <summary>
        /// Send a message to a client
        /// </summary>
        /// <param name="target">your target</param>
        /// <param name="message">your message</param>
        /// <param name="sendMode">how reliable</param>
        /// <param name="shouldRelease">dispose after use</param>
        public static void Send(P2PConnection target, P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true) => Send(target.Id, message, sendMode, shouldRelease);
    }
}
