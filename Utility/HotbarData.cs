using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberHidesStaticFromOuterClass

namespace CrossUp;
public sealed unsafe partial class CrossUp
{
    /// <summary>Reference (and some default properties) for all the hotbar nodes we're working with.</summary>
    public class Bars
    {
        /// <summary>Whether all the hotbars are loaded</summary>
        public static bool AllExist => Cross.Exists && WXLL.Exists && WXRR.Exists && ActionContents.Exists && ActionBars.All(static bar => bar.Exists);

        /// <summary>Get fresh new pointers for all the hotbar AtkUnitBases</summary>
        public static void Init()
        {
            Cross.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            WXLL.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);
            WXRR.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);
            ActionContents.Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionContents", 1);
            
            for (var i = 0; i < 10; i++) ActionBars[i] = new(i);
        }

        /// <summary>The Main Cross Hotbar</summary>
        public class Cross
        {
            public static AtkUnitBase* Base { get; set; }
            private static AddonActionBarBase* AddonBase => (AddonActionBarBase*)Base;
            private static AddonActionCross* AddonCross => (AddonActionCross*)Base;
            public static bool Exists => BaseExists(Base) || BaseExists((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1)) || (IsSetUp = false);
            public static bool Enabled => LastEnabledState= CharConfig.CrossEnabled;
            private static bool LastEnabledState = true;
            public static bool EnableStateChanged => LastEnabledState != Enabled;

            /// <summary>The selection state of the Cross Hotbar</summary>
            public static class Selection
            {
                public enum Select { None, Left, Right, LR, RL, LL, RR }
                internal static Select Previous = Select.None;
                internal static Select Current => AddonCross->LeftBar     ? Select.Left :
                                                AddonCross->RightBar      ? Select.Right :
                                                AddonCross->LRBar > 0     ? Select.LR :
                                                AddonCross->RLBar > 0     ? Select.RL :
                                                WXLL.AddonCross->Selected ? Select.LL :
                                                WXRR.AddonCross->Selected ? Select.RR : 
                                                                            Select.None ;
            }
            public static NodeWrapper Root        => new() { Node = Base->GetNodeById(1)};
            public static NodeWrapper Container   => new() { Node = Base->GetNodeById(23), DefaultPos = { X = 18F, Y = 79F } };
            public static NodeWrapper SelectBG    => new() { Node = Base->GetNodeById(40), DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelectL => new() { Node = Base->GetNodeById(39), DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper MiniSelectR => new() { Node = Base->GetNodeById(38), DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper VertLine    => new() { Node = Base->GetNodeById(37), DefaultPos = { X = 271F, Y = 21F }, DefaultSize = { X = 9, Y = 76 } };
            public static NodeWrapper RTtext      => new() { Node = Base->GetNodeById(25), DefaultPos = { X = 367F, Y = 11F }};
            public static NodeWrapper LTtext      => new() { Node = Base->GetNodeById(24), DefaultPos = { X = 83F, Y = 11F } };
            public static NodeWrapper SetDisplay  => new() { Node = Base->GetNodeById(18), DefaultPos = { X = 230F, Y = 170F } };
            public static NodeWrapper SetText     => new() { Node = Base->GetNodeById(19) };
            public static NodeWrapper SetNum      => new() { Node = Base->GetNodeById(20) };
            public static NodeWrapper SetButton   => new() { Node = Base->GetNodeById(21) };
            public static NodeWrapper SetBorder   => new() { Node = Base->GetNodeById(22) };
            public static NodeWrapper Padlock     => new() { Node = Base->GetNodeById(17), DefaultPos = { X = 284F, Y = 152F } };
            public static NodeWrapper PadlockIcon => new() { Node = Padlock.ChildNode(1) };
            public static class ChangeSetDisplay
            {
                public static NodeWrapper Container => new() { Node = Base->GetNodeById(2), DefaultPos = { X = 146F, Y = 0F } };
                public static IEnumerable<NodeWrapper> Nums => new NodeWrapper[]
                {
                    new() { Node = Base->GetNodeById(14) },
                    new() { Node = Base->GetNodeById(15) },
                    new() { Node = Base->GetNodeById(16) },
                    new() { Node = Base->GetNodeById(13) },
                    new() { Node = Base->GetNodeById(10) },
                    new() { Node = Base->GetNodeById(11) },
                    new() { Node = Base->GetNodeById(12) },
                    new() { Node = Base->GetNodeById(9) }
                };
                public static NodeWrapper Text => new() { Node = Base->GetNodeById(3) };
            }
            public static class Left
            {
                public static NodeWrapper GroupL => new() { Node = Base->GetNodeById(33), DefaultPos = { X = 0F, Y = 0F } };
                public static NodeWrapper GroupR => new() { Node = Base->GetNodeById(34), DefaultPos = { X = 138F, Y = 0F } };
            }
            public static class Right
            {
                public static NodeWrapper GroupL => new() { Node = Base->GetNodeById(35), DefaultPos = { X = 284F, Y = 0F } };
                public static NodeWrapper GroupR => new() { Node = Base->GetNodeById(36), DefaultPos = { X = 422F, Y = 0F } };
            }
            public static Command[] Actions => CrossUp.Actions.GetByBarID(AddonCross->PetBar ? 19 : SetID, 16);
            public static int LastKnownSetID;
            private static int SetID => LastKnownSetID = AddonBase->HotbarID;
            public static bool SetChanged(AddonActionBarBase* barBase) => LastKnownSetID != (LastKnownSetID = barBase->HotbarID);
            private static bool SetChanged() => LastKnownSetID != SetID;
        }

        /// <summary>The Left WXHB</summary>
        public class WXLL
        {
            public static AtkUnitBase* Base { get; set; }
            public static bool Exists => BaseExists(Base) || BaseExists((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1));
            public static AddonActionDoubleCrossBase* AddonCross => (AddonActionDoubleCrossBase*)Base;
            public static NodeWrapper SelectBG   => new() { Node = Base->GetNodeById(8), DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelect => new() { Node = Base->GetNodeById(7), DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper GroupL     => new() { Node = Base->GetNodeById(6) };
            public static NodeWrapper GroupR     => new() { Node = Base->GetNodeById(5) };
        }

        /// <summary>The Right WXHB</summary>
        public class WXRR
        {
            public static AtkUnitBase* Base { get; set; }
            public static bool Exists => BaseExists(Base) || BaseExists((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1));
            public static AddonActionDoubleCrossBase* AddonCross => (AddonActionDoubleCrossBase*)Base;
            public static NodeWrapper SelectBG   => new() { Node = Base->GetNodeById(8), DefaultSize = { X = 304, Y = 140 } };
            public static NodeWrapper MiniSelect => new() { Node = Base->GetNodeById(7), DefaultSize = { X = 166, Y = 140 } };
            public static NodeWrapper GroupL     => new() { Node = Base->GetNodeById(6) };
            public static NodeWrapper GroupR     => new() { Node = Base->GetNodeById(5) };
        }

        /// <summary>The L->R Expanded Hold Bar</summary>
        public class LR
        {
            public static int BorrowID => Config.LRborrow;
            public static ActionBar BorrowBar => ActionBars[BorrowID];
            public static Command[] Actions
            {
                get
                {
                    var map = CrossUp.Actions.LRMap;
                    return CrossUp.Actions.GetByBarID(map.barID, 8, map.useLeft ? 0 : 8);
                }
            }
            public static bool Exists => BorrowBar.Exists;
        }

        /// <summary>The R->L Expanded Hold Bar</summary>
        public class RL
        {
            public static int BorrowID => Config.RLborrow;
            public static ActionBar BorrowBar => ActionBars[BorrowID];
            public static Command[] Actions
            {
                get
                {
                    var map = CrossUp.Actions.RLMap;
                    return CrossUp.Actions.GetByBarID(map.barID, 8, map.useLeft ? 0 : 8);
                }
            }
            public static bool Exists => BorrowBar.Exists;
        }

        /// <summary>The "Duty Action" pane (ie, in Bozja/Eureka)</summary>
        public class ActionContents
        {
            public static AtkUnitBase* Base { get; set; }
            public static bool Exists => BaseExists(Base) || BaseExists((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionContents", 1)) ;
            public static NodeWrapper BG1 => new() { Node = Base->GetNodeById(14) };
            public static NodeWrapper BG2 => new() { Node = Base->GetNodeById(13) };
            public static NodeWrapper BG3 => new() { Node = Base->GetNodeById(10) };
            public static NodeWrapper BG4 => new() { Node = Base->GetNodeById(9) };
        }

        /// <summary>Mouse/KB Action Bars</summary>
        public static readonly ActionBar[] ActionBars = { new(0),new(1),new(2),new(3),new(4),new(5),new(6),new(7),new(8),new(9) };
        public sealed class ActionBar
        {
            public ActionBar(int barID)
            {
                BarID = barID;
                Base = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar" + (barID == 0 ? "" : "0" + barID), 1);
            }
            public readonly int BarID;
            public AtkUnitBase* Base { get; }
            private int GridType => CharConfig.Hotbar.GridType[BarID];
            public bool Exists => BaseExists(Base) || BaseExists((AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar" + (BarID == 0 ? "" : "0" + BarID), 1)) || (IsSetUp = false);
            public NodeWrapper Root       => new() { Node = Base->GetNodeById(1), DefaultSize = DefaultBarSizes[CharConfig.Hotbar.GridType[BarID]] };
            public NodeWrapper BarNumText => new() { Node = Base->GetNodeById(5) };
            public NodeWrapper GetButton(int b, bool includeDefault=false) => new(Base->GetNodeById((uint)(b + 8)), includeDefault ? DefaultBarGrids[GridType, b] :  null);
            public Command[] Actions => CrossUp.Actions.GetByBarID(BarID, 12);
        }

        /// <summary>Check the existence of a particular AtkUnitBase</summary>
        private static bool BaseExists(AtkUnitBase* unitBase) => unitBase != null && unitBase->UldManager.NodeListSize > 0 /*&& unitBase->X > 0 && unitBase->Y > 0*/;

        /// <summary>Keeps track of the original actions from the Mouse/KB bars, before being borrowed/altered</summary>
        public static readonly Command[]?[] StoredActions = new Command[10][];

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