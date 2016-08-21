﻿using System;
using System.Diagnostics;
using Hexpoint.Blox.Hosts.Ui;
using Hexpoint.Blox.Hosts.World;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Hexpoint.Blox.GameActions;

namespace Hexpoint.Blox.Hosts
{
	internal class BlockCursorHost : IHost
	{
		#region Constructors
		internal BlockCursorHost()
		{
            _game = Settings.Game;
			Position = new Position();
			_positionAdd = new Position();
            _game.Mouse.ButtonDown += OnMouseButtonDown;
            _game.Mouse.ButtonUp += OnMouseButtonUp;
            _game.KeyPress += OnKeyPress;
			_blockCursorUpdateTimer = new Stopwatch();
			_blockCursorUpdateTimer.Start();
		}
		#endregion

		#region Properties
        private readonly Game _game; //provides a shortcut to game (Settings.Game could also be used instead)
		private const int BLOCK_CURSOR_UPDATE_INTERVAL_MS = 100;
		private readonly Stopwatch _blockCursorUpdateTimer;

		/// <summary>Returns the position the block cursor is on (the block that would be destroyed).</summary>
		internal static Position Position;

		private static Position _positionAdd;
		/// <summary>Returns the coords of where a block will be added (the block cursor + 1 in the direction of the selected surface).</summary>
		internal static Position PositionAdd
		{
			get
			{
				_positionAdd.X = Position.X;
				_positionAdd.Y = Position.Y;
				_positionAdd.Z = Position.Z;
				switch (SelectedFace)
				{
					case Face.Right:
						_positionAdd.X++;
						break;
					case Face.Left:
						_positionAdd.X--;
						break;
					case Face.Top:
						_positionAdd.Y++;
						break;
					case Face.Bottom:
						_positionAdd.Y--;
						break;
					case Face.Front:
						_positionAdd.Z++;
						break;
					case Face.Back:
						_positionAdd.Z--;
						break;
				}
				return _positionAdd;
			}
		}

		internal static Face SelectedFace { get; private set; }

		/// <summary>Get the distance between the player and the block cursor.</summary>
		private static float CursorDistance
		{
            get { return Game.CameraCoords.ToPosition().GetDistanceExact(ref Position); }
		}

		private Vector3d _mouseVector;
		private Vector3d _mouseVectorXPlusOne;
		private Vector3d _mouseVectorYPlusOne;
        private static Position _leftMouseDownBlock;
        private static Position _leftMouseUpBlock;
        private static bool _leftMouseDown;
        private static bool _rightMouseDown;
		#endregion

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
            case MouseButton.Left:
                _leftMouseDownBlock = PositionAdd;
                _leftMouseUpBlock = PositionAdd;
                _leftMouseDown = true;
                break;
            case MouseButton.Right:
                _rightMouseDown = true;
                break;
            }
        }

        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
            case MouseButton.Left:
                _leftMouseDown = false;
                _leftMouseUpBlock = PositionAdd;
                Game.CharacterHost.SelectCharacters(_leftMouseDownBlock, _leftMouseUpBlock);
                break;
            case MouseButton.Right:
                _rightMouseDown = false;
                Game.CharacterHost.PerformCharacterAction(PositionAdd);
                break;
            }
        }

        /// <summary>
        /// This event is better suited for handling text while chatting as the key presses can be transferred straight through unlike the OpenTK KeyDown and KeyUp events.
        /// This event fires for ESC and ENTER but not for Function keys, PrtScr, Home, End, PgUp, PgDn, etc.
        /// Allows key repeat by default.
        /// </summary>
        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'z')
            {
                AddRemoveBlocks(Block.BlockType.Bricks);
            }
            else if (e.KeyChar == 'c')
            {
                AddRemoveBlocks(Block.BlockType.Air);
            }
        }

        private void AddRemoveBlocks(Block.BlockType blockType)
        {
            int xIncr = (_leftMouseDownBlock.X <= _leftMouseUpBlock.X) ? 1 : -1;
            int yIncr = (_leftMouseDownBlock.Y <= _leftMouseUpBlock.Y) ? 1 : -1;
            int zIncr = (_leftMouseDownBlock.Z <= _leftMouseUpBlock.Z) ? 1 : -1;
            Position block = new Position();
            for (block.X = _leftMouseDownBlock.X; block.X != _leftMouseUpBlock.X+xIncr; block.X += xIncr)
                for (block.Z = _leftMouseDownBlock.Z; block.Z != _leftMouseUpBlock.Z+zIncr; block.Z += zIncr)
                    for (block.Y = _leftMouseDownBlock.Y; block.Y != _leftMouseUpBlock.Y+yIncr; block.Y += yIncr)
                        NetworkClient.SendAddOrRemoveBlock(block, blockType);
        }

		public void Update(FrameEventArgs e)
		{
			//todo: doing this in the update makes the game choppy, works fine when moved to the render apparently...
			//UpdateCursor();
		}

		public void Render(FrameEventArgs e)
		{
			if (Settings.UiDisabled) return;

			//update the block cursor location if the specified interval has elapsed
			if (_blockCursorUpdateTimer.ElapsedMilliseconds > BLOCK_CURSOR_UPDATE_INTERVAL_MS)
			{
				UpdateCursor();
                if (_leftMouseDown)
                    _leftMouseUpBlock = PositionAdd;
				_blockCursorUpdateTimer.Restart();
			}

			Utilities.GlHelper.ResetTexture();
			GL.PushAttrib(AttribMask.PolygonBit); //http://www.talisman.org/opengl-1.1/Reference/glPushAttrib.html
			GL.Enable(EnableCap.PolygonOffsetLine); //enable polygon offset to stitch the cursor on top of the existing block face polygon (red book pg 293)
			GL.PolygonMode(MaterialFace.Front, PolygonMode.Line); //gm: have to use polygon line mode and draw a wire quad for PolygonOffsetLine to work (http://dmi.uib.es/~josemaria/files/OpenGLFAQ/polygonoffset.htm)
			GL.LineWidth(Math.Max(3 - CursorDistance / 3, 1)); //change line width based on distance

			//set cursor color based on the current tool
			switch (Buttons.CurrentTool)
			{
				case ToolType.Default:
					GL.Color3(0.2f, 0.2f, 1f);
					break;
				case ToolType.FastBuild:
					GL.Color3(0.2f, 1f, 0.2f);
					break;
				case ToolType.FastDestroy:
					GL.Color3(1f, 0.2f, 0.2f);
					break;
				default:
					GL.Color3(0.8f, 0.8f, 0.2f);
					break;
			}
            //if (_mouseDown)
            {
                int xIncr = (_leftMouseDownBlock.X <= _leftMouseUpBlock.X) ? 1 : -1;
                int yIncr = (_leftMouseDownBlock.Y <= _leftMouseUpBlock.Y) ? 1 : -1;
                int zIncr = (_leftMouseDownBlock.Z <= _leftMouseUpBlock.Z) ? 1 : -1;
                Position block = new Position();
                for (block.X = _leftMouseDownBlock.X; block.X != _leftMouseUpBlock.X+xIncr; block.X += xIncr)
                    for (block.Z = _leftMouseDownBlock.Z; block.Z != _leftMouseUpBlock.Z+zIncr; block.Z += zIncr)
                        for (block.Y = _leftMouseDownBlock.Y; block.Y != _leftMouseUpBlock.Y+yIncr; block.Y += yIncr)
                            DrawBlockHighlight(block);
            }
            //else
            {
                DrawBlockHighlight(_positionAdd);
            }

			GL.PopAttrib();
			Utilities.GlHelper.ResetColor();
		}

        private void DrawBlockHighlight(Position Position)
        {
            GL.PushMatrix();
            GL.Translate(Position.X, Position.Y, Position.Z);
            GL.Begin(BeginMode.Quads);
            //switch (SelectedFace)
            {
                //  case Face.Right:
                GL.Vertex3(Constants.BLOCK_SIZE, Constants.BLOCK_SIZE, Constants.BLOCK_SIZE);
                GL.Vertex3(Constants.BLOCK_SIZE, 0f, Constants.BLOCK_SIZE);
                GL.Vertex3(Constants.BLOCK_SIZE, 0f, 0f);
                GL.Vertex3(Constants.BLOCK_SIZE, Constants.BLOCK_SIZE, 0f);
                //      break;
                //  case Face.Left:
                GL.Vertex3(0f, Constants.BLOCK_SIZE, 0f);
                GL.Vertex3(0f, 0f, 0f);
                GL.Vertex3(0f, 0f, Constants.BLOCK_SIZE);
                GL.Vertex3(0f, Constants.BLOCK_SIZE, Constants.BLOCK_SIZE);
                //      break;
                //  case Face.Top:
                GL.Vertex3(0f, Constants.BLOCK_SIZE, Constants.BLOCK_SIZE);
                GL.Vertex3(Constants.BLOCK_SIZE, Constants.BLOCK_SIZE, Constants.BLOCK_SIZE);
                GL.Vertex3(Constants.BLOCK_SIZE, Constants.BLOCK_SIZE, 0f);
                GL.Vertex3(0f, Constants.BLOCK_SIZE, 0f);
                //      break;
                //  case Face.Bottom:
                GL.Vertex3(Constants.BLOCK_SIZE, 0f, 0f);
                GL.Vertex3(Constants.BLOCK_SIZE, 0f, Constants.BLOCK_SIZE);
                GL.Vertex3(0f, 0f, Constants.BLOCK_SIZE);
                GL.Vertex3(0f, 0f, 0f);
                //      break;
                //  case Face.Front:
                GL.Vertex3(0f, Constants.BLOCK_SIZE, Constants.BLOCK_SIZE);
                GL.Vertex3(0f, 0f, Constants.BLOCK_SIZE);
                GL.Vertex3(Constants.BLOCK_SIZE, 0f, Constants.BLOCK_SIZE);
                GL.Vertex3(Constants.BLOCK_SIZE, Constants.BLOCK_SIZE, Constants.BLOCK_SIZE);
                //      break;
                //  case Face.Back:
                GL.Vertex3(Constants.BLOCK_SIZE, Constants.BLOCK_SIZE, 0f);
                GL.Vertex3(Constants.BLOCK_SIZE, 0f, 0f);
                GL.Vertex3(0f, 0f, 0f);
                GL.Vertex3(0f, Constants.BLOCK_SIZE, 0f);
                //      break;
            }
            GL.End();
            GL.PopMatrix();
        }

		public void Resize(EventArgs e)
		{
			
		}

		private void UpdateCursor()
		{
			//GetMousePosition will get the position of the mouse pointer
			//If you get the vector of the mouse from x+1 and y+1 you have 3 points now... since you're on one surface each of the points will have one matching coord (x/y/z)... whichever it is is the face they have selected.
			_mouseVector = GetMousePosition(ref Game.Projection, ref Game.ModelView, false, false);
			_mouseVectorXPlusOne = GetMousePosition(ref Game.Projection, ref Game.ModelView, true, false);
			_mouseVectorYPlusOne = GetMousePosition(ref Game.Projection, ref Game.ModelView, false, true);

			var xDelta = GetDelta(_mouseVector.X, _mouseVectorXPlusOne.X, _mouseVectorYPlusOne.X);
			var yDelta = GetDelta(_mouseVector.Y, _mouseVectorXPlusOne.Y, _mouseVectorYPlusOne.Y);
			var zDelta = GetDelta(_mouseVector.Z, _mouseVectorXPlusOne.Z, _mouseVectorYPlusOne.Z);

			if (xDelta < Math.Min(yDelta, zDelta))
			{
				SelectedFace = _mouseVector.X > Game.CameraCoords.Xf ? Face.Left : Face.Right;
			}
			else if (yDelta < Math.Min(xDelta, zDelta))
			{
                SelectedFace = _mouseVector.Y > Game.CameraCoords.Yf + 1 ? Face.Bottom : Face.Top;
			}
			else if (zDelta < Math.Min(xDelta, yDelta))
			{
                SelectedFace = _mouseVector.Z > Game.CameraCoords.Zf ? Face.Back : Face.Front;
			}

			switch (SelectedFace)
			{
				case Face.Right:
					Position.X = (int)Math.Round(_mouseVector.X - 1);
					break;
				case Face.Left:
					Position.X = (int)Math.Round(_mouseVector.X);
					break;
				default:
					Position.X = (int)Math.Round(_mouseVector.X - .5);
					break;
			}

			switch (SelectedFace)
			{
				case Face.Top:
					Position.Y = (int)Math.Round(_mouseVector.Y - 1, 0);
					break;
				case Face.Bottom:
					Position.Y = (int)Math.Round(_mouseVector.Y);
					break;
				default:
					Position.Y = (int)Math.Round(_mouseVector.Y - .5, 0);
					break;
			}

			switch (SelectedFace)
			{
				case Face.Front:
					Position.Z = (int)Math.Round(_mouseVector.Z - 1);
					break;
				case Face.Back:
					Position.Z = (int)Math.Round(_mouseVector.Z);
					break;
				default:
					Position.Z = (int)Math.Round(_mouseVector.Z - .5);
					break;
			}
		}

		/// <summary>Gets the vector of where the mouse cursor currently is.</summary>
		/// <param name="projection"></param>
		/// <param name="modelView"></param>
		/// <param name="offsetX">will get the coords of the pixel to the right of the mouse cursor if true</param>
		/// <param name="offsetY">will get the coords of the pixel below the mouse cursor if true</param>
		private static Vector3d GetMousePosition(ref Matrix4d projection, ref Matrix4d modelView, bool offsetX, bool offsetY)
		{
			Matrix4d view;
			Matrix4d.Mult(ref modelView, ref projection, out view);
			int x = Settings.Game.Mouse.X + (offsetX ? 1 : 0);
			int y = Settings.Game.Height + (offsetY ? 1 : 0) - Settings.Game.Mouse.Y - 1; //invert Y, window coords are opposite
			float depth = 0;
			GL.ReadPixels(x, y, 1, 1, PixelFormat.DepthComponent, PixelType.Float, ref depth);
			var viewPosition = new Vector4d
			(
				(float)x / Settings.Game.Width * 2.0f - 1.0f,	//map X to -1 to 1 range
				(float)y / Settings.Game.Height * 2.0f - 1.0f,	//map Y to -1 to 1 range
				depth * 2.0f - 1.0f,							//map Z to -1 to 1 range
				1.0f
			);
			var temp = Vector4d.Transform(viewPosition, Matrix4d.Invert(view));
			return new Vector3d(temp.X, temp.Y, temp.Z) / temp.W;
		}

		/// <summary>Gets the difference between the biggest and smallest of 3 numbers.</summary>
		private static double GetDelta(double d1, double d2, double d3)
		{
			return Math.Max(Math.Max(d1, d2), d3) - Math.Min(Math.Min(d1, d2), d3);
		}

		public void Dispose()
		{
			
		}

		public bool Enabled { get; set; }
	}
}
