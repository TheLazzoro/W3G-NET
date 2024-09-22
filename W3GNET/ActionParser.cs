using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace W3GNET
{
    public interface Action
    {
        int Id { get; }
    }

    public class UnitBuildingAbilityActionNoParams : Action
    {
        public int Id { get; set; } = 0x10;
        public int abilityFlags;
        public byte[] itemId;
    }

    public class UnitBuildingAbilityActionTargetPosition : Action
    {
        public int Id { get; set; } = 0x11;
        public int abilityFlags;
        public byte[] itemId;
        public float targetX;
        public float targetY;
    }

    public class UnitBuildingAbilityActionTargetPositionTargetObjectId : Action
    {
        public int Id { get; set; } = 0x12;
        public int abilityFlags;
        public byte[] itemId;
        public float targetX;
        public float targetY;
        public uint objectId1;
        public uint objectId2;
    }

    public class TransferResourcesAction : Action
    {
        public int Id { get; set; } = 0x51;
        public int slot;
        public int gold;
        public int lumber;
    }

    public class GiveItemToUnitAciton : Action
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

    public class UnitBuildingAbilityActionTwoTargetPositions : Action
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

    public class ChangeSelectionAction : Action
    {
        public int Id { get; set; } = 0x16;
        public int selectMode;
        public int numberUnits;
        public Tuple<byte[], byte[]>[] actions;
    }

    public class AssignGroupHotkeyAction : Action
    {
        public int Id { get; set; } = 0x17;
        public int groupNumber;
        public int numberUnits;
        public Tuple<byte[], byte[]>[] actions;
    }

    public class SelectGroupHotkeyAction : Action
    {
        public int Id { get; set; } = 0x18;
        public int groupNumber;
    }

    public class SelectGroundItemAction : Action
    {
        public int Id { get; set; } = 0x1c;
        public byte[] itemId1;
        public byte[] itemId2;
    }

    public class SelectSubgroupAction : Action
    {
        public int Id { get; set; } = 0x19;
        public byte[] itemId;
        public uint objectId1;
        public uint objectId2;
    }

    public class CancelHeroRevival : Action
    {
        public int Id { get; set; } = 0x1d;
        public byte[] itemId1;
        public byte[] itemId2;
    }

    public class ChooseHeroSkillSubmenu : Action
    {
        public int Id { get; set; } = 0x65 | 0x66;
    }

    public class EnterBuildingSubmenu : Action
    {
        public int Id { get; set; } = 0x67;
    }

    public class ESCPressedAction : Action
    {
        public int Id { get; set; } = 0x61;
    }

    public class RemoveUnitFromBuildingQueue : Action
    {
        public int Id { get; set; } = 0x1e | 0x1f;
        public int slotNumber;
        public byte[] itemId;
    }

    public class PreSubselectionAction : Action
    {
        public int Id { get; set; } = 0x1a;
    }

    public class W3MMDAction : Action
    {
        public int Id { get; set; } = 0x6b;
        public string filename;
        public string missionKey;
        public string key;
        public int value;
    }

    public class ActionParser
    {
        BinaryReader reader;

        public ActionParser()
        {

        }

        public List<Action> Parse(FileStream input)
        {
            this.reader = new BinaryReader(input);
            var actions = new List<Action>();
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

        private Action? ParseAction(byte actionId)
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
                    reader.ReadString(); // skip
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
                    for (int i = 0; i < 9; i++) // skip 9 bytes
                    {
                        reader.ReadByte();
                    }
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
                    for (int i = 0; i < 9; i++) // skip 9 bytes
                    {
                        reader.ReadByte();
                    }
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
                    itemId2 = new byte[]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                    };
                    return new RemoveUnitFromBuildingQueue { slotNumber = slotNumber, itemId = itemId };
                default:
                    break;
            }
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
