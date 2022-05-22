using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Numerics;
using Dalamud.Logging;

namespace CrossUp;
public sealed unsafe partial class CrossUp
{
    public class BarUnitBases
    {
        public readonly AtkUnitBase*[] ActionBar = {
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar01", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar02", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar03", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar04", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar05", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar06", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar07", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar08", 1),
            (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionBar09", 1)
        };

        public readonly AtkUnitBase* Cross = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
        public readonly AtkUnitBase* LL = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);
        public readonly AtkUnitBase* RR = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);
    }
        
    public struct NodeRef
    {
        public Vector2 Position;
        public Vector2 Size;
        public AtkResNode* Node;
    }
    
    public class BarNodes
    {
        public class Cross
        {
            public static NodeRef RootNode => new() { Node = GetChild(UnitBases.Cross,0) };
            public static NodeRef Component => new() { Node = GetChild(UnitBases.Cross, 1), Position = { X = 18F, Y = 79F } };
            public static NodeRef SelectBG => new() { Node = GetChild(UnitBases.Cross, 4), Size = { X = 304, Y = 140 } };
            public static NodeRef MiniSelectL => new() { Node = GetChild(UnitBases.Cross, 5), Size = { X = 166, Y = 140 } };
            public static NodeRef MiniSelectR => new() { Node = GetChild(UnitBases.Cross, 6), Size = { X = 166, Y = 140 } };
            public static NodeRef VertLine => new() { Node = GetChild(UnitBases.Cross, 7), Position = { X = 271F, Y = 21F }, Size = { X = 9, Y = 76 } };
            public class Sets
            {
                public static NodeRef Left1 => new() { Node = GetChild(UnitBases.Cross, 11), Position = { X = 0F, Y = 0F } };
                public static NodeRef Left2 => new() { Node = GetChild(UnitBases.Cross, 10), Position = { X = 138F, Y = 0F } };
                public static NodeRef Right1 => new() { Node = GetChild(UnitBases.Cross, 9), Position = { X = 284F, Y = 0F } };
                public static NodeRef Right2 => new() { Node = GetChild(UnitBases.Cross, 8), Position = { X = 422F, Y = 0F } };
            }
            public static NodeRef RT => new() { Node = GetChild(UnitBases.Cross, 19), Position = { X = 367F, Y = 11F } };
            public static NodeRef LT => new() { Node = GetChild(UnitBases.Cross, 20), Position = { X = 83F, Y = 11F } };
            public static NodeRef SetText => new() { Node = GetChild(UnitBases.Cross, 21), Position = { X = 230F, Y = 170F } };
            public static NodeRef Padlock => new() { Node = GetChild(UnitBases.Cross, 26), Position = { X = 284F, Y = 152F } };
            public static NodeRef ChangeSet => new() { Node = GetChild(UnitBases.Cross, 27), Position = { X = 146F, Y = 0F } };
        }
        public class LL
        {
            public static NodeRef SelectBG => new() { Node = GetChild(UnitBases.LL, 3), Size = { X = 304, Y = 140 } };
            public static NodeRef MiniSelect => new() { Node = GetChild(UnitBases.LL, 4), Size = { X = 166, Y = 140 } };
        }
        public class RR
        {
            public static NodeRef SelectBG => new() { Node = GetChild(UnitBases.RR, 3), Size = { X = 304, Y = 140 } };
            public static NodeRef MiniSelect => new() { Node = GetChild(UnitBases.RR, 4), Size = { X = 166, Y = 140 } };
        }
    }

    public class ScaleTween
    {
        public DateTime Start { get; init; }
        public TimeSpan Duration { get; init; }
        public float FromScale { get; init; }
        public float ToScale { get; init; }
    }
    public class MetaSlot
    {
        public bool Visible { get; set; }
        public int X { get; init; }
        public int Y { get; init; }
        public float Scale { get; set; }
        public int OrigX { get; init; }
        public int OrigY { get; init; }
        public ScaleTween? Tween { get; set; }
        public int ScaleIndex { get; set; }
        public AtkResNode* Node { get; set; }
        public float Xmod { get; set; }
        public float Ymod { get; set; }
        public static implicit operator NodeEdit.PropertySet(MetaSlot p) => new(){ X = p.X + p.Xmod, Y = p.Y + p.Ymod, Scale = p.Scale, Visible = p.Visible, OrigX = p.OrigX,OrigY = p.OrigY};
    }

        // MetaSlots = all the potential positions we might place a borrowed button, depending on which bar it's imitating
    private static readonly MetaSlot[] MetaSlots =
    {
        // EXHB L->R
        new() { Scale = 1.0F, X = 0, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 2 },
        new() { Scale = 1.0F, X = 42, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 2 },
        new() { Scale = 1.0F, X = 84, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 2 },
        new() { Scale = 1.0F, X = 42, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 2 },

        new() { Scale = 1.0F, X = 138, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 2 },
        new() { Scale = 1.0F, X = 180, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 2 },
        new() { Scale = 1.0F, X = 222, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 2 },
        new() { Scale = 1.0F, X = 180, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 2 },

        // EXHB R->L
        new() { Scale = 1.0F, X = 0, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 3 },
        new() { Scale = 1.0F, X = 42, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 3 },
        new() { Scale = 1.0F, X = 84, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 3 },
        new() { Scale = 1.0F, X = 42, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 3 },

        new() { Scale = 1.0F, X = 138, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 3 },
        new() { Scale = 1.0F, X = 180, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 3 },
        new() { Scale = 1.0F, X = 222, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 3 },
        new() { Scale = 1.0F, X = 180, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 3 },

        // MAIN XHB
        new() { Scale = 1.0F, X = -142, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 0 },
        new() { Scale = 1.0F, X = -100, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 0 },
        new() { Scale = 1.0F, X = -58, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 0 },
        new() { Scale = 1.0F, X = -100, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 0 },

        new() { Scale = 1.0F, X = -9, Y = 24, OrigX = 95, OrigY = 39, Visible = true, ScaleIndex = 0 },
        new() { Scale = 1.0F, X = 33, Y = 0, OrigX = 53, OrigY = 63, Visible = true, ScaleIndex = 0 },
        new() { Scale = 1.0F, X = 75, Y = 24, OrigX = 11, OrigY = 39, Visible = true, ScaleIndex = 0 },
        new() { Scale = 1.0F, X = 33, Y = 48, OrigX = 53, OrigY = 15, Visible = true, ScaleIndex = 0 },
            
        new() { Scale = 1.0F, X = 142, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 1 },
        new() { Scale = 1.0F, X = 184, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 1 },
        new() { Scale = 1.0F, X = 226, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 1 },
        new() { Scale = 1.0F, X = 184, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 1 },

        new() { Scale = 1.0F, X = 275, Y = 24, OrigX = 95, OrigY = 39, Visible = true, ScaleIndex = 1 },
        new() { Scale = 1.0F, X = 317, Y = 0, OrigX = 53, OrigY = 63, Visible = true, ScaleIndex = 1 },
        new() { Scale = 1.0F, X = 359, Y = 24, OrigX = 11, OrigY = 39, Visible = true, ScaleIndex = 1 },
        new() { Scale = 1.0F, X = 317, Y = 48, OrigX = 53, OrigY = 15, Visible = true, ScaleIndex = 1 }
    };

        // index of metaSlots by bar
    public static class Pos
    {
        public static readonly int[] LeftEx = { 0, 1, 2, 3, 4, 5, 6, 7 };
        public static readonly int[] RightEx = { 8, 9, 10, 11, 12, 13, 14, 15 };
        public static readonly int[,] XHB = { { 16, 17, 18, 19 }, { 20, 21, 22, 23 }, { 24, 25, 26, 27 }, { 28, 29, 30, 31 } };
    }

        // correct scales for specific metaSlots in specific cross bar selections
    private static readonly float[,] ScaleMap =
    {
        { 1F, 1F, 1F, 1F }, //0: none selected
        { 1.1F, 0.85F, 0.85F, 0.85F }, //1: left selected
        { 0.85F, 1.1F, 0.85F, 0.85F }, //2: right selected
        { 0.85F, 0.85F, 1.1F, 0.85F }, //3: LR selected
        { 0.85F, 0.85F, 0.85F, 1.1F }, //4: RL selected
        { 0.85F, 0.85F, 0.85F, 0.85F }, //5: WXHB L selected
        { 0.85F, 0.85F, 0.85F, 0.85F } //6: WXHB R selected
    };

        // default bar sizes for when restoring borrowed hotbars to their normal state
    private static readonly Vector2[] ActionBarSizes =
    {
        new() { X = 624, Y = 72 },
        new() { X = 331, Y = 121 },
        new() { X = 241, Y = 170 },
        new() { X = 162, Y = 260 },
        new() { X = 117, Y = 358 },
        new() { X = 72, Y = 618 }
    };

        // default grid layouts for when restoring borrowed hotbars to their normal state
    private static readonly Vector2[,] ActionBarGrids =
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
}