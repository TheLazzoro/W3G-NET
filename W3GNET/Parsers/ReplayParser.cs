using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace W3GNET.Parsers
{
    public class ParserOutput
    {
        public Header header;
        public SubHeader subHeader;
        public ReplayMetadata metadata;
    }

    public class ReplayParser
    {
        public event Action<GameDataBlock> OnGameDataBlock;
        public event Action<ParserOutput> OnBasicReplayInformation;
        private CustomReplayParser rawParser;
        private MetadataParser metadataParser;
        private GameDataParser gameDataParser;

        public ReplayParser(bool parseActions)
        {
            rawParser = new CustomReplayParser();
            metadataParser = new MetadataParser();
            gameDataParser = new GameDataParser(parseActions);
            gameDataParser.GameDataBlock += GameDataParser_GameDataBlock;
        }

        private void GameDataParser_GameDataBlock(GameDataBlock obj)
        {
            OnGameDataBlock?.Invoke(obj);
        }

        public async Task<ParserOutput> Parse(Stream input)
        {
            var rawParserResult = await rawParser.Parse(input);
            var metadataParserResult = await metadataParser.Parse(rawParserResult.blocks);
            var result = new ParserOutput
            {
                header = rawParserResult.header,
                subHeader = rawParserResult.subheader,
                metadata = metadataParserResult,
            };

            OnBasicReplayInformation?.Invoke(result);
            await gameDataParser.Parse(metadataParserResult.gameData);

            return result;
        }
    }
}
