using System;
using System.Net.Sockets;
using Sean.WorldClient.Hosts.World;
using OpenTK;
using System.Text;
using Sean.Shared;

namespace Sean.WorldClient.GameActions
{
    public class GameAction
    {
        public GameAction()
        {
        }

		public void SendLoginAction(string username, string password)
		{
	
		}

        public TcpClient TcpClient { get; protected set; }
        internal int DataLength;
		internal CommsMessages.MsgType ActionType { get; }


        #region Send
        private byte[] _byteQueue;
        private int _byteQueueIndex;
        private bool _isQueued;
		/*
		protected virtual void Queue()
        {
			_byteQueue = new byte[sizeof(ushort) + DataLength];
			Write(BitConverter.GetBytes(DataLength), sizeof(ushort));

			Write (PlayerId);
			Write (Encoding.ASCII.GetBytes (UserName.PadRight (16)), 16);
			Write (Encoding.ASCII.GetBytes (Version.PadRight (20)), 20);
			Write (ref Coords);
        }*/

        internal bool Immediate;
        internal void Send()
        {
            if (!_isQueued)
            {
                //Queue();
                if (_byteQueueIndex != _byteQueue.Length) throw new Exception(string.Format("{0} DataLength {1} + {2} but queued {3}", ActionType, sizeof(ushort) + sizeof(int), DataLength, _byteQueueIndex));

                _isQueued = true;
                return;
            }
        }
			/*
		internal override void Receive()
		{
			lock (TcpClient)
			{
				var bytes = ReadStream(DataLength);
				PlayerId = BitConverter.ToInt32(bytes, 0);
				UserName = Encoding.ASCII.GetString(bytes, sizeof(int), 16).TrimEnd();
				Version = Encoding.ASCII.GetString(bytes, sizeof(int) + 16, 20).TrimEnd();
				Coords = new Coords(bytes, sizeof(int) + 16 + 20);
			}

			//todo: include position in this packet?
			NetworkClient.Players.TryAdd(PlayerId, new Player(PlayerId, UserName, Coords)); //note: it is not possible for the add to fail on ConcurrentDictionary, see: http://www.albahari.com/threading/part5.aspx#_Concurrent_Collections
			if (Game.UiHost != null) //ui host will be null for a client that is launching the game
			{
				Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Server, string.Format("{0} has connected.", UserName)));
				Sounds.Audio.PlaySound(Sounds.SoundType.PlayerConnect);
			}
		}
*/
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
