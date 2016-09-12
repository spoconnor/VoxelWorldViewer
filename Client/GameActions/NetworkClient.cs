using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Sean.WorldClient.GameObjects.GameItems;
using Sean.WorldClient.GameObjects.Units;
using Sean.WorldClient.Hosts.Ui;
using Sean.WorldClient.Hosts.World;
using Sean.Shared;
using System.Text;
using Sean.Shared.Comms;

namespace Sean.WorldClient.GameActions
{
    internal static class NetworkClient
    {
        internal static IPAddress ServerIp { get; private set; }
        internal static ushort ServerPort { get; private set; }
        internal static TcpClient TcpClient;
        private static NetworkStream _tcpStream;
        internal static bool AcceptPackets;

        /// <summary>Timer used for sending player info to the server periodically.</summary>
        private static System.Timers.Timer _playerInfoTimer;

		internal static void Connect()//object sender, DoWorkEventArgs e)
        {
			IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8084);

			TcpClient client = new TcpClient();
			Console.WriteLine ("Connecting...");
			client.Connect(remoteEP);

			var connection = ClientConnection.CreateClientConnection(client, ProcessMessage);
			connection.StartClient();

            Settings.Launcher.UpdateProgressInvokable("Connected...", 0, 0);
		
			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		private static void ProcessMessage (Guid clientId, Message msg)
		{
			Console.WriteLine ("[ProcessMessage]");
		}


        #region Send
		public static void SendPing()
		{
			ClientConnection.BroadcastMessage(new Message()
				{
					Ping = new PingMessage()
					{
						Message = "Hi"
					}
				});
		}

		public static void SendMapRequest()
		{
			ClientConnection.BroadcastMessage(new Message()
				{
					MapRequest = new MapRequestMessage()
					{
						Coords = new Sean.Shared.ChunkCoords(100, 100)
					}
				});
		}

		/*
        private static Coords _prevCoords = new Coords(0, 0, 0);

        public static void SendPlayerLocation(Coords newCoords, bool forceSend = false)
        {
            var minDeltaToBeDiff = (Players.Count >= 5 ? Constants.BLOCK_SIZE / 4 : Constants.BLOCK_SIZE / 8);
            var minAngleToBeDiff = (Players.Count >= 5 ? Constants.PI_OVER_6 : Constants.PI_OVER_12);
            if (forceSend || Math.Abs(_prevCoords.Xf - newCoords.Xf) > minDeltaToBeDiff || Math.Abs(_prevCoords.Yf - newCoords.Yf) > minDeltaToBeDiff || Math.Abs(_prevCoords.Zf - newCoords.Zf) > minDeltaToBeDiff || Math.Abs(_prevCoords.Direction - newCoords.Direction) > minAngleToBeDiff || Math.Abs(_prevCoords.Pitch - newCoords.Pitch) > minAngleToBeDiff)
            {
                //new PlayerMove(newCoords, Game.Player.Id).Send();

                var chunk = WorldData.Chunks[newCoords];
                foreach (var gameItem in chunk.GameItems.Values)
                {
                    if (gameItem.Type == GameItemType.BlockItem && newCoords.GetDistanceExact(ref gameItem.Coords) <= 2)
                    {
                  //          new PickupBlockItem(Game.Player.Id, gameItem.Id).Send();
                    }
                }

                _prevCoords = newCoords;
            }
        }
		*/

		/*
        /// <summary>Send message to the server to add or remove a block. Block will be removed if the block type is Air.</summary>
        /// <param name="position">position to add the block</param>
        /// <param name="blockType">block type to add</param>
        public static void SendAddOrRemoveBlock(Position position, Block.BlockType blockType)
        {
            //if (!position.IsValidBlockLocation) return;
            if (blockType == Block.BlockType.Air) //remove block
            {
                if (position.Y == 0) { Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Error, "Cannot remove a block at the base of the world. Block cancelled.")); return; }
              //  new RemoveBlock(ref position).Send();
            }
            else //add block
            {
                var head = Game.Player.CoordsHead;
                if (Game.Player.Inventory[(int)blockType] <= 0) { Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Error, string.Format("No {0} in inventory.", blockType))); return; }
                if (Block.IsBlockTypeSolid(blockType) && (position.IsOnBlock(ref Game.Player.Coords) || position.IsOnBlock(ref head))) { Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Error, "Attempted to build solid block on self. Not smart. Block cancelled.")); return; }

                foreach (var player in Players.Values)
                {
                    head = player.CoordsHead;
                    if (!Block.IsBlockTypeSolid(blockType) || (!position.IsOnBlock(ref player.Coords) && !position.IsOnBlock(ref head))) continue;
                    Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Error, "Attempted to build solid block on other player. Not nice. Block cancelled.")); return;
                }
                //new AddBlock(ref position, blockType).Send();
                Game.Player.Inventory[(int)blockType]--;
            }
        }
		*/

        public static void Disconnect()
        {
			/*
            lock (TcpClient)
            {
                //new Disconnect(Game.Player.Id, "Quit").Send();
                if (TcpClient.Connected)
                {
                    _tcpStream.Close();
                    TcpClient.Close();
                }
            }
            */
        }
			
        #endregion

        internal static void HandleNetworkError(Exception ex)
        {
			/*
            lock (TcpClient)
            {
                if (TcpClient.Connected)
                {
                    TcpClient.GetStream().Close();
                    TcpClient.Close();
                }
            }

            var msg = string.Format("Disconnected from Server: {0}\n", ex.Message);
#if DEBUG
            msg += ex.StackTrace;
#endif
            if (Settings.Game == null) Utilities.Misc.MessageError(msg);
            if (Game.UiHost != null)
            {
                foreach (var line in msg.Split('\n')) Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Error, line));
            }
            Debug.WriteLine(msg);
*/
        }
    }
}
