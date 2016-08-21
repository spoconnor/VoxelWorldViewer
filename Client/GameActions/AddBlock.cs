using System;
using Sean.WorldClient.Hosts.World;

namespace Sean.WorldClient.GameActions
{
    internal class AddBlock : GameAction
    {
        public AddBlock()
        {
            DataLength = Position.SIZE + sizeof(ushort); //coords + block type
        }

        public AddBlock(ref Position position, Block.BlockType blockType) : this()
        {
            if (blockType == Block.BlockType.Air) throw new Exception("You can't place air, use RemoveBlock");
            Position = position;
            BlockType = blockType;
        }

        public override string ToString()
        {
            return String.Format("AddBlock {0} {1}", BlockType, Position);
        }

        internal override ActionType ActionType { get { return ActionType.AddBlock; } }
        public Position Position;
        public Block.BlockType BlockType;

        protected override void Queue()
        {
            base.Queue();
            Write(ref Position);
            Write((ushort)BlockType);
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var bytes = ReadStream(DataLength);
                    Position = new Position(bytes, 0);
                    BlockType = (Block.BlockType)BitConverter.ToUInt16(bytes, Position.SIZE);
                }

            WorldData.PlaceBlock(Position, BlockType);
        }
    }
}