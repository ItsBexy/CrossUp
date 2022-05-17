using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Numerics;

namespace CrossUp
{
    public sealed unsafe partial class CrossUp
    {
        public class UnitBases
        {
            public static AtkUnitBase*[] ActionBar =
            {
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

            public static AtkUnitBase* Cross = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            public static AtkUnitBase* LL = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);
            public static AtkUnitBase* RR = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);
            public static AtkUnitBase* HudScreen = null;
        };

        public struct NodeRef
        {
            public AtkUnitBase* unitBase;
            public int Id;
            public Vector2 pos;
            public Vector2 size;
        };

        public class barNodes
        {
            public class Cross
            {
                public static readonly NodeRef RootNode = new() { unitBase = UnitBases.Cross, Id = 0 };
                public static readonly NodeRef Component = new() { unitBase = UnitBases.Cross, Id = 1, pos = { X = 18F, Y = 79F } };
                public static readonly NodeRef selectBG = new() { unitBase = UnitBases.Cross, Id = 4, size = { X = 304, Y = 140 } };
                public static readonly NodeRef miniSelectL = new() { unitBase = UnitBases.Cross, Id = 5, size = { X = 166, Y = 140 } };
                public static readonly NodeRef miniSelectR = new() { unitBase = UnitBases.Cross, Id = 6, size = { X = 166, Y = 140 } };
                public static readonly NodeRef VertLine = new() { unitBase = UnitBases.Cross, Id = 7, pos = { X = 271F, Y = 21F }, size = { X = 9, Y = 76 } };
                public static readonly NodeRef[] Sets =
                {
                    new NodeRef { unitBase = UnitBases.Cross, Id = 11, pos = { X = 0F, Y = 0F } },
                    new NodeRef { unitBase = UnitBases.Cross, Id = 10, pos = { X = 138F, Y = 0F } },
                    new NodeRef { unitBase = UnitBases.Cross, Id = 9, pos = { X = 284F, Y = 0F } },
                    new NodeRef { unitBase = UnitBases.Cross, Id = 8, pos = { X = 422F, Y = 0F } },
                };
                public static readonly NodeRef RT = new() { unitBase = UnitBases.Cross, Id = 19, pos = { X = 367F, Y = 11F } };
                public static readonly NodeRef LT = new() { unitBase = UnitBases.Cross, Id = 20, pos = { X = 83F, Y = 11F } };
                public static readonly NodeRef setText = new() { unitBase = UnitBases.Cross, Id = 21, pos = { X = 230F, Y = 170F } };
                public static readonly NodeRef padlock = new() { unitBase = UnitBases.Cross, Id = 26, pos = { X = 284F, Y = 152F } };
                public static readonly NodeRef changeSet = new() { unitBase = UnitBases.Cross, Id = 27, pos = { X = 146F, Y = 0F } };
            }
        };

        public static class Pos
        {
            public static readonly int[] leftEX = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            public static readonly int[] rightEX = { 8, 9, 10, 11, 12, 13, 14, 15 };
            public static readonly int[,] XHB = { { 16, 17, 18, 19 }, { 20, 21, 22, 23 }, { 24, 25, 26, 27 }, { 28, 29, 30, 31 } };
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
            public static implicit operator NodeEdit.PropertySet(MetaSlot p) => new() { X = p.X + p.Xmod, Y = p.Y + p.Ymod, Scale = p.Scale, Visible = p.Visible, OrigX = p.OrigX, OrigY = p.OrigY };
        }

