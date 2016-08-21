using System;
using System.Net.Sockets;
using System.Text;
using Sean.WorldClient.GameObjects.Units;
using Sean.WorldClient.Hosts.Ui;
using Sean.WorldClient.Hosts.World;

namespace Sean.WorldClient.GameActions
{
    internal class Login : GameAction
    {
        public Login()
        {
            DataLength = sizeof(int) + 16 + 20 + Coords.SIZE;
        }

        public Login(int playerId, string userName) : this()
        {
            PlayerId = playerId;
            UserName = userName.Length > 16 ? userName.Substring(0, 16) : userName;
            if (Settings.VersionDisplay.Length > 20) throw new Exception("Version string cannot be more than 20 characters.");
            Version = Settings.VersionDisplay;
        }

        public override string ToString()
        {
            return string.Format("Connect ({0}) {1} v{2}", PlayerId, UserName, Version);
        }

		internal override CommsMessages.MsgType ActionType { get { return CommsMessages.MsgType.eLogin; } }
        internal int PlayerId;
        internal string UserName;
        internal string Version;

        protected override void Queue()
        {
            base.Queue();
            Write(PlayerId);
            Write(Encoding.ASCII.GetBytes(UserName.PadRight(16)), 16);
            Write(Encoding.ASCII.GetBytes(Version.PadRight(20)), 20);
            Write(ref Coords);
        }

        internal override void Receive()
        {
            lock (TcpClient)
            {
                base.Receive();
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

    }
}
