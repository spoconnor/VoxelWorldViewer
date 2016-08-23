using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Sean.WorldClient.GameObjects.GameItems;
using Sean.WorldClient.GameObjects.Units;
using Sean.WorldClient.Hosts.Ui;
using Sean.Shared;

namespace Sean.WorldClient.Hosts.World
{
    /// <summary>
    /// World environment type. Integer value is saved in world settings XML, so these integer values cannot be changed without breaking existing worlds.
    /// Start at 1 so we can ensure this gets loaded properly and not defaulted to zero.
    /// </summary>
    internal enum WorldType : byte
    {
        Grass = 1,
        Winter = 2,
        Desert = 3
    }

    internal static class WorldData
    {
        static WorldData()
        {
        }

        #region Properties (Saved)
        internal static WorldType WorldType { get; set; }
        /// <summary>Original Raw Seed used to generate this world. Blank if no seed was used.</summary>
        internal static string RawSeed { get; set; }
        /// <summary>Original program version used when this world was generated.</summary>
        internal static string GeneratorVersion { get; set; }

        internal static int GameObjectIdSeq;
        internal static int NextGameObjectId
        {
            get { return System.Threading.Interlocked.Increment(ref GameObjectIdSeq); }
        }

        private static int _sizeInChunksX;
        /// <summary>Number of chunks in X direction that make up the world.</summary>
        internal static int SizeInChunksX
        {
            get { return _sizeInChunksX; }
            set
            {
                _sizeInChunksX = value;
                SizeInBlocksX = _sizeInChunksX * Chunk.CHUNK_SIZE;
            }
        }

        private static int _sizeInChunksZ;
        /// <summary>Number of chunks in Z direction that make up the world.</summary>
        internal static int SizeInChunksZ
        {
            get { return _sizeInChunksZ; }
            set
            {
                _sizeInChunksZ = value;
                SizeInBlocksZ = _sizeInChunksZ * Chunk.CHUNK_SIZE;
            }
        }

        /// <summary>Number of blocks in X direction that make up the world.</summary>
        internal static int SizeInBlocksX { get; private set; }

        /// <summary>Number of blocks in Z direction that make up the world.</summary>
        internal static int SizeInBlocksZ { get; private set; }

        #endregion

        #region Properties (Dynamic)
        /// <summary>True when the world has been completely loaded from disk for server and single player or when world has been completely received in multiplayer.</summary>
        public static bool IsLoaded { get; set; }
        public static Chunks Chunks;
        public static bool GenerateWithTrees;
        #endregion

        #region Lookup Functions
        /// <summary>Get a block using world coords.</summary>
        internal static Block GetBlock(ref Coords coords)
        {
            return Chunks[coords].Blocks[coords];
        }

        /// <summary>Get a block using world x,y,z. Use this overload to avoid constructing coords when they arent needed.</summary>
        /// <remarks>For example, this provided ~40% speed increase in the World.PropagateLight function compared to constructing coords and calling the above overload.</remarks>
        internal static Block GetBlock(int x, int y, int z)
        {
            return Chunks[x / Chunk.CHUNK_SIZE, z / Chunk.CHUNK_SIZE].Blocks[x % Chunk.CHUNK_SIZE, y, z % Chunk.CHUNK_SIZE];
        }

        /// <summary>Get light color of a block location by looking up its light strength and returning the color from the LightTable.</summary>
        /// <returns>0-255 color value</returns>
        internal static byte GetBlockLightColor(int x, int y, int z)
        {
            if (!IsValidBlockLocation(x, y, z)) return Lighting.LightTable[SkyHost.SunLightStrength]; //give full light coming from outside edge of the world
            return Lighting.LightTable[Math.Max(SkyLightMap[x, y, z] - (SkyHost.BRIGHTEST_SKYLIGHT_STRENGTH - SkyHost.SunLightStrength), ItemLightMap[x, y, z])];
        }

        /// <summary>Get light strength of a block location.</summary>
        /// <returns>0-15 strength value</returns>
        internal static byte GetBlockLightStrength(int x, int y, int z)
        {
            if (!IsValidBlockLocation(x, y, z)) return SkyHost.SunLightStrength; //give full light coming from outside edge of the world
            return (byte)Math.Max(SkyLightMap[x, y, z] - (SkyHost.BRIGHTEST_SKYLIGHT_STRENGTH - SkyHost.SunLightStrength), ItemLightMap[x, y, z]);
        }