        public static MetaSlot[] metaSlots =
        {
            // the positions for all the borrowed buttons

            //EXHB LEFT 0-7
            new() { Scale = 1.0F, X = 0, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 2 },
            new() { Scale = 1.0F, X = 42, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 2 },
            new() { Scale = 1.0F, X = 84, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 2 },
            new() { Scale = 1.0F, X = 42, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 2 },

            new() { Scale = 1.0F, X = 138, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 2 },
            new() { Scale = 1.0F, X = 180, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 2 },
            new() { Scale = 1.0F, X = 222, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 2 },
            new() { Scale = 1.0F, X = 180, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 2 },

            //EXHB RIGHT 8-15
            new() { Scale = 1.0F, X = 0, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 3 },
            new() { Scale = 1.0F, X = 42, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 3 },
            new() { Scale = 1.0F, X = 84, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 3 },
            new() { Scale = 1.0F, X = 42, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 3 },

            new() { Scale = 1.0F, X = 138, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 3 },
            new() { Scale = 1.0F, X = 180, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 3 },
            new() { Scale = 1.0F, X = 222, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 3 },
            new() { Scale = 1.0F, X = 180, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 3 },

            //MAIN BAR LEFT 16-23
            new() { Scale = 1.0F, X = -142, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 0 },
            new() { Scale = 1.0F, X = -100, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 0 },
            new() { Scale = 1.0F, X = -58, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 0 },
            new() { Scale = 1.0F, X = -100, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 0 },

            new() { Scale = 1.0F, X = -9, Y = 24, OrigX = 95, OrigY = 39, Visible = true, ScaleIndex = 0 },
            new() { Scale = 1.0F, X = 33, Y = 0, OrigX = 53, OrigY = 63, Visible = true, ScaleIndex = 0 },
            new() { Scale = 1.0F, X = 75, Y = 24, OrigX = 11, OrigY = 39, Visible = true, ScaleIndex = 0 },
            new() { Scale = 1.0F, X = 33, Y = 48, OrigX = 53, OrigY = 15, Visible = true, ScaleIndex = 0 },

            //MAIN BAR RIGHT 24-31
            new() { Scale = 1.0F, X = 142, Y = 24, OrigX = 94, OrigY = 39, Visible = true, ScaleIndex = 1 },
            new() { Scale = 1.0F, X = 184, Y = 0, OrigX = 52, OrigY = 63, Visible = true, ScaleIndex = 1 },
            new() { Scale = 1.0F, X = 226, Y = 24, OrigX = 10, OrigY = 39, Visible = true, ScaleIndex = 1 },
            new() { Scale = 1.0F, X = 184, Y = 48, OrigX = 52, OrigY = 15, Visible = true, ScaleIndex = 1 },

            new() { Scale = 1.0F, X = 275, Y = 24, OrigX = 95, OrigY = 39, Visible = true, ScaleIndex = 1 },
            new() { Scale = 1.0F, X = 317, Y = 0, OrigX = 53, OrigY = 63, Visible = true, ScaleIndex = 1 },
            new() { Scale = 1.0F, X = 359, Y = 24, OrigX = 11, OrigY = 39, Visible = true, ScaleIndex = 1 },
            new() { Scale = 1.0F, X = 317, Y = 48, OrigX = 53, OrigY = 15, Visible = true, ScaleIndex = 1 },
        };

        public static readonly Vector2[] barSizes = // default bar sizes for same purpose
        {
            new() { X = 624, Y = 72 },
            new() { X = 331, Y = 121 },
            new() { X = 241, Y = 170 },
            new() { X = 162, Y = 260 },
            new() { X = 117, Y = 358 },
            new() { X = 72, Y = 618 },
        };

        public static readonly float[,] scaleMap = {
            // the scale each section of buttons should be at in each state
            { 1F, 1F, 1F, 1F }, //0: none selected
            { 1.1F, 0.85F, 0.85F, 0.85F }, //1: left selected
            { 0.85F, 1.1F, 0.85F, 0.85F }, //2: right selected
            { 0.85F, 0.85F, 1.1F, 0.85F }, //3: LR selected
            { 0.85F, 0.85F, 0.85F, 1.1F }, //4: RL selected
            { 0.85F, 0.85F, 0.85F, 0.85F }, //5: WXHB L selected
            { 0.85F, 0.85F, 0.85F, 0.85F }, //6: WXHB R selected
        };

        public static readonly Vector2[,] barGrids =
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
                new() { X = 529, Y = 0 },
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
                new() { X = 259, Y = 49 },
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
                new() { X = 169, Y = 98 },
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
                new() { X = 90, Y = 147 },
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
                new() { X = 45, Y = 245 },
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
                new() { X = 0, Y = 509 },
            },
        }; // default grid layouts for when restoring borrowed hotbars to their normal state
    }
}