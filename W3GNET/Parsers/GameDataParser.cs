using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using W3GNET.Extensions;
using W3GNET.Types;
using W3GNET.Util;

namespace W3GNET.Parsers
{
    public class LeaveGameBlock : GameDataBlock
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
        internal byte playerId;
        internal uint mode;
        internal string message;
    }

    public interface GameDataBlock
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
        private bool parseActions;
        private ActionParser actionParser;
        private BinaryReader reader;
        public event Action<GameDataBlock> GameDataBlock;

        public GameDataParser(bool parseActions)
        {
            this.parseActions = parseActions;
            this.actionParser = new ActionParser();
        }

        internal async Task Parse(Stream input)
        {
            this.reader = new BinaryReader(input);
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                GameDataBlock block = await ParseBlock();
                if (block != null)
                {
                    GameDataBlock?.Invoke(block);
                }

                // TODO: What is 'setImmediatePromise' here?
                // await setImmediatePromise(); // from the TS library
            }
        }

        private async Task<GameDataBlock> ParseBlock()
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
                    return await ParseTimeslotBlock();
                case 0x1e:
                    return await ParseTimeslotBlock();
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
            return null;
        }

        private void parseUnknown0x22()
        {
            var length = reader.ReadByte();
            reader.SkipBytes(length);
        }

        private PlayerChatMessageBlock parseChatMessage()
        {
            var playerId = reader.ReadByte();
            var byteCount = reader.ReadUInt16();
            var flags = reader.ReadByte();
            uint mode = 0;
            if (flags == 0x20)
            {
                mode = reader.ReadUInt32();
            }
            var message = reader.ReadZeroTermString(StringEncoding.UTF8);
            return new PlayerChatMessageBlock
            {
                message = message,
                mode = mode,
                playerId = playerId,
            };
        }

        private LeaveGameBlock ParseLeaveGameBlock()
        {
            var reason = reader.ReadInt32();
            var playerId = reader.ReadByte();
            var result = reader.ReadInt32();
            reader.SkipBytes(4);
            return new LeaveGameBlock { playerId = playerId, reason = reason, result = result };
        }

        private async Task<TimeslotBlock> ParseTimeslotBlock()
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
                var actions = reader.SliceFromCurrentOffset(actionBlockLength);
                if (parseActions)
                {
                    commandBlock.actions = actionParser.Parse(actions); // Very performance-heavy. Throws many exceptions...
                }
                reader.SkipBytes(actionBlockLength);
                commandBlocks.Add(commandBlock);
            }

            return new TimeslotBlock { commandBlocks = commandBlocks, timeIncrement = timeIncrement };
        }
    }
}
