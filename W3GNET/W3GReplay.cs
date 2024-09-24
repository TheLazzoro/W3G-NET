using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Web;
using W3GNET.Parsers;

namespace W3GNET
{
    internal class TransferResourceActionWithPlayer
    {
        public string playerName;
        public byte playerId;
    }

    public enum ChatMessageMode
    {
        All,
        Private,
        Team,
        Observers
    }

    internal enum ObserverMode
    {
        ON_DEFEAT,
        FULL,
        REFEREES,
        NONE
    }

    public class ChatMessage
    {
        public string playerName;
        public byte playerId;
        public ChatMessageMode mode;
        public int timeMS;
        public string message;
    }

    public class Team
    {
        public List<byte> key = new List<byte>();
    }

    public class W3GReplay
    {
        public event Action<ParserOutput> OnBasicReplayInformation;
        public event Action<GameDataBlock> OnGameDataBlock;

        public ParserOutput BasicReplayInformation;
        public List<string> Players = new List<string>();
        public List<string> Observers = new List<string>();
        public List<ChatMessage> ChatLog = new List<ChatMessage>();
        public string id = string.Empty;
        public List<LeaveGameBlock> LeaveEvents = new List<LeaveGameBlock>();
        public List<W3MMDAction> W3MMD = new List<W3MMDAction>();
        public SlotRecord[] Slots;
        public List<Team> Teams = new List<Team>();
        public ReplayMetadata Meta;
        public PlayerRecord[] PlayerList;
        public int TotalTimeTracker = 0;
        public int TimeSegmentTracker = 0;
        public int PlayerActionTrackInterval = 60_000;
        public string Gametype = string.Empty;
        public string Matchup = string.Empty;
        public int ParseStartTime;
        public ReplayParser Parser;
        public string Filename;
        public int MS_ELAPSED;
        public Dictionary<byte, byte> SlotToPlayerId = new Dictionary<byte, byte>();
        public HashSet<string> KnownPlayerIds;
        public int WinningTeam = -1;

        private BinaryReader reader;


        public W3GReplay()
        {
            Parser = new ReplayParser();
            Parser.OnBasicReplayInformation += Parser_OnBasicReplayInformation;
            Parser.OnGameDataBlock += Parser_OnGameDataBlock;
        }

        private void Parser_OnBasicReplayInformation(ParserOutput obj)
        {
            HandleBasicReplayInformation(obj);
            OnBasicReplayInformation?.Invoke(obj);
        }

        private void Parser_OnGameDataBlock(GameDataBlock obj)
        {
            OnGameDataBlock?.Invoke(obj);
            ProcessGameDataBlock(obj);
        }

        private void HandleBasicReplayInformation(ParserOutput info)
        {
            BasicReplayInformation = info;
            Slots = info.metadata.slotRecords;
            PlayerList = info.metadata.playerRecords;
            Meta = info.metadata;
            var tempPlayers = new Dictionary<int, PlayerRecord>();
            Teams = new List<Team>();
            Players = new List<string>();

            foreach (var player in PlayerList)
            {
                tempPlayers.Add(player.PlayerId, player);
            }

            if(info.metadata.reforgedPlayerMetadata.Length > 0)
            {
                var extraPlayerList = info.metadata.reforgedPlayerMetadata;
                foreach (var extraPlayer in extraPlayerList)
                {
                    if(tempPlayers.ContainsKey(extraPlayer.PlayerId))
                    {
                        tempPlayers[extraPlayer.PlayerId].PlayerName = extraPlayer.Name;
                    }
                }
            }
        }

        private void ProcessGameDataBlock(GameDataBlock obj)
        {
            throw new NotImplementedException();
        }
    }
}
