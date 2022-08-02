using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using NodeTools;
using static CrossUp.CrossUp.Bars.Cross.Selection;

namespace CrossUp;
public sealed unsafe partial class CrossUp
{
    public static partial class Layout
    {
        /// <summary>Plugin feature that displays the bars for Expanded Hold Controls separately from the rest of the Cross Hotbar</summary>
        public class SeparateEx
        {
            /// <summary>Confirms that the Separate Expanded Hold feature is active and that two valid bars are selected</summary>
            internal static bool Ready => IsSetUp && Config.SepExBar && Bars.LR.ID > 0 && Bars.RL.ID > 0;

            /// <summary>Enable the Separate Expanded Hold Bars feature</summary>
            internal static void Enable()
            {
                PrepBar(Bars.LR.ID);
                PrepBar(Bars.RL.ID);

                PluginLog.LogDebug($"Borrowing Hotbar {Bars.LR.ID + 1} to serve as L→R Expanded Hold Bar");
                PluginLog.LogDebug($"Borrowing Hotbar {Bars.RL.ID + 1} to serve as R→L Expanded Hold Bar");

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

                for (var i = 0; i < 12; i++)
                {
                    Bars.ActionBars[barID].Buttons[i].Node->Flags_2 |= 0xD;
                }
            }

            /// <summary>Disable the Separate Expanded Hold Bars feature.</summary>
            internal static void Disable()
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
                    var button = bar.Buttons[i];
                    button[2u].SetVis(false);
                    button[3u].SetVis(!Config.HideUnassigned || actions[i].CommandType != HotbarSlotType.Empty);
                    button.SetAlpha(alpha);
                }
            }

