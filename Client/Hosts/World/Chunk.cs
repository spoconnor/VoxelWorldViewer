﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Sean.WorldClient.GameActions;
using Sean.WorldClient.GameObjects;
using Sean.WorldClient.GameObjects.GameItems;
using Sean.WorldClient.GameObjects.Units;
using Sean.WorldClient.Hosts.World.Render;
using Sean.WorldClient.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sean.Shared;
using Sean.Shared.Textures;

namespace Sean.WorldClient.Hosts.World
{
    public class Chunk
    {
        #region Constructors
        internal Chunk(int x, int z)
        {
            //Coords = new ChunkCoords(x, z);
            Blocks = new Blocks(CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE);
            HeightMap = new int[CHUNK_SIZE, CHUNK_SIZE];
            Clutters = new HashSet<Clutter>();
            LightSources = new ConcurrentDictionary<int, LightSource>();
            Mobs = new HashSet<Mob>();
            GameItems = new ConcurrentDictionary<int, GameItemDynamic>();

            SkyLightMapInitial = new byte[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE]; //takes 96kb @ 32x96x32
            ItemLightMapInitial = new byte[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE]; //takes 96kb @ 32x96x32
        }
        #endregion

        #region Properties
        public const int CHUNK_SIZE = 32;
        public const int CHUNK_HEIGHT = 96;
        public const int SIZE_IN_BYTES = CHUNK_SIZE * CHUNK_HEIGHT * CHUNK_SIZE * sizeof(ushort);
        private const int CLUTTER_RENDER_DISTANCE = CHUNK_SIZE * 4;
        private const int GAME_ITEM_RENDER_DISTANCE = CLUTTER_RENDER_DISTANCE;

		//public ChunkCoords Coords;
        public Blocks Blocks;

        /// <summary>Heighest level in each vertical column containing a non transparent block. Sky light does not shine through this point. Used in rendering and lighting calculations.</summary>
        public int[,] HeightMap;
        public byte[,,] SkyLightMapInitial;
        public byte[,,] ItemLightMapInitial;

        /// <summary>Clutter contained in this chunk. Clutter can be stored at the chunk level only because it can never move off the chunk.</summary>
        /// <remarks>HashSet because currently Clutter cannot be added outside of initial world generation. Collection is locked during removal.</remarks>
        internal HashSet<Clutter> Clutters;

        /// <summary>
        /// Light sources contained in this chunk. Light sources can be stored at the chunk level only because they can never move off the chunk.
        /// TBD: when a light source is destroyed, does it become a GameItem?
        /// </summary>
        internal ConcurrentDictionary<int, LightSource> LightSources;

        internal HashSet<Mob> Mobs; //also stored at World level in ConcurrentDictionary
        
        internal ConcurrentDictionary<int, GameItemDynamic> GameItems; //also stored at World level

        /// <summary>Distance of the chunk from the player in number of blocks.</summary>
        public double DistanceFromPlayer()
        {
            //return Math.Sqrt(Math.Pow(Game.Player.Coords.Xf - Coords.WorldCoordsX, 2) + Math.Pow(Game.Player.Coords.Zf - Coords.WorldCoordsZ, 2));
			return 0;
        }
        
        /// <summary>Lookup for the Chunk Vbo containing the position, normal and texCoords Vbo's for this chunk and texture type.</summary>
        private readonly ChunkVbo[] _chunkVbos = new ChunkVbo[Enum.GetNames(typeof(BlockTextureType)).Length];

        /// <summary>Total number of vbo's being rendered for blocks in this chunk.</summary>
        internal int VboCount { get { return _chunkVbos.Count(chunkVbo => chunkVbo != null); } }

        /// <summary>Total number of primitives being rendered for blocks in this chunk.</summary>
        internal int PrimitiveCount { get { return _chunkVbos.Where(chunkVbo => chunkVbo != null).Sum(chunkVbo => chunkVbo.PrimitiveCount); } }

