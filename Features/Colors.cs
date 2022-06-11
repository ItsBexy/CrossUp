using System;
using System.Numerics;

namespace CrossUp;

public sealed partial class CrossUp
{
    /// <summary>Color Customization features</summary>
    public class Color
    {
        public struct Preset
        {
            public static readonly Vector3 White = new(1F, 1F, 1F);
            public static readonly Vector3 TextGlow = new(157f / 255f, 131f / 255f, 91f / 255f);
            public static readonly Vector3 MultiplyNeutral = new(100f / 255f, 100f / 255f, 100f / 255f);
        }

        /// <summary>Apply selected highlight colour to all XHB and WXHB highlight ninegrids</summary>
        public static void SetSelectBG(bool reset = false)
        {
            var multiply = reset ? Preset.MultiplyNeutral : Config.SelectColorMultiply;
            var displayType = reset ? 0 : Config.SelectDisplayType;

            var blend = (uint)(displayType == 2 ? 2 : 0);
            var hide = displayType == 1;

            var normSize = hide ? new Vector2 { X = 0, Y = 0 } : new Vector2 { X = 304, Y = 140 };
            var miniSize = hide ? new Vector2 { X = 0, Y = 0 } : new Vector2 { X = 166, Y = 140 };

            if (Bars.Cross.Exists)
            {
                Bars.Cross.SelectBG.SetMultiply(multiply).SetBlend(blend).SetSize(normSize);
                Bars.Cross.MiniSelectR.SetMultiply(multiply).SetBlend(blend).SetSize(miniSize);
                Bars.Cross.MiniSelectL.SetMultiply(multiply).SetBlend(blend).SetSize(miniSize);
            }

            if (Bars.WXLL.Exists && Bars.WXRR.Exists)
            {
                Bars.WXLL.SelectBG.SetMultiply(multiply).SetBlend(blend).SetSize(normSize);
                Bars.WXLL.MiniSelect.SetMultiply(multiply).SetBlend(blend).SetSize(miniSize);
        
                Bars.WXRR.SelectBG.SetMultiply(multiply).SetBlend(blend).SetSize(normSize);
                Bars.WXRR.MiniSelect.SetMultiply(multiply).SetBlend(blend).SetSize(miniSize);
            }

            if (Bars.ActionContents.Exists)
            {
                var dutyActionColor = Preset.White;
                if (!reset)
                {
                    var saturation = System.Drawing.Color.FromArgb((int)(multiply.X * 255), (int)(multiply.Y * 255), (int)(multiply.Z * 255)).GetSaturation();

                    dutyActionColor = new Vector3
                    {
                        X = multiply.X * saturation + Math.Min(1f, multiply.X * 2.55f) * (1f - saturation),
                        Y = multiply.Y * saturation + Math.Min(1f, multiply.Y * 2.55f) * (1f - saturation),
                        Z = multiply.Z * saturation + Math.Min(1f, multiply.Z * 2.55f) * (1f - saturation)
                    };
                }

                Bars.ActionContents.BG1.SetColor(dutyActionColor);
                Bars.ActionContents.BG2.SetColor(dutyActionColor).SetBlend(blend);
                Bars.ActionContents.BG3.SetColor(dutyActionColor).SetBlend(blend);
                Bars.ActionContents.BG4.SetColor(dutyActionColor).SetBlend(blend);
            }
        }

        /// <summary>Set/Reset colors of pressed buttons</summary>
        public static void SetPulse(bool reset = false)
        {
            var glowA = reset ? Preset.White : Config.GlowA;
            var glowB = reset ? Preset.White : Config.GlowB;

            if (Bars.Cross.Exists)
            {
                var groups = new[] {
                    Bars.Cross.Left.GroupL,
                    Bars.Cross.Left.GroupR,
                    Bars.Cross.Right.GroupL,
                    Bars.Cross.Right.GroupR
                };

                foreach (var group in groups)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        group.ChildNode(i, 2, 10).SetColor(glowA);
                        group.ChildNode(i, 2, 14).SetColor(glowB);
                    }
                }
            }

            if (Bars.WXLL.Exists && Bars.WXRR.Exists)
            {
                var groups = new[] {
                    Bars.WXLL.GroupL,
                    Bars.WXLL.GroupR,
                    Bars.WXRR.GroupL,
                    Bars.WXRR.GroupR
                };

                foreach (var group in groups)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        group.ChildNode(i, 2, 10).SetColor(glowA);
                        group.ChildNode(i, 2, 14).SetColor(glowB);
                    }
                }
            }

            if (Layout.SeparateEx.Ready && Bars.LR.BorrowBar.Exists && Bars.RL.BorrowBar.Exists)
            {
                for (var i = 0; i < 12; i++)
                {
                    var buttonLR = Bars.LR.BorrowBar.GetButton(i);
                    buttonLR.ChildNode(0, 2, 10).SetColor(glowA);
                    buttonLR.ChildNode(0, 2, 14).SetColor(glowB);
    
                    var buttonRL = Bars.RL.BorrowBar.GetButton(i);
                    buttonRL.ChildNode(0, 2, 10).SetColor(glowA);
                    buttonRL.ChildNode(0, 2, 14).SetColor(glowB);
                }
            }
        }

        /// <summary>Set/Reset Text and border colors</summary>
        public static void SetText(bool reset=false)
        {
            if (!Bars.Cross.Exists) return;

            var color = reset ? Preset.White : Config.TextColor;
            var glow = reset ? Preset.TextGlow : Config.TextGlow;
            var border = reset ? Preset.White : Config.BorderColor;

            Bars.Cross.LTtext.SetTextColor(color,glow);
            Bars.Cross.RTtext.SetTextColor(color, glow);
            Bars.Cross.SetText.SetTextColor(color, glow);
            Bars.Cross.SetNum.SetTextColor(color, glow);
            Bars.Cross.SetButton.SetTextColor(color, glow);

            var numText = Bars.Cross.ChangeSetDisplay.Nums;
            foreach (var num in numText)
            {
                num.ChildNode(1).SetTextColor(color, glow);
                num.ChildNode(0).SetColor(border);
            }

            Bars.Cross.ChangeSetDisplay.Text.SetTextColor(color, glow);
            Bars.Cross.VertLine.SetColor(border);
            Bars.Cross.SetBorder.SetColor(border);
        }

        /// <summary>Reset all colours to defaults (called on dispose)</summary>
        public static void Reset()
        {
            SetSelectBG(true);
            SetPulse(true);
            SetText(true);
        }
    }
}