        /// <summary>
        /// Is this position a valid block location. Includes blocks on the base of the world even though they cannot be removed.
        /// This is because the cursor can still point at them, they can still receive light, etc.
        /// Coords/Position structs have the same method. Use this one to avoid contructing coords/position when they arent needed. Large performance boost in some cases.
        /// </summary>
        internal static bool IsValidBlockLocation(int x, int y, int z)
        {
            return x >= 0 && x < SizeInBlocksX && y >= 0 && y < Chunk.CHUNK_HEIGHT && z >= 0 && z < SizeInBlocksZ;
        }

        internal static bool IsOnChunkBorder(int x, int z)
        {
            return x % Chunk.CHUNK_SIZE == 0 || z % Chunk.CHUNK_SIZE == 0 || x % Chunk.CHUNK_SIZE == Chunk.CHUNK_SIZE - 1 || z % Chunk.CHUNK_SIZE == Chunk.CHUNK_SIZE - 1;
        }

        internal static int GetHeightMapLevel(int x, int z)
        {
            return Chunks[x / Chunk.CHUNK_SIZE, z / Chunk.CHUNK_SIZE].HeightMap[x % Chunk.CHUNK_SIZE, z % Chunk.CHUNK_SIZE];
        }

        /// <summary>Check if any of 4 directly adjacent blocks receive direct sunlight. Uses the heightmap so that the server can also use this method. If the server stored light info then it could be used instead.</summary>
        internal static bool HasAdjacentBlockReceivingDirectSunlight(int x, int y, int z)
        {
            return (x < SizeInBlocksX - 1 && GetHeightMapLevel(x + 1, z) <= y) ||
                   (x > 0 && GetHeightMapLevel(x - 1, z) <= y) ||
                   (z < SizeInBlocksZ - 1 && GetHeightMapLevel(x, z + 1) <= y) ||
                   (z > 0 && GetHeightMapLevel(x, z - 1) <= y);
        }

