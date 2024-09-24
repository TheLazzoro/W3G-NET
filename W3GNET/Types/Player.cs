using System;
using System.Collections.Generic;
using System.Text;
using W3GNET.Util;

namespace W3GNET.Types
{
    public interface Ability : AbilityOrRetraining
    {
        int Time { get; set; }
        string Value { get; set; }
    }

    public interface Retraining : AbilityOrRetraining
    {
        int Time { get; set; }
    }

    public interface HeroInfo
    {
        int Level { get; set; }
        Dictionary<string, int> Abilities { get; set; }
        int Order { get; set; }
        int Id { get; set; }
        RetrainingHistory retrainingHistory { get; set; }
        AbilityOrRetraining[] AbilityOrder { get; set; }
    }

    public interface RetrainingHistory
    {
        int Time { get; set; }
        Dictionary<string, int> Abilities { get; set; }
    }

    public class Handle
    {
        public Dictionary<string, int> Summary;
        public Order Order;
    }

    public class Order
    {
        public string Id;
        public int MS;
    }

    public class Actions
    {
        public List<float> Timed;
        public int AssignedGroup;
        public int RightClick;
        public int Basic;
        public int BuildTrain;
        public int Ability;
        public int Item;
        public int Select;
        public int RemoveUnit;
        public int SubGroup;
        public int SelectHotkey;
        public int Esc;
    }

    public class Hotkey
    {
        public int Assigned;
        public int Used;
    }

    public interface AbilityOrRetraining
    {
        int Time { get; set; }
    }

    public class RetrainingMetadata
    {
        public int Start;
        public int End;
    }

    public class Player
    {
        public int Id;
        public string Name;
        public int TeamId;
        public string Color;
        public Race Race;
        public string RaceDetected;
        public Handle Units;
        public Handle Upgrades;
        public Handle Items;
        public Handle Buildings;
        public List<HeroInfo> Heroes;
        public Dictionary<string, HeroInfo> HeroCollector;
        public int HeroCount;
        public Actions Actions;
        public Dictionary<int, Hotkey> GroupHotkeys;
        public List<TransferResourceActionWithPlayerAndTimestamp> ResourcesTransfers = new List<TransferResourceActionWithPlayerAndTimestamp>();
        public int _currentlyTrackedAPM;
        public Dictionary<string, RetrainingMetadata> _retrainingMetadata;
        public int _lastRetrainingTime;
        public bool _lastActionWasDeselect;
        public int CurrentTimePlayed;
        public int apm;

        public Player(int id, string name, int teamid, int color, Race race)
        {
            Id = id;
            Name = name;
            TeamId = teamid;
            Color = ConvertUtil.PlayerColor(color);
            Race = race;
            RaceDetected = "";
            Units = new Handle();
            Upgrades = new Handle();
            Items = new Handle();
            Buildings = new Handle();
            Heroes = new List<HeroInfo>();
            HeroCollector = new Dictionary<string, HeroInfo>();
            ResourcesTransfers = new List<TransferResourceActionWithPlayerAndTimestamp>();
            HeroCount = 0;
            Actions = new Actions
            {
                Timed = new List<int>(),
                AssignedGroup = 0,
                RightClick = 0,
                Basic = 0,
                BuildTrain = 0,
                Ability = 0,
                Item = 0,
                Select = 0,
                RemoveUnit = 0,
                SubGroup = 0,
                SelectHotkey = 0,
                Esc = 0,
            };
            GroupHotkeys = new Dictionary<int, Hotkey>
            {
                { 1, new Hotkey { Assigned = 0, Used = 0 } },
                { 2, new Hotkey { Assigned = 0, Used = 0 } },
                { 3, new Hotkey { Assigned = 0, Used = 0 } },
                { 4, new Hotkey { Assigned = 0, Used = 0 } },
                { 5, new Hotkey { Assigned = 0, Used = 0 } },
                { 6, new Hotkey { Assigned = 0, Used = 0 } },
                { 7, new Hotkey { Assigned = 0, Used = 0 } },
                { 8, new Hotkey { Assigned = 0, Used = 0 } },
                { 9, new Hotkey { Assigned = 0, Used = 0 } },
                { 0, new Hotkey { Assigned = 0, Used = 0 } },
            };
            _currentlyTrackedAPM = 0;
            _lastActionWasDeselect = false;
            _retrainingMetadata = new Dictionary<string, RetrainingMetadata>();
            _lastRetrainingTime = 0;
            CurrentTimePlayed = 0;
            apm = 0;
        }

        public void NewActionTrackingSegment(int timeTrackingInterval = 60_000)
        {
            Actions.Timed.Add(MathF.Floor(_currentlyTrackedAPM * (60_000 / timeTrackingInterval)));
            _currentlyTrackedAPM = 0;
        }

        public void DetectRaceByActionId(string actionId)
        {
            switch(actionId[0])
            {
                case 'e':
                    RaceDetected = "N";
                    break;
                case 'o':
                    RaceDetected = "O";
                    break;
                case 'h':
                    RaceDetected = "H";
                    break;
                case 'u':
                    RaceDetected = "U";
                    break;
            }
        }

        public void HandleStringEncodedItemId(string actionId, int gametime)
        {
            if (Units[actionId])
        }

        private bool IsRightClickAction(int[] input)
        {
            return input[0] == 0x03 && input[1] == 0;
        }

    }
}