        /// <summary>
        /// The build state of this chunk. When a chunk gets built it is set to 'Built' state and then marked dirty so the vbo will then get created/recreated.
        /// When a change happens in the chunk, its build state is set to 'Queued' for it to get rebuilt. When loading the initial chunk frustum, chunks are
        /// set to QueuedInitialFrustum because they dont need to be pushed to the ChangedChunkQueue. Chunks that should be built in order in the distance are
        /// set to QueuedFar and placed on the FarChunkQueue.
        /// </summary>
        internal enum BuildState : byte
        {
            /// <summary>Chunk is not loaded.</summary>
            NotLoaded,
            /// <summary>Chunk is queued for build. It will be on the ChangedChunkQueue.</summary>
            Queued,
            /// <summary>Chunk is queued for build in the distance. It will be on the FarChunkQueue.</summary>
            QueuedFar,
            /// <summary>
            /// Chunk is queued for build as part of day/night lighting cycle. It will be on the FarChunkQueue.
            /// Useful because we can determine the reason the chunk is on the far queue.
            /// -this status could eventually have logic to just do light calcs and rebuffer stored arrays if we decide to store them
            /// </summary>
            QueuedDayNight,
            /// <summary>Chunk is queued for build as part of the initial frustum of chunks loaded before entering the world.</summary>
            QueuedInitialFrustum,
            /// <summary>Chunk is queued for build as part of the initial set of chunks outside the initial radius after entering the world.</summary>
            QueuedInitialFar,
            /// <summary>Chunk is currently building.</summary>
            Building,
            /// <summary>Chunk is built.</summary>
            Built
        }

        private volatile BuildState _chunkBuildState = BuildState.NotLoaded;
        internal BuildState ChunkBuildState
        {
            get { return _chunkBuildState; }
            set
            {
                _chunkBuildState = value;
                switch (value)
                {
                    case BuildState.Queued:
                        WorldHost.ChangedChunkQueue.Enqueue(this);
                        break;
                    case BuildState.QueuedDayNight:
                    case BuildState.QueuedFar:
                    case BuildState.QueuedInitialFar:
                        WorldHost.FarChunkQueue.Enqueue(this);
                        break;
                    case BuildState.Built:
                        if (ChunkBufferState == BufferState.VboBuffered) ChunkBufferState = BufferState.VboDirty;
                        break;
                    case BuildState.NotLoaded:
                        ChunkBufferState = BufferState.VboNotBuffered;
                        UnloadData();
                        break;
                }
            }
        }

        /// <summary>
        /// The buffer state of this chunk. Refers to whether a vbo is created 'VboBuffered', needs to be created or recreated 'VboDirty' or has not yet been buffered 'VboNotBuffered'.
        /// The reason the buffer state and build state are different enums is because the chunk needs to wait to be 'Built' before it can be buffered to a vbo.
        /// </summary>
        internal enum BufferState { VboNotBuffered, VboDirty, VboBuffered }
        internal volatile BufferState ChunkBufferState = BufferState.VboNotBuffered;
        #endregion

        #region Render
        /// <summary>Render all opaque faces in the chunk by looping through each texture vbo in the chunk, binding that texture and then rendering all of the applicable faces.</summary>
        /// <remarks>The vbo will be null if this chunk does not contain any faces with the corresponding texture.</remarks>
        public void RenderOpaqueFaces(FrameEventArgs e)
        {
            //render items within range without checking if they are in the frustum, otherwise the frustum check will eliminate many projectiles that should be rendered
            if (DistanceFromPlayer() < GAME_ITEM_RENDER_DISTANCE)
            {
                foreach (var gameItem in GameItems.Values) gameItem.Render(e);
                GameObject.ResetColor();
            }

            //if (ChunkBufferState == BufferState.VboNotBuffered || !IsInFrustum) return;

            foreach (var chunkVbo in _chunkVbos)
            {
                if (chunkVbo == null || chunkVbo.IsTransparent) continue;
                chunkVbo.Render();
            }
            Game.PerformanceHost.ChunksRendered++;
        }

        #endregion

        #region Build
        /// <summary>
        /// Queue this chunk for immediate rebuild if it is within range (ie: loaded). If this chunk was QueuedFar it will get added to the immediate chunk queue.
        /// This will potentially cause the chunk to get built twice (once on each queue), however is required because if the chunk is on the far queue for day/night lighting or another slow
        /// moving process and a change is made to it, it needs to respond quickly and not wait for the far queue. This will be rare and so is not worth checking for, also if the
        /// near queue finishes before the far queue picks it up it will get skipped anyway.
        /// </summary>
        public void QueueImmediate()
        {
            //if (ChunkBuildState == BuildState.NotLoaded || ChunkBuildState == BuildState.Queued || ChunkBuildState == BuildState.Building) return; //gm: if its 'Building' we prob need to requeue anyway or it could miss something
            if (ChunkBuildState == BuildState.NotLoaded || ChunkBuildState == BuildState.Queued) return;
            ChunkBuildState = BuildState.Queued; //adds the chunk to the immediate queue
        }

        public void BuildData()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            BuildState initialState;
            lock (this) //bm: multiple threads building a chunk at once can mess up the VBOs. contention ought to be rare.
            {
            }

            //monitor chunk build times, for now ignore initial chunk builds, can be easily modified
            //switch (initialState)
            //{
                //case BuildState.QueuedInitialFrustum: //ignore to prevent debug spam
                //case BuildState.QueuedInitialFar: //ignore to prevent debug spam
                //case BuildState.QueuedDayNight: //ignore to prevent debug spam
                //case BuildState.Built: //gm: i wouldnt expect to see this status here, but now we will know (as it turns out this was showing up when there were too many auto grass changes too fast for example)
                //case BuildState.Building: //gm: i wouldnt expect this status here, but now we will know
                //case BuildState.NotLoaded: //gm: i wouldnt expect this status here, but now we will know
                //case BuildState.Queued:
                //case BuildState.QueuedFar:
                //    break;
            //}
        }

        /// <summary>Buffer the chunks data to a vbo so it can be rendered.</summary>
        /// <remarks>When benchmarking this method, it happens so fast quite often the stopwatch would report 0ms, so dont worry about it.</remarks>
        private void BufferData()
        {
            foreach (var chunkVbo in _chunkVbos.Where(chunkVbo => chunkVbo != null))
            {
                chunkVbo.BufferData();
            }
            //_shortestFaceHeight = _shortestFaceHeightTemp;
            ChunkBufferState = BufferState.VboBuffered;
        }

        /// <summary>Remove the chunks vbo when it is outside rendering range.</summary>
        private void UnloadData()
        {
            for (var i = 0; i < _chunkVbos.Length; i++)
            {
                if (_chunkVbos[i] == null) continue;
                _chunkVbos[i].DeleteBuffers();
                _chunkVbos[i] = null;
            }
        }

        /// <summary>Decide if this block face needs to be added to a VBO and rendered. XYZ are world relative coordinates.</summary>
        private void AddBlockFace(ref Block block, Face face, int x, int y, int z)
        {
            int adjacentX = x, adjacentY = y, adjacentZ = z;

            //check if surface is adjacent to another solid texture (if so dont draw it)
            switch (face)
            {
                case Face.Right: adjacentX++; break;
                case Face.Left: adjacentX--; break;
                case Face.Top: adjacentY++; break;
                case Face.Bottom: adjacentY--; break;
                case Face.Front: adjacentZ++; break;
                default: adjacentZ--; break; //back
            }

            //todo: possible optimization, get the Block from this.Blocks array when x/z are NOT on chunk edge, would result in not looking up chunk in World.GetBlock 99% of time, but tiny bit more checks for blocks on edges, try it out some other time
            //-could accept a bool param needToCheckOverChunkEdge, the DTL logic would always pass false for it because its alrdy figured it out, so both ways get the optimization
            #region todo: none of this would need to be checked for the DTL added faces, could grab the block straight out of this chunk instead, wouldnt need to check adjacent block either
            if (!WorldData.IsValidBlockLocation(adjacentX, adjacentY, adjacentZ)) return; //adjacent block is outside the world so theres no need to render this face
            var adjacentBlock = WorldData.GetBlock(adjacentX, adjacentY, adjacentZ);
            if (!adjacentBlock.IsTransparent) return; //no need to render this face because the adjacent face is not transparent
            
            //dont render inner faces of neighboring transparent blocks of same type, makes trees hollow and doesnt draw additional water faces under water
            //improves fps by limiting the number of faces needed and looks better on weak laptop gpu's as well (note: we know the adjacent face is transparent if we get this far)
            if (block.IsTransparent && block.Type == adjacentBlock.Type) return;
            #endregion

            var texture = Block.FaceTexture(block.Type, face);
            //check if there is an existing vbo for this chunk/texture to add this face to
            if (_chunkVbos[(int)texture] == null)
            {
                //vbo not created yet for this chunk/texture so we need to start one
                _chunkVbos[(int)texture] = new ChunkVbo(ref block, TextureLoader.GetBlockTexture(texture));
            }

            //we need position 4 no matter what, so retrieve it here, position 4 is used for the lighting values of all 4 vertices of this face when smooth lighting is off
            byte lightColor = WorldData.GetBlockLightColor(adjacentX, adjacentY, adjacentZ);

            //_shortestFaceHeightTemp = Math.Min(_shortestFaceHeightTemp, y);

            if (block.Type == Block.BlockType.Leaves || block.Type == Block.BlockType.SnowLeaves)
            {
                //dont bother with smooth lighting for leaves, it would rarely matter anyway
                if (face == Face.Bottom) lightColor = (byte)(lightColor * 0.6); //make bottom leaves faces darker by giving them 60% of the light they would otherwise have
                BlockRender.AddBlockFaceToVbo(_chunkVbos[(int)texture], face, x, y, z, lightColor);
                return;
            }

            if (Config.SmoothLighting)
            {
                #region Smooth Lighting
                //SMOOTH LIGHTING MAP (block 4 is used by all 4 vertices, blocks 1,3,5,7 are used by 2 vertices each, blocks 0,2,6,8 are used by one vertex only)
                //    0  1  2
                //   v1 v0
                //    3  4  5
                //   v2 v3
                //    6  7  8
                var xyz = new Position[9];
                var smoothLighting = new byte[9];
                smoothLighting[4] = lightColor; //we already have the directly adjacent color which goes in position 4

				//average 4 colors to get the color for each vertex
                var v0Color = (byte)((smoothLighting[1] + smoothLighting[2] + smoothLighting[4] + smoothLighting[5]) / 4);
                var v1Color = (byte)((smoothLighting[0] + smoothLighting[1] + smoothLighting[3] + smoothLighting[4]) / 4);
                var v2Color = (byte)((smoothLighting[3] + smoothLighting[4] + smoothLighting[6] + smoothLighting[7]) / 4);
                var v3Color = (byte)((smoothLighting[4] + smoothLighting[5] + smoothLighting[7] + smoothLighting[8]) / 4);

                BlockRender.AddBlockFaceToVbo(_chunkVbos[(int)texture], face, x, y, z, v0Color, v1Color, v2Color, v3Color);
                #endregion
            }
            else //use simple lighting
            {
                BlockRender.AddBlockFaceToVbo(_chunkVbos[(int)texture], face, x, y, z, lightColor);
            }
        }
        #endregion

        #region Height Map
        /// <summary>Y level of the deepest transparent block in this chunk. When building the vbo, we only need to start at 1 level below this.</summary>
        internal int DeepestTransparentLevel { get; set; }

        /// <summary>Y level of the highest non air block. Improves chunk build times. Nothing is rendered higher then this so when building the chunk vbo theres no need to go any higher.</summary>
        internal int HighestNonAirLevel { get; set; }

        /// <summary>
        /// Build a heightmap for this chunk. This is the highest non transparent block in each vertical column.
        /// Leaves, water and other transparent blocks that light can shine through do not count.
        /// </summary>
        /// <remarks>The height map is used for lighting. Its also used to determine the players starting Y position.</remarks>
        internal void BuildHeightMap()
        {
            DeepestTransparentLevel = CHUNK_HEIGHT; //initialize to top of chunk until this gets calculated
            HighestNonAirLevel = 0; //initialize to bottom of chunk until this gets calculated
            for (var x = 0; x < CHUNK_SIZE; x++)
            {
                for (var z = 0; z < CHUNK_SIZE; z++)
                {
                    for (var y = CHUNK_HEIGHT - 1; y >= 0; y--) //loop from the highest block position downward until we find a solid block
                    {
                        var block = Blocks[x, y, z];
                        if (y > HighestNonAirLevel && block.Type != Block.BlockType.Air) HighestNonAirLevel = y;
                        if (block.IsTransparent) continue;
                        HeightMap[x, z] = y;
                        break;
                    }

                    for (var y = 0; y < CHUNK_HEIGHT - 1; y++) //loop from the base of the world upwards until finding a transparent block
                    {
                        if (!Blocks[x, y, z].IsTransparent) continue;
                        if (y < DeepestTransparentLevel) DeepestTransparentLevel = y; //record this as the deepest transparent level if it is deeper then what we had previously
                        break;
                    }
                }
            }
        }

        /// <summary>Updates the heightmap following a block placement. Usually a lot quicker then re-building the heightmap.</summary>
        internal void UpdateHeightMap(ref Block block, int chunkRelativeX, int yLevel, int chunkRelativeZ)
        {
            var currentHeight = HeightMap[chunkRelativeX, chunkRelativeZ];
            if (block.IsTransparent) //transparent block
            {
                //update height map
                if (yLevel == currentHeight)
                {
                    //transparent block being placed at the previous heightmap level, most likely removing a block (which places Air), so we need to find the next non transparent block for the heightmap
                    for (var y = currentHeight - 1; y >= 0; y--) //start looking down from the previous heightmap level
                    {
                        if (y > 0 && Blocks[chunkRelativeX, y, chunkRelativeZ].IsTransparent) continue;
                        //found the next non transparent block, update the heightmap and exit
                        HeightMap[chunkRelativeX, chunkRelativeZ] = y;
                        break;
                    }
                }

                //update deepest transparent level
                if (yLevel < DeepestTransparentLevel) DeepestTransparentLevel = yLevel;
            }
            else //non transparent block
            {
                //update height map
                //when placing a non transparent block, check if its above the current heightmap value and if so update the heightmap
                if (yLevel > currentHeight) HeightMap[chunkRelativeX, chunkRelativeZ] = yLevel;

                //update deepest transparent level
                if (yLevel == DeepestTransparentLevel)
                {
                    //this block is being set at the DeepestTransparentLevel of this chunk
                    //we will need to calc if this is still the deepest level (because theres another transparent block at this depth) or what the new level is
                    //the easiest way to do that is just rebuild the height map, even though all we really need to do is the portion that updates the deepest level
                    BuildHeightMap();
                    return; //no need to continue on to check anything else when doing a full heightmap rebuild
                }
            }

            //update HighestNonAirLevel property
            //1. if placing air (removing block), is it at same level as previous HighestNonAir?, just rebuild HeightMap in this case, otherwise do nothing
            //2. if placing anything other then air, simply check if its > HighestNonAirLevel and set it
            if (block.Type == Block.BlockType.Air) //removing a block
            {
                if (yLevel == HighestNonAirLevel) BuildHeightMap();
            }
            else //adding a block
            {
                if (yLevel > HighestNonAirLevel) HighestNonAirLevel = yLevel;
            }
        }
        #endregion

        #region Updates
        internal void Update(FrameEventArgs e)
        {
            if (Settings.ChunkUpdatesDisabled) return;

            if (WaterExpanding && WorldData.Chunks.UpdateCounter % WATER_UPDATE_INTERVAL == 0) WaterExpand();
            if (GrassGrowing && (WorldData.Chunks.UpdateCounter + _grassOffset) % GRASS_UPDATE_INTERVAL == 0) GrassGrow();
            
            var dist = DistanceFromPlayer();

            if (ChunkBuildState != BuildState.NotLoaded && dist > Settings.ZFarForChunkUnload)
            {
                ChunkBuildState = BuildState.NotLoaded;
                return;
            }

            if (ChunkBuildState == BuildState.NotLoaded && dist < Settings.ZFarForChunkLoad)
            {
                ChunkBuildState = BuildState.QueuedFar;
                return;
            }

            if (ChunkBuildState != BuildState.Built) return;
            if (ChunkBufferState != BufferState.VboNotBuffered) Game.PerformanceHost.ChunksInMemory++;
            if (ChunkBufferState == BufferState.VboBuffered) return;

            BufferData();
        }