        internal static bool IsValidStaticItemPosition(Position position) //cannot accept position by ref here
        {
            if (!IsValidBlockLocation(position.X, position.Y, position.Z))
            {
                Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Error, string.Format("Invalid item position.")));
                return false;
            }
            var chunk = Chunks[position];
            if (chunk.LightSources.Any(lightSource => position.IsOnBlock(ref lightSource.Value.Coords)) || chunk.Clutters.Any(clutter => position.IsOnBlock(ref clutter.Coords)))
            {
                Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.Error, string.Format("Item already exists on selected block.")));
                return false;
            }
            return true;
        }
        #endregion

        #region Block Place
        /// <summary>Place a single block in the world. This will mark the block as dirty.</summary>
        /// <param name="position">position to place the block at</param>
        /// <param name="type">type of block to place</param>
        /// <param name="isMultipleBlockPlacement">Use this when placing multiple blocks at once so lighting and chunk queueing only happens once.</param>
        internal static void PlaceBlock(Position position, Block.BlockType type, bool isMultipleBlockPlacement = false)
        {
            if (!position.IsValidBlockLocation || position.Y <= 0) return;

            //this was a multiple block placement, prevent placing blocks on yourself and getting stuck; used to be able to place cuboids on yourself and get stuck
            //only check in single player for now because in multiplayer this could allow the blocks on different clients to get out of sync and placements of multiple blocks in multiplayer will be rare
            if (isMultipleBlockPlacement && (position.IsOnBlock(ref Game.Player.Coords) || position == Game.Player.CoordsHead.ToPosition())) return;

            if (type == Block.BlockType.Air)
            {
                //if destroying a block under water, fill with water instead of air
                if (position.Y + 1 < Chunk.CHUNK_HEIGHT && GetBlock(position.X, position.Y + 1, position.Z).Type == Block.BlockType.Water) type = Block.BlockType.Water;
            }

            var chunk = Chunks[position];
            var block = position.GetBlock();
            var oldType = block.Type;
            block.Type = type; //assign the new type
            var isTransparentBlock = Block.IsBlockTypeTransparent(type);
            var isTransparentOldBlock = Block.IsBlockTypeTransparent(oldType);
            block.BlockData = (ushort)(block.BlockData | 0x8000); //mark the block as dirty for the save file "diff" logic
            chunk.Blocks[position] = block; //insert the new block
            chunk.UpdateHeightMap(ref block, position.X % Chunk.CHUNK_SIZE, position.Y, position.Z % Chunk.CHUNK_SIZE);

            if (!isTransparentBlock || type == Block.BlockType.Water)
            {
                var below = position;
                below.Y--;
                if (below.Y > 0)
                {
                    if (below.GetBlock().Type == Block.BlockType.Grass || below.GetBlock().Type == Block.BlockType.Snow)
                    {
                        PlaceBlock(below, Block.BlockType.Dirt, true); //dont queue with this dirt block change, the source block changing takes care of it, prevents double queueing the chunk and playing sound twice
                    }
                }
            }

            if (!chunk.WaterExpanding) //water is not expanding, check if this change should make it start
            {
                switch (type)
                {
                    case Block.BlockType.Water:
                        chunk.WaterExpanding = true;
                        break;
                    case Block.BlockType.Air:
                        for (var q = 0; q < 5; q++)
                        {
                            Position adjacent;
                            switch (q)
                            {
                                case 0:
                                    adjacent = new Position(position.X + 1, position.Y, position.Z);
                                    break;
                                case 1:
                                    adjacent = new Position(position.X - 1, position.Y, position.Z);
                                    break;
                                case 2:
                                    adjacent = new Position(position.X, position.Y + 1, position.Z);
                                    break;
                                case 3:
                                    adjacent = new Position(position.X, position.Y, position.Z + 1);
                                    break;
                                default:
                                    adjacent = new Position(position.X, position.Y, position.Z - 1);
                                    break;
                            }
                            if (adjacent.IsValidBlockLocation && adjacent.GetBlock().Type == Block.BlockType.Water)
                            {
                                Chunks[adjacent].WaterExpanding = true;
                            }
                        }
                        break;
                }
            }

            //its easier to just set .GrassGrowing on all affected chunks to true, and then let that logic figure it out and turn it off, this way the logic is contained in one spot
            //and also the logic here doesnt need to run every time a block gets placed. ie: someone is building a house, its running through this logic for every block placement;
            //now it can only check once on the grass grow interval and turn it back off
            //gm: an additional optimization, grass could never start growing unless this is an air block and its replacing a non transparent block
            //OR this is a non transparent block filling in a previously transparent block to cause grass to die
            if (!isTransparentBlock || (type == Block.BlockType.Air && !isTransparentOldBlock))
            {
                chunk.GrassGrowing = true;
                if (position.IsOnChunkBorder)
                {
                    foreach (var adjacentChunk in position.BorderChunks) adjacentChunk.GrassGrowing = true;
                }
            }

            //determine if any static game items need to be removed as a result of this block placement
            if (type != Block.BlockType.Air)
            {
                lock (chunk.Clutters) //lock because clutter is stored in a HashSet
                {
                    //if theres clutter on this block then destroy it to place the block (FirstOrDefault returns null if no match is found)
                    var clutterToRemove = chunk.Clutters.FirstOrDefault(clutter => position.IsOnBlock(ref clutter.Coords));
                    if (clutterToRemove != null) chunk.Clutters.Remove(clutterToRemove);
                }

                var lightSourceToRemove = chunk.LightSources.FirstOrDefault(lightSource => position.IsOnBlock(ref lightSource.Value.Coords));
                if (lightSourceToRemove.Value != null)
                {
                    LightSource temp;
                    chunk.LightSources.TryRemove(lightSourceToRemove.Key, out temp);
                }
            }
            else //destroying block
            {
                lock (chunk.Clutters) //lock because clutter is stored in a HashSet
                {
                    //if theres clutter on top of this block then remove it as well (FirstOrDefault returns null if no match is found)
                    var clutterToRemove = chunk.Clutters.FirstOrDefault(clutter => clutter.Coords.Xblock == position.X && clutter.Coords.Yblock == position.Y + 1 && clutter.Coords.Zblock == position.Z); //add one to Y to look on the block above
                    if (clutterToRemove != null) chunk.Clutters.Remove(clutterToRemove);
                }

                //look on ALL 6 adjacent blocks for static items, and those only get destroyed if its on the matching opposite attached to face
                var adjacentPositions = position.AdjacentPositionFaces;
                foreach (var tuple in adjacentPositions)
                {
                    var adjBlock = tuple.Item1.GetBlock();
                    if (adjBlock.Type != Block.BlockType.Air) continue; //position cannot contain an item if the block is not air
                    var adjChunk = tuple.Item2 == Face.Top || tuple.Item2 == Face.Bottom ? chunk : Chunks[tuple.Item1]; //get the chunk in case the adjacent position crosses a chunk boundary
                    var lightSourceToRemove = adjChunk.LightSources.FirstOrDefault(lightSource => tuple.Item1.IsOnBlock(ref lightSource.Value.Coords));
                    if (lightSourceToRemove.Value != null && lightSourceToRemove.Value.AttachedToFace == tuple.Item2.ToOpposite()) //remove the light source
                    {
                        LightSource temp;
                        chunk.LightSources.TryRemove(lightSourceToRemove.Key, out temp);
                    }
                }

                //if theres a dynamic item on top of this block then let it fall
                foreach (var item in chunk.GameItems.Values)
                {
                    if (!item.IsMoving && item.Coords.Xblock == position.X && item.Coords.Yblock == position.Y + 1 && item.Coords.Zblock == position.Z)
                    {
                        item.IsMoving = true;
                    }
                }
            }

            if (!isMultipleBlockPlacement)
            {
                Debug.WriteLineIf(type == Block.BlockType.Ice && oldType == Block.BlockType.Water, "Growth change Water->Ice; Multiple lightbox updates and chunk queues are possible");
                ModifyLightAndQueueChunksForBlockChange(position, isTransparentOldBlock != isTransparentBlock, type);

                //sounds dont play for multi/cuboid placements; they are responsible for their own sounds; prevents sound spam
                switch (type)
                {
                    case Block.BlockType.Air:
                        switch (oldType)
                        {
                            case Block.BlockType.Water: //remove water
                                Sounds.Audio.PlaySound(Sounds.SoundType.JumpOutOfWater, ref position);
                                break;
                            default: //remove another type
                                Sounds.Audio.PlaySound(Sounds.SoundType.RemoveBlock, ref position);
                                break;
                        }
                        break;
                    case Block.BlockType.Water: //place water
                        Sounds.Audio.PlaySound(Sounds.SoundType.JumpOutOfWater, ref position);
                        break;
                    default:
                        //only play the add block sound if the old type was air; the only way blocks can change otherwise are the auto changes in chunks for grass/snow/dirt/etc.
                        if (oldType == Block.BlockType.Air) Sounds.Audio.PlaySound(Sounds.SoundType.AddBlock, ref position);
                        break;
                }
            }
        }

        /// <summary>Place multiple blocks in the world of the same type.</summary>
        /// <param name="startPosition">start placing blocks at</param>
        /// <param name="endPosition">stop placing blocks at</param>
        /// <param name="type">type of block to place</param>
        /// <param name="isMultipleCuboidPlacement">Use this when placing multiple cuboids at once so lighting and chunk queueing only happens once.</param>
        internal static void PlaceCuboid(Position startPosition, Position endPosition, Block.BlockType type, bool isMultipleCuboidPlacement = false)
        {
            for (var x = Math.Min(startPosition.X, endPosition.X); x <= Math.Max(startPosition.X, endPosition.X); x++)
            {
                for (var y = Math.Min(startPosition.Y, endPosition.Y); y <= Math.Max(startPosition.Y, endPosition.Y); y++)
                {
                    for (var z = Math.Min(startPosition.Z, endPosition.Z); z <= Math.Max(startPosition.Z, endPosition.Z); z++)
                    {
                        PlaceBlock(new Position(x, y, z), type, true);
                    }
                }
            }
            if (!isMultipleCuboidPlacement) ModifyLightAndQueueChunksForCuboidChange(startPosition, endPosition);
        }
        #endregion

        #region Lighting
        /// <summary>Sky lightmap of the entire world.</summary>
        /// <remarks>
        /// -could become a circular array down the road if we want even bigger worlds.
        /// -could also hold both sky light and item light by using bit operations, both values 0-15 can fit in one byte
        /// </remarks>
        internal static byte[, ,] SkyLightMap;

        /// <summary>Item lightmap of the entire world. Stored separately because item light is not affected by the sky.</summary>
        internal static byte[, ,] ItemLightMap;

        /// <summary>
        /// Build light maps for every chunk. Done after all chunks have loaded and had their height maps already built. Does not need to be done for servers.
        /// Calculates a light map for each chunk in separate tasks to greatly improve performance. When all chunk light map tasks have finished, then "pull"
        /// light across chunk borders by looking at every block on a chunk border.
        /// </summary>
        /// <remarks>note: changing the tasks from queue to an array showed no performance benefit in this case</remarks>
        internal static void InitializeAllLightMaps()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            SkyLightMap = new byte[SizeInBlocksX, Chunk.CHUNK_HEIGHT, SizeInBlocksZ];
            ItemLightMap = new byte[SizeInBlocksX, Chunk.CHUNK_HEIGHT, SizeInBlocksZ];

            var tasks = new Queue<Task>();
            foreach (Chunk chunk in Chunks)
            {
                Chunk c = chunk;
                tasks.Enqueue(Task.Factory.StartNew(() => Lighting.InitializeLightMap(c)));
            }
            Task.WaitAll(tasks.ToArray());

            //cross chunk light pulling
            tasks.Clear();
            Settings.Launcher.UpdateProgressInvokable("Pulling light across Chunks...", 0, 0);
            foreach (Chunk chunk in Chunks)
            {
                Chunk c = chunk;
                tasks.Enqueue(Task.Factory.StartNew(() => Lighting.InitializeCrossChunkPulling(c)));
            }
            Task.WaitAll(tasks.ToArray());

            //after this theres no need to still have the lightmap data on the chunk (cant null it until cross chunk pulling is done because it can still get accessed by an adjacent chunk)
            Task.Factory.StartNew(Chunks.ClearInitialLightMaps); //no need to wait for this

            Debug.WriteLine("Light Maps initialize time: {0}ms", stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Update the applicable light box and queue the chunks that were affected. Skip light update if the changing block is transparent as it would have no effect on light.
        /// Light box update is done in a task which queues the affected chunks only after the light box update is finished.
        /// </summary>
        /// <param name="position">Position the light change is originating at (the block that is changing).</param>
        /// <param name="changeInTransparency">Lighting only needs to be recalculated if we are replacing a solid with a transparent or vice versa</param>
        /// <param name="blockType">New block type being placed. Will be Air for block remove.</param>
        private static void ModifyLightAndQueueChunksForBlockChange(Position position, bool changeInTransparency, Block.BlockType blockType)
        {
            if (!changeInTransparency) //transparency did not change; no effect on lighting
            {
                if (position.IsOnChunkBorder)
                {
                    var chunks = new Queue<Chunk>();
                    chunks.Enqueue(Chunks[position]); //queue this chunk first
                    foreach (Chunk chunk in position.BorderChunks) chunks.Enqueue(chunk); //now queue 1 adjacent chunk as well when on chunk border or 2 adjacent chunks when on chunk corner
                    Debug.WriteLine("Growth change on chunk border {0}:{1}; Lighting not affected; Queueing {2} chunks", blockType, position, chunks.Count);
                    QueueAffectedChunks(chunks);
                }
                else //not on a chunk border, queue this chunk only
                {
                    Debug.WriteLine("Growth change {0}:{1}; Lighting not affected; Queue local chunk only", blockType, position);
                    Chunks[position].QueueImmediate();
                }
            }
            else
            {
                Task<Queue<Chunk>>.Factory.StartNew(() => Lighting.UpdateLightBox(ref position, null, true, blockType == Block.BlockType.Air)).ContinueWith(task => QueueAffectedChunks(task.Result));
            }
        }

        /// <summary>
        /// Update the applicable light box and queue the chunks that were affected. No optimizations are made for transparent blocks because every block in the cuboid would need to be checked.
        /// Light box update is done in a task which queues the affected chunks only after the light box update is finished.
        /// </summary>
        /// <param name="position1">First corner of the cuboid.</param>
        /// <param name="position2">Diagonal corner of the cuboid.</param>
        internal static void ModifyLightAndQueueChunksForCuboidChange(Position position1, Position position2)
        {
            Task<Queue<Chunk>>.Factory.StartNew(() => Lighting.UpdateLightBox(ref position1, position2, true, false)).ContinueWith(task => QueueAffectedChunks(task.Result));
        }

        internal static void QueueAffectedChunks(IEnumerable<Chunk> chunks)
        {
            foreach (var chunk in chunks) chunk.QueueImmediate();
        }
        #endregion
    }
}
