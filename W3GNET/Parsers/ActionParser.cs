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

    public class ActionParser
    {
        BinaryReader reader;

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
                    Debug.WriteLine(ex.Message);
                }
            }

            return actions;
        }

        private W3Action? ParseAction(byte actionId)
        {
            ushort abiityFlags;
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

            switch (actionId)
            {
                case 0x1:
                    break;
                case 0x2:
                    break;
                case 0x3:
                    reader.ReadByte(); // skip
                    break;
                case 0x4:
                case 0x5:
                    break;
                case 0x6:
                    reader.ReadZeroTermString(StringEncoding.UTF8); // skip
                    break;
                case 0x7:
                    reader.ReadByte(); // skip
                    reader.ReadByte(); // skip
                    break;
                case 0x10:
                    abiityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    reader.ReadUInt32(); // unknown, skip
                    reader.ReadUInt32(); // unknown, skip
                    return new UnitBuildingAbilityActionNoParams { abilityFlags = abiityFlags, itemId = itemId };
                case 0x11:
                    abiityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    reader.ReadUInt32(); // unknown, skip
                    reader.ReadUInt32(); // unknown, skip
                    targetX = reader.ReadSingle();
                    targetY = reader.ReadSingle();
                    return new UnitBuildingAbilityActionTargetPosition { abilityFlags = abiityFlags, itemId = itemId, targetX = targetX, targetY = targetY };
                case 0x12:
                    abiityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    reader.ReadUInt32(); // unknown, skip
                    reader.ReadUInt32(); // unknown, skip
                    targetX = reader.ReadSingle();
                    targetY = reader.ReadSingle();
                    objectId1 = reader.ReadUInt32();
                    objectId2 = reader.ReadUInt32();
                    return new UnitBuildingAbilityActionTargetPositionTargetObjectId
                    {
                        abilityFlags = abiityFlags,
                        itemId = itemId,
                        targetX = targetX,
                        targetY = targetY,
                        objectId1 = objectId1,
                        objectId2 = objectId2,
                    };
                case 0x13:
                    abiityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    reader.ReadUInt32(); // unknown, skip
                    reader.ReadUInt32(); // unknown, skip
                    targetX = reader.ReadSingle();
                    targetY = reader.ReadSingle();
                    objectId1 = reader.ReadUInt32();
                    objectId2 = reader.ReadUInt32();
                    itemObjectId1 = reader.ReadUInt32();
                    itemObjectId2 = reader.ReadUInt32();
                    return new GiveItemToUnitAciton
                    {
                        abilityFlags = abiityFlags,
                        itemId = itemId,
                        targetX = targetX,
                        targetY = targetY,
                        objectId1 = objectId1,
                        objectId2 = objectId2,
                        itemObjectId1 = itemObjectId1,
                        itemObjectId2 = itemObjectId2,
                    };
                case 0x14:
                    abiityFlags = reader.ReadUInt16();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    reader.ReadUInt32(); // unknown, skip
                    reader.ReadUInt32(); // unknown, skip
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
                        abilityFlags = abiityFlags,
                        itemId1 = itemId,
                        targetAX = targetAX,
                        targetAY = targetAY,
                        itemId2 = itemId2,
                        targetBX = targetBX,
                        targetBY = targetBY,
                    };
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
                case 0x1f:
                    var slotNumber = reader.ReadByte();
                    itemId = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    return new RemoveUnitFromBuildingQueue { slotNumber = slotNumber, itemId = itemId };
                case 0x27:
                case 0x28:
                case 0x2d:
                    reader.SkipBytes(5);
                    break;
                case 0x2e:
                    reader.SkipBytes(4);
                    break;
                case 0x50:
                    reader.ReadByte();
                    reader.ReadUInt32();
                    return null;
                case 0x51:
                    var slot = reader.ReadByte();
                    var gold = reader.ReadUInt32();
                    var lumber = reader.ReadUInt32();
                    return new TransferResourcesAction { slot = slot, gold = gold, lumber = lumber };
                case 0x60:
                    reader.SkipBytes(8);
                    reader.ReadZeroTermString(StringEncoding.UTF8);
                    return null;
                case 0x61:
                    return new ESCPressedAction();
                case 0x62:
                    reader.SkipBytes(12);
                    return null;
                case 0x65:
                case 0x66:
                    return new ChooseHeroSkillSubmenu();
                case 0x67:
                    return new EnterBuildingSubmenu();
                case 0x68:
                    reader.SkipBytes(12);
                    return null;
                case 0x69:
                case 0x6a:
                    reader.SkipBytes(16);
                    return null;
                case 0x6b:
                    var filename = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var missionkey = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var key = reader.ReadZeroTermString(StringEncoding.UTF8);
                    var value = reader.ReadUInt32();
                    return new W3MMDAction { filename = filename, missionKey = missionkey, key = key, value = value };
                case 0x75:
                    reader.SkipBytes(1);
                    return null;
                case 0x77:
                    reader.SkipBytes(13);
                    return null;
                case 0x7a:
                    reader.SkipBytes(20);
                    return null;
                case 0x7b:
                    reader.SkipBytes(16);
                    return null;
                default:
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