            /// <summary>Visually arrange the borrowed bars to replicate the Expanded Hold bars</summary>
            internal static void Arrange(Select select, Select previous, float scale, int split, bool mixBar, (int, int, int, int) coords, bool forceArrange)
            {
                SetActions(select);
                HiddenCheck();

                var anchorX = (int)(Bars.Cross.Root.Node->X + 146 * scale);
                var anchorY = (int)(Bars.Cross.Root.Node->Y + 70 * scale);
                var (lrX, lrY, rlX, rlY) = coords;

                Bars.LR.BorrowBar.Root.SetScale(scale)
                                      .SetVis(true)
                                      .SetSize(295, 120)
                                      .SetPos(anchorX + (lrX + split) * scale, anchorY + lrY * scale);

                Bars.RL.BorrowBar.Root.SetScale(scale)
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
                                MetaSlots.LR[i].Insert(Bars.LR.Buttons[i], 0F, 0F, exScale); // L->R bar
                                MetaSlots.RL[i].Insert(Bars.RL.Buttons[i], 0F, 0F, exScale); // R->L bar
                                if (i >= 4) continue;

                                MetaSlots.Cross[0][i].SetScale(exScale).SetVis(false); // hide XHB metaSlots
                                MetaSlots.Cross[1][i].SetScale(exScale).SetVis(false);
                                MetaSlots.Cross[2][i].SetScale(exScale).SetVis(false);
                                MetaSlots.Cross[3][i].SetScale(exScale).SetVis(false);

                                Bars.LR.Buttons[i + 8].SetVis(false).SetScale(0.85F); // hide unneeded borrowed buttons
                                Bars.RL.Buttons[i + 8].SetVis(false).SetScale(0.85F);
                            }
                            break;
                        }
                        case Select.Left:
                        {
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.LR[i].Insert(Bars.LR.Buttons[i], 0, 0, 0.85F); // L->R bar
                                MetaSlots.RL[i].Insert(Bars.RL.Buttons[i], 0, 0, 0.85F); // R->L bar
                                if (i >= 4) continue;

                                // XHB
                                MetaSlots.Cross[0][i].SetVis(false).SetScale(1.1F);
                                var fromLR = previous == Select.LR;
                                MetaSlots.Cross[1][i].SetVis(fromLR && mixBar).SetScale(!mixBar ? 1.1F : 0.85F);
                                MetaSlots.Cross[2][i].SetVis(fromLR && !mixBar).SetScale(!mixBar ? 0.85F : 1.1F);
                                MetaSlots.Cross[3][i].SetVis(fromLR).SetScale(0.85F).Insert(Bars.RL.Buttons[i + 8], -rlX + split, -rlY, 0.85F);

                                (!mixBar ? MetaSlots.Cross[2][i] : 
                                           MetaSlots.Cross[1][i]).Insert(Bars.LR.Buttons[i + 8], (!mixBar ? split : -split) - lrX, -lrY, 0.85F);
                            }
                            break;
                        }
                        case Select.Right:
                        {
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.LR[i].Insert(Bars.LR.Buttons[i], 0, 0, 0.85F); // L->R bar
                                MetaSlots.RL[i].Insert(Bars.RL.Buttons[i], 0, 0, 0.85F); // R->L bar
                                if (i >= 4) continue;

                                // XHB
                                var fromRL = previous == Select.RL;
                                MetaSlots.Cross[0][i].SetVis(fromRL).SetScale(0.85F).Insert(Bars.LR.Buttons[i + 8], -lrX - split, -lrY, 0.85F);
                                MetaSlots.Cross[1][i].SetVis(fromRL && !mixBar).SetScale(!mixBar ? 0.85F : 1.1F);
                                MetaSlots.Cross[2][i].SetVis(fromRL && mixBar).SetScale(!mixBar ? 1.1F : 0.85F);
                                MetaSlots.Cross[3][i].SetVis(false).SetScale(1.1F);

                                (!mixBar ? MetaSlots.Cross[1][i] :
                                           MetaSlots.Cross[2][i]).Insert(Bars.RL.Buttons[i + 8], (!mixBar ? -split : split) - rlX, -rlY, 0.85F);
                            }
                            break;
                        }
                        case Select.LR:
                        {
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.LR[i].Scale = 1.1F;
                                MetaSlots.RL[i].Insert(Bars.RL.Buttons[i], 0, 0, 0.85F); // R->L bar
                                if (i >= 4) continue;

                                // XHB
                                MetaSlots.Cross[0][i].SetVis(true).Insert(Bars.LR.Buttons[i], -lrX - split, -lrY, 0.85F);
                                MetaSlots.Cross[1][i].SetVis(true).Insert(Bars.LR.Buttons[i + (!mixBar ? 4 : 8)], -lrX - split, -lrY, 0.85F);
                                MetaSlots.Cross[2][i].SetVis(true).Insert(Bars.LR.Buttons[i + (!mixBar ? 8 : 4)], -lrX + split, -lrY, 0.85F);
                                MetaSlots.Cross[3][i].SetVis(true).Insert(Bars.RL.Buttons[i + 8], -rlX + split, -rlY, 0.85F);
                            }
                            break;
                        }
                        case Select.RL:
                            for (var i = 0; i < 8; i++)
                            {
                                MetaSlots.RL[i].SetScale(1.1F);
                                MetaSlots.LR[i].SetScale(Config.OnlyOneEx ? 1.1F : 0.85F);
                                (!Config.OnlyOneEx ? MetaSlots.LR : MetaSlots.RL)[i]
                                    .Insert(Bars.LR.Buttons[i], 0, 0, 0.85F); // L->R bar
                                if (i >= 4) continue;

                                // XHB
                                MetaSlots.Cross[0][i].SetVis(true).Insert(Bars.LR.Buttons[i + 8], -lrX - split, -lrY, 0.85F);
                                MetaSlots.Cross[1][i].SetVis(true).Insert(Bars.RL.Buttons[i + (!mixBar ? 8 : 0)], -rlX - split, -rlY, 0.85F);
                                MetaSlots.Cross[2][i].SetVis(true).Insert(Bars.RL.Buttons[i + (!mixBar ? 0 : 8)], -rlX + split, -rlY, 0.85F);
                                MetaSlots.Cross[3][i].SetVis(true).Insert(Bars.RL.Buttons[i + 4], -rlX + split, -rlY, 0.85F);
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
                var lrID = Bars.LR.ID;
                var rlID = Bars.RL.ID;
                if (lrID < 1 || rlID < 1) return;

                var crossVis = CharConfig.Cross.Visible;
                var lrVis = CharConfig.Hotbar.Visible[lrID];
                var rlVis = CharConfig.Hotbar.Visible[rlID];

                if (lrVis == crossVis && rlVis == crossVis) return;

                lrVis.Set(crossVis);
                rlVis.Set(crossVis);
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
                        Actions.Copy(actions.LR, 0, Bars.LR.ID, 0, 8);
                        Actions.Copy(actions.RL, 0, Bars.RL.ID, 0, 8);
                        break;
                    }
                    case Select.Left:
                    {
                        Actions.Copy(actions.LR, 0, Bars.LR.ID, 0, 8);
                        Actions.Copy(actions.RL, 0, Bars.RL.ID, 0, 8);
                        Actions.Copy(actions.Cross, 8, Bars.LR.ID, 8, 4);
                        Actions.Copy(actions.Cross, 12, Bars.RL.ID, 8, 4);
                        break;
                    }
                    case Select.Right:
                    {
                        Actions.Copy(actions.LR, 0, Bars.LR.ID, 0, 8);
                        Actions.Copy(actions.RL, 0, Bars.RL.ID, 0, 8);
                        Actions.Copy(actions.Cross, 0, Bars.LR.ID, 8, 4);
                        Actions.Copy(actions.Cross, 4, Bars.RL.ID, 8, 4);
                        break;
                    }
                    case Select.LR:
                    {
                        Actions.Copy(actions.RL, 0, Bars.RL.ID, 0, 8);
                        Actions.Copy(actions.Cross, 0, Bars.LR.ID, 0, 12);
                        Actions.Copy(actions.Cross, 12, Bars.RL.ID, 8, 4);
                        break;
                    }
                    case Select.RL:
                    {
                        Actions.Copy(actions.LR, 0, Bars.LR.ID, 0, 8);
                        Actions.Copy(actions.Cross, 0, Bars.LR.ID, 8, 4);
                        Actions.Copy(actions.Cross, 4, Bars.RL.ID, 8, 4);
                        Actions.Copy(actions.Cross, 8, Bars.RL.ID, 0, 8);
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
                    internal MetaSlot(Vector2 pos, Vector2 orig)
                    {
                        Position = pos;
                        Origin = orig;
                    }
                    internal bool Visible { get; set; } = true;
                    private Vector2 Position { get; }
                    private Vector2 Origin { get; }
                    private Vector2 Mod { get; set; }
                    internal float Scale { get; set; } = 1.0F;
                    public sealed class ScaleTween
                    {
                        internal DateTime Start { get; init; }
                        internal TimeSpan Duration { get; init; }
                        internal float FromScale { get; init; }
                        internal float ToScale { get; init; }
                    }
                    private ScaleTween? Tween { get; set; }
                    private NodeWrapper? Button { get; set; }

                    public static implicit operator NodeWrapper.PropertySet(MetaSlot p) => new()
                    {
                        Position = new(p.Position.X + p.Mod.X, p.Position.Y + p.Mod.Y),
                        Origin = p.Origin,
                        Scale = p.Scale,
                        Visible = p.Visible
                    };

                    internal MetaSlot SetVis(bool show)
                    {
                        Visible = show;
                        return this;
                    }

                    internal MetaSlot SetScale(float scale)
                    {
                        Scale = scale;
                        return this;
                    }

                    /// <summary>Places a button node into a MetaSlot, and sets it up to be animated if necessary.</summary>
                    internal MetaSlot Insert(NodeWrapper button, float xMod = 0, float yMod = 0, float scale = 1F)
                    {
                        Mod = new Vector2(xMod,yMod);
                        Button = button;

                        if (Math.Abs(scale - Scale) > 0.01f)
                        {
                            if (Tween == null || Math.Abs(Tween.ToScale - scale) > 0.01f)
                            {
                                Tween = new()
                                {
                                    FromScale = Scale,
                                    ToScale = scale,
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
                            Scale = scale;
                        }

                        Button.SetProps(this);
                        return this;
                    }

                    /// <summary>Checks if this MetaSlot has an active animation tween, and if so, processes it</summary>
                    internal void RunTween()
                    {
                        if (Tween == null || Button == null || Button.Node == null) return;

                        TweensExist = true;

                        var timePassed = DateTime.Now - Tween.Start;
                        var progress = (float)decimal.Divide(timePassed.Milliseconds, Tween.Duration.Milliseconds);
                        var to = Tween.ToScale;
                        var from = Tween.FromScale;

                        Tween = progress < 1 ? Tween : null;
                        Scale = progress < 1 ? (to - from) * progress + from : to;

                        Button.SetProps(this);
                    }
                }

                /// <summary>Whether or not there are currently any animation tweens to run</summary>
                internal static bool TweensExist { get; private set; }

                /// <summary>Run all animations tweens for all MetaSlots</summary>
                internal static void TweenAll()
                {
                    TweensExist = false; // will set back to true if we find any incomplete tweens
                    foreach (var mSlot in LR) mSlot.RunTween();
                    foreach (var mSlot in RL) mSlot.RunTween();
                    foreach (var mSlot in Cross[0]) mSlot.RunTween();
                    foreach (var mSlot in Cross[1]) mSlot.RunTween();
                    foreach (var mSlot in Cross[2]) mSlot.RunTween();
                    foreach (var mSlot in Cross[3]) mSlot.RunTween();
                }

                internal static readonly MetaSlot[] LR =
                {
                    new(pos: new(0,24), orig: new(102,39)),
                    new(pos: new(42,0), orig: new(60,63)),
                    new(pos: new(84,24), orig: new(18,39)),
                    new(pos: new(42,48), orig: new(60,15)),

                    new(pos: new(138,24), orig: new(54,39)),
                    new(pos: new(180,0 ), orig: new(12,63)),
                    new(pos: new(222,24), orig: new(-30,39)),
                    new(pos: new(180,48), orig: new(12,15))
                };

                internal static readonly MetaSlot[] RL =
                {
                    new(pos: new(0,24), orig: new(102,39)),
                    new(pos: new(42,0), orig: new(60,63)),
                    new(pos: new(84,24), orig: new(18,39)),
                    new(pos: new(42,48), orig: new(60,15)),

                    new(pos: new(138,24), orig: new(54,39)),
                    new(pos: new(180,0 ), orig: new(12,63)),
                    new(pos: new(222,24), orig: new(-30,39)),
                    new(pos: new(180,48), orig: new(12,15))
                };

                internal static readonly MetaSlot[][] Cross = {
                    new MetaSlot[]
                    {
                        new(pos: new(-142, 24), orig: new(94, 39)),
                        new(pos: new(-100, 0),  orig: new(52, 63)),
                        new(pos: new(-58, 24),  orig: new(10, 39)),
                        new(pos: new(-100, 48), orig: new(52, 15))
                    },
                    new MetaSlot[]
                    {
                        new(pos: new(-4, 24 ), orig: new(62,  39 )),
                        new(pos: new(38, 0  ), orig: new(20,  63 )),
                        new(pos: new(80, 24 ), orig: new(-22, 39 )),
                        new(pos: new(38, 48 ), orig: new(20,  15 ))
                    },
                    new MetaSlot[]
                    {
                        new(pos: new(142, 24 ), orig: new(94, 39)),
                        new(pos: new(184, 0  ), orig: new(52, 63)),
                        new(pos: new(226, 24 ), orig: new(10, 39)),
                        new(pos: new(184, 48 ), orig: new(52, 15))
                    },
                    new MetaSlot[]
                    {
                        new(pos: new(280, 24 ), orig: new(62,  39 )),
                        new(pos: new(322, 0  ), orig: new(20,  63 )),
                        new(pos: new(364, 24 ), orig: new(-22, 39 )),
                        new(pos: new(322, 48 ), orig: new(20,  15 ))
                    }
                };
            }
        }
    }
}