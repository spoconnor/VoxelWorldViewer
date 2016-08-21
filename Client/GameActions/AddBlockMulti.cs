using System;
using System.Collections.Generic;
using Sean.WorldClient.Hosts.World;

namespace Sean.WorldClient.GameActions
{
    internal class AddBlockMulti : GameAction
    {
        public override string ToString()
        {
            return String.Format("AddBlockMulti ({0} blocks)", Blocks.Count);
        }

        internal override ActionType ActionType { get { return ActionType.AddBlockMulti; } }
        internal readonly List<AddBlock> Blocks = new List<AddBlock>();
        
        protected override void Queue()
        {
            DataLength = sizeof(int) + Blocks.Count * (Position.SIZE + sizeof(ushort)); //num blocks + each block
            base.Queue();
            Write(Blocks.Count);
            foreach (var block in Blocks)
            {
                Write(ref block.Position);
                Write((ushort)block.BlockType);
            }
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var blockCount = BitConverter.ToInt32(ReadStream(sizeof(int)), 0);
                    
                    for (var i = 0; i < blockCount; i++)
                    {
                        var bytes = ReadStream(Position.SIZE + sizeof(ushort));
                        var position = new Position(bytes, 0);
                        var blockType = (Block.BlockType)BitConverter.ToUInt16(bytes, Position.SIZE);
                        Blocks.Add(new AddBlock(ref position, blockType));
                    }
            }

            Settings.ChunkUpdatesDisabled = true;
            foreach (var addBlock in Blocks) WorldData.PlaceBlock(addBlock.Position, addBlock.BlockType);
            Settings.ChunkUpdatesDisabled = false;

        }
    }
}