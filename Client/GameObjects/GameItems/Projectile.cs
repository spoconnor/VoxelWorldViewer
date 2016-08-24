using System.Collections.Generic;
using Sean.WorldClient.GameActions;
using Sean.WorldClient.Hosts.World;
using Sean.WorldClient.Utilities;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sean.Shared;

namespace Sean.WorldClient.GameObjects.GameItems
{
    internal class Projectile : GameItemDynamic
    {
        internal Projectile(ref Coords coords, Block.BlockType blockType, bool allowBounce, Vector3? velocity = null, int id = -1) : base(ref coords, GameItemType.Projectile, allowBounce, velocity, id)
        {
            BlockType = blockType;

            //Stop += OnItemStop;
            Decay += OnItemDecay;
        }

        internal Block.BlockType BlockType;

        internal override void Render(FrameEventArgs e)
        {
            base.Render(e);

            GL.PushMatrix();
            GL.Translate(Coords.Xf, Coords.Yf, Coords.Zf);
            GL.Rotate(WorldHost.RotationCounter, -Vector3.UnitY);
            DisplayList.RenderDisplayList(DisplayList.BlockQuarterId, Block.FaceTexture(BlockType, Face.Top));
            GL.PopMatrix();
        }

        internal override int DecaySeconds { get { return 1; } } //projectiles decay as soon as they stop

        /// <summary>Projectile explodes on decay.</summary>
        internal void OnItemDecay(FrameEventArgs e)
        {
        
        }
    }
}
