using System;
using System.Net.Sockets;
using Sean.WorldClient.Hosts.World;
using OpenTK;
using System.Text;
using Sean.Shared;

namespace Sean.WorldClient.GameActions
{
    internal abstract class GameAction
    {
        protected GameAction()
        {
            //multiplayer client doesnt always need to pass this because we know how to get it
            TcpClient = NetworkClient.TcpClient;
        }

        public TcpClient TcpClient { get; protected set; }
        public abstract override string ToString();
        internal int DataLength;
		internal abstract CommsMessages.MsgType ActionType { get; }

        #region Send
        private byte[] _byteQueue;
        private int _byteQueueIndex;
        private bool _isQueued;
        protected virtual void Queue()
        {
            _byteQueue = new byte[sizeof(ushort) + sizeof(int) + DataLength];
            Write(BitConverter.GetBytes((ushort)ActionType), sizeof(ushort));
            Write(BitConverter.GetBytes(DataLength), sizeof(int));
        }

        internal bool Immediate;
        internal virtual void Send()
        {
            if (!_isQueued)
            {
                Queue();
                if (_byteQueueIndex != _byteQueue.Length) throw new Exception(string.Format("{0} DataLength {1} + {2} but queued {3}", ActionType, sizeof(ushort) + sizeof(int), DataLength, _byteQueueIndex));

                _isQueued = true;
                return;
            }

            try
            {
                lock (TcpClient)
                {
                    TcpClient.GetStream().Write(_byteQueue, 0, _byteQueue.Length);
                }
            }
            catch (Exception ex)
            {
                NetworkClient.HandleNetworkError(ex);
                throw new ServerDisconnectException(ex);
            }
        }
			
		protected void SendMessage(CommsMessages.Message message, byte[] data)
		{
			var messageBytes = WriteMessage (message);

			//byte[] msg = Encoding.ASCII.GetBytes ("This is a test<EOF>");
			var msg = new byte[messageBytes.Length + data.Length];
			//msg[0] = (byte)((messageBytes.Length + data.Length)/256);
			//msg[1] = (byte)((messageBytes.Length + data.Length)%256);
			messageBytes.CopyTo(msg, 0);
			data.CopyTo(msg, messageBytes.Length);

			Write(msg, msg.Length);
		}

        private static CommsMessages.Message ReadMessage(byte[] data)
        {
            byte[] msgBuffer = new byte[data[0]];
            Array.Copy(data, 1, msgBuffer, 0, data[0]); // Skip length byte

            {
                var builder = new StringBuilder();
                for (int i = 0; i < data[0]; i++)
                {
                    builder.Append(msgBuffer[i].ToString());
                    builder.Append(",");
                }
                Console.WriteLine("{0}", builder.ToString());
            }
            var recv = CommsMessages.Message.ParseFrom(msgBuffer);
            var msgType = (CommsMessages.MsgType)recv.Msgtype;
            Console.WriteLine("Msg Type: {0}", msgType);
            return recv;
        }

        public static byte[] WriteMessage(CommsMessages.Message message)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                memoryStream.WriteByte(0); // reserve for length
                message.WriteTo(memoryStream);
                var messageBytes = memoryStream.ToArray();
                messageBytes[0] = (byte)(messageBytes.Length - 1); // ignore nul at end
                return messageBytes;
            }
        }

        protected void Write(byte[] buffer, int count)
        {
            Buffer.BlockCopy(buffer, 0, _byteQueue, _byteQueueIndex, count);
            _byteQueueIndex += count;
        }

        protected void Write(ref Position position)
        {
            Write(position.ToByteArray(), Position.SIZE);
        }

        protected void Write(ref Coords coords)
        {
            Write(coords.ToByteArray(), Coords.SIZE);
        }

        protected void Write(ref Vector3 vector)
        {
            Write(BitConverter.GetBytes(vector.X), sizeof(float));
            Write(BitConverter.GetBytes(vector.Y), sizeof(float));
            Write(BitConverter.GetBytes(vector.Z), sizeof(float));
        }

        protected void Write(bool x)
        {
            Write(BitConverter.GetBytes(x), sizeof(bool));
        }

        protected void Write(int x)
        {
            Write(BitConverter.GetBytes(x), sizeof(int));
        }

        protected void Write(ushort x)
        {
            Write(BitConverter.GetBytes(x), sizeof(ushort));
        }

        protected void Write(short x)
        {
            Write(BitConverter.GetBytes(x), sizeof(short));
        }

        protected void Write(float x)
        {
            Write(BitConverter.GetBytes(x), sizeof(float));
        }
        #endregion

        #region Receive
        internal virtual void Receive()
        {
            lock (TcpClient) //this will generally already be locked but not all actions override this method
            {
                DataLength = BitConverter.ToInt32(ReadStream(sizeof(int)), 0);
            }
        }

        /// <summary>Helper to ensure the requested amount is all read before we continue.</summary>
        protected byte[] ReadStream(int length)
        {
            var bytes = new byte[length];
            var bytesRead = 0;
            while (bytesRead < length)
            {
                bytesRead += TcpClient.GetStream().Read(bytes, bytesRead, length - bytesRead);
            }
            return bytes;
        }
        #endregion

    }
}
