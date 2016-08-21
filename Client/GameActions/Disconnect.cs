using System;
using System.Text;
using Sean.WorldClient.Hosts.Ui;

namespace Sean.WorldClient.GameActions
{
    internal class Disconnect : GameAction
    {
        public Disconnect()
        {
            DataLength = sizeof(int) + 30;
        }

        public Disconnect(int playerId, string reason) : this()
        {
            PlayerId = playerId;
            Reason = reason.Length > 30 ? reason.Substring(0, 30) : reason;
        }

        public override string ToString()
        {
            return string.Format("Disconnect ({0}): {1}", PlayerId, Reason);
        }

        internal override ActionType ActionType { get { return ActionType.Disconnect; } }
        /// <summary>Id of the player that disconnected.</summary>
        internal int PlayerId;
        internal string Reason;

        protected override void Queue()
        {
            base.Queue();
            Write(PlayerId);
            Write(Encoding.ASCII.GetBytes(Reason.PadRight(30)), 30);
        }

        internal override void Receive()
        {
            lock (TcpClient)
            {
                base.Receive();
                var bytes = ReadStream(DataLength);
                PlayerId = BitConverter.ToInt32(bytes, 0);
                Reason = Encoding.ASCII.GetString(bytes, sizeof(int), 30).TrimEnd();
            }

                if (PlayerId == -1 || (Game.Player != null && PlayerId == Game.Player.Id)) throw new Exception(Reason);
                
                GameObjects.Units.Player disconnectedPlayer;
                NetworkClient.Players.TryRemove(PlayerId, out disconnectedPlayer);
                if (disconnectedPlayer != null && Game.UiHost != null) Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Server, string.Format("{0} has disconnected: {1}", disconnectedPlayer.UserName, Reason)));    
        }
    }
}
