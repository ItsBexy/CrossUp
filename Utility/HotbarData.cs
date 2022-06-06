using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;

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
            public static AtkUnitBase* Base { get; set; } = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            private static AddonActionBarBase* AddonBase => (AddonActionBarBase*)Base;
            private static AddonActionCross* AddonCross => (AddonActionCross*)Base;
            private static AtkResNode* NodeAtIndex(int i) => Base->UldManager.NodeListSize > i ? Base->UldManager.NodeList[i] : null;
            public static bool Exist => BaseCheck(Base) || BaseCheck((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1));
            public static bool Enabled => LastEnabledState= CharConfig.CrossEnabled;
            private static bool LastEnabledState = true;
            public static bool EnableStateChanged => LastEnabledState != Enabled;
            public static int LastKnownSelection = 0;
            public static int Selection => Exist ? 
                       AddonCross->LeftBar       ? 1 :
                       AddonCross->RightBar      ? 2 :
                       AddonCross->LRBar > 0     ? 3 :
                       AddonCross->RLBar > 0     ? 4 :
                       WXLL.AddonCross->Selected ? 5 :
                       WXRR.AddonCross->Selected ? 6 : 
                                                   0 :
                                   LastKnownSelection;
            public static NodeWrapper Root => new() { Node = NodeAtIndex(0) };
            public static NodeWrapper Component => new() { Node = NodeAtIndex(1), DefaultPos = { X = 18F, Y = 79F } };
            public static NodeWrapper SelectBG => new() { Node = NodeAtIndex(4), DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelectL => new() { Node = NodeAtIndex(5), DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper MiniSelectR => new() { Node = NodeAtIndex(6), DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper VertLine => new() { Node = NodeAtIndex(7), DefaultPos = { X = 271F, Y = 21F }, DefaultSize = { X = 9, Y = 76 } };
            public static NodeWrapper RTtext => new() { Node = NodeAtIndex(19), DefaultPos = { X = 367F, Y = 11F }};
            public static NodeWrapper LTtext => new() { Node = NodeAtIndex(20), DefaultPos = { X = 83F, Y = 11F } };
            public static NodeWrapper SetDisplay => new() { Node = NodeAtIndex(21), DefaultPos = { X = 230F, Y = 170F } };
            public static NodeWrapper SetText => new() { Node = NodeAtIndex(25) };
            public static NodeWrapper Padlock => new() { Node = NodeAtIndex(26), DefaultPos = { X = 284F, Y = 152F } };
            public static NodeWrapper PadlockIcon => new() { Node = Padlock.ChildNode(1) };
            public static NodeWrapper ChangeSet => new() { Node = NodeAtIndex(27), DefaultPos = { X = 146F, Y = 0F } };
            public static NodeWrapper ChangeSetText => new() { Node = NodeAtIndex(41) };
            public static class Left
            {
                public static NodeWrapper GroupL => new() { Node = NodeAtIndex(11), DefaultPos = { X = 0F, Y = 0F } };
                public static NodeWrapper GroupR => new() { Node = NodeAtIndex(10), DefaultPos = { X = 138F, Y = 0F } };
            }
            public static class Right
            {
                public static NodeWrapper GroupL => new() { Node = NodeAtIndex(9), DefaultPos = { X = 284F, Y = 0F } };
                public static NodeWrapper GroupR => new() { Node = NodeAtIndex(8), DefaultPos = { X = 422F, Y = 0F } };
            }
            public static Action[] Actions { get; private set; } = new Action[16];
            public static void UpdateActions() => Actions = CrossUp.Actions.GetByBarID(AddonCross->PetBar ? 19 : SetID, 16);
            public static int LastKnownSetID;
            private static int SetID => AddonBase->HotbarID;
        }
        /// <summary>The Left WXHB</summary>
        public class WXLL
        {
            public static AtkUnitBase* Base { get; set; } = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);
            public static bool Exist => BaseCheck(Base) || BaseCheck((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1));
            public static AddonActionDoubleCrossBase* AddonCross => (AddonActionDoubleCrossBase*)Base;
            private static AtkResNode* NodeAtIndex(int i) => Base->UldManager.NodeListSize > i ? Base->UldManager.NodeList[i] : null;
            public static NodeWrapper SelectBG => new() { Node = NodeAtIndex(3), DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelect => new() { Node = NodeAtIndex(4), DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper GroupL => new() { Node = NodeAtIndex(5) };
            public static NodeWrapper GroupR => new() { Node = NodeAtIndex(6) };
        }
        /// <summary>The Right WXHB</summary>
        public class WXRR
        {
            public static AtkUnitBase* Base { get; set; } = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);
            public static bool Exist => BaseCheck(Base) || BaseCheck((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1));
            public static AddonActionDoubleCrossBase* AddonCross => (AddonActionDoubleCrossBase*)Base;
            private static AtkResNode* NodeAtIndex(int i) => Base->UldManager.NodeListSize > i ? Base->UldManager.NodeList[i] : null;
            public static NodeWrapper SelectBG => new() { Node = NodeAtIndex(3), DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelect => new() { Node = NodeAtIndex(4), DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper GroupL => new() { Node = NodeAtIndex(5) };
            public static NodeWrapper GroupR => new() { Node = NodeAtIndex(6) };
        }
        /// <summary>The L->R Expanded Hold Bar</summary>
        public class LR
        {
            public static int BorrowID => Config.borrowBarL;
            public static ActionBar BorrowBar => ActionBars[BorrowID];
            public static Action[] Actions
            {
                get
                {
                    var mapSet = CrossUp.Actions.LRMap;
                    return CrossUp.Actions.GetByBarID(mapSet.BarID, 8, mapSet.UseLeft ? 0 : 8);
                }
            }
            public static bool Exist => BaseCheck(BorrowID);
        }
        /// <summary>The R->L Expanded Hold Bar</summary>
        public class RL
        {
            public static int BorrowID => Config.borrowBarR;
            public static ActionBar BorrowBar => ActionBars[BorrowID];
            public static Action[] Actions
            {
                get
                {
                    var mapSet = CrossUp.Actions.RLMap;
                    return CrossUp.Actions.GetByBarID(mapSet.BarID, 8, mapSet.UseLeft ? 0 : 8);
                }
            }
            public static bool Exist => BaseCheck(BorrowBar.BarID);
        }
        /// <summary>The "Duty Action" pane (ie, in Bozja/Eureka)</summary>
        public class ActionContents
        {
            public static AtkUnitBase* Base { get; set; } = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionContents", 1);
            public static bool Exist => BaseCheck(Base) || BaseCheck((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionContents", 1));
            private static AtkResNode* NodeAtIndex(int i) => Base->UldManager.NodeListSize > i ? Base->UldManager.NodeList[i] : null;
            public static NodeWrapper BG1 => new() { Node = NodeAtIndex(3) };
            public static NodeWrapper BG2 => new() { Node = NodeAtIndex(4) };
            public static NodeWrapper BG3 => new() { Node = NodeAtIndex(6) };
            public static NodeWrapper BG4 => new() { Node = NodeAtIndex(7) };
        }
        /// <summary>Mouse/KB Action Bars</summary>
        public static readonly ActionBar[] ActionBars = new ActionBar[10];
        public class ActionBar
        {
            public int BarID;
            public AtkUnitBase* Base;
            public bool Exist => BaseCheck(Base);
            private AtkResNode* NodeAtIndex(int i) => Base->UldManager.NodeListSize > i ? Base->UldManager.NodeList[i] : null;
            public NodeWrapper Root => new() {Node = NodeAtIndex(0), DefaultSize = DefaultBarSizes[CharConfig.Hotbar.GridType[BarID]] };
            public NodeWrapper BarNumText => NodeAtIndex(24);
            public NodeWrapper[] Button = new NodeWrapper[12];
            public Action[] Actions => CrossUp.Actions.GetByBarID(BarID, 12);
        }
        /// <summary>Updates the references for the Cross Hotbar UnitBases, and populates the Mouse/KB Action Bar data</summary>
        public static void Init()
        {
            Cross.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            WXLL.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);
            WXRR.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);
            ActionContents.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionContents", 1);

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
        /// <summary>Check the existence of a particular bar's AtkUnitBase</summary>
        public static bool BaseCheck(int barID) => BaseCheck(ActionBars[barID].Base);
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