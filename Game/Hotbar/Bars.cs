using System.Collections.Generic;
using System.Numerics;
using CrossUp.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using static CrossUp.CrossUp;
using static CrossUp.Game.Hotbar.Actions;
using static CrossUp.Utility.Service;

namespace CrossUp.Game.Hotbar;

/// <summary>Reference (and some default properties) for all the hotbar nodes we're working with.</summary>
internal static unsafe class Bars
{
    /// <summary>Get fresh new pointers for all the hotbar AtkUnitBases</summary>
    internal static bool GetBases()
    {
        try
        {
            Cross.Base = new("_ActionCross");
            WXHB.LL.Base = new("_ActionDoubleCrossL");
            WXHB.RR.Base = new("_ActionDoubleCrossR");
            ActionContents.Base = new("_ActionContents");
            MainMenu.Base = new("_MainCross");

            for (var i = 0; i < 10; i++) ActionBars[i] = new(i);
            return true;
        }
        catch
        {
            Log.Warning("Couldn't retrieve bar bases!");
            return false;
        }
    }

    /// <summary>The Main Cross Hotbar</summary>
    internal class Cross
    {
        public static BaseWrapper Base = new("_ActionCross");
        private static AddonActionBarBase* AddonBase => (AddonActionBarBase*)Base.UnitBase;
        public static AddonActionCross* AddonCross => (AddonActionCross*)Base.UnitBase;
        public static bool Exists => Base.Exists();
        internal static bool Enabled => LastEnabledState = GameConfig.Cross.Enabled;
        private static bool LastEnabledState = true;
        internal static bool EnableStateChanged => LastEnabledState != Enabled;

        /// <summary>The selection state of the Cross Hotbar</summary>
        internal static class Selection
        {
            internal static ActionCrossSelect Previous = ActionCrossSelect.None;
            internal static ActionCrossSelect Current = ActionCrossSelect.None;

            internal static void Check()
            {
                var selected = AddonCross->Selected;
                if (selected == Current) return;
                Log.Verbose(Current+"->"+selected);
                Previous = Current;
                Current = selected;
            }
        }

        internal static NodeWrapper Root        => Base[1u];
        internal static NodeWrapper Container   => new(Base[23u], pos: new(18, 79));
        internal static NodeWrapper SelectBG    => new(Base[40u], size: new(304, 140));
        internal static NodeWrapper MiniSelectL => new(Base[39u], size: new(166, 140));
        internal static NodeWrapper MiniSelectR => new(Base[38u], size: new(166, 140));
        internal static NodeWrapper VertLine    => new(Base[37u], pos: new(271, 21), size: new(9, 76));
        internal static NodeWrapper RTtext      => new(Base[25u], pos: new(367, 11));
        internal static NodeWrapper LTtext      => new(Base[24u], pos: new(83, 11));
        internal static NodeWrapper SetDisplay  => new(Base[18u], pos: new(230, 170));
        internal static NodeWrapper SetText     => Base[19u];
        internal static NodeWrapper SetNum      => Base[20u];
        internal static NodeWrapper SetButton   => Base[21u];
        internal static NodeWrapper SetBorder   => Base[22u];
        internal static NodeWrapper Padlock     => new(Base[17u], pos: new(284, 152));

        internal static class ChangeSetDisplay
        {
            internal static NodeWrapper Container => new(Base[2u], pos: new(146, 0));

            internal static IEnumerable<NodeWrapper> Nums => new[]
            {
                Base[14u],
                Base[15u],
                Base[16u],
                Base[13u],
                Base[12u],
                Base[11u],
                Base[10u],
                Base[9u]
            };

            internal static NodeWrapper Text => Base[3u];
        }

        internal static Action[] Actions => GetByBarID(AddonCross->PetBar ? 19 : SetID.Current, 16);

        public static class SetID
        {
            private static int Previous;
            internal static int Current => Previous = AddonBase->RaptureHotbarId;
            internal static bool HasChanged() => Previous != Current;
        }

        internal sealed class ButtonSet
        {
            internal NodeWrapper this[int i] => new(Base[(uint)(33 + i)], pos: new(142f * i - (i % 2 == 0 ? 0 : 4), 0));
        }

        internal static readonly ButtonSet Buttons = new();
    }

    internal class WXHB
    {
        internal static bool Exists => LL.Exists && RR.Exists;

        internal class LL
        {
            internal static BaseWrapper Base = new("_ActionDoubleCrossL");
            internal static bool Exists => Base.Exists();
            internal static NodeWrapper SelectBG => new(Base[8u], size: new(304, 140));
            internal static NodeWrapper MiniSelect => new(Base[7u], size: new(166, 140));
        }

        internal class RR
        {
            internal static BaseWrapper Base = new("_ActionDoubleCrossR");
            internal static bool Exists => Base.Exists();
            internal static NodeWrapper SelectBG => new(Base[8u], size: new(304, 140));
            internal static NodeWrapper MiniSelect => new(Base[7u], size: new(166, 140));
        }

        internal static readonly ButtonSet Buttons = new();
        internal sealed class ButtonSet
        {
            internal NodeWrapper this[int i] => (i < 2 ? LL.Base : RR.Base)[(uint)(6 - i % 2)];
        }
    }

    /// <summary>The L->R Expanded Hold Bar</summary>
    internal class LR
    {
        internal static int ID => Config.LRborrow;
        internal static ActionBar BorrowBar => ActionBars[ID];
        internal static bool Exists => BorrowBar.Exists;
        internal static Action[] Actions => GetExHoldActions(ExSide.LR);
        internal static ActionBarButtonNodes Buttons => BorrowBar.Buttons;
        private static BaseWrapper Base => BorrowBar.Base;
        internal static NodeWrapper Root => new(Base[1u]);
    }

    /// <summary>The R->L Expanded Hold Bar</summary>
    internal class RL
    {
        internal static int ID => Config.RLborrow;
        internal static ActionBar BorrowBar => ActionBars[ID];
        internal static bool Exists => BorrowBar.Exists;
        internal static Action[] Actions => GetExHoldActions(ExSide.RL);
        internal static ActionBarButtonNodes Buttons => BorrowBar.Buttons;
        private static BaseWrapper Base => BorrowBar.Base;
        internal static NodeWrapper Root => new(Base[1u]);
    }

    /// <summary>The "Duty Action" pane (ie, in Bozja/Eureka)</summary>
    internal class ActionContents
    {
        internal static BaseWrapper Base = new("_ActionContents");
        internal static bool Exists => Base.Exists();
        internal static NodeWrapper BG1 => Base[14u];
        internal static NodeWrapper BG2 => Base[13u];
        internal static NodeWrapper BG3 => Base[10u];
        internal static NodeWrapper BG4 => Base[9u];
    }

    /// <summary>All Mouse/KB hotbars</summary>
    internal static readonly ActionBar[] ActionBars = { new(0), new(1), new(2), new(3), new(4), new(5), new(6), new(7), new(8), new(9) };

    /// <summary>A Mouse/KB hotbar</summary>
    internal sealed class ActionBar
    {
        internal ActionBar(int barID)
        {
            ID = barID;
            Base = new($"_ActionBar{(barID == 0 ? "" : $"0{barID}")}");
            Buttons = new ActionBarButtonNodes(ID);
        }

        private readonly int ID;
        internal readonly BaseWrapper Base;
        internal bool Exists => Base.Exists();
        internal int GridType => (int)((AddonActionBarX*)Base.UnitBase)->ActionBarLayout;
        internal NodeWrapper Root => new(Base[1u], size: DefaultBarSizes[GridType]);
        internal NodeWrapper BarNumText => Base[5u];
        internal Action[] Actions => GetByBarID(ID, 12);
        internal readonly ActionBarButtonNodes Buttons;
    }

    /// <summary>The buttons on a Mouse/KB hotbar</summary>
    internal sealed class ActionBarButtonNodes
    {
        internal ActionBarButtonNodes(int id) => ID = id;
        private readonly int ID;
        private BaseWrapper Base => ActionBars[ID].Base;
        internal NodeWrapper this[int b, bool getDef = false] => new(Base[(uint)(b + 8)], getDef ? DefaultBarGrids[ActionBars[ID].GridType, b] : null);
    }

    /// <summary>Keeps track of the original actions from the Mouse/KB bars, before being borrowed/altered</summary>
    internal static readonly Action[]?[] StoredActions = new Action[10][];

    /// <summary>Original sizes for the Mouse/KB bars, by [int gridType]</summary>
    private static readonly Vector2[] DefaultBarSizes =
    {
        new(624, 72),
        new(331, 121),
        new(241, 170),
        new(162, 260),
        new(117, 358),
        new(72, 618)
    };

    /// <summary>Original button positions for the Mouse/KB bars, by [int gridType]</summary>
    private static readonly Vector2[,] DefaultBarGrids =
    {
        {
            new(34,  0),  new(79, 0), new(124, 0), new(169, 0), new(214, 0), new(259, 0), new(304, 0), new(349, 0), new(394, 0), new(439, 0), new(484, 0), new(529, 0)
        },
        {
            new(34, 0),  new(79, 0),  new(124, 0),  new(169, 0),  new(214, 0),  new(259, 0),
            new(34, 49), new(79, 49), new(124, 49), new(169, 49), new(214, 49), new(259, 49)
        },
        {
            new(34, 0),  new(79, 0),  new(124, 0),  new(169, 0),
            new(34, 49), new(79, 49), new(124, 49), new(169, 49),
            new(34, 98), new(79, 98), new(124, 98), new(169, 98)
        },
        {
            new(0, 0),   new(45, 0),   new(90, 0),
            new(0, 49),  new(45, 49),  new(90, 49),
            new(0, 98),  new(45, 98),  new(90, 98),
            new(0, 147), new(45, 147), new(90, 147)
        },
        {
            new(0, 0),   new(45, 0),
            new(0, 49),  new(45, 49),
            new(0, 98),  new(45, 98),
            new(0, 147), new(45, 147),
            new(0, 196), new(45, 196),
            new(0, 245), new(45, 245)
        },
        {
            new(0, 14),
            new(0, 59),
            new(0, 104),
            new(0, 149),
            new(0, 194),
            new(0, 239),
            new(0, 284),
            new(0, 329),
            new(0, 374),
            new(0, 419),
            new(0, 464),
            new(0, 509)
        }
    };

    /// <summary>Visibility status of Mouse/KB  bars before they were borrowed</summary>
    internal static readonly bool[] WasHidden = new bool[10];

    /// <summary>The Main Menu</summary>
    internal class MainMenu
    {
        public static BaseWrapper Base = new("_MainCross");
        public static bool Exists => Base.Exists(false);
    }
}