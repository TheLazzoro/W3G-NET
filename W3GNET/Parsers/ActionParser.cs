using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using W3GNET.Extensions;

namespace W3GNET.Parsers
{
    public interface W3Action
    {
        int Id { get; }
    }

    public class PauseGame : W3Action
    {
        public int Id { get; set; } = 0x01;
        public int PlayerId;
        public int timeMS;
    }

    public class ResumeGame : W3Action
    {
        public int Id { get; set; } = 0x02;
        public int PlayerId;
        public int timeMS;
    }

    public class UnitBuildingAbilityActionNoParams : W3Action
    {
        public int Id { get; set; } = 0x10;
        public int abilityFlags;
        public byte[] itemId;
    }

    public class UnitBuildingAbilityActionTargetPosition : W3Action
    {
        public int Id { get; set; } = 0x11;
        public int abilityFlags;
        public byte[] itemId;
        public float targetX;
        public float targetY;
    }

    public class UnitBuildingAbilityActionTargetPositionTargetObjectId : W3Action
    {
        public int Id { get; set; } = 0x12;
        public int abilityFlags;
        public byte[] itemId;
        public float targetX;
        public float targetY;
        public uint objectId1;
        public uint objectId2;
    }

    public class TransferResourcesAction : W3Action
    {
        public int Id { get; set; } = 0x51;
        public byte slot;
        public uint gold;
        public uint lumber;
    }

    public class GiveItemToUnitAciton : W3Action
    {
        public int Id { get; set; } = 0x13;
        public int abilityFlags;
        public byte[] itemId;
        public float targetX;
        public float targetY;
        public uint objectId1;
        public uint objectId2;
        public uint itemObjectId1;
        public uint itemObjectId2;
    }

    public class UnitBuildingAbilityActionTwoTargetPositions : W3Action
    {
        public int Id { get; set; } = 0x14;
        public int abilityFlags;
        public byte[] itemId1;
        public float targetAX;
        public float targetAY;
        public byte[] itemId2;
        public float targetBX;
        public float targetBY;
    }

    public class ChangeSelectionAction : W3Action
    {
        public int Id { get; set; } = 0x16;
        public int selectMode;
        public int numberUnits;
        public Tuple<byte[], byte[]>[] actions;
    }

    public class AssignGroupHotkeyAction : W3Action
    {
        public int Id { get; set; } = 0x17;
        public int groupNumber;
        public int numberUnits;
        public Tuple<byte[], byte[]>[] actions;
    }

    public class SelectGroupHotkeyAction : W3Action
    {
        public int Id { get; set; } = 0x18;
        public int groupNumber;
    }

    public class SelectGroundItemAction : W3Action
    {
        public int Id { get; set; } = 0x1c;
        public byte[] itemId1;
        public byte[] itemId2;
    }

    public class SelectSubgroupAction : W3Action
    {
        public int Id { get; set; } = 0x19;
        public byte[] itemId;
        public uint objectId1;
        public uint objectId2;
    }

    public class CancelHeroRevival : W3Action
    {
        public int Id { get; set; } = 0x1d;
        public byte[] itemId1;
        public byte[] itemId2;
    }

    public class ChooseHeroSkillSubmenu : W3Action
    {
        public int Id { get; set; } = 0x65 | 0x66;
    }

    public class EnterBuildingSubmenu : W3Action
    {
        public int Id { get; set; } = 0x67;
    }

    public class ESCPressedAction : W3Action
    {
        public int Id { get; set; } = 0x61;
    }

    public class RemoveUnitFromBuildingQueue : W3Action
    {
        public int Id { get; set; } = 0x1e | 0x1f;
        public int slotNumber;
        public byte[] itemId;
    }

    public class PreSubselectionAction : W3Action
    {
        public int Id { get; set; } = 0x1a;
    }

    public class W3MMDAction : W3Action
    {
        public int Id { get; set; } = 0x6b;
        public string filename;
        public string missionKey;
        public string key;
        public uint value;
    }

    public class TriggerChatCommand : W3Action
    {
        public int Id { get; set; } = 0x60;
        public int unknown1;
        public int unknown2;
        public int playerId;
        public int timeMS;
        public string chatMessage;
    }

    public class SyncData : W3Action
    {
        public int Id { get; set; } = 0x77;
        public int PlayerId;
        public string SyncHeader;
        public string Data;
    }

    public class ActionParser
    {
        public int exceptionCounter = 0;
        BinaryReader reader;
        private List<byte> unknownIds = new List<byte>();

        public ActionParser()
        {

        }

        public List<W3Action> Parse(Stream input)
        {
            reader = new BinaryReader(input);
            var actions = new List<W3Action>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                try
                {
                    var actionId = reader.ReadByte();
                    var action = ParseAction(actionId);
                    if (action != null)
                    {
                        actions.Add(action);
                    }
                }
                catch (Exception ex)
                {
                    exceptionCounter++;
                    Debug.WriteLine(exceptionCounter + ": " + ex.Message);
                    break;
                }
            }

            return actions;
        }

        /// <summary>
        /// TODO: Unfinished parser by 'w3gjs' library.
        /// It reads the blocks incorrectly, throwing exceptions because the stream goes out of bounds.
        /// </summary>
        private W3Action? ParseAction(byte actionId)
        {
            ushort abilityFlags;
            byte[] itemId;
            byte[] itemId2;
            float targetX;
            float targetY;
            uint objectId1;
            uint objectId2;
            uint itemObjectId1;
            uint itemObjectId2;
            float targetAX;
            float targetAY;
            float targetBX;
            float targetBY;
            ushort numberUnits;
            byte groupNumber;
            Tuple<byte[], byte[]>[] actions;
            uint unknown1;
            uint unknown2;
            string syncString1;
            string syncString2;
            string syncString3;

            switch (actionId)
            {
                case 0x00:
                    reader.SkipBytes(6);
                    break;
                case 0x01:
                    return new PauseGame();
                case 0x02:
                    return new ResumeGame();
                case 0x3:
                    reader.ReadByte();
                    break;
                case 0x4:
                case 0x5:
                    break;
                case 0x6: // Save game
                    var saveName = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var saveFileName = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var quicksave = reader.ReadByte();
                    break;
                case 0x7:
                    reader.SkipBytes(4);
                    break;
                case 0x10:
                    abilityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    unknown1 = reader.ReadUInt32(); // unknown, skip
                    unknown2 = reader.ReadUInt32(); // unknown, skip
                    return new UnitBuildingAbilityActionNoParams { abilityFlags = abilityFlags, itemId = itemId };
                case 0x11:
                    abilityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    unknown1 = reader.ReadUInt32(); // unknown, skip
                    unknown2 = reader.ReadUInt32(); // unknown, skip
                    targetX = reader.ReadSingle();
                    targetY = reader.ReadSingle();
                    return new UnitBuildingAbilityActionTargetPosition { abilityFlags = abilityFlags, itemId = itemId, targetX = targetX, targetY = targetY };
                case 0x12:
                    abilityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    unknown1 = reader.ReadUInt32(); // unknown, skip
                    unknown2 = reader.ReadUInt32(); // unknown, skip
                    targetX = reader.ReadSingle();
                    targetY = reader.ReadSingle();
                    objectId1 = reader.ReadUInt32();
                    objectId2 = reader.ReadUInt32();
                    return new UnitBuildingAbilityActionTargetPositionTargetObjectId
                    {
                        abilityFlags = abilityFlags,
                        itemId = itemId,
                        targetX = targetX,
                        targetY = targetY,
                        objectId1 = objectId1,
                        objectId2 = objectId2,
                    };
                case 0x13:
                    abilityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    unknown1 = reader.ReadUInt32(); // unknown, skip
                    unknown2 = reader.ReadUInt32(); // unknown, skip
                    targetX = reader.ReadSingle();
                    targetY = reader.ReadSingle();
                    objectId1 = reader.ReadUInt32();
                    objectId2 = reader.ReadUInt32();
                    itemObjectId1 = reader.ReadUInt32();
                    itemObjectId2 = reader.ReadUInt32();
                    return new GiveItemToUnitAciton
                    {
                        abilityFlags = abilityFlags,
                        itemId = itemId,
                        targetX = targetX,
                        targetY = targetY,
                        objectId1 = objectId1,
                        objectId2 = objectId2,
                        itemObjectId1 = itemObjectId1,
                        itemObjectId2 = itemObjectId2,
                    };
                case 0x14:
                    abilityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    unknown1 = reader.ReadUInt32(); // unknown, skip
                    unknown2 = reader.ReadUInt32(); // unknown, skip
                    targetAX = reader.ReadSingle();
                    targetAY = reader.ReadSingle();
                    itemId2 = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    reader.SkipBytes(9);
                    targetBX = reader.ReadSingle();
                    targetBY = reader.ReadSingle();
                    return new UnitBuildingAbilityActionTwoTargetPositions
                    {
                        abilityFlags = abilityFlags,
                        itemId1 = itemId,
                        targetAX = targetAX,
                        targetAY = targetAY,
                        itemId2 = itemId2,
                        targetBX = targetBX,
                        targetBY = targetBY,
                    };
                case 0x15:
                    var orderFlags = reader.ReadUInt16();
                    var order = reader.ReadUInt32();
                    var handle1 = reader.ReadUInt32();
                    var handle2 = reader.ReadUInt32();

                    // vec2
                    var orderTargetX = reader.ReadInt32();
                    var orderTargetY = reader.ReadInt32();

                    // net tag
                    var targetObjectX = reader.ReadInt32();
                    var targetObjectY = reader.ReadInt32();

                    var ghostImageId = reader.ReadUInt32();
                    var ghostFlags = reader.ReadUInt32();
                    var ghostCategory = reader.ReadUInt32();
                    var ghostOwner = reader.ReadByte();
                    var ghostPositionX = reader.ReadUInt32();
                    var ghostPositionY = reader.ReadUInt32();
                    var tagetObjectX = reader.ReadUInt32();
                    var tagetObjectY = reader.ReadUInt32();
                    return null;
                case 0x16:
                    var selectMode = reader.ReadByte();
                    numberUnits = reader.ReadUInt16();
                    actions = ReadSelectionUnits(numberUnits);
                    return new ChangeSelectionAction { selectMode = selectMode, numberUnits = numberUnits, actions = actions };
                case 0x17:
                    groupNumber = reader.ReadByte();
                    numberUnits = reader.ReadUInt16();
                    actions = ReadSelectionUnits(numberUnits);
                    return new AssignGroupHotkeyAction { groupNumber = groupNumber, numberUnits = numberUnits, actions = actions };
                case 0x18:
                    groupNumber = reader.ReadByte();
                    reader.ReadByte(); // skip
                    return new SelectGroupHotkeyAction { groupNumber = groupNumber };
                case 0x19:
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    objectId1 = reader.ReadUInt32();
                    objectId2 = reader.ReadUInt32();
                    return new SelectSubgroupAction { itemId = itemId, objectId1 = objectId1, objectId2 = objectId2 };
                case 0x21: // Deprecated? Or wrong assumtion about the format?
                    reader.SkipBytes(8);
                    break;
                case 0x1a:
                    return new PreSubselectionAction();
                case 0x1b:
                    reader.SkipBytes(9);
                    return null;
                case 0x1c:
                    reader.ReadByte(); // skip
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    itemId2 = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    return new SelectGroundItemAction { itemId1 = itemId, itemId2 = itemId2 };
                case 0x1d:
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    itemId2 = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    return new CancelHeroRevival { itemId1 = itemId, itemId2 = itemId2 };
                case 0x1e:
                    var slotNumber = reader.ReadByte();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    return new RemoveUnitFromBuildingQueue { slotNumber = slotNumber, itemId = itemId };
                case 0x1f: // Deprecated? Or wrong assumtion about the format?
                    return null;
                case 0x27:
                case 0x28:
                case 0x2d:
                    reader.SkipBytes(5); // We should not even hit these. They are cheat-code actions.
                    break;
                case 0x2e:
                    reader.SkipBytes(4); // We should not even hit these. They are cheat-code actions.
                    break;
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36:
                    reader.SkipBytes(6);
                    return null;
                case 0x41: // Deprecated? Or wrong assumtion about the format?
                    reader.SkipBytes(4);
                    break;
                case 0x47:
                    reader.SkipBytes(21);
                    return null;
                case 0x48:
                    reader.SkipBytes(6);
                    return null;
                case 0x50: // Change ally options
                    reader.ReadByte();
                    reader.ReadUInt32();
                    return null;
                case 0x51:
                    var slot = reader.ReadByte();
                    var gold = reader.ReadUInt32();
                    var lumber = reader.ReadUInt32();
                    return new TransferResourcesAction { slot = slot, gold = gold, lumber = lumber };
                case 0x60:
                    var u1 = reader.ReadInt32();
                    var u2 = reader.ReadInt32();
                    var chatmessage = reader.ReadZeroTermString(StringEncoding.UTF8);
                    return new TriggerChatCommand { chatMessage = chatmessage, unknown1 = u1, unknown2 = u2 };
                case 0x61:
                    return new ESCPressedAction();
                case 0x62: // ResumeTriggerExec
                    reader.SkipBytes(12);
                    return null;
                case 0x63: // TriggerSyncReady
                    reader.SkipBytes(8);
                    return null;
                case 0x64: // TrackableHit
                    reader.SkipBytes(8);
                    return null;
                case 0x65: // TrackableTrack
                    reader.SkipBytes(8);
                    return null;
                case 0x66:
                    return new ChooseHeroSkillSubmenu();
                case 0x67:
                    return new EnterBuildingSubmenu();
                case 0x68: // minimap signal
                    targetX = reader.ReadSingle();
                    targetY = reader.ReadSingle();
                    var duration = reader.ReadSingle();
                    return null;
                case 0x69: // DialogButtonClick 
                    reader.SkipBytes(16);
                    return null;
                case 0x6a: // DialogClick
                    reader.SkipBytes(16);
                    return null;
                case 0x6b: // SyncStoreInteger
                    var filename = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var missionkey = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var key = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var value = reader.ReadUInt32();
                    return new W3MMDAction { filename = filename, missionKey = missionkey, key = key, value = value };

                case 0x6c: // SyncStoreReal
                case 0x6d: // SyncStoreBoolean
                    syncString1 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    syncString2 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    syncString3 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    reader.SkipBytes(4); // stored value
                    return null;
                case 0x6e: // SyncStoreUnit
                    syncString1 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    syncString2 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    syncString3 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var unitId = reader.ReadUInt32();
                    var itemCount = reader.ReadUInt32();
                    for (int i = 0; i < itemCount; i++)
                    {
                        var item_id = reader.ReadInt32();
                        var charges = reader.ReadInt32();
                        var flags = reader.ReadInt32();
                    }
                    // sync hero data
                    var xp = reader.ReadUInt32();
                    var level = reader.ReadUInt32();
                    var skillPoints = reader.ReadUInt32();
                    var properNameId = reader.ReadUInt32();
                    var strength = reader.ReadUInt32();
                    var strengthBonus = reader.ReadSingle();
                    var agility = reader.ReadUInt32();
                    var speedMod = reader.ReadSingle();
                    var cooldownMod = reader.ReadSingle();
                    var agilityBonus = reader.ReadSingle();
                    var intelligence = reader.ReadUInt32();
                    var intelligenceBonus = reader.ReadSingle();
                    var heroAbilityCount = reader.ReadUInt32();
                    for (int i = 0; i < heroAbilityCount; i++)
                    {
                        var abilityId = reader.ReadUInt32();
                        var abilityLevel = reader.ReadUInt32();
                    }
                    var maxLife = reader.ReadSingle();
                    var maxMana = reader.ReadSingle();
                    if (true) // TODO: version 6030 and above
                    {
                        var sight = reader.ReadSingle();
                        var damageCount = reader.ReadSingle();
                        for (int i = 0; i < damageCount; i++)
                        {
                            var damage = reader.ReadUInt32();
                        }
                        var defense = reader.ReadSingle();
                    }
                    if (true) // TODO: version 6031 and above
                    {
                        var controlGroup = reader.ReadUInt16();
                    }
                    return null;
                case 0x70: // SyncClearInteger
                case 0x71: // SyncClearReal
                case 0x72: // SyncClearBoolean
                case 0x73: // SyncClearUnit
                    syncString1 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    syncString2 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    syncString3 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    return null;
                case 0x75: // ArrowKey
                    var arrowKey = reader.ReadByte();
                    return null;
                case 0x76: // Mouse
                    var mouseEvent = reader.ReadByte();
                    var mousePositionX = reader.ReadInt32();
                    var mousePositionY = reader.ReadInt32();
                    var mouseButton = reader.ReadByte();
                    return null;
                case 0x77:
                    var prefix = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var syncData = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var fromServer = reader.ReadUInt32();
                    return new SyncData { SyncHeader = prefix, Data = syncData };
                case 0x78:
                    var frameTag1 = reader.ReadUInt32();
                    var frameTag2 = reader.ReadUInt32();
                    var frameEvent = reader.ReadUInt32();
                    var frameEventData = reader.ReadSingle();
                    var frameEventData2 = reader.ReadZeroTermString(StringEncoding.UTF8);
                    return null;
                case 0x79: // KeyEvent
                    var keyTag = reader.ReadUInt32();
                    var keyEvent = reader.ReadUInt32();
                    var keyId = reader.ReadUInt32();
                    var metaKey = reader.ReadUInt32();
                    return null;
                case 0x7a: // CommandClick
                    var commandClickTag1 = reader.ReadUInt32();
                    var commandClickTag2 = reader.ReadUInt32();
                    var commandAbilityId = reader.ReadUInt32();
                    var orderId = reader.ReadUInt32();
                    return null;
                default:
                    unknownIds.Add(actionId);
                    break;
            }

            return null;
        }

        private Tuple<byte[], byte[]>[] ReadSelectionUnits(int length)
        {
            Tuple<byte[], byte[]>[] v = new Tuple<byte[], byte[]>[length];
            for (int i = 0; i < length; i++)
            {
                var obj = new Tuple<byte[], byte[]>(
                    new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    },
                    new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    });

                v[i] = obj;
            }
            return v;
        }
    }
}
