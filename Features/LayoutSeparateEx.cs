using System;
using System.Threading.Tasks;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using static CrossUp.CrossUp.Bars.Cross.Selection;
// ReSharper disable UnusedMethodReturnValue.Global

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    public static partial class Layout
    {
        /// <summary>Plugin feature that displays the bars for Expanded Hold Controls separately from the rest of the Cross Hotbar</summary>
        public class SeparateEx
        {
            /// <summary>Confirms that the Separate Expanded Hold feature is active and that two valid bars are selected</summary>
            public static bool Ready => IsSetUp && Config.SepExBar && Config.LRborrow > 0 && Config.RLborrow > 0;

            /// <summary>Enable the Separate Expanded Hold Bars feature</summary>
            public static void Enable()
            {
                PrepBar(Config.LRborrow);
                PrepBar(Config.RLborrow);

                PluginLog.LogDebug($"Borrowing Hotbar {Config.LRborrow + 1} to serve as L→R Expanded Hold Bar");
                PluginLog.LogDebug($"Borrowing Hotbar {Config.RLborrow + 1} to serve as R→L Expanded Hold Bar");

                Update(true);
                Task.Delay(20).ContinueWith(static delegate { if (IsSetUp) Update(true); });
            }

            /// <summary>Turn on each borrowed bar</summary>
            private static void PrepBar(int barID)
            {
                if (!Bars.ActionBars[barID].Exists) return;
                Actions.Store(barID);

                if (!CharConfig.Hotbar.Visible[barID])
                {
                    Bars.WasHidden[barID] = true;
                    CharConfig.Hotbar.Visible[barID].Set(1);
                }

                for (var i = 0; i < 12; i++) Bars.ActionBars[barID].GetButton(i).Node->Flags_2 |= 0xD;
            }

            /// <summary>Disable the Separate Expanded Hold Bars feature (does not set Config.SepExBar to false; that should be happening before/as this is called)</summary>
            public static void Disable()
            {
                Update(true);
                ResetBars();

                for (var barID = 1; barID <= 9; barID++) Bars.StoredActions[barID] = null;
            }

            /// <summary>Sets the keybind visibility, empty slot visibility, and the overall alpha for Expanded Hold bars</summary>
            private static void StyleSlots(Bars.ActionBar bar, byte alpha)
            {
                var actions = bar.Actions;
                for (var i = 0; i < 12; i++)
                {
                    var button = bar.GetButton(i);
                    button.ChildNode(1).SetVis(false);
                    button.ChildNode(0).SetVis(!Config.HideUnassigned || actions[i].CommandType != HotbarSlotType.Empty);
                    button.SetAlpha(alpha);
                }
            }

            /// <summary>Arrange the borrowed bars to replicate the Expanded Hold bars</summary>
            public static void Arrange(Select select, Select previous, float scale, int split, bool mixBar, (int, int, int, int) coords, bool forceArrange)
            {
                SetActions(select);
                HiddenCheck();

                var anchorX = (int)(Bars.Cross.Root.Node->X + 146 * scale);
                var anchorY = (int)(Bars.Cross.Root.Node->Y + 70 * scale);
                var (lrX, lrY, rlX, rlY) = coords;

                Bars.LR.BorrowBar.Root
                    .SetScale(scale)
                    .SetVis(true)
                    .SetSize(295, 120)
                    .SetPos(anchorX + (lrX + split) * scale, anchorY + lrY * scale);

                Bars.RL.BorrowBar.Root
                    .SetScale(scale)
                    .SetVis(true)
                    .SetSize(295, 120)
                    .SetPos(anchorX + (rlX + split) * scale, anchorY + rlY * scale);

                Bars.LR.BorrowBar.BarNumText.SetScale(0F);
                Bars.RL.BorrowBar.BarNumText.SetScale(0F);

                for (var i = 0; i < 8; i++) MetaSlots.RL[i].Visible = !Config.OnlyOneEx;

                if (forceArrange || select != previous)
                {
                    switch (select)
                    {
                        case Select.None:
                        case Select.LL:
                        case Select.RR:
                        default:
                        {
                            var exScale = select is Select.LL or Select.RR ? 0.85F : 1F;
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.LR[i].Insert(Bars.LR.BorrowBar.GetButton(i), 0F, 0F, exScale); // L->R bar
                                MetaSlots.RL[i].Insert(Bars.RL.BorrowBar.GetButton(i), 0F, 0F, exScale); // R->L bar
                                if (i >= 4) continue;

                                MetaSlots.Left.GroupL[i].SetVis(false); // hide XHB metaSlots
                                MetaSlots.Left.GroupR[i].SetVis(false);
                                MetaSlots.Right.GroupL[i].SetVis(false);
                                MetaSlots.Right.GroupR[i].SetVis(false);

                                Bars.LR.BorrowBar.GetButton(i + 8).SetVis(false).SetScale(0.85F); // hide unneeded borrowed buttons
                                Bars.RL.BorrowBar.GetButton(i + 8).SetVis(false).SetScale(0.85F);
                            }
                            break;
                        }
                        case Select.Left:
                        {
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.LR[i].Insert(Bars.LR.BorrowBar.GetButton(i), 0, 0, 0.85F); // L->R bar
                                MetaSlots.RL[i].Insert(Bars.RL.BorrowBar.GetButton(i), 0, 0, 0.85F); // R->L bar
                                if (i >= 4) continue;

                                // XHB
                                MetaSlots.Left.GroupL[i].SetVis(false).SetScale(1.1F);
                                MetaSlots.Left.GroupR[i].SetVis(previous == Select.LR && mixBar).SetScale(!mixBar ? 1.1F : 0.85F);
                                MetaSlots.Right.GroupL[i].SetVis(previous == Select.LR && !mixBar).SetScale(!mixBar ? 0.85F : 1.1F);
                                MetaSlots.Right.GroupR[i].SetVis(previous == Select.LR).SetScale(0.85F).Insert(Bars.RL.BorrowBar.GetButton(i + 8), -rlX + split, -rlY, 0.85F);

                                (!mixBar ? MetaSlots.Right.GroupL[i] : 
                                           MetaSlots.Left.GroupR[i]).Insert(Bars.LR.BorrowBar.GetButton(i + 8), (!mixBar ? split : -split) - lrX, -lrY, 0.85F);
                            }
                            break;
                        }
                        case Select.Right:
                        {
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.LR[i].Insert(Bars.LR.BorrowBar.GetButton(i), 0, 0, 0.85F); // L->R bar
                                MetaSlots.RL[i].Insert(Bars.RL.BorrowBar.GetButton(i), 0, 0, 0.85F); // R->L bar
                                if (i >= 4) continue;

                                // XHB
                                MetaSlots.Left.GroupL[i].SetVis(previous == Select.RL).SetScale(0.85F).Insert(Bars.LR.BorrowBar.GetButton(i + 8), -lrX - split, -lrY, 0.85F);
                                MetaSlots.Left.GroupR[i].SetVis(previous == Select.RL && !mixBar).SetScale(!mixBar ? 0.85F : 1.1F);
                                MetaSlots.Right.GroupL[i].SetVis(previous == Select.RL && mixBar).SetScale(!mixBar ? 1.1F : 0.85F);
                                MetaSlots.Right.GroupR[i].SetVis(false).SetScale(1.1F);

                                (!mixBar ? MetaSlots.Left.GroupR[i] :
                                           MetaSlots.Right.GroupL[i]).Insert(Bars.RL.BorrowBar.GetButton(i + 8), (!mixBar ? -split : split) - rlX, -rlY, 0.85F);
                            }
                            break;
                        }
                        case Select.LR:
                        {
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.LR[i].Scale = 1.1F;
                                MetaSlots.RL[i].Insert(Bars.RL.BorrowBar.GetButton(i), 0, 0, 0.85F); // R->L bar
                                if (i >= 4) continue;

                                // XHB
                                MetaSlots.Left.GroupL[i].SetVis(true).Insert(Bars.LR.BorrowBar.GetButton(i), -lrX - split, -lrY, 0.85F);
                                MetaSlots.Left.GroupR[i].SetVis(true).Insert(Bars.LR.BorrowBar.GetButton(i + (!mixBar ? 4 : 8)), -lrX - split, -lrY, 0.85F);
                                MetaSlots.Right.GroupL[i].SetVis(true).Insert(Bars.LR.BorrowBar.GetButton(i + (!mixBar ? 8 : 4)), -lrX + split, -lrY, 0.85F);
                                MetaSlots.Right.GroupR[i].SetVis(true).Insert(Bars.RL.BorrowBar.GetButton(i + 8), -rlX + split, -rlY, 0.85F);
                            }
                            break;
                        }
                        case Select.RL:
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.RL[i].SetScale(1.1F);
                                MetaSlots.LR[i].SetScale(Config.OnlyOneEx ? 1.1F : 0.85F);
                                (!Config.OnlyOneEx ? MetaSlots.LR : MetaSlots.RL)[i]
                                    .Insert(Bars.LR.BorrowBar.GetButton(i), 0, 0, 0.85F); // L->R bar
                                if (i >= 4) continue;

                                // XHB
                                MetaSlots.Left.GroupL[i].SetVis(true).Insert(Bars.LR.BorrowBar.GetButton(i + 8), -lrX - split, -lrY, 0.85F);
                                MetaSlots.Left.GroupR[i].SetVis(true).Insert(Bars.RL.BorrowBar.GetButton(i + (!mixBar ? 8 : 0)), -rlX - split, -rlY, 0.85F);
                                MetaSlots.Right.GroupL[i].SetVis(true).Insert(Bars.RL.BorrowBar.GetButton(i + (!mixBar ? 0 : 8)), -rlX + split, -rlY, 0.85F);
                                MetaSlots.Right.GroupR[i].SetVis(true).Insert(Bars.RL.BorrowBar.GetButton(i + 4), -rlX + split, -rlY, 0.85F);
                            }
                            break;
                    }
                }

                var alpha = (select == Select.None ? CharConfig.Transparency.Standard : CharConfig.Transparency.Inactive).IntToAlpha;

                StyleSlots(Bars.LR.BorrowBar, alpha);
                StyleSlots(Bars.RL.BorrowBar, alpha);
            }

            /// <summary>Checks if the borrowed bars have been unexpectedly hidden (ie by the user or another plugin)</summary>
            private static void HiddenCheck()
            {
                var lrID = Bars.LR.BorrowID;
                var rlID = Bars.RL.BorrowID;
                if (CharConfig.Hotbar.Visible[lrID] == 0) CharConfig.Hotbar.Visible[lrID].Set(1);
                if (CharConfig.Hotbar.Visible[rlID] == 0) CharConfig.Hotbar.Visible[rlID].Set(1);
            }

            /// <summary>Copies the appropriate actions to the borrowed bars</summary>
            private static void SetActions(Select select)
            {
                var actions = (Cross: Bars.Cross.Actions, LR: Bars.LR.Actions, RL: Bars.RL.Actions);

                switch (select)
                {
                    case Select.None:
                    case Select.LL:
                    case Select.RR:
                    default:
                    {
                        Actions.Copy(actions.LR, 0, Bars.LR.BorrowID, 0, 8);
                        Actions.Copy(actions.RL, 0, Bars.RL.BorrowID, 0, 8);
                        break;
                    }
                    case Select.Left:
                    {
                        Actions.Copy(actions.LR, 0, Bars.LR.BorrowID, 0, 8);
                        Actions.Copy(actions.RL, 0, Bars.RL.BorrowID, 0, 8);
                        Actions.Copy(actions.Cross, 8, Bars.LR.BorrowID, 8, 4);
                        Actions.Copy(actions.Cross, 12, Bars.RL.BorrowID, 8, 4);
                        break;
                    }
                    case Select.Right:
                    {
                        Actions.Copy(actions.LR, 0, Bars.LR.BorrowID, 0, 8);
                        Actions.Copy(actions.RL, 0, Bars.RL.BorrowID, 0, 8);
                        Actions.Copy(actions.Cross, 0, Bars.LR.BorrowID, 8, 4);
                        Actions.Copy(actions.Cross, 4, Bars.RL.BorrowID, 8, 4);
                        break;
                    }
                    case Select.LR:
                    {
                        Actions.Copy(actions.RL, 0, Bars.RL.BorrowID, 0, 8);
                        Actions.Copy(actions.Cross, 0, Bars.LR.BorrowID, 0, 12);
                        Actions.Copy(actions.Cross, 12, Bars.RL.BorrowID, 8, 4);
                        break;
                    }
                    case Select.RL:
                    {
                        Actions.Copy(actions.LR, 0, Bars.LR.BorrowID, 0, 8);
                        Actions.Copy(actions.Cross, 0, Bars.LR.BorrowID, 8, 4);
                        Actions.Copy(actions.Cross, 4, Bars.RL.BorrowID, 8, 4);
                        Actions.Copy(actions.Cross, 8, Bars.RL.BorrowID, 0, 8);
                        break;
                    }
                }
            }

            /// <summary>MetaSlots are various positions that a "borrowed" bar's buttons can be placed in, to imitate elements of the Cross Hotbar.</summary>
            public static class MetaSlots
            {
                /// <summary>A position in which a "borrowed" button can be placed</summary>
                public sealed class MetaSlot
                {
                    public bool Visible { get; set; } = true;
                    public int X { get; init; }
                    public int Y { get; init; }
                    public float Scale { get; set; } = 1.0F;
                    public float OrigX { get; init; }
                    public float OrigY { get; init; }

                    public sealed class ScaleTween
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

                    public static implicit operator NodeWrapper.PropertySet(MetaSlot p) => new()
                    {
                        X = p.X + p.Xmod, Y = p.Y + p.Ymod, Scale = p.Scale, Visible = p.Visible, OrigX = p.OrigX,
                        OrigY = p.OrigY
                    };

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
                        Xmod = xMod;
                        Ymod = yMod;
                        NodeRef = nodeWrapper;

                        if (Math.Abs(to - Scale) >
                            0.01f) //only make a new tween if the button isn't already at the target scale, otherwise just set
                        {
                            if (Tween == null ||
                                Math.Abs(Tween.ToScale - to) >
                                0.01f) //only make a new tween if the button doesn't already have one with the same target scale
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
                }

                /// <summary>Whether or not there are currently any animation tweens to run</summary>
                public static bool TweensExist { get; private set; }

                /// <summary>Run all animations tweens for all MetaSlots</summary>
                public static void TweenAll()
                {
                    TweensExist = false; // will set back to true if we find any incomplete tweens
                    foreach (var mSlot in LR) mSlot.RunTween();
                    foreach (var mSlot in RL) mSlot.RunTween();
                    foreach (var mSlot in Left.GroupL) mSlot.RunTween();
                    foreach (var mSlot in Left.GroupR) mSlot.RunTween();
                    foreach (var mSlot in Right.GroupL) mSlot.RunTween();
                    foreach (var mSlot in Right.GroupR) mSlot.RunTween();
                }

                public static readonly MetaSlot[] LR =
                {
                    new() { X = 0, Y = 24, OrigX = 102, OrigY = 39 },
                    new() { X = 42, Y = 0, OrigX = 60, OrigY = 63 },
                    new() { X = 84, Y = 24, OrigX = 18, OrigY = 39 },
                    new() { X = 42, Y = 48, OrigX = 60, OrigY = 15 },

                    new() { X = 138, Y = 24, OrigX = 54, OrigY = 39 },
                    new() { X = 180, Y = 0, OrigX = 12, OrigY = 63 },
                    new() { X = 222, Y = 24, OrigX = -30, OrigY = 39 },
                    new() { X = 180, Y = 48, OrigX = 12, OrigY = 15 }
                };

                public static readonly MetaSlot[] RL =
                {
                    new() { X = 0, Y = 24, OrigX = 102, OrigY = 39 },
                    new() { X = 42, Y = 0, OrigX = 60, OrigY = 63 },
                    new() { X = 84, Y = 24, OrigX = 18, OrigY = 39 },
                    new() { X = 42, Y = 48, OrigX = 60, OrigY = 15 },

                    new() { X = 138, Y = 24, OrigX = 54, OrigY = 39 },
                    new() { X = 180, Y = 0, OrigX = 12, OrigY = 63 },
                    new() { X = 222, Y = 24, OrigX = -30, OrigY = 39 },
                    new() { X = 180, Y = 48, OrigX = 12, OrigY = 15 }
                };

                public static class Left
                {
                    public static readonly MetaSlot[] GroupL =
                    {
                        new() { X = -142, Y = 24, OrigX = 94, OrigY = 39 },
                        new() { X = -100, Y = 0, OrigX = 52, OrigY = 63 },
                        new() { X = -58, Y = 24, OrigX = 10, OrigY = 39 },
                        new() { X = -100, Y = 48, OrigX = 52, OrigY = 15 }
                    };

                    public static readonly MetaSlot[] GroupR =
                    {
                        new() { X = -4, Y = 24, OrigX = 62, OrigY = 39 },
                        new() { X = 38, Y = 0, OrigX = 20, OrigY = 63 },
                        new() { X = 80, Y = 24, OrigX = -22, OrigY = 39 },
                        new() { X = 38, Y = 48, OrigX = 20, OrigY = 15 }
                    };
                }

                public static class Right
                {
                    public static readonly MetaSlot[] GroupL =
                    {
                        new() { X = 142, Y = 24, OrigX = 94, OrigY = 39 },
                        new() { X = 184, Y = 0, OrigX = 52, OrigY = 63 },
                        new() { X = 226, Y = 24, OrigX = 10, OrigY = 39 },
                        new() { X = 184, Y = 48, OrigX = 52, OrigY = 15 }
                    };

                    public static readonly MetaSlot[] GroupR =
                    {
                        new() { X = 280, Y = 24, OrigX = 62, OrigY = 39 },
                        new() { X = 322, Y = 0, OrigX = 20, OrigY = 63 },
                        new() { X = 364, Y = 24, OrigX = -22, OrigY = 39 },
                        new() { X = 322, Y = 48, OrigX = 20, OrigY = 15 }
                    };
                }
            }
        }
    }
}