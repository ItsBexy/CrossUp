using System;
using System.Numerics;
using CrossUp.Utility;

namespace CrossUp.Features.Layout
{
    public unsafe partial class SeparateEx
    {
        /// <summary>MetaSlots are various positions that a "borrowed" bar's buttons can be placed in, to imitate elements of the Cross Hotbar. Essentially: slots to place slots in.</summary>
        public static class MetaSlots
        {
            /// <summary>A position in which a "borrowed" button can be placed</summary>
            public sealed class MetaSlot
            {
                internal MetaSlot(Vector2 pos, Vector2 origin)
                {
                    Position = pos;
                    Origin = origin;
                }

                internal bool Visible { get; set; } = true;
                private Vector2 Position { get; }
                private Vector2 Origin { get; }
                private Vector2 Mod { get; set; }
                internal float Scale { get; set; } = 1.0F;

                private sealed class ScaleTween
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
                // ReSharper disable once UnusedMethodReturnValue.Global
                internal MetaSlot Insert(NodeWrapper button, float xMod = 0, float yMod = 0, float scale = 1F)
                {
                    Mod = new Vector2(xMod, yMod);
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

            /// <summary>Run all animation tweens for all MetaSlots</summary>
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
                new(pos: new(0, 24),   origin: new(102, 39)),
                new(pos: new(42, 0),   origin: new(60, 63)),
                new(pos: new(84, 24),  origin: new(18, 39)),
                new(pos: new(42, 48),  origin: new(60, 15)),

                new(pos: new(138, 24), origin: new(54, 39)),
                new(pos: new(180, 0),  origin: new(12, 63)),
                new(pos: new(222, 24), origin: new(-30, 39)),
                new(pos: new(180, 48), origin: new(12, 15))
            };

            internal static readonly MetaSlot[] RL =
            {
                new(pos: new(0, 24),   origin: new(102, 39)),
                new(pos: new(42, 0),   origin: new(60, 63)),
                new(pos: new(84, 24),  origin: new(18, 39)),
                new(pos: new(42, 48),  origin: new(60, 15)),

                new(pos: new(138, 24), origin: new(54, 39)),
                new(pos: new(180, 0),  origin: new(12, 63)),
                new(pos: new(222, 24), origin: new(-30, 39)),
                new(pos: new(180, 48), origin: new(12, 15))
            };

            internal static readonly MetaSlot[][] Cross =
            {
                new MetaSlot[]
                {
                    new(pos: new(-142, 24), origin: new(94, 39)),
                    new(pos: new(-100, 0),  origin: new(52, 63)),
                    new(pos: new(-58, 24),  origin: new(10, 39)),
                    new(pos: new(-100, 48), origin: new(52, 15))
                },
                new MetaSlot[]
                {
                    new(pos: new(-4, 24),   origin: new(62, 39)),
                    new(pos: new(38, 0),    origin: new(20, 63)),
                    new(pos: new(80, 24),   origin: new(-22, 39)),
                    new(pos: new(38, 48),   origin: new(20, 15))
                },
                new MetaSlot[]
                {
                    new(pos: new(142, 24),  origin: new(94, 39)),
                    new(pos: new(184, 0),   origin: new(52, 63)),
                    new(pos: new(226, 24),  origin: new(10, 39)),
                    new(pos: new(184, 48),  origin: new(52, 15))
                },
                new MetaSlot[]
                {
                    new(pos: new(280, 24),  origin: new(62, 39)),
                    new(pos: new(322, 0),   origin: new(20, 63)),
                    new(pos: new(364, 24),  origin: new(-22, 39)),
                    new(pos: new(322, 48),  origin: new(20, 15))
                }
            };
        }
    }
}