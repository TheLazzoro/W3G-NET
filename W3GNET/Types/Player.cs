using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using W3GNET.Util;

namespace W3GNET.Types
{
    public class Ability : AbilityOrRetraining
    {
        public int Time { get; set; }
        public string Value { get; set; }
    }

    public class Retraining : AbilityOrRetraining
    {
        public int Time { get; set; }
    }

    public class HeroInfo
    {
        public int Level { get; set; }
        public Dictionary<string, int> Abilities { get; set; }
        public int Order { get; set; }
        public string Id { get; set; }
        public List<RetrainingHistory> retrainingHistory { get; set; }
        public List<AbilityOrRetraining> AbilityOrder { get; set; }
    }

    public class RetrainingHistory
    {
        public int Time { get; set; }
        public Dictionary<string, int> Abilities { get; set; }
    }

    public class Handle
    {
        public Dictionary<string, int> Summary;
        public List<Order> Order;
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
            switch (actionId[0])
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

        internal void HandleStringEncodedItemId(string actionId, int gametime)
        {
            if (Mappings.Units.ContainsKey(actionId))
            {
                // TODO: more weird TS syntax
                //Units.Summary[actionId] = Units.Summary[actionId] + 1 || 1;
                Units.Summary[actionId] = Units.Summary[actionId] + 1;
                Units.Order.Add(new Order { Id = actionId, MS = gametime });
            }
            else if (Mappings.Items.ContainsKey(actionId))
            {
                Items.Summary[actionId] = Items.Summary[actionId] + 1;
                Items.Order.Add(new Order { Id = actionId, MS = gametime });
            }
            else if (Mappings.Buildings.ContainsKey(actionId))
            {
                Buildings.Summary[actionId] = Buildings.Summary[actionId] + 1;
                Buildings.Order.Add(new Order { Id = actionId, MS = gametime });
            }
            else if (Mappings.Upgrades.ContainsKey(actionId))
            {
                Upgrades.Summary[actionId] = Upgrades.Summary[actionId] + 1;
                Upgrades.Order.Add(new Order { Id = actionId, MS = gametime });
            }
        }

        internal void HandleHeroSkill(string actionId, int gametime)
        {
            var heroId = Mappings.AbilityToHero[actionId];
            if (this.HeroCollector[heroId] == null)
            {
                HeroCount++;
                HeroCollector[heroId] = new HeroInfo
                {
                    Level = 0,
                    Abilities = new Dictionary<string, int>(),
                    Order = HeroCount,
                    Id = heroId,
                    AbilityOrder = new List<AbilityOrRetraining>(),
                    retrainingHistory = new List<RetrainingHistory>(),
                };
            }

            if (_lastRetrainingTime > 0)
            {
                HeroCollector[heroId].retrainingHistory.Add(new RetrainingHistory
                {
                    Time = _lastRetrainingTime,
                    Abilities = HeroCollector[heroId].Abilities,
                });
                HeroCollector[heroId].Abilities = new Dictionary<string, int>();
                HeroCollector[heroId].AbilityOrder.Add(new Retraining
                {
                    Time = _lastRetrainingTime,
                });
                _lastRetrainingTime = 0;
            }

            // TODO: more weird TS syntax
            //HeroCollector[heroId].Abilities[actionId] = HeroCollector[heroId].Abilities[actionId] ?? 0;
            HeroCollector[heroId].Abilities[actionId] = HeroCollector[heroId].Abilities[actionId];
            HeroCollector[heroId].Abilities[actionId] += 1;
            HeroCollector[heroId].AbilityOrder.Add(new Ability
            {
                Time = gametime,
                Value = actionId,
            });
        }

        private void HandleRetraining(int gametime)
        {
            _lastRetrainingTime = gametime;
        }

        private void Handle0x10(ItemID itemId, int gametime)
        {
            switch (itemId.Value[0])
            {
                case 'A':
                    HandleHeroSkill(itemId.Value, gametime);
                    break;
                case 'R':
                    HandleStringEncodedItemId(itemId.Value, gametime);
                    break;
                case 'u':
                case 'e':
                case 'h':
                case 'o':
                    if (string.IsNullOrEmpty(RaceDetected))
                    {
                        DetectRaceByActionId(itemId.Value);
                    }
                    HandleStringEncodedItemId(itemId.Value, gametime);
                    break;
                default:
                    HandleStringEncodedItemId(itemId.Value, gametime);
                    break;
            }

            if (itemId.Value[0] != '0')
                Actions.BuildTrain++;
            else
                Actions.Ability++;

            _currentlyTrackedAPM++;
        }

        private void Handle0x12(ItemID itemId)
        {
            if(IsRightClickAction(itemId))
        }

        private bool IsRightClickAction(int[] input)
        {
            return input[0] == 0x03 && input[1] == 0;
        }

    }
}
