using System;
using Hexpoint.Blox.Hosts.World;

namespace Hexpoint.Blox.GameActions
{
    internal class PlayerMove : GameAction
    {
        public PlayerMove()
        {
            DataLength = Coords.SIZE + sizeof(int);
        }

        public PlayerMove(Coords coords, int playerId) : this()
        {
            Coords = coords;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return String.Format("PlayerMove {0} d{1:f1} p{2:f1}", Coords, Coords.Direction, Coords.Pitch);
        }

        internal override ActionType ActionType { get { return ActionType.PlayerMove; } }
        public Coords Coords;
        public int PlayerId;

        protected override void Queue()
        {
            base.Queue();
            Write(ref Coords);
            Write(PlayerId);
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var bytes = ReadStream(DataLength);
                    Coords = new Coords(bytes, 0);
                    PlayerId = BitConverter.ToInt32(bytes, Coords.SIZE);
                }

                //gm: this assignment will be roughly 3x slower for ConcurrentDictionary, however is worth it for simpler code, less bugs and some performance gains for not having to lock while iterating
                //see: http://www.albahari.com/threading/part5.aspx#_Concurrent_Collections
                NetworkClient.Players[PlayerId].Coords = Coords;
        }
    }
}
