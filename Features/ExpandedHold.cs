using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Logging;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    // SEPARATE EXPANDED HOLD CONTROLS

    // turn on the feature
    public void EnableEx()
    {
        Bars.LR.BorrowID = Config.borrowBarL;
        Bars.RL.BorrowID = Config.borrowBarR;

        if (Bars.LR.BorrowID < 0 || Bars.RL.BorrowID < 0) return;

        PrepBorrowedBar(Bars.LR.BorrowID);
        PrepBorrowedBar(Bars.RL.BorrowID);

        PluginLog.LogDebug($"Borrowing {Bars.LR.BorrowBar.Name} to serve as L->R Expanded Hold Bar");
        PluginLog.LogDebug($"Borrowing {Bars.RL.BorrowBar.Name} to serve as R->L Expanded Hold Bar");

        UpdateBarState(true);
        Task.Delay(20).ContinueWith(delegate { if (Status.Initialized) UpdateBarState(true); });
    }

    // turn on each specific bar
    private static void PrepBorrowedBar(int id)
    {
        if (Bars.ActionBars[id].Base == null || Bars.ActionBars[id].NodeCount == 0) return;
        var job = CharConfig.Hotbar.Shared[id] ? 0 : GetPlayerJob();
        Bars.StoredActions[id] = GetSavedBar(job, id);

        if (!CharConfig.Hotbar.Visible[id])
        {
            Bars.WasHidden[id] = true;
            CharConfig.Hotbar.Visible[id].Set(1);
        }

        for (var i = 0; i < 12; i++) Bars.ActionBars[id].Button[i].Node->Flags_2 |= 0xD;
    }

    // disable the feature (does not set Config.SepExBar to false; that should be happening before/as this is called)
    public void DisableEx()
    {
        ArrangeCrossBar(0, 0, true, false);
        ResetHud();
       
        for (var barID = 1; barID <= 9; barID++) Bars.StoredActions[barID] = null;
    }

    // arrange the borrowed hotbars representing the EXHB
    private void ArrangeExBars(int select, int prevSelect, float scale, int anchorX, int anchorY, bool forceArrange = false)
    {
        if (!Bars.Cross.Exist) return;

        if (Bars.LR.BorrowBar.Base == null || Bars.RL.BorrowBar.Base == null) return;

        foreach (var button in Bars.LR.BorrowBar.Button)
        {
            button.ChildNode(0).SetVis(true);
            button.ChildNode(1).SetVis(false);
        }

        foreach (var button in Bars.RL.BorrowBar.Button)
        {
            button.ChildNode(0).SetVis(true);
            button.ChildNode(1).SetVis(false);
        }

        bool mixBar = CharConfig.MixBar ;

        var lX = Config.lX;
        var lY = Config.lY;
        var rX = !Config.OnlyOneEx ? Config.rX : Config.lX;
        var rY = !Config.OnlyOneEx ? Config.rY : Config.lY;

        Bars.LR.BorrowBar.Root.SetScale(scale)
                              .SetVis(true)
                              .SetSize(295, 120)
                              .SetPos(anchorX + (lX + Config.Split) * scale, anchorY + lY * scale);

        Bars.RL.BorrowBar.Root.SetScale(scale)
                              .SetVis(true)
                              .SetSize(295, 120)
                              .SetPos(anchorX + (rX + Config.Split) * scale, anchorY + rY * scale);

        Bars.LR.BorrowBar.BarNumText.SetScale(0F);
        Bars.RL.BorrowBar.BarNumText.SetScale(0F);

        var inactiveAlpha = TransToAlpha(CharConfig.Transparency.Inactive);
        var standardAlpha = TransToAlpha(CharConfig.Transparency.Active);
        SetExAlpha(select == 0 ? standardAlpha : inactiveAlpha);

        for (var i = 0; i < 8; i++) MetaSlots.RL[i].Visible = !Config.OnlyOneEx;

        switch (select)
        {
            case 0: // NONE
            case 5: // LEFT WXHB
            case 6: // RIGHT WXHB
                {
                    CopyButtons(Bars.LR.Actions, 0, Bars.LR.BorrowID, 0, 8);
                    CopyButtons(Bars.RL.Actions, 0, Bars.RL.BorrowID, 0, 8);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            PlaceButton(Bars.LR.BorrowBar.Button[i], MetaSlots.LR[i], 0, 0, select);  //left EXHB
                            PlaceButton(Bars.LR.BorrowBar.Button[i + 4], MetaSlots.LR[i + 4], 0, 0, select);

                            PlaceButton(Bars.RL.BorrowBar.Button[i], MetaSlots.RL[i], 0, 0, select); // right EXHB
                            PlaceButton(Bars.RL.BorrowBar.Button[i + 4], MetaSlots.RL[i + 4], 0, 0, select);

                            MetaSlots.Cross.LeftL[i].Visible = false; // hide metaSlots for XHB
                            MetaSlots.Cross.LeftR[i].Visible = false;
                            MetaSlots.Cross.RightL[i].Visible = false;
                            MetaSlots.Cross.RightR[i].Visible = false;

                            Bars.LR.BorrowBar.Button[i + 8].SetVis(false); // hide unneeded borrowed buttons
                            Bars.RL.BorrowBar.Button[i + 8].SetVis(false);
                        }
                    }
                    break;
                }
            case 1: // LEFT BAR
                {
                    CopyButtons(Bars.LR.Actions, 0, Bars.LR.BorrowID, 0, 8);
                    CopyButtons(Bars.RL.Actions, 0, Bars.RL.BorrowID, 0, 8);

                    var crossActions = Bars.Cross.Actions;
                    CopyButtons(crossActions, 8, Bars.LR.BorrowID, 8, 4);
                    CopyButtons(crossActions, 12, Bars.RL.BorrowID, 8, 4);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            MetaSlots.Cross.LeftL[i].Visible = false;
                            MetaSlots.Cross.LeftR[i].Visible = prevSelect == 3 && mixBar;
                            MetaSlots.Cross.RightL[i].Visible = prevSelect == 3 && !mixBar;
                            MetaSlots.Cross.RightR[i].Visible = prevSelect == 3;

                            MetaSlots.Cross.LeftL[i].Scale = MetaSlots.ScaleMap[1, 0];
                            MetaSlots.Cross.LeftR[i].Scale = MetaSlots.ScaleMap[1, mixBar ? 1 : 0];
                            MetaSlots.Cross.RightL[i].Scale = MetaSlots.ScaleMap[1, mixBar ? 0 : 1];
                            MetaSlots.Cross.RightR[i].Scale = MetaSlots.ScaleMap[1, 1];

                            PlaceButton(Bars.LR.BorrowBar.Button[i], MetaSlots.LR[i], 0, 0, select);  //left EXHB
                            PlaceButton(Bars.LR.BorrowBar.Button[i + 4], MetaSlots.LR[i + 4], 0, 0, select);

                            PlaceButton(Bars.RL.BorrowBar.Button[i], MetaSlots.RL[i], 0, 0, select); // right EXHB
                            PlaceButton(Bars.RL.BorrowBar.Button[i + 4], MetaSlots.RL[i + 4], 0, 0, select);

                            if (!mixBar) PlaceButton(Bars.LR.BorrowBar.Button[i + 8], MetaSlots.Cross.RightL[i], -lX + Config.Split, -lY, select); //right XHB (left buttons)
                            else PlaceButton(Bars.LR.BorrowBar.Button[i + 8], MetaSlots.Cross.LeftR[i], -lX - Config.Split, -lY, select);

                            PlaceButton(Bars.RL.BorrowBar.Button[i + 8], MetaSlots.Cross.RightR[i], -rX + Config.Split, -rY, select);              //right XHB (right buttons)
                        }
                    }
                    break;
                }
            case 2: // RIGHT BAR
                {
                    CopyButtons(Bars.LR.Actions, 0, Bars.LR.BorrowID, 0, 8);
                    CopyButtons(Bars.RL.Actions, 0, Bars.RL.BorrowID, 0, 8);

                    var crossActions = Bars.Cross.Actions;
                    CopyButtons(crossActions, 0, Bars.LR.BorrowID, 8, 4);
                    CopyButtons(crossActions, 4, Bars.RL.BorrowID, 8, 4);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            MetaSlots.Cross.LeftL[i].Visible = prevSelect == 4;
                            MetaSlots.Cross.LeftR[i].Visible = prevSelect == 4 && !mixBar;
                            MetaSlots.Cross.RightL[i].Visible = prevSelect == 4 && mixBar;
                            MetaSlots.Cross.RightR[i].Visible = false;

                            MetaSlots.Cross.LeftL[i].Scale = MetaSlots.ScaleMap[2, 0];
                            MetaSlots.Cross.LeftR[i].Scale = MetaSlots.ScaleMap[2, mixBar ? 1 : 0];
                            MetaSlots.Cross.RightL[i].Scale = MetaSlots.ScaleMap[2, mixBar ? 0 : 1];
                            MetaSlots.Cross.RightR[i].Scale = MetaSlots.ScaleMap[2, 1];

                            PlaceButton(Bars.LR.BorrowBar.Button[i], MetaSlots.LR[i], 0, 0, select); // left EXHB
                            PlaceButton(Bars.LR.BorrowBar.Button[i + 4], MetaSlots.LR[i + 4], 0, 0, select);

                            PlaceButton(Bars.RL.BorrowBar.Button[i], MetaSlots.RL[i], 0, 0, select); // right EXHB
                            PlaceButton(Bars.RL.BorrowBar.Button[i + 4], MetaSlots.RL[i + 4], 0, 0, select);

                            PlaceButton(Bars.LR.BorrowBar.Button[i + 8], MetaSlots.Cross.LeftL[i], -lX - Config.Split, -lY, select);              // left XHB (left buttons)

                            if (!mixBar) PlaceButton(Bars.RL.BorrowBar.Button[i + 8], MetaSlots.Cross.LeftR[i], -rX - Config.Split, -rY, select); // left XHB (right buttons)
                            else PlaceButton(Bars.RL.BorrowBar.Button[i + 8], MetaSlots.Cross.RightL[i], -rX + Config.Split, -rY, select);
                        }
                    }
                    break;
                }
            case 3: // L->R BAR
                {
                    var crossActions = Bars.Cross.Actions;
                    CopyButtons(crossActions, 0, Bars.LR.BorrowID, 0, 12);
                    CopyButtons(crossActions, 12, Bars.RL.BorrowID, 8, 4);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            MetaSlots.LR[i].Scale = 1.1F;
                            MetaSlots.LR[i + 4].Scale = 1.1F;

                            MetaSlots.Cross.LeftL[i].Visible = true;
                            MetaSlots.Cross.LeftR[i].Visible = true;
                            MetaSlots.Cross.RightL[i].Visible = true;
                            MetaSlots.Cross.RightR[i].Visible = true;

                            PlaceButton(Bars.RL.BorrowBar.Button[i], MetaSlots.RL[i], 0, 0, select); // right EXHB
                            PlaceButton(Bars.RL.BorrowBar.Button[i + 4], MetaSlots.RL[i + 4], 0, 0, select);

                            PlaceButton(Bars.LR.BorrowBar.Button[i], MetaSlots.Cross.LeftL[i], -lX - Config.Split, -lY, select); // inactive XHB
                            PlaceButton(Bars.LR.BorrowBar.Button[i + (!mixBar ? 4 : 8)], MetaSlots.Cross.LeftR[i], -lX - Config.Split, -lY, select);
                            PlaceButton(Bars.LR.BorrowBar.Button[i + (!mixBar ? 8 : 4)], MetaSlots.Cross.RightL[i], -lX + Config.Split, -lY, select);
                            PlaceButton(Bars.RL.BorrowBar.Button[i + 8], MetaSlots.Cross.RightR[i], -rX + Config.Split, -rY, select);
                        }
                    }
                    break;
                }
            case 4: // R->L BAR
                {
                    var crossActions = Bars.Cross.Actions;
                    CopyButtons(crossActions, 0, Bars.LR.BorrowID, 8, 4);
                    CopyButtons(crossActions, 4, Bars.RL.BorrowID, 8, 4);
                    CopyButtons(crossActions, 8, Bars.RL.BorrowID, 0, 8);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {

                            MetaSlots.RL[i].Scale = 1.1F;
                            MetaSlots.RL[i + 4].Scale = 1.1F;

                            if (Config.OnlyOneEx)
                            {
                                MetaSlots.LR[i].Scale = 1.1F;
                                MetaSlots.LR[i + 4].Scale = 1.1F;
                            }

                            MetaSlots.Cross.LeftL[i].Visible = true;
                            MetaSlots.Cross.LeftR[i].Visible = true;
                            MetaSlots.Cross.RightL[i].Visible = true;
                            MetaSlots.Cross.RightR[i].Visible = true;

                            PlaceButton(Bars.LR.BorrowBar.Button[i], !Config.OnlyOneEx ? MetaSlots.LR[i] : MetaSlots.RL[i], 0, 0, select); // left EXHB
                            PlaceButton(Bars.LR.BorrowBar.Button[i + 4], !Config.OnlyOneEx ? MetaSlots.LR[i + 4] : MetaSlots.RL[i + 4], 0, 0, select);

                            PlaceButton(Bars.LR.BorrowBar.Button[i + 8], MetaSlots.Cross.LeftL[i], -lX - Config.Split, -lY, select); // inactive XHB
                            PlaceButton(Bars.RL.BorrowBar.Button[i + (!mixBar ? 8 : 0)], MetaSlots.Cross.LeftR[i], -rX - Config.Split, -rY, select);
                            PlaceButton(Bars.RL.BorrowBar.Button[i + (!mixBar ? 0 : 8)], MetaSlots.Cross.RightL[i], -rX + Config.Split, -rY, select);
                            PlaceButton(Bars.RL.BorrowBar.Button[i + 4], MetaSlots.Cross.RightR[i], -rX + Config.Split, -rY, select);
                        }
                    }
                    break;
                }
        }
    }

    // move a borrowed button into position and set its scale to animate if needed
    private static void PlaceButton(NodeRef nodeRef, MetaSlots.MetaSlot mSlot, float xMod = 0, float yMod = 0, int select = 0)
    {
        var to = MetaSlots.ScaleMap[select, mSlot.ScaleIndex];
        mSlot.Xmod = xMod;
        mSlot.Ymod = yMod;
        mSlot.NodeRef = nodeRef;

        if (Math.Abs(to - mSlot.Scale) > 0.01f) //only make a new tween if the button isn't already at the target scale, otherwise just set
        {
            if (mSlot.Tween == null || Math.Abs(mSlot.Tween.ToScale - to) > 0.01f) //only make a new tween if the button doesn't already have one with the same target scale
            {
                mSlot.Tween = new()
                {
                    FromScale = mSlot.Scale,
                    ToScale = to,
                    Start = DateTime.Now,
                    Duration = new(0, 0, 0, 0, 40)
                };
                MetaSlots.TweensExist = true;
                return;
            }
        }
        else
        {
            mSlot.Tween = null;
            mSlot.Scale = to;
        }

        nodeRef.SetProps(mSlot);
    }

    // convert in-game "transparency" slider to an actual alpha value
    private static byte TransToAlpha(int t) => (byte)((100 - t) * 2.55);

    // set alpha of borrowed buttons in accordance with in-game settings
    private static void SetExAlpha(byte alpha)
    {
        for (var i = 0; i < 12; i++)
        {
            Bars.LR.BorrowBar.Button[i].SetAlpha(alpha);
            Bars.RL.BorrowBar.Button[i].SetAlpha(alpha);
        }
    }

    // MetaSlots = all the potential positions we might place a borrowed button, depending on which bar it's imitating

    public static class MetaSlots
    {
        public class MetaSlot
        {
            public bool Visible { get; set; } = true;
            public int X { get; init; }
            public int Y { get; init; }
            public float Scale { get; set; } = 1.0F;
            public float OrigX { get; init; }
            public float OrigY { get; init; }
            public class ScaleTween
            {
                public DateTime Start { get; init; }
                public TimeSpan Duration { get; init; }
                public float FromScale { get; init; }
                public float ToScale { get; init; }
            }
            public ScaleTween? Tween { get; set; }
            public int ScaleIndex { get; set; }
            public NodeRef? NodeRef { get; set; }
            public float Xmod { get; set; }
            public float Ymod { get; set; }
            public void RunTween()
            {
                if (Tween == null || NodeRef == null || NodeRef.Node == null) return;

                TweensExist = true;

                var timePassed = DateTime.Now - Tween.Start;
                var progress = (float)decimal.Divide(timePassed.Milliseconds, Tween.Duration.Milliseconds);
                var to = Tween.ToScale;
                var from = Tween.FromScale;

                if (progress >= 1)
                {
                    Scale = to;
                    Tween = null;
                }
                else
                {
                    Scale = (to - from) * progress + from;
                }

                NodeRef.SetProps(this);
            }
            public static implicit operator NodeRef.PropertySet(MetaSlot p) => new() { X = p.X + p.Xmod, Y = p.Y + p.Ymod, Scale = p.Scale, Visible = p.Visible, OrigX = p.OrigX, OrigY = p.OrigY };
        }

        public static readonly MetaSlot[] LR =
        {
            new() { X = 0, Y = 24, OrigX = 94, OrigY = 39, ScaleIndex = 2 },
            new() { X = 42, Y = 0, OrigX = 52, OrigY = 63, ScaleIndex = 2 },
            new() { X = 84, Y = 24, OrigX = 10, OrigY = 39, ScaleIndex = 2 },
            new() { X = 42, Y = 48, OrigX = 52, OrigY = 15, ScaleIndex = 2 },

            new() { X = 138, Y = 24, OrigX = 94, OrigY = 39, ScaleIndex = 2 },
            new() { X = 180, Y = 0, OrigX = 52, OrigY = 63, ScaleIndex = 2 },
            new() { X = 222, Y = 24, OrigX = 10, OrigY = 39, ScaleIndex = 2 },
            new() { X = 180, Y = 48, OrigX = 52, OrigY = 15, ScaleIndex = 2 }
        };
        public static readonly MetaSlot[] RL =
        {
            new() { X = 0, Y = 24, OrigX = 94, OrigY = 39, ScaleIndex = 3 },
            new() { X = 42, Y = 0, OrigX = 52, OrigY = 63, ScaleIndex = 3 },
            new() { X = 84, Y = 24, OrigX = 10, OrigY = 39, ScaleIndex = 3 },
            new() { X = 42, Y = 48, OrigX = 52, OrigY = 15, ScaleIndex = 3 },

            new() { X = 138, Y = 24, OrigX = 94, OrigY = 39, ScaleIndex = 3 },
            new() { X = 180, Y = 0, OrigX = 52, OrigY = 63, ScaleIndex = 3 },
            new() { X = 222, Y = 24, OrigX = 10, OrigY = 39, ScaleIndex = 3 },
            new() { X = 180, Y = 48, OrigX = 52, OrigY = 15, ScaleIndex = 3 }
        };
        public class Cross
        {
            public static readonly MetaSlot[] LeftL = {
                new() { X = -142, Y = 24, OrigX = 94, OrigY = 39, ScaleIndex = 0 },
                new() { X = -100, Y = 0, OrigX = 52, OrigY = 63, ScaleIndex = 0 },
                new() { X = -58, Y = 24, OrigX = 10, OrigY = 39, ScaleIndex = 0 },
                new() { X = -100, Y = 48, OrigX = 52, OrigY = 15, ScaleIndex = 0 }
            };
            public static readonly MetaSlot[] LeftR = {
                new() { X = -9, Y = 24, OrigX = 94+1/0.85F, OrigY = 39, ScaleIndex = 0 },
                new() { X = 33, Y = 0, OrigX = 52+1/0.85F, OrigY = 63, ScaleIndex = 0 },
                new() { X = 75, Y = 24, OrigX = 10+1/0.85F, OrigY = 39, ScaleIndex = 0 },
                new() { X = 33, Y = 48, OrigX = 52+1/0.85F, OrigY = 15, ScaleIndex = 0 }
            };
            public static readonly MetaSlot[] RightL = {
                new() { X = 142, Y = 24, OrigX = 94, OrigY = 39, ScaleIndex = 1 },
                new() { X = 184, Y = 0, OrigX = 52, OrigY = 63, ScaleIndex = 1 },
                new() { X = 226, Y = 24, OrigX = 10, OrigY = 39, ScaleIndex = 1 },
                new() { X = 184, Y = 48, OrigX = 52, OrigY = 15, ScaleIndex = 1 }
            };
            public static readonly MetaSlot[] RightR = {
                new() { X = 275, Y = 24, OrigX = 94+1/0.85F, OrigY = 39, ScaleIndex = 1 },
                new() { X = 317, Y = 0, OrigX = 52+1/0.85F, OrigY = 63, ScaleIndex = 1 },
                new() { X = 359, Y = 24, OrigX = 10+1/0.85F, OrigY = 39, ScaleIndex = 1 },
                new() { X = 317, Y = 48, OrigX = 52+1/0.85F, OrigY = 15, ScaleIndex = 1 }
            };
        }
        public static bool TweensExist { get; set; }
        public static void TweenAll()
        {
            TweensExist = false;

            foreach (var mSlot in LR) mSlot.RunTween();
            foreach (var mSlot in RL) mSlot.RunTween();
            foreach (var mSlot in Cross.LeftL) mSlot.RunTween();
            foreach (var mSlot in Cross.LeftR) mSlot.RunTween();
            foreach (var mSlot in Cross.RightL) mSlot.RunTween();
            foreach (var mSlot in Cross.RightR) mSlot.RunTween();
        }

        // correct scales for specific metaSlots in specific cross bar selections
        public static readonly float[,] ScaleMap =
        {
            { 1F, 1F, 1F, 1F }, //0: none selected
            { 1.1F, 0.85F, 0.85F, 0.85F }, //1: left selected
            { 0.85F, 1.1F, 0.85F, 0.85F }, //2: right selected
            { 0.85F, 0.85F, 1.1F, 0.85F }, //3: LR selected
            { 0.85F, 0.85F, 0.85F, 1.1F }, //4: RL selected
            { 0.85F, 0.85F, 0.85F, 0.85F }, //5: WXHB L selected
            { 0.85F, 0.85F, 0.85F, 0.85F } //6: WXHB R selected
        };
    }
}