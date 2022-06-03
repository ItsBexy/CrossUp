using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedField.Global

namespace CrossUp;
public sealed unsafe partial class CrossUp
{
    // handy reference (and some default properties) for all the hotbar nodes we're working with
    public class Bars
    {
        /// <summary>The Main Cross Hotbar</summary>
        public class Cross
        {
            public string Name => $"Cross Hotbar Set {SetID}";
            public static AtkUnitBase* Base { get; set; } = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            private static AddonActionBarBase* AddonBase => (AddonActionBarBase*)Base;
            private static AddonActionCross* AddonCross => (AddonActionCross*)Base;
            private static AtkResNode** NodeList => Base->UldManager.NodeList;
            public static int NodeCount => Base->UldManager.NodeListSize;
            public static bool Exist => BaseCheck(Base) || BaseCheck((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1));
            public static bool Enabled => LastEnabledState= CharConfig.CrossEnabled;
            public static bool LastEnabledState = true;
            public static bool EnableStateChanged => LastEnabledState != Enabled;
            public static int LastKnownSelection = 0;
            public static int Selection => Exist ? 
                       AddonCross->LeftBar       ? 1 :
                       AddonCross->RightBar      ? 2 :
                       AddonCross->LRBar > 0     ? 3 :
                       AddonCross->RLBar > 0     ? 4 :
                       WXLL.AddonCross->Selected ? 5 :
                       WXLL.AddonCross->Selected ? 6 : 
                                                   0 :
                                   LastKnownSelection;
            public static NodeWrapper Root => new() { Node = NodeList[0] };
            public static NodeWrapper Component => new() { Node = NodeList[1], DefaultPos = { X = 18F, Y = 79F } };
            public static NodeWrapper SelectBG => new() { Node = NodeList[4], DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelectL => new() { Node = NodeList[5], DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper MiniSelectR => new() { Node = NodeList[6], DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper VertLine => new() { Node = NodeList[7], DefaultPos = { X = 271F, Y = 21F }, DefaultSize = { X = 9, Y = 76 } };
            public static NodeWrapper RTtext => new() { Node = NodeList[19], DefaultPos = { X = 367F, Y = 11F }};
            public static NodeWrapper LTtext => new() { Node = NodeList[20], DefaultPos = { X = 83F, Y = 11F } };
            public static NodeWrapper SetText => new() { Node = NodeList[21], DefaultPos = { X = 230F, Y = 170F } };
            public static NodeWrapper Padlock => new() { Node = NodeList[26], DefaultPos = { X = 284F, Y = 152F } };
            public static NodeWrapper PadlockIcon => new() { Node = Padlock.ChildNode(1) };
            public static NodeWrapper ChangeSet => new() { Node = NodeList[27], DefaultPos = { X = 146F, Y = 0F } };
            public static class Left
            {
                public static NodeWrapper GroupL => new() { Node = NodeList[11], DefaultPos = { X = 0F, Y = 0F } };
                public static NodeWrapper GroupR => new() { Node = NodeList[10], DefaultPos = { X = 138F, Y = 0F } };
            }
            public static class Right
            {
                public static NodeWrapper GroupL => new() { Node = NodeList[9], DefaultPos = { X = 284F, Y = 0F } };
                public static NodeWrapper GroupR => new() { Node = NodeList[8], DefaultPos = { X = 422F, Y = 0F } };
            }
            public static Action[] Actions { get; private set; } = new Action[16];
            public static void UpdateActions() => Actions = CrossUp.Actions.GetByBarID(AddonCross->PetBar ? 19 : SetID, 16);
            public static int LastKnownSetID;
            private static int SetID => AddonBase->HotbarID;
        }

        /// <summary>The Left WXHB</summary>
        public class WXLL
        {
            public string Name = "Left WXHB";
            public static AtkUnitBase* Base { get; set; } = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);
            public static AddonActionDoubleCrossBase* AddonCross => (AddonActionDoubleCrossBase*)Base;
            private static AtkResNode** NodeList => Base->UldManager.NodeList;
            public static int NodeCount => Base->UldManager.NodeListSize;
            public static NodeWrapper SelectBG => new() { Node = NodeList[3], DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelect => new() { Node = NodeList[4], DefaultSize = { X = 166, Y = 140 } };
        }
        /// <summary>The Right WXHB</summary>
        public class WXRR
        {
            public string Name = "Right WXHB";
            public static AtkUnitBase* Base { get; set; } = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);
            public static AddonActionDoubleCrossBase* AddonCross => (AddonActionDoubleCrossBase*)Base;
            private static AtkResNode** NodeList => Base->UldManager.NodeList;
            public static int NodeCount => Base->UldManager.NodeListSize;
            public static NodeWrapper SelectBG => new() { Node = NodeList[3], DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelect => new() { Node = NodeList[4], DefaultSize = { X = 166, Y = 140 } };
        }

        /// <summary>The L->R Expanded Hold Bar</summary>
        public class LR
        {
            public string Name = "Expanded Hold L->R";
            public static int BorrowID = -1;
            public static ActionBar BorrowBar => ActionBars[BorrowID];
            public static Action[] Actions
            {
                get
                {
                    var mapSet = CrossUp.Actions.LRMap;
                    return CrossUp.Actions.GetByBarID(mapSet.BarID, 8, mapSet.UseLeft ? 0 : 8);
                }
            }
        }
        /// <summary>The R->L Expanded Hold Bar</summary>
        public class RL
        {
            public string Name = "Expanded Hold R->L";
            public static int BorrowID = -1;
            public static ActionBar BorrowBar => ActionBars[BorrowID];
            public static Action[] Actions
            {
                get
                {
                    var mapSet = CrossUp.Actions.RLMap;
                    return CrossUp.Actions.GetByBarID(mapSet.BarID, 8, mapSet.UseLeft ? 0 : 8);
                }
            }
        }

        /// <summary>Mouse/KB Action Bars</summary>
        public static readonly ActionBar[] ActionBars = new ActionBar[10];
        public class ActionBar
        {
            public int BarID;
            public string Name => $"Hotbar {BarID+1}";
            public AtkUnitBase* Base;
            private AtkResNode** NodeList => Base->UldManager.NodeList;
            public bool Exist => BaseCheck(Base);
            public int NodeCount => Base->UldManager.NodeListSize;
            public NodeWrapper Root => new() {Node = NodeList[0], DefaultSize = DefaultBarSizes[CharConfig.Hotbar.GridType[BarID]] };
            public NodeWrapper BarNumText => NodeList[24];
            public NodeWrapper[] Button = new NodeWrapper[12];
            public Action[] Actions => CrossUp.Actions.GetByBarID(BarID, 12);
        }
        /// <summary>Updates the references for the Cross Hotbar UnitBases, and populates the Mouse/KB Action Bar data</summary>
        public static void Init()
        {
            Cross.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            WXLL.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);
            WXRR.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);

            for (var i = 0; i < 10; i++)
            {
                var barBase = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar" + (i == 0 ? "" : "0" + i), 1);
                var nodeList = barBase->UldManager.NodeList;
                var button = new NodeWrapper[12];
                int gridType = CharConfig.Hotbar.GridType[i];

                for (var b = 0; b < 12; b++) button[b] = new() { Node = nodeList[20 - b], DefaultPos = DefaultBarGrids[gridType, b] };

                ActionBars[i] = new()
                {
                    BarID = i,
                    Base = barBase,
                    Button = button
                };
            }
        }
        /// <summary>Check the existence of a particular AtkUnitBase</summary>
        private static bool BaseCheck(AtkUnitBase* unitBase) => unitBase != null && unitBase->UldManager.NodeListSize > 0 && unitBase->X > 0 && unitBase->Y > 0;

        /// <summary>Keeps track of the original actions from the Mouse/KB bars, before being borrowed/altered</summary>
        public static readonly Action[]?[] StoredActions = new Action[10][];

        /// <summary>Original sizes for the Mouse/KB bars, by [int gridType]</summary>
        private static readonly Vector2[] DefaultBarSizes =
        {
            new() { X = 624, Y = 72 },
            new() { X = 331, Y = 121 },
            new() { X = 241, Y = 170 },
            new() { X = 162, Y = 260 },
            new() { X = 117, Y = 358 },
            new() { X = 72, Y = 618 }
        };
        /// <summary>Original button positions for the Mouse/KB bars, by [int gridType]</summary>
        private static readonly Vector2[,] DefaultBarGrids =
        {
            {
                new() { X = 34, Y = 0 },
                new() { X = 79, Y = 0 },
                new() { X = 124, Y = 0 },
                new() { X = 169, Y = 0 },
                new() { X = 214, Y = 0 },
                new() { X = 259, Y = 0 },
                new() { X = 304, Y = 0 },
                new() { X = 349, Y = 0 },
                new() { X = 394, Y = 0 },
                new() { X = 439, Y = 0 },
                new() { X = 484, Y = 0 },
                new() { X = 529, Y = 0 }
            },
            {
                new() { X = 34, Y = 0 },
                new() { X = 79, Y = 0 },
                new() { X = 124, Y = 0 },
                new() { X = 169, Y = 0 },
                new() { X = 214, Y = 0 },
                new() { X = 259, Y = 0 },
                new() { X = 34, Y = 49 },
                new() { X = 79, Y = 49 },
                new() { X = 124, Y = 49 },
                new() { X = 169, Y = 49 },
                new() { X = 214, Y = 49 },
                new() { X = 259, Y = 49 }
            },
            {
                new() { X = 34, Y = 0 },
                new() { X = 79, Y = 0 },
                new() { X = 124, Y = 0 },
                new() { X = 169, Y = 0 },
                new() { X = 34, Y = 49 },
                new() { X = 79, Y = 49 },
                new() { X = 124, Y = 49 },
                new() { X = 169, Y = 49 },
                new() { X = 34, Y = 98 },
                new() { X = 79, Y = 98 },
                new() { X = 124, Y = 98 },
                new() { X = 169, Y = 98 }
            },
            {
                new() { X = 0, Y = 0 },
                new() { X = 45, Y = 0 },
                new() { X = 90, Y = 0 },
                new() { X = 0, Y = 49 },
                new() { X = 45, Y = 49 },
                new() { X = 90, Y = 49 },
                new() { X = 0, Y = 98 },
                new() { X = 45, Y = 98 },
                new() { X = 90, Y = 98 },
                new() { X = 0, Y = 147 },
                new() { X = 45, Y = 147 },
                new() { X = 90, Y = 147 }
            },
            {
                new() { X = 0, Y = 0 },
                new() { X = 45, Y = 0 },
                new() { X = 0, Y = 49 },
                new() { X = 45, Y = 49 },
                new() { X = 0, Y = 98 },
                new() { X = 45, Y = 98 },
                new() { X = 0, Y = 147 },
                new() { X = 45, Y = 147 },
                new() { X = 0, Y = 196 },
                new() { X = 45, Y = 196 },
                new() { X = 0, Y = 245 },
                new() { X = 45, Y = 245 }
            },
            {
                new() { X = 0, Y = 14 },
                new() { X = 0, Y = 59 },
                new() { X = 0, Y = 104 },
                new() { X = 0, Y = 149 },
                new() { X = 0, Y = 194 },
                new() { X = 0, Y = 239 },
                new() { X = 0, Y = 284 },
                new() { X = 0, Y = 329 },
                new() { X = 0, Y = 374 },
                new() { X = 0, Y = 419 },
                new() { X = 0, Y = 464 },
                new() { X = 0, Y = 509 }
            }
        };

        /// <summary>Visibility status of Mouse/KB  bars before they were borrowed</summary>
        public static readonly bool[] WasHidden = new bool[10];
    }
}