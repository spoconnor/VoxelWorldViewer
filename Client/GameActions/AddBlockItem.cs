﻿using System;
using Sean.WorldClient.GameObjects.GameItems;
using Sean.WorldClient.Hosts.World;
using OpenTK;

namespace Sean.WorldClient.GameActions
{
    internal class AddBlockItem : GameAction
    {
        public AddBlockItem()
        {
            DataLength = Coords.SIZE + Vector3.SizeInBytes + sizeof(ushort) + sizeof(int); //coords + velocity + block type + item ID
        }

        public AddBlockItem(ref Coords coords, ref Vector3 velocity, Block.BlockType blockType, int gameObjectId = -1) : this()
        {
            Coords = coords;
            Velocity = velocity;
            BlockType = blockType;
            GameObjectId = gameObjectId;
        }

        public override string ToString()
        {
            return String.Format("AddBlockItem P{0} V{1} {2}", Coords, Velocity, BlockType);
        }

        internal override ActionType ActionType { get { return ActionType.AddBlockItem; } }
        public Coords Coords;
        public Vector3 Velocity;
        public Block.BlockType BlockType;
        public int GameObjectId;

        protected override void Queue()
        {
            base.Queue();
            Write(ref Coords);
            Write(ref Velocity);
            Write((ushort)BlockType);
            Write(GameObjectId);
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var bytes = ReadStream(DataLength);
                    Coords = new Coords(bytes, 0);
                    Velocity = new Vector3(BitConverter.ToSingle(bytes, sizeof(float) * 5),
                                           BitConverter.ToSingle(bytes, sizeof(float) * 6),
                                           BitConverter.ToSingle(bytes, sizeof(float) * 7));
                    BlockType = (Block.BlockType)BitConverter.ToUInt16(bytes, Coords.SIZE + Vector3.SizeInBytes);
                    GameObjectId = BitConverter.ToInt32(bytes, Coords.SIZE + Vector3.SizeInBytes + sizeof(ushort));
                }

            //add the new block item to the chunk game items (note: constructor adds the item to the collection)
            var newBlockItem = new BlockItem(ref Coords, BlockType, Velocity, GameObjectId);
        }
    }
}
