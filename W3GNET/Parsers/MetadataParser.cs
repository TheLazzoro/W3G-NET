using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;
using W3GNET.Extensions;
using W3GNET.Types;
using W3GNET.Util;

namespace W3GNET.Parsers
{
    public class ReplayMetadata
    {
        public Stream gameData;
        public MapMetadata map;
        public List<PlayerRecord> playerRecords;
        public List<SlotRecord> slotRecords;
        public ReforgedPlayerMetadata[] reforgedPlayerMetadata;
        public uint RandomSeed;
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
        public byte PlayerId;
        public int slotStatus;
        public int computerFlag;
        public int teamId;
        public int color;
        public int raceFlag;
        public int aiStrength;
        public int handicapFlag;
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
                    var block2 = inflater.Copy(block.BlockDecompressedSize);
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
            var gameName = reader.ReadZeroTermString(StringEncoding.UTF8);
            reader.ReadZeroTermString(StringEncoding.UTF8);
            var encodedString = reader.ReadZeroTermString(StringEncoding.HEX);
            var mapMetadata = ParseEncodedMapMetaString(DecodeGameMetaString(encodedString));
            reader.SkipBytes(12);
            var playerListFinal = playerRecords.Concat(ParsePlayerList()).ToList();
            var reforgedPlayerMetadata = new List<ReforgedPlayerMetadata>();
            if (reader.ReadByte() != 25)
            {
                reader.BaseStream.Position = reader.BaseStream.Position - 1;
                reforgedPlayerMetadata = ParseReforgedPlayerMetadata();
            }
            reader.SkipBytes(2);
            var slotRecordCount = reader.ReadByte();
            var slotRecords = ParseSlotRecords(slotRecordCount);
            var randomSeed = reader.ReadUInt32();
            reader.SkipBytes(1);
            var startSpotCount = reader.ReadByte();

            return new ReplayMetadata
            {
                gameData = await BufferHelper.Slice(reader.BaseStream, (int)reader.BaseStream.Position, (int)(reader.BaseStream.Length - reader.BaseStream.Position)),
                map = mapMetadata,
                playerRecords = playerListFinal,
                GameName = gameName,
                RandomSeed = randomSeed,
                reforgedPlayerMetadata = reforgedPlayerMetadata.ToArray(),
                StartSpotCount = startSpotCount,
                slotRecords = slotRecords,
            };
        }

        private List<SlotRecord> ParseSlotRecords(byte slotRecordCount)
        {
            var slots = new List<SlotRecord>();
            for (int i = 0; i < slotRecordCount; i++)
            {
                var record = new SlotRecord();
                record.PlayerId = reader.ReadByte();
                reader.SkipBytes(1);
                record.slotStatus = reader.ReadByte();
                record.computerFlag = reader.ReadByte();
                record.teamId = reader.ReadByte();
                record.color = reader.ReadByte();
                record.raceFlag = reader.ReadByte();
                record.aiStrength = reader.ReadByte();
                record.handicapFlag = reader.ReadByte();
                slots.Add(record);
            }
            return slots;
        }

        private List<ReforgedPlayerMetadata> ParseReforgedPlayerMetadata()
        {
            var result = new List<ReforgedPlayerMetadata>();
            while (reader.ReadByte() == 0x39)
            {
                var subtype = reader.ReadByte();
                var followingBytes = reader.ReadUInt32();
                var data = BufferHelper.Slice(reader.BaseStream, (int)reader.BaseStream.Position, (int)followingBytes);
                if (subtype == 0x3)
                {
                    // TODO: The equivalent TS code looks odd.
                    var decoded = new ProtoPlayer();
                    if (decoded.clan == null)
                    {
                        decoded.clan = string.Empty;
                    }
                    result.Add(new ReforgedPlayerMetadata
                    {
                        PlayerId = decoded.PlayerId,
                        Name = decoded.battletag,
                        Clan = decoded.clan,
                    });
                }
                else if (subtype == 0x4)
                {
                }
                reader.SkipBytes(followingBytes);
            }

            return result;
        }

        private byte[] DecodeGameMetaString(string str)
        {
            var hexRepresentation = str.FromHexToByteArray();

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
                        decoded[dpos] = (byte)(hexRepresentation[i] - 1);
                        dpos++;
                    }
                    else
                    {
                        decoded[dpos] = hexRepresentation[i];
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
                var reader = new BinaryReader(s);

                var speed = reader.ReadByte();
                var secondByte = reader.ReadByte();
                var thirdByte = reader.ReadByte();
                var fourthByte = reader.ReadByte();
                reader.SkipBytes(5);
                var checksum = reader.ReadBytes(4);
                reader.SkipBytes(0);
                var mapName = reader.ReadZeroTermString(StringEncoding.UTF8);
                var creator = reader.ReadZeroTermString(StringEncoding.UTF8);
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
            var playerName = reader.ReadZeroTermString(StringEncoding.UTF8);
            var addData = reader.ReadByte();
            if (addData == 1)
            {
                reader.SkipBytes(1);
            }
            else if (addData == 2)
            {
                reader.SkipBytes(2);
            }
            else if (addData == 8)
            {
                reader.SkipBytes(8);
            }

            return new PlayerRecord { PlayerId = playerId, PlayerName = playerName };
        }
    }
}
