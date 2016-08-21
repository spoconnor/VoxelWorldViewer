using System;

namespace Hexpoint.Blox.GameActions
{
    internal class PlayerInfo : GameAction
    {
        /// <summary>Interval used for sending player info to the server.</summary>
        internal const int PLAYER_INFO_SEND_INTERVAL = 20000; //20 seconds

        internal PlayerInfo()
        {
            DataLength = sizeof(short) * 2;
        }

        internal PlayerInfo(short fps, short memory) : this()
        {
            Fps = fps;
            Memory = memory;
        }

        public override string ToString()
        {
            return String.Format("PlayerInfo FPS {0} Memory {1}", Fps, Memory);
        }

        internal override ActionType ActionType { get { return ActionType.PlayerInfo; } }
        internal short Fps;
        internal short Memory;

        protected override void Queue()
        {
            base.Queue();
            Write(BitConverter.GetBytes(Fps), sizeof(short));
            Write(BitConverter.GetBytes(Memory), sizeof(short));
        }

    }
}
