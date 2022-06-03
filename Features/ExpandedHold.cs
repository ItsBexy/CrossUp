using System;
using System.Threading.Tasks;
using Dalamud.Logging;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    // SEPARATE EXPANDED HOLD CONTROLS

    /// <summary>Enable the Separate Expanded Hold Bars feature</summary>
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
        Task.Delay(20).ContinueWith(delegate { if (Initialized) UpdateBarState(true); });
    }

    /// <summary>Turn on each borrowed bar</summary>
    private static void PrepBorrowedBar(int barID)
    {
        if (Bars.ActionBars[barID].Base == null || Bars.ActionBars[barID].NodeCount == 0) return;

        Actions.Store(barID);

        if (!CharConfig.Hotbar.Visible[barID])
        {
            Bars.WasHidden[barID] = true;
            CharConfig.Hotbar.Visible[barID].Set(1);
        }

        for (var i = 0; i < 12; i++) Bars.ActionBars[barID].Button[i].Node->Flags_2 |= 0xD;
    }

    /// <summary>Disable the Separate Expanded Hold Bars feature (does not set Config.SepExBar to false; that should be happening before/as this is called)</summary>
    public void DisableEx()
    {
        ArrangeCrossBar(0, 0, true, false);
        ResetHud();
       
        for (var barID = 1; barID <= 9; barID++) Bars.StoredActions[barID] = null;
    }

    /// <summary>Arrange the borrowed hotbars representing the EXHB</summary>
    private void ArrangeExBars(int select, int prevSelect, float scale, int anchorX, int anchorY, bool forceArrange = false)
    {
        if (!Bars.Cross.Exist) return;

        if (Bars.LR.BorrowBar.Base == null || Bars.RL.BorrowBar.Base == null) return;

        var exAlpha = (byte)((100 - (int)(select == 0 ? CharConfig.Transparency.Active : CharConfig.Transparency.Inactive)) * 2.55);
        bool mixBar = CharConfig.MixBar;
        var lX = Config.lX;
        var lY = Config.lY;
        var rX = !Config.OnlyOneEx ? Config.rX : Config.lX;
        var rY = !Config.OnlyOneEx ? Config.rY : Config.lY;
        var split = Config.Split;

        foreach (var button in Bars.LR.BorrowBar.Button)
        {
            button.ChildNode(0).SetVis(true);
            button.ChildNode(1).SetVis(false);
            button.SetAlpha(exAlpha);
        }

        foreach (var button in Bars.RL.BorrowBar.Button)
        {
            button.ChildNode(0).SetVis(true);
            button.ChildNode(1).SetVis(false);
            button.SetAlpha(exAlpha);
        }

        Bars.LR.BorrowBar.Root.SetScale(scale)
                              .SetVis(true)
                              .SetSize(295, 120)
                              .SetPos(anchorX + (lX + split) * scale, anchorY + lY * scale);

        Bars.RL.BorrowBar.Root.SetScale(scale)
                              .SetVis(true)
                              .SetSize(295, 120)
                              .SetPos(anchorX + (rX + split) * scale, anchorY + rY * scale);

        Bars.LR.BorrowBar.BarNumText.SetScale(0F);
        Bars.RL.BorrowBar.BarNumText.SetScale(0F);

        for (var i = 0; i < 8; i++) MetaSlots.RL[i].Visible = !Config.OnlyOneEx;

        Bars.Cross.UpdateActions();

        switch (select)
        {
            case 0: // NONE
            case 5: // LEFT WXHB
            case 6: // RIGHT WXHB
                {
                    Actions.Copy(Bars.LR.Actions, 0, Bars.LR.BorrowID, 0, 8);
                    Actions.Copy(Bars.RL.Actions, 0, Bars.RL.BorrowID, 0, 8);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            MetaSlots.LR[i].Insert(Bars.LR.BorrowBar.Button[i]);  //left EXHB
                            MetaSlots.LR[i + 4].Insert(Bars.LR.BorrowBar.Button[i + 4]);

                            MetaSlots.RL[i].Insert(Bars.RL.BorrowBar.Button[i]); // right EXHB
                            MetaSlots.RL[i + 4].Insert(Bars.RL.BorrowBar.Button[i + 4]);

                            MetaSlots.Left.GroupL[i].SetVis(false); // hide metaSlots for XHB
                            MetaSlots.Left.GroupR[i].SetVis(false);
                            MetaSlots.Right.GroupL[i].SetVis(false);
                            MetaSlots.Right.GroupR[i].SetVis(false);

                            Bars.LR.BorrowBar.Button[i + 8].SetVis(false).SetScale(0.85F); // hide unneeded borrowed buttons
                            Bars.RL.BorrowBar.Button[i + 8].SetVis(false).SetScale(0.85F);
                        }
                    }
                    break;
                }
            case 1: // LEFT BAR
                {
                    Actions.Copy(Bars.LR.Actions, 0, Bars.LR.BorrowID, 0, 8);
                    Actions.Copy(Bars.RL.Actions, 0, Bars.RL.BorrowID, 0, 8);

                    Actions.Copy(Bars.Cross.Actions, 8, Bars.LR.BorrowID, 8, 4);
                    Actions.Copy(Bars.Cross.Actions, 12, Bars.RL.BorrowID, 8, 4);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            MetaSlots.Left.GroupL[i].SetVis(false).SetScale(1.1F);
                            MetaSlots.Left.GroupR[i].SetVis(prevSelect == 3 && mixBar).SetScale(!mixBar ? 1.1F : 0.85F);
                            MetaSlots.Right.GroupL[i].SetVis(prevSelect == 3 && !mixBar).SetScale(!mixBar ? 0.85F : 1.1F);
                            MetaSlots.Right.GroupR[i].SetVis(prevSelect == 3).SetScale(0.85F).Insert(Bars.RL.BorrowBar.Button[i + 8], -rX + split, -rY,0.85F);

                            MetaSlots.LR[i].Insert(Bars.LR.BorrowBar.Button[i],  0, 0,0.85F);
                            MetaSlots.LR[i + 4].Insert(Bars.LR.BorrowBar.Button[i + 4], 0, 0, 0.85F);

                            MetaSlots.RL[i].Insert(Bars.RL.BorrowBar.Button[i],  0, 0, 0.85F);
                            MetaSlots.RL[i + 4].Insert(Bars.RL.BorrowBar.Button[i + 4],  0, 0, 0.85F);

                            (!mixBar ? MetaSlots.Right.GroupL[i]:
                                       MetaSlots.Left.GroupR[i]).Insert(Bars.LR.BorrowBar.Button[i + 8], (!mixBar ? split : -split) - lX, -lY, 0.85F);
                        }
                    }
                    break;
                }
            case 2: // RIGHT BAR
                {
                    Actions.Copy(Bars.LR.Actions, 0, Bars.LR.BorrowID, 0, 8);
                    Actions.Copy(Bars.RL.Actions, 0, Bars.RL.BorrowID, 0, 8);

                    Actions.Copy(Bars.Cross.Actions, 0, Bars.LR.BorrowID, 8, 4);
                    Actions.Copy(Bars.Cross.Actions, 4, Bars.RL.BorrowID, 8, 4);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            MetaSlots.Left.GroupL[i].SetVis(prevSelect == 4).SetScale(0.85F).Insert(Bars.LR.BorrowBar.Button[i + 8], -lX - split, -lY,0.85F);
                            MetaSlots.Left.GroupR[i].SetVis(prevSelect == 4 && !mixBar).SetScale(!mixBar ? 0.85F : 1.1F);
                            MetaSlots.Right.GroupL[i].SetVis(prevSelect == 4 && mixBar).SetScale(!mixBar ? 1.1F : 0.85F);
                            MetaSlots.Right.GroupR[i].SetVis(false).SetScale(1.1F);

                            MetaSlots.LR[i].Insert(Bars.LR.BorrowBar.Button[i],  0, 0, 0.85F);
                            MetaSlots.LR[i + 4].Insert(Bars.LR.BorrowBar.Button[i + 4],  0, 0, 0.85F);

                            MetaSlots.RL[i].Insert(Bars.RL.BorrowBar.Button[i], 0, 0, 0.85F);
                            MetaSlots.RL[i + 4].Insert(Bars.RL.BorrowBar.Button[i + 4],  0, 0, 0.85F);

                           (!mixBar ? MetaSlots.Left.GroupR[i] : 
                                      MetaSlots.Right.GroupL[i]).Insert(Bars.RL.BorrowBar.Button[i + 8], (!mixBar ? -split : split) - rX, -rY, 0.85F);
 
                        }
                    }
                    break;
                }
            case 3: // L->R BAR
                {
                    Actions.Copy(Bars.Cross.Actions, 0, Bars.LR.BorrowID, 0, 12);
                    Actions.Copy(Bars.Cross.Actions, 12, Bars.RL.BorrowID, 8, 4);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            MetaSlots.LR[i].Scale = 1.1F;
                            MetaSlots.LR[i + 4].Scale = 1.1F;

                            MetaSlots.Left.GroupL[i].SetVis(true).Insert(Bars.LR.BorrowBar.Button[i], -lX - split, -lY,0.85F);
                            MetaSlots.Left.GroupR[i].SetVis(true).Insert(Bars.LR.BorrowBar.Button[i + (!mixBar ? 4 : 8)], -lX - split, -lY, 0.85F);
                            MetaSlots.Right.GroupL[i].SetVis(true).Insert(Bars.LR.BorrowBar.Button[i + (!mixBar ? 8 : 4)], -lX + split, -lY, 0.85F);
                            MetaSlots.Right.GroupR[i].SetVis(true).Insert(Bars.RL.BorrowBar.Button[i + 8], -rX + split, -rY, 0.85F);

                            MetaSlots.RL[i].Insert(Bars.RL.BorrowBar.Button[i],  0, 0, 0.85F);
                            MetaSlots.RL[i + 4].Insert(Bars.RL.BorrowBar.Button[i + 4],  0, 0, 0.85F);
                        }
                    }
                    break;
                }
            case 4: // R->L BAR
                {
                    Actions.Copy(Bars.Cross.Actions, 0, Bars.LR.BorrowID, 8, 4);
                    Actions.Copy(Bars.Cross.Actions, 4, Bars.RL.BorrowID, 8, 4);
                    Actions.Copy(Bars.Cross.Actions, 8, Bars.RL.BorrowID, 0, 8);

                    if (forceArrange || select != prevSelect)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            MetaSlots.RL[i].SetScale(1.1F);
                            MetaSlots.RL[i + 4].SetScale(1.1F);

                            if (Config.OnlyOneEx)
                            {
                                MetaSlots.LR[i].SetScale(1.1F);
                                MetaSlots.LR[i + 4].SetScale(1.1F);
                            }

                            MetaSlots.Left.GroupL[i].SetVis(true).Insert(Bars.LR.BorrowBar.Button[i + 8], -lX - split, -lY, 0.85F);
                            MetaSlots.Left.GroupR[i].SetVis(true).Insert(Bars.RL.BorrowBar.Button[i + (!mixBar ? 8 : 0)], -rX - split, -rY, 0.85F);
                            MetaSlots.Right.GroupL[i].SetVis(true).Insert(Bars.RL.BorrowBar.Button[i + (!mixBar ? 0 : 8)], -rX + split, -rY, 0.85F);
                            MetaSlots.Right.GroupR[i].SetVis(true).Insert(Bars.RL.BorrowBar.Button[i + 4], -rX + split, -rY, 0.85F);

                            (!Config.OnlyOneEx ? MetaSlots.LR[i] : MetaSlots.RL[i]).Insert(Bars.LR.BorrowBar.Button[i],  0, 0, 0.85F);
                            (!Config.OnlyOneEx ? MetaSlots.LR[i + 4] : MetaSlots.RL[i + 4]).Insert(Bars.LR.BorrowBar.Button[i + 4],  0, 0, 0.85F);
                        }
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// MetaSlots are various positions that a "borrowed" bar's buttons can be placed in, to imitate elements of the Cross Hotbar.
    /// </summary>
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
            private ScaleTween? Tween { get; set; }
            private NodeWrapper? NodeRef { get; set; }
            private float Xmod { get; set; }
            private float Ymod { get; set; }
            public MetaSlot SetVis(bool show)
            {
                Visible = show;
                return this;
            }
            public MetaSlot SetScale(float scale)
            {
                Scale = scale;
                return this;
            }

            /// <summary>Places a button node into a MetaSlot</summary>
            public MetaSlot Insert(NodeWrapper nodeWrapper, float xMod = 0, float yMod = 0, float to = 1F)
            {
               // var to = ScaleMap[select, ScaleIndex];
                Xmod = xMod;
                Ymod = yMod;
                NodeRef = nodeWrapper;

                if (Math.Abs(to - Scale) > 0.01f) //only make a new tween if the button isn't already at the target scale, otherwise just set
                {
                    if (Tween == null || Math.Abs(Tween.ToScale - to) > 0.01f) //only make a new tween if the button doesn't already have one with the same target scale
                    {
                        Tween = new()
                        {
                            FromScale = Scale,
                            ToScale = to,
                            Start = DateTime.Now,
                            Duration = new(0, 0, 0, 0, 40)
                        };
                        TweensExist = true;
                        return this;
                    }
                }
                else
                {
                    Tween = null;
                    Scale = to;
                }

                NodeRef.SetProps(this);
                return this;
            }
            /// <summary>Checks if this MetaSlot has an active animation tween, and if so, processes it</summary>
            public void RunTween()
            {
                if (Tween == null || NodeRef == null || NodeRef.Node == null) return;

                TweensExist = true;

                var timePassed = DateTime.Now - Tween.Start;
                var progress = (float)decimal.Divide(timePassed.Milliseconds, Tween.Duration.Milliseconds);
                var to = Tween.ToScale;
                var from = Tween.FromScale;

                Tween = progress < 1 ? Tween : null;
                Scale = progress < 1 ? (to - from) * progress + from : to;

                NodeRef.SetProps(this);
            }
            public static implicit operator NodeWrapper.PropertySet(MetaSlot p) => new() { X = p.X + p.Xmod, Y = p.Y + p.Ymod, Scale = p.Scale, Visible = p.Visible, OrigX = p.OrigX, OrigY = p.OrigY };
        }
        /// <summary>Whether or not there are currently any animation tweens to run</summary>
        public static bool TweensExist { get; private set; }
        /// <summary>Run all animations tweens for all MetaSlots</summary>
        public static void TweenAll()
        {
            TweensExist = false;
            foreach (var mSlot in LR) mSlot.RunTween();
            foreach (var mSlot in RL) mSlot.RunTween();
            foreach (var mSlot in Left.GroupL) mSlot.RunTween();
            foreach (var mSlot in Left.GroupR) mSlot.RunTween();
            foreach (var mSlot in Right.GroupL) mSlot.RunTween();
            foreach (var mSlot in Right.GroupR) mSlot.RunTween();
        }
        public static readonly MetaSlot[] LR =
        {
            new() { X = 0, Y = 24, OrigX = 102, OrigY = 39},
            new() { X = 42, Y = 0, OrigX = 60, OrigY = 63},
            new() { X = 84, Y = 24, OrigX = 18, OrigY = 39},
            new() { X = 42, Y = 48, OrigX = 60, OrigY = 15},

            new() { X = 138, Y = 24, OrigX = 54, OrigY = 39},
            new() { X = 180, Y = 0, OrigX = 12, OrigY = 63},
            new() { X = 222, Y = 24, OrigX = -30, OrigY = 39},
            new() { X = 180, Y = 48, OrigX = 12, OrigY = 15}
        };
        public static readonly MetaSlot[] RL =
        {
            new() { X = 0, Y = 24, OrigX = 102, OrigY = 39},
            new() { X = 42, Y = 0, OrigX = 60, OrigY = 63},
            new() { X = 84, Y = 24, OrigX = 18, OrigY = 39},
            new() { X = 42, Y = 48, OrigX = 60, OrigY = 15},

            new() { X = 138, Y = 24, OrigX = 54, OrigY = 39},
            new() { X = 180, Y = 0, OrigX = 12, OrigY = 63},
            new() { X = 222, Y = 24, OrigX = -30, OrigY = 39},
            new() { X = 180, Y = 48, OrigX = 12, OrigY = 15}
        };
        public static class Left
        {
            public static readonly MetaSlot[] GroupL = {
                new() { X = -142, Y = 24, OrigX = 94, OrigY = 39},
                new() { X = -100, Y = 0, OrigX = 52, OrigY = 63},
                new() { X = -58, Y = 24, OrigX = 10, OrigY = 39},
                new() { X = -100, Y = 48, OrigX = 52, OrigY = 15}
            };

            public static readonly MetaSlot[] GroupR = {
                new() { X = -4, Y = 24, OrigX = 62, OrigY = 39},
                new() { X = 38, Y = 0, OrigX = 20, OrigY = 63},
                new() { X = 80, Y = 24, OrigX = -22, OrigY = 39},
                new() { X = 38, Y = 48, OrigX = 20, OrigY = 15}
            };
        }
        public static class Right
        {
            public static readonly MetaSlot[] GroupL = {
                new() { X = 142, Y = 24, OrigX = 94, OrigY = 39},
                new() { X = 184, Y = 0, OrigX = 52, OrigY = 63},
                new() { X = 226, Y = 24, OrigX = 10, OrigY = 39},
                new() { X = 184, Y = 48, OrigX = 52, OrigY = 15}
            };

            public static readonly MetaSlot[] GroupR = {
                new() { X = 280, Y = 24, OrigX = 62, OrigY = 39},
                new() { X = 322, Y = 0, OrigX = 20, OrigY = 63},
                new() { X = 364, Y = 24, OrigX = -22, OrigY = 39},
                new() { X = 322, Y = 48, OrigX = 20, OrigY = 15}
            };
        }
    }
}