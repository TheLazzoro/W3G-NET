using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using W3GNET.Extensions;

namespace W3GNET.Parsers
{
    internal class LeaveGameBlock : GameDataBlock
    {
        public int Id { get; } = 0x17;
        internal int playerId;
        internal int reason;
        internal int result;
    }

    internal class TimeslotBlock : GameDataBlock
    {
        public int Id { get; } = 0x1f | 0x1e;
        internal int timeIncrement;
        internal List<CommandBlock> commandBlocks;
    }


    internal class PlayerChatMessageBlock : GameDataBlock
    {
        public int Id { get; } = 0x20;
        internal int playerId;
        internal int mode;
        internal string message;
    }

    internal interface GameDataBlock
    {
        int Id { get; }
    }

    internal class CommandBlock
    {
        internal int playerId;
        internal List<W3Action> actions;
    }


    internal class GameDataParser
    {
        private ActionParser actionParser;
        private BinaryReader reader;
        public event Action<GameDataBlock> GameDataBlock;

        public GameDataParser(Stream input)
        {
            this.reader = new BinaryReader(input);
            this.actionParser = new ActionParser();
        }

        internal void Parse()
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                GameDataBlock block = ParseBlock();
                if (block != null)
                {
                    GameDataBlock?.Invoke(block);
                }

                // TODO: What is 'setImmediatePromise' here?
                // await setImmediatePromise(); // from the TS library
            }
        }

        private GameDataBlock ParseBlock()
        {
            var id = reader.ReadByte();
            switch (id)
            {
                case 0x17:
                    return ParseLeaveGameBlock();
                case 0x1a:
                case 0x1b:
                case 0x1c:
                    reader.SkipBytes(4);
                    break;
                case 0x1f:
                    return ParseTimeslotBlock();
                case 0x1e:
                    return parseTimeslotBlock();
                case 0x20:
                    return parseChatMessage();
                case 0x22:
                    parseUnknown0x22();
                    break;
                case 0x23:
                    reader.SkipBytes(10);
                    break;
                case 0x2f:
                    reader.SkipBytes(8);
                    break;
            }
        }

        private GameDataBlock parseChatMessage()
        {
            throw new NotImplementedException();
        }

        private void parseUnknown0x22()
        {
            throw new NotImplementedException();
        }

        private GameDataBlock parseTimeslotBlock()
        {
            throw new NotImplementedException();
        }

        private LeaveGameBlock ParseLeaveGameBlock()
        {
            var reason = reader.ReadInt32();
            var playerId = reader.ReadByte();
            var result = reader.ReadInt32();
            reader.SkipBytes(4);
            return new LeaveGameBlock { playerId = playerId, reason = reason, result = result };
        }

        private TimeslotBlock ParseTimeslotBlock()
        {
            var byteCount = reader.ReadUInt16();
            var timeIncrement = reader.ReadUInt16();
            var actionBlockLastOffset = reader.BaseStream.Position + byteCount - 2;
            var commandBlocks = new List<CommandBlock>();
            while (reader.BaseStream.Position < actionBlockLastOffset)
            {
                var commandBlock = new CommandBlock();
                commandBlock.playerId = reader.ReadByte();
                var actionBlockLength = reader.ReadUInt16();
                byte[] sliced = new byte[actionBlockLength];
                reader.BaseStream.WriteAsync(sliced, (int)reader.BaseStream.Position, actionBlockLength);
                using (Stream actions = new MemoryStream(sliced))
                {
                    commandBlock.actions = actionParser.Parse(actions);
                }
                reader.SkipBytes(actionBlockLength);
                commandBlocks.Add(commandBlock);
            }

            return new TimeslotBlock { commandBlocks = commandBlocks, timeIncrement = timeIncrement };
        }
    }
}