        internal bool WaterExpanding { get; set; }
        private const int WATER_UPDATE_INTERVAL = (int)(Constants.UPDATES_PER_SECOND * 1.5) / Chunks.CHUNK_UPDATE_INTERVAL; //1.5s
        /// <summary>Only called for SinglePlayer and Servers.</summary>
        private void WaterExpand()
        {
            var newWater = new List<Position>();
            for (var i = 0; i < CHUNK_SIZE; i++)
            {
                for (var j = 0; j < CHUNK_HEIGHT; j++)
                {
                    for (var k = 0; k < CHUNK_SIZE; k++)
                    {
                        if (Blocks[i, j, k].Type != Block.BlockType.Water) continue;
                        var belowCurrent = new Position();
                        for (var q = 0; q < 5; q++)
                        {
                            Position adjacent;

                            //if (newWater.Contains(adjacent)) continue;

                        }
                    }
                }
            }

            if (newWater.Count == 0)
            {
                WaterExpanding = false;
                return;
            }

            //var addBlocks = new List<AddBlock>();
            //Settings.ChunkUpdatesDisabled = true; //change blocks while updates are disabled so chunk is only rebuilt once
            //foreach (var newWaterPosition in newWater.Where(newWaterCoords => newWaterCoords.GetBlock().Type != Block.BlockType.Water))
            //{
            //    WorldData.PlaceBlock(newWaterPosition, Block.BlockType.Water);
            //        var temp = newWaterPosition;
            //        addBlocks.Add(new AddBlock(ref temp, Block.BlockType.Water));
            //}
            Settings.ChunkUpdatesDisabled = false;
        }

        internal bool GrassGrowing { get; set; }
        private const int GRASS_UPDATE_INTERVAL = Constants.UPDATES_PER_SECOND * 75 / Chunks.CHUNK_UPDATE_INTERVAL; //75s
        private readonly int _grassOffset = Settings.Random.Next(0, GRASS_UPDATE_INTERVAL); //stagger grass growth randomly for each chunk
        /// <summary>Only called for SinglePlayer and Servers.</summary>
        private void GrassGrow()
        {
            var possibleChanges = new List<Tuple<Block.BlockType, Position>>();

            if (possibleChanges.Count == 0)
            {
                //this happens after a change is made in the chunk that did not cause any possible grass grow style changes
                GrassGrowing = false;
                return;
            }

            var changesMade = 0;
            Settings.ChunkUpdatesDisabled = true; //change blocks while updates are disabled so chunk is only rebuilt once
            {
                foreach (var change in possibleChanges)
                {
                    //add some randomness so the changes dont happen all at once
                    if (possibleChanges.Count > 1)
                    {
                        switch (change.Item1) //can assign different percentages based on block type
                        {
                            case Block.BlockType.Ice:
                                if (Settings.Random.NextDouble() > 0.05) continue; //give ice forming a very low chance because its a change in transparency and causes lightbox updates and must queue multiple chunks
                                break;
                            default:
                                if (Settings.Random.NextDouble() > 0.18) continue;
                                break;
                        }
                    }
                    else //when only one possible change is left, greatly increase its chance; prevents tons of chunks lingering performing the logic until the final change gets made
                    {
                        if (Settings.Random.NextDouble() > 0.5) continue;
                    }

                    changesMade++;
                    var changePosition = change.Item2;
                    WorldData.PlaceBlock(changePosition, change.Item1);
                }
            }
            Settings.ChunkUpdatesDisabled = false;

            if (changesMade == possibleChanges.Count)
            {
                //when all possible changes have been made we can stop GrassGrowing here without waiting for the next iteration to confirm it
                GrassGrowing = false;
            }
        }
        #endregion

        #region Xml
        internal XmlNode GetXml(XmlDocument xmlDocument)
        {
            var xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "C", string.Empty);
            if (xmlNode.Attributes == null) throw new Exception("Node attributes is null.");
            xmlNode.Attributes.Append(xmlDocument.CreateAttribute("WaterExpanding")).Value = WaterExpanding.ToString();
            xmlNode.Attributes.Append(xmlDocument.CreateAttribute("GrassGrowing")).Value = GrassGrowing.ToString();
            return xmlNode;
        }
        #endregion
    }
}
