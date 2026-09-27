using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace REP2P
{
    /// <summary>
    /// Remote P2P Connection that receives messages
    /// </summary>
    public abstract class P2PConnection
    {
        /// <summary>
        /// Their Id
        /// </summary>
        public int Id { get; internal set; }
        /// <summary>
        /// Are they the host?
        /// </summary>
        public abstract bool IsHost { get; }
        /// <summary>
        /// Are they still with us?
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// Sends a message to that connection
        /// </summary>
        /// <param name="message">your message to them</param>
        /// <param name="sendMode">is it reliable or nah</param>
        /// <param name="shouldRelease">disposes the message after use</param>
        public void Send(P2PMessage message, P2PSendMode sendMode, bool shouldRelease = true)
        {
            if (P2PNetwork.Debug) P2PLogger.Log($"Sending Message {message.messageId} to Client {Id}");
            Send(message.ToArray(), sendMode);
            message.Dispose();
        }
        /// <summary>
        /// low level message to that connection
        /// </summary>
        /// <param name="data">message into a byte array</param>
        /// <param name="sendMode">is the packet reliable</param>
        protected abstract void Send(byte[] data, P2PSendMode sendMode);
    }
}
