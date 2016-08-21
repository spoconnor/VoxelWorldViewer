using System;
using Sean.WorldClient.GameObjects.GameItems;
using Sean.WorldClient.Hosts.World;

namespace Sean.WorldClient.GameActions
{
    internal class RemoveBlock : GameAction
    {
        public RemoveBlock()
        {
            DataLength = Position.SIZE;
        }

        public RemoveBlock(ref Position position) : this()
        {
            Position = position;
        }

        public override string ToString()
        {
            return String.Format("RemoveBlock {0}", Position);
        }

        internal override ActionType ActionType { get { return ActionType.RemoveBlock; } }
        public Position Position;

        protected override void Queue()
        {
            base.Queue();
            Write(ref Position);
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var bytes = ReadStream(DataLength);
                    Position = new Position(bytes, 0);
                }

            var existingBlock = Position.GetBlock(); //store the existing block before we overwrite it
            WorldData.PlaceBlock(Position, Block.BlockType.Air);

            //if destroying a block, create an item
            BlockItem newBlockItem = null;
                if (!existingBlock.IsTransparent)
                {
                    var temp = Position.ToCoords();
                    newBlockItem = new BlockItem(ref temp, existingBlock.Type);
                }

        }
    }
}
