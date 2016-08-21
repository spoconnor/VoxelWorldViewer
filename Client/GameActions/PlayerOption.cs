using System;
using Sean.WorldClient.Hosts.Ui;

namespace Sean.WorldClient.GameActions
{
    internal class PlayerOption : GameAction
    {
        internal PlayerOption()
        {
        }

        internal PlayerOption(OptionType option, byte[] value)
        {
            Option = option;
            Value = value;
            DataLength = sizeof(ushort) + sizeof(int) + value.Length;
        }

        internal override ActionType ActionType { get { return ActionType.PlayerOption; } }

        public override string ToString()
        {
            string valueStr;
            switch (Option)
            {
                case OptionType.Admin:
                    valueStr = System.Text.Encoding.UTF8.GetString(Value);
                    break;
                case OptionType.Creative:
                    valueStr = BitConverter.ToInt32(Value, 0) != 0 ? "On" : "Off";
                    break;
                case OptionType.Speed:
                    valueStr = BitConverter.ToInt32(Value, 0) + "x";
                    break;
                default:
                    throw new Exception("Unknown PlayerOptions: " + Option);
            }
            return String.Format("PlayerOptions {0}: {1}", Option, valueStr);
        }

        internal OptionType Option;
        internal byte[] Value;

        internal enum OptionType : byte
        {
            Admin,
            Creative,
            Speed
        }

        protected override void Queue()
        {
            base.Queue();
            Write((ushort)Option);
            Write(Value.Length);
            Write(Value, Value.Length);
        }

        internal override void Receive()
        {
                lock (TcpClient)
                {
                    base.Receive();
                    var bytes = ReadStream(DataLength);
                    Option = (OptionType)BitConverter.ToUInt16(bytes, 0);
                    Value = new byte[BitConverter.ToInt32(bytes, sizeof(ushort))];
                    Buffer.BlockCopy(bytes, sizeof(ushort) + sizeof(int), Value, 0, Value.Length);
                }

                switch (Option)
                {
                    case OptionType.Admin:
                        Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.SlashResult, "Command not necessary in Single Player mode."));
                        break;
                    case OptionType.Creative:
                        if (Value.Length != sizeof(int)) throw new Exception("Invalid value length.");
                        Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.SlashResult, string.Format("Creative Mode: {0}", BitConverter.ToInt32(Value, 0) != 0 ? "On (middle mouse button toggles flying)" : "Off")));
                        break;
                    case OptionType.Speed:
                        if (Value.Length != sizeof(int)) throw new Exception("Invalid value length.");
                        Settings.MoveSpeed = Constants.MOVE_SPEED_DEFAULT * BitConverter.ToInt32(Value, 0);
                        Settings.JumpSpeed = Constants.INITIAL_JUMP_VELOCITY * BitConverter.ToInt32(Value, 0);
                        Game.UiHost.AddChatMessage(new ChatMessage(ChatMessageType.SlashResult, string.Format("Move speed: {0}x", BitConverter.ToInt32(Value, 0))));
                        break;
                }
        }
    }
}
