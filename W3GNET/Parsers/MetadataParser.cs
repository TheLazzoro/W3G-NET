using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;
using W3GNET.Extensions;

namespace W3GNET.Parsers
{
    internal class ReplayMetadata
    {
        public Stream gameData;
        public MapMetadata map;
        public PlayerRecord[] playerRecords;
        public ReforgedPlayerMetadata[] reforgedPlayerMetadata;
        public int RandomSeed;
        public string GameName;
        public int StartSpotCount;
    }

    public class PlayerRecord
    {
        public int PlayerId;
        public string PlayerName;
    }

    public class SlotRecord
    {
        int PlayerId;
        int slotStatus;
        int computerFlag;
        int teamId;
        int color;
        int raceFlag;
        int aiStrength;
        int handicapFlag;
    }

    public class ReforgedPlayerMetadata
    {
        public int PlayerId;
        public string Name;
        public string Clan;
    }

    public class MapMetadata
    {
        public int speed;
        public int hideTerrain;
        public int mapExplored;
        public int alwaysVisible;
        public int _default;
        public int observerMode;
        public int teamsTogether;
        public int fixedTeams;
        public int fullSharedUnitControl;
        public int randomHero;
        public int randomRaces;
        public int referees;
        public byte[] mapChecksum;
        public byte[] mapChecksumSha1;
        public string mapName;
        public string creator;
    }

    internal class MetadataParser
    {
        private BinaryReader reader;

        public async Task<ReplayMetadata> Parse(DataBlock[] blocks)
        {
            var buffers = new List<byte>();
            foreach (var block in blocks)
            {
                using (var inflater = new ZlibStream(block.buffer, CompressionMode.Decompress))
                {
                    inflater.FlushMode = FlushType.Sync;
                    var block2 = new byte[inflater.BufferSize];
                    await inflater.ReadAsync(block2);
                    if (block2.Length > 0 && block.BlockSize > 0)
                    {
                        buffers.AddRange(block2);
                    }
                }
            }

            var bufferStream = new MemoryStream(buffers.ToArray());
            reader = new BinaryReader(bufferStream);
            reader.SkipBytes(5);
            var playerRecords = new List<PlayerRecord>();
            playerRecords.Add(ParseHostRecord());
            var gameName = reader.ReadString();
            reader.ReadString(); // privateString
            var encodedString = reader.ReadString();
            var mapMetadata = ParseEncodedMapMetaString(DecodeGameMetaString(encodedString));
            reader.SkipBytes(12);
            var playerListFinal = playerRecords.Concat(ParsePlayerList());
            var reforgedPlayerMetadata
        }

        private byte[] DecodeGameMetaString(string str)
        {
            var hexRepresentation = Encoding.Default.GetBytes(str);
            byte[] decoded = new byte[hexRepresentation.Length];
            int mask = 0;
            int dpos = 0;

            for (int i = 0; i < hexRepresentation.Length; i++)
            {
                if (i % 8 == 0)
                {
                    mask = hexRepresentation[i];
                }
                else
                {
                    if ((mask & (0x1 << i % 8)) == 0)
                    {
                        decoded[i] = (byte)(hexRepresentation[i] - 1);
                        dpos++;
                    }
                    else
                    {
                        decoded[i] = hexRepresentation[i];
                        dpos++;
                    }
                }
            }
            return decoded;
        }

        private MapMetadata ParseEncodedMapMetaString(byte[] buffer)
        {
            using (var s = new MemoryStream(buffer))
            {
                var speed = reader.ReadByte();
                var secondByte = reader.ReadByte();
                var thirdByte = reader.ReadByte();
                var fourthByte = reader.ReadByte();
                reader.SkipBytes(5);
                var checksum = reader.ReadBytes(4);
                reader.SkipBytes(0);
                var mapName = reader.ReadString();
                var creator = reader.ReadString();
                reader.SkipBytes(1);
                var checksumSha1 = reader.ReadBytes(20);

                return new MapMetadata
                {
                    speed = speed,
                    hideTerrain = (secondByte & 0b00000001),
                    mapExplored = (secondByte & 0b00000010),
                    alwaysVisible = (secondByte & 0b00000100),
                    _default = (secondByte & 0b00001000),
                    observerMode = (secondByte & 0b00110000) >> 4,
                    teamsTogether = (secondByte & 0b01000000),
                    fixedTeams = (secondByte & 0b00000110),
                    fullSharedUnitControl = (fourthByte & 0b00000001),
                    randomHero = (fourthByte & 0b00000010),
                    randomRaces = (fourthByte & 0b00000100),
                    referees = (fourthByte & 0b01000000),
                    mapName = mapName,
                    creator = creator,
                    mapChecksum = checksum,
                    mapChecksumSha1 = checksumSha1,
                };
            }
        }

        private List<PlayerRecord> ParsePlayerList()
        {
            var list = new List<PlayerRecord>();
            while (reader.ReadByte() == 22)
            {
                list.Add(ParseHostRecord());
                reader.SkipBytes(4);
            }
            reader.BaseStream.Position = reader.BaseStream.Position - 1;
            return list;
        }

        private PlayerRecord ParseHostRecord()
        {
            var playerId = reader.ReadByte();
            var playerName = reader.ReadString();
            var addData = reader.ReadByte();
            if (addData == 1)
            {
                reader.SkipBytes(1);
            }
            else if (addData == 2)
            {
                reader.SkipBytes(2);
            }
            else if (addData == 3)
            {
                reader.SkipBytes(8);
            }

            return new PlayerRecord { PlayerId = playerId, PlayerName = playerName };
        }
    }
}
