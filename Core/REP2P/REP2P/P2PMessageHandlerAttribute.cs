// This file uses code from Riptide by Tom Weiland licensed under MIT
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

using System;

namespace REP2P
{
    /// <summary>
    /// High Level Message Handler, Attach this to a static method which follows <see cref="P2PMessageHandler"/> principle
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class P2PMessageHandlerAttribute : Attribute
    {
        /// <summary>
        /// The ID of the Message
        /// </summary>
        public readonly int MessageId;
        /// <summary>
        /// High Level Message Handler, Attach this to a static method which follows <see cref="P2PMessageHandler"/> principle
        /// </summary>
        public P2PMessageHandlerAttribute(ushort messageId)
        {
            MessageId = messageId;
        }
    }
}
