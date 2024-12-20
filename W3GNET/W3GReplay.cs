﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using W3GNET.Parsers;
using W3GNET.Types;

namespace W3GNET
{
    public class TransferResourcesActionWithPlayer : TransferResourcesAction
    {
        public string playerName;
        public byte playerId;
    }

    public class TransferResourceActionWithPlayerAndTimestamp
    {
        public TransferResourcesActionWithPlayer TransferResourceActionWithPlayer;
        public int msElapsed;
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
        public List<byte> _Team = new List<byte>();
    }

    public class W3GReplay
    {
        public event Action<ParserOutput> OnBasicReplayInformation;
        public event Action<GameDataBlock> OnGameDataBlock;

        public ParserOutput BasicReplayInformation;
        public Dictionary<int, Player> Players = new Dictionary<int, Player>();
        public List<string> Observers = new List<string>();
        public List<ChatMessage> ChatLog = new List<ChatMessage>();
        public List<TriggerChatCommand> TriggerChatCommand = new List<TriggerChatCommand>();
        public string id = string.Empty;
        public List<LeaveGameBlock> LeaveEvents = new List<LeaveGameBlock>();
        public List<W3MMDAction> W3MMD = new List<W3MMDAction>();
        public List<SlotRecord> Slots = new List<SlotRecord>();
        public Dictionary<int, Team> Teams = new Dictionary<int, Team>();
        public ReplayMetadata Meta;
        public List<PlayerRecord> PlayerList = new List<PlayerRecord>();
        public int TotalTimeTracker = 0;
        public int TimeSegmentTracker = 0;
        public int PlayerActionTrackInterval = 60_000;
        public string Gametype = string.Empty;
        public string Matchup = string.Empty;
        public DateTime ParseStartTime;
        public TimeSpan TotalParseTime;
        public ReplayParser Parser;
        public string Filename;
        public int MS_ELAPSED;
        public Dictionary<byte, byte?> SlotToPlayerId = new Dictionary<byte, byte?>();
        public HashSet<string> KnownPlayerIds = new HashSet<string>();
        public int WinningTeam = -1;


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

        public async Task Parse(Stream stream)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MS_ELAPSED = 0;
            ParseStartTime = DateTime.Now;
            Filename = string.Empty;
            id = string.Empty;
            ChatLog.Clear();
            LeaveEvents.Clear();
            W3MMD.Clear();
            Players.Clear();
            SlotToPlayerId.Clear();
            TotalTimeTracker = 0;
            TimeSegmentTracker = 0;
            Slots.Clear();
            PlayerList.Clear();
            PlayerActionTrackInterval = 60_000;

            await Parser.Parse(stream);

            GenerateId();
            DetermineMatchup();
            DetermineWinningTeam();
            stopwatch.Stop();
            TotalParseTime = stopwatch.Elapsed;
        }

        private void GenerateId()
        {
            var sb = new StringBuilder();
            sb.Append(this.BasicReplayInformation.metadata.RandomSeed);
            foreach (var player in PlayerList)
            {
                sb.Append(player.PlayerName);
            }
            sb.Append(this.Meta.GameName);

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                
                id = builder.ToString();
            }
        }

        private bool IsObserver(Player p)
        {
            return (
                (p.TeamId == 24 && BasicReplayInformation.subHeader.version >= 29) ||
                (p.TeamId == 12 && BasicReplayInformation.subHeader.version < 29)
            );
        }

        private void DetermineMatchup()
        {
            var teamRaces = new Dictionary<string, List<string>>();
            foreach (var p in Players.Values)
            {
                if (!IsObserver(p))
                {
                    List<string> list;
                    string race = string.IsNullOrEmpty(p.RaceDetected) == false ? p.RaceDetected : p.Race.ToString();
                    if (!teamRaces.TryGetValue(p.TeamId.ToString(), out list))
                    {
                        list = new List<string>();
                        teamRaces.Add(p.TeamId.ToString(), list);
                    }

                    list.Add(race);
                }
            }

            // TODO
            //Gametype = teamRaces.Values
            //    .Select(e => e)
            //    .OrderByDescending(e => e)
            //    .Aggregate("", (current, next) => current + "on" + next);
        }

        private void DetermineWinningTeam()
        {
            // TODO
        }

        private void HandleBasicReplayInformation(ParserOutput info)
        {
            BasicReplayInformation = info;
            Slots = info.metadata.slotRecords;
            PlayerList = info.metadata.playerRecords;
            Meta = info.metadata;
            var tempPlayers = new Dictionary<int, PlayerRecord>();
            Teams = new Dictionary<int, Team>();
            Players = new Dictionary<int, Player>();

            foreach (var player in PlayerList)
            {
                tempPlayers.Add(player.PlayerId, player);
            }

            if (info.metadata.reforgedPlayerMetadata.Length > 0)
            {
                var extraPlayerList = info.metadata.reforgedPlayerMetadata;
                foreach (var extraPlayer in extraPlayerList)
                {
                    if (tempPlayers.ContainsKey(extraPlayer.PlayerId))
                    {
                        tempPlayers[extraPlayer.PlayerId].PlayerName = extraPlayer.Name;
                    }
                }
            }

            for (byte i = 0; i < Slots.Count; i++)
            {
                var slot = Slots[i];
                if (slot.slotStatus > 1)
                {
                    SlotToPlayerId[i] = slot.PlayerId;
                    if (Teams.ContainsKey(slot.teamId) == false)
                    {
                        Teams[slot.teamId] = new Team();
                    }
                    Teams[slot.teamId]._Team.Add(slot.PlayerId);

                    var playerName = tempPlayers.ContainsKey(slot.PlayerId) ? tempPlayers[slot.PlayerId].PlayerName : "Computer";
                    Players[slot.PlayerId] = new Player(
                        slot.PlayerId,
                        playerName,
                        slot.teamId,
                        slot.color,
                        RaceFlagFormatter(slot.raceFlag));
                }
            }

            foreach (var item in Players.Keys)
            {
                KnownPlayerIds.Add(item.ToString());
            }
        }

        private Race RaceFlagFormatter(int raceFlag)
        {
            switch (raceFlag)
            {
                case 0x01:
                case 0x41:
                    return Race.Human;
                case 0x02:
                case 0x42:
                    return Race.Orc;
                case 0x04:
                case 0x44:
                    return Race.NightElf;
                case 0x08:
                case 0x48:
                    return Race.Undead;
                case 0x20:
                case 0x60:
                    return Race.Random;
            }
            return Race.Random;
        }

        private void ProcessGameDataBlock(GameDataBlock block)
        {
            switch (block.Id)
            {
                case 31:
                case 30:
                    var timeSlotBlock = block as TimeslotBlock;
                    TotalTimeTracker += timeSlotBlock.timeIncrement;
                    TimeSegmentTracker += timeSlotBlock.timeIncrement;
                    if (TimeSegmentTracker > PlayerActionTrackInterval)
                    {
                        foreach (var p in Players.Values)
                        {
                            p.NewActionTrackingSegment();
                        }
                        TimeSegmentTracker = 0;
                    }
                    HandleTimeSlot(timeSlotBlock);
                    break;
                case 0x20:
                    HandleChatMessage((PlayerChatMessageBlock)block, TotalTimeTracker);
                    break;
                case 23:
                    var leaveGameBlock = (LeaveGameBlock)block;
                    leaveGameBlock.timeMS = TotalTimeTracker;
                    LeaveEvents.Add(leaveGameBlock);
                    break;
            }
        }

        private void HandleChatMessage(PlayerChatMessageBlock block, int timeMS)
        {
            var message = new ChatMessage
            {
                playerName = Players[block.playerId].Name,
                playerId = block.playerId,
                message = block.message,
                mode = NumericalChatModeToChatMessageMode(block.mode),
                timeMS = timeMS,
            };
            ChatLog.Add(message);
        }

        private ChatMessageMode NumericalChatModeToChatMessageMode(uint mode)
        {
            switch (mode)
            {
                case 0x00:
                    return ChatMessageMode.All;
                case 0x01:
                    return ChatMessageMode.Team;
                case 0x02:
                    return ChatMessageMode.Observers;
                default:
                    return ChatMessageMode.Private;
            }
        }

        private void HandleTimeSlot(TimeslotBlock block)
        {
            MS_ELAPSED += block.timeIncrement;
            foreach (var commandBlock in block.commandBlocks)
            {
                ProcessCommandDataBlock(commandBlock);
            }
        }

        private void ProcessCommandDataBlock(CommandBlock commandBlock)
        {
            if (KnownPlayerIds.Contains(commandBlock.playerId.ToString()) == false)
            {
                Debug.WriteLine($"detected unknown playerId in CommandBlock: ${commandBlock.playerId} - time elapsed: ${TotalTimeTracker}");
                return;
            }

            var currentPlayer = Players[commandBlock.playerId];
            currentPlayer.CurrentTimePlayed = TotalTimeTracker;
            currentPlayer._lastActionWasDeselect = false;

            foreach (var action in commandBlock.actions)
            {
                HandleActionBlock(action, currentPlayer);
            }
        }

        // TODO: check if this reflection type of switch case is slow...
        private void HandleActionBlock(W3Action action, Player currentPlayer)
        {
            switch (action)
            {
                case UnitBuildingAbilityActionNoParams UnitBuildingAbilityActionNoParams:
                    var id = ObjectIdFormatter(UnitBuildingAbilityActionNoParams.itemId).Value;
                    if (id == "tert" || id == "tret")
                    {
                        currentPlayer.HandleRetraining(TotalTimeTracker);
                    }
                    currentPlayer.Handle0x10(
                        ObjectIdFormatter(UnitBuildingAbilityActionNoParams.itemId),
                        TotalTimeTracker
                        );
                    break;
                case UnitBuildingAbilityActionTargetPosition UnitBuildingAbilityActionTargetPosition:
                    currentPlayer.Handle0x11(
                        ObjectIdFormatter(UnitBuildingAbilityActionTargetPosition.itemId),
                        TotalTimeTracker
                        );
                    break;
                case UnitBuildingAbilityActionTargetPositionTargetObjectId UnitBuildingAbilityActionTargetPositionTargetObjectId:
                    currentPlayer.Handle0x12(ObjectIdFormatter(UnitBuildingAbilityActionTargetPositionTargetObjectId.itemId));
                    break;
                case GiveItemToUnitAciton GiveItemToUnitAciton:
                    currentPlayer.Handle0x13();
                    break;
                case UnitBuildingAbilityActionTwoTargetPositions UnitBuildingAbilityActionTwoTargetPositions:
                    currentPlayer.Handle0x14(ObjectIdFormatter(UnitBuildingAbilityActionTwoTargetPositions.itemId1));
                    break;
                case ChangeSelectionAction ChangeSelectionAction:
                    if (ChangeSelectionAction.selectMode == 0x02)
                    {
                        currentPlayer._lastActionWasDeselect = true;
                        currentPlayer.Handle0x16(ChangeSelectionAction.selectMode, true);
                    }
                    else
                    {
                        if (currentPlayer._lastActionWasDeselect == false)
                        {
                            currentPlayer.Handle0x16(ChangeSelectionAction.selectMode, true);
                        }
                        currentPlayer._lastActionWasDeselect = false;
                    }
                    break;
                case AssignGroupHotkeyAction a:
                case SelectGroupHotkeyAction b:
                case SelectGroundItemAction c:
                case CancelHeroRevival d:
                case RemoveUnitFromBuildingQueue e:
                case ESCPressedAction f:
                case ChooseHeroSkillSubmenu g:
                case EnterBuildingSubmenu h:
                    currentPlayer.HandleOther(action);
                    break;
                case TransferResourcesAction TransferResourcesAction:
                    var playerId = GetPlayerBySlotId(TransferResourcesAction.slot);
                    if (playerId != null)
                    {
                        var actionWithoutId = new TransferResourcesActionWithPlayer
                        {
                            gold = TransferResourcesAction.gold,
                            Id = TransferResourcesAction.Id,
                            lumber = TransferResourcesAction.lumber,
                            playerId = (byte)playerId,
                            playerName = Players[(byte)playerId].Name,
                        };
                        currentPlayer.Handle0x51(actionWithoutId);
                    }
                    break;
                case W3MMDAction W3MMDAction:
                    this.W3MMD.Add(W3MMDAction);
                    break;
                case TriggerChatCommand triggerChatCommand:
                    triggerChatCommand.playerId = currentPlayer.Id;
                    triggerChatCommand.timeMS = TotalTimeTracker;
                    TriggerChatCommand.Add(triggerChatCommand);
                    break;
            }
        }

        private byte? GetPlayerBySlotId(byte slot)
        {
            byte? playerId;
            SlotToPlayerId.TryGetValue(slot, out playerId);
            return playerId;
        }

        private ItemID ObjectIdFormatter(byte[] arr)
        {
            if (arr[3] >= 0x41 && arr[3] <= 0x7a)
            {
                return new ItemID
                {
                    Value = string.Join("", arr.Reverse())
                };
            }

            return new ItemID
            {
                Value = arr.ToString(),
                IsAlphanumeric = true
            };
        }
    }
}
