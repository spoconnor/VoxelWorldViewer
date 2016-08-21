using System;
using System.Collections.Generic;
using Hexpoint.Blox.GameObjects.GameItems;
using Hexpoint.Blox.Hosts.World;

namespace Hexpoint.Blox.GameActions
{
    internal class PickupBlockItem : GameAction
    {
        public PickupBlockItem()
        {
            DataLength = sizeof(int) * 2; //player id, item id
        }

        public PickupBlockItem(int playerId, int gameObjectId) : this()
        {
            PlayerId = playerId;
            GameObjectId = gameObjectId;
        }

        public override string ToString()
        {
            return String.Format("PickupBlockItem Player {0} Obj {1}", PlayerId, GameObjectId);
        }

        internal override ActionType ActionType { get { return ActionType.PickupBlockItem; } }
        public int PlayerId;
        public int GameObjectId;

        protected override void Queue()
        {
            base.Queue();
            Write(PlayerId);
            Write(GameObjectId);
        }

        internal override void Send()
        {
            if (PendingPickups.Contains(GameObjectId)) return;
            base.Send();
            PendingPickups.Add(GameObjectId);
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var bytes = ReadStream(DataLength);
                    PlayerId = BitConverter.ToInt32(bytes, 0);
                    GameObjectId = BitConverter.ToInt32(bytes, sizeof(int));
                }

        }

        private readonly static List<int> PendingPickups = new List<int>(); //keep track of the items we've requested to pick up, avoid spamming the requests
    }
}
