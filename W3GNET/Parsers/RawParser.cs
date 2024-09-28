using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using W3GNET.Extensions;
using W3GNET.Util;

namespace W3GNET.Parsers
{
    public class Header
    {
        public uint compressedSize;
        public uint headerVersion;
        public uint decompressedSize;
        public uint compressedDataBlockCount;
    }

    public class SubHeader
    {
        public string gameIdentifier;
        public uint version;
        public UInt16 buildNo;
        public uint replayLengthMS;
    }

    public class RawReplayData
    {
        public Header header;
        public SubHeader subheader;
        public DataBlock[] blocks;
    }

    public class DataBlock
    {
        public int BlockSize;
        public int BlockDecompressedSize;
        public Stream buffer;
    }

    internal class CustomReplayParser
    {
        private BinaryReader reader;
        private Header header;
        private SubHeader subheader;

        public async Task<RawReplayData> Parse(Stream input)
        {
            reader = new BinaryReader(input);
            header = ParseHeader();
            subheader = ParseSubheader();

            return new RawReplayData
            {
                header = header,
                subheader = subheader,
                blocks = await ParseBlocks()
            };
        }

        private async Task<DataBlock[]> ParseBlocks()
        {
            var blocks = new List<DataBlock>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var block = await ParseBlock();
                if(block.BlockDecompressedSize == 8192)
                {
                    blocks.Add(block);
                }
            }

            return blocks.ToArray();
        }

        private async Task<DataBlock> ParseBlock()
        {
            var isReforged = subheader.buildNo < 6089 ? false : true;
            var blockSize = reader.ReadUInt16();
            if (isReforged)
            {
                reader.SkipBytes(2);
            }
            var blockDecompressedSize = reader.ReadUInt16();
            if (isReforged)
                reader.SkipBytes(6);
            else
                reader.SkipBytes(4);

            var blockContent = await BufferHelper.Slice(reader.BaseStream, (int)reader.BaseStream.Position, blockSize);
            reader.SkipBytes(blockSize);

            return new DataBlock
            {
                BlockDecompressedSize = blockDecompressedSize,
                BlockSize = blockSize,
                buffer = blockContent,
            };
        }

        private int FindParseStartOffset()
        {
            return 0; // TODO: TS solution looks odd here
        }

        private Header ParseHeader()
        {
            var offset = FindParseStartOffset();
            reader.BaseStream.Position = offset;
            reader.ReadZeroTermString(StringEncoding.ASCII);
            reader.SkipBytes(4);
            var compressedSize = reader.ReadUInt32();
            var headerVersion = reader.ReadUInt32();
            var decompressedSize = reader.ReadUInt32();
            var compressedDataBlockCount = reader.ReadUInt32();

            return new Header
            {
                compressedSize = compressedSize,
                headerVersion = headerVersion,
                decompressedSize = decompressedSize,
                compressedDataBlockCount = compressedDataBlockCount,
            };
        }

        private SubHeader ParseSubheader()
        {
            var gameIdentifier = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            var buildNo = reader.ReadUInt16();
            reader.SkipBytes(2);
            var replayLengthMS = reader.ReadUInt32();
            reader.SkipBytes(4);

            return new SubHeader
            {
                buildNo = buildNo,
                gameIdentifier = Encoding.Default.GetString(BitConverter.GetBytes(gameIdentifier)),
                replayLengthMS = replayLengthMS,
                version = version,
            };
        }
    }
}
