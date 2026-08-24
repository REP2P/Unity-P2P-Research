using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    // https://github.com/RugbugRedfern/Mycelium-Networking-For-Content-Warning/blob/master/MyceliumNetworkingForCW/Stream.cs
    // https://github.com/tom-weiland/tcp-udp-networking/blob/master/UnityGameServer/Assets/Scripts/Stream.cs
    public class BitStream : IDisposable
    {
        // Max size of a Stream, in bytes
        public static int MaxSize { get; set; } = ushort.MaxValue;

        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;

        private BitStream()
        {
            buffer = new List<byte>();
            readPos = 0;
        }
        /// <summary>Creates a new Stream.</summary>
        public static BitStream Create()
        {
            return new BitStream();
        }
        /// <summary>Creates a new Stream with data.</summary>
        public static BitStream Create(byte[] data)
        {
            return new BitStream().SetBytes(data);
        }

        #region Functions
        /// <summary>Sets the Stream's content and prepares it to be read.</summary>
        /// <param name="data">The bytes to add to the Stream.</param>
        public BitStream SetBytes(byte[] data)
        {
            buffer.AddRange(data);
            readableBuffer = buffer.ToArray();
            return this;
        }

        /// <summary>Inserts the length of the Stream's content at the start of the buffer.</summary>
        public BitStream WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the Stream at the very beginning
            return this;
        }

        /// <summary>Gets the Stream's content in array form.</summary>
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public NetworkPacket ToPacket(int packetId)
        {
            return NetworkPacket.Create(packetId, ToArray());
        }

        /// <summary>Gets the length of the Stream's content.</summary>
        public int Length()
        {
            return buffer.Count; // Return the length of buffer
        }

        /// <summary>Gets the length of the unread data contained in the Stream.</summary>
        public int UnreadLength()
        {
            return Length() - readPos; // Return the remaining length (unread)
        }

        /// <summary>Resets the Stream instance to allow it to be reused.</summary>
        /// <param name="shouldReset">Whether or not to reset the Stream.</param>
        public BitStream Reset(bool shouldReset = true)
        {
            if (shouldReset)
            {
                buffer.Clear(); // Clear buffer
                readableBuffer = null;
                readPos = 0; // Reset readPos
            }
            else
            {
                readPos -= 4; // "Unread" the last read int
            }
            return this;
        }
        #endregion

        #region Write Data
        /// <summary>Adds a byte to the Stream.</summary>
        /// <param name="value">The byte to add.</param>
        public BitStream Write(byte value)
        {
            buffer.Add(value);
            return this;
        }
        /// <summary>Adds an array of bytes to the Stream.</summary>
        /// <param name="value">The byte array to add.</param>
        public BitStream Write(byte[] value)
        {
            Write(value.Length);
            buffer.AddRange(value);
            return this;
        }
        /// <summary>Adds an array of bytes to the Stream.</summary>
        /// <param name="value">The byte array to add.</param>
        public BitStream Write(bool[] value)
        {
            Write(value.Length);
            byte[] bytes = new byte[value.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = value[i] ? (byte)1 : (byte)0;
            }
            buffer.AddRange(bytes);
            return this;
        }
        /// <summary>Adds a short to the Stream.</summary>
        /// <param name="value">The short to add.</param>
        public BitStream Write(short value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Adds a ushort to the Stream.</summary>
        /// <param name="value">The ushort to add.</param>
        public BitStream Write(ushort value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Adds an int to the Stream.</summary>
        /// <param name="value">The int to add.</param>
        public BitStream Write(int value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Adds a uint to the Stream.</summary>
        /// <param name="value">The uint to add.</param>
        public BitStream Write(uint value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Inserts the given int at the start of the Stream (after the header).</summary>
        /// <param name="value">The int to insert.</param>
        public BitStream InsertInt(int value)
        {
            buffer.InsertRange(1, BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Adds a long to the Stream.</summary>
        /// <param name="value">The long to add.</param>
        public BitStream Write(long value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Adds a ulong to the Stream.</summary>
        /// <param name="value">The ulong to add.</param>
        public BitStream Write(ulong value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Adds a float to the Stream.</summary>
        /// <param name="value">The float to add.</param>
        public BitStream Write(float value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Adds a bool to the Stream.</summary>
        /// <param name="value">The bool to add.</param>
        public BitStream Write(bool value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
            return this;
        }
        /// <summary>Adds a string to the Stream.</summary>
        /// <param name="value">The string to add.</param>
        public BitStream Write(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes.Length); // Add the length of the byte array to the Stream
            buffer.AddRange(bytes); // Add the string itself
            return this;
        }
        #endregion

        #region Read Data
        /// <summary>Reads a byte from the Stream.</summary>
        public byte ReadByte()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte value = readableBuffer[readPos]; // Get the byte at readPos' position
                                                      // If moveReadPos is true
                readPos += 1; // Increase readPos by 1
                return value; // Return the byte
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>Reads an array of bytes from the Stream.</summary>
        public byte[] ReadBytes()
        {
            int length = ReadInt();

            if (length == 0)
                return new byte[0];

            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte[] value = buffer.GetRange(readPos, length).ToArray(); // Get the bytes at readPos' position with a range of length
                                                                           // If moveReadPos is true
                readPos += length; // Increase readPos by length
                return value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        public bool[] ReadBools()
        {
            int length = ReadInt();
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                bool[] value = new bool[length];
                for (int i = 0; i < length; i++)
                {
                    byte current = buffer[readPos + i];
                    value[i] = current == 1;
                }

                readPos += length; // Increase readPos by length
                return value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'bool[]'!");
            }
        }


        /// <summary>Reads a short from the Stream.</summary>
        public short ReadShort()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                short value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
                                                                             // If moveReadPos is true and there are unread bytes
                readPos += 2; // Increase readPos by 2
                return value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>Reads a ushort from the Stream.</summary>
        public ushort ReadUShort()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                ushort value = BitConverter.ToUInt16(readableBuffer, readPos); // Convert the bytes to a short
                                                                               // If moveReadPos is true and there are unread bytes
                readPos += 2; // Increase readPos by 2
                return value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>Reads an int from the Stream.</summary>
        public int ReadInt()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                int value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
                                                                           // If moveReadPos is true
                readPos += 4; // Increase readPos by 4
                return value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>Reads a uint from the Stream.</summary>
        public uint ReadUInt()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                uint value = BitConverter.ToUInt32(readableBuffer, readPos); // Convert the bytes to an uint
                                                                             // If moveReadPos is true
                readPos += 4; // Increase readPos by 4
                return value; // Return the uint
            }
            else
            {
                throw new Exception("Could not read value of type 'uint'!");
            }
        }

        /// <summary>Reads a long from the Stream.</summary>
        public long ReadLong()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                long value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
                readPos += 8; // Increase readPos by 8
                return value; // Return the long
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>Reads a ulong from the Stream.</summary>
        public ulong ReadULong()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                ulong value = BitConverter.ToUInt64(readableBuffer, readPos); // Convert the bytes to a long
                readPos += 8; // Increase readPos by 8
                return value; // Return the ulong
            }
            else
            {
                throw new Exception("Could not read value of type 'ulong'!");
            }
        }

        /// <summary>Reads a float from the Stream.</summary>
        public float ReadFloat()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                float value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
                readPos += 4; // Increase readPos by 4
                return value; // Return the float
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>Reads a bool from the Stream.</summary>
        public bool ReadBool()
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                bool value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
                readPos += 1; // Increase readPos by 1
                return value; // Return the bool
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>Reads a string from the Stream.</summary>
        public string ReadString()
        {
            try
            {
                int length = ReadInt(); // Get the length of the byte array (NOT the length of the string)
                string value = Encoding.UTF8.GetString(readableBuffer, readPos, length); // Convert the bytes to a string
                if (value.Length > 0)
                {
                    // string is not empty
                    readPos += length; // Increase readPos by the length of the string
                }
                return value; // Return the string
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }
        #endregion

        public int GetSize()
        {
            return readableBuffer.Length;
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}