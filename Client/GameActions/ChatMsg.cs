using System;
using System.Text;
using Sean.WorldClient.Hosts.Ui;

namespace Sean.WorldClient.GameActions
{
    internal class ChatMsg : GameAction
    {
        public ChatMsg(int fromPlayerId = 0, string message = null)
        {
            FromPlayerId = fromPlayerId;
            Message = message;
            DataLength = sizeof(int);
            if (message != null) DataLength += message.Length;
        }

        public override string ToString()
        {
            return String.Format("Chat ({0}): {1}", FromPlayerId, Message);
        }

        internal override ActionType ActionType { get { return ActionType.ChatMsg; } }
        public int FromPlayerId;
        public string Message;

        protected override void Queue()
        {
            Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Global, string.Format("<{0}> {1}", Game.Player.UserName, Message)));
            
            base.Queue();
            Write(FromPlayerId);
            Write(Encoding.ASCII.GetBytes(Message), Message.Length);
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var bytes = ReadStream(DataLength);
                    FromPlayerId = BitConverter.ToInt32(bytes, 0);
                    Message = Encoding.ASCII.GetString(bytes, sizeof(int), DataLength - sizeof(int));
                }

                //only plays for clients receiving a message because the sender doesnt receive their own message
                Sounds.Audio.PlaySound(Sounds.SoundType.Message);
                Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Global, string.Format("<{0}> {1}", NetworkClient.Players[FromPlayerId].UserName, Message)));

        }
    }
}