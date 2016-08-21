using System;
using Hexpoint.Blox.GameObjects.GameItems;
using Hexpoint.Blox.Hosts.World;

namespace Hexpoint.Blox.GameActions
{
    internal class RemoveBlockItem : GameAction
    {
        public RemoveBlockItem()
        {
            DataLength = sizeof(int) + sizeof(bool); //GameObjectId int + IsDecayed bool
        }

        public RemoveBlockItem(int gameObjectId, bool isDecayed) : this()
        {
            GameObjectId = gameObjectId;
            IsDecayed = isDecayed;
        }

        public override string ToString()
        {
            return String.Format("RemoveBlockItem {0}", GameObjectId);
        }

        internal override ActionType ActionType { get { return ActionType.RemoveBlockItem; } }
        public int GameObjectId;
        public bool IsDecayed;

        protected override void Queue()
        {
            base.Queue();
            Write(GameObjectId);
            Write(IsDecayed);
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var bytes = ReadStream(DataLength);
                    GameObjectId = BitConverter.ToInt32(bytes, 0);
                    IsDecayed = BitConverter.ToBoolean(bytes, sizeof(int));
                }

                //if the item decayed then dont play a sound, otherwise play the sound of another player picking up the item
                //if (!IsDecayed) Sounds.Audio.PlaySound(Sounds.SoundType.ItemPickup, ref blockItem.Coords);
        }
    }
}
