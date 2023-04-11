using System;
using System.Numerics;
using Dalamud.Logging;

namespace CrossUp;

public sealed partial class CrossUp
{
    /// <summary>Color Customization features</summary>
    internal class Color
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
            var multiply = reset ? Preset.MultiplyNeutral : Profile.SelectColorMultiply;
            var displayType = reset ? 0 : Profile.SelectDisplayType;

            var blend = (uint)(displayType == 2 ? 2 : 0);
            var hide = displayType == 1;

            Vector2 normSize = hide ? new(0) : new(304, 140);
            Vector2 miniSize = hide ? new(0) : new(166, 140);

            if (Bars.Cross.Exists)
            {
                Bars.Cross.SelectBG.SetMultiply(multiply).SetBlend(blend).SetSize(normSize);
                Bars.Cross.MiniSelectR.SetMultiply(multiply).SetBlend(blend).SetSize(miniSize);
                Bars.Cross.MiniSelectL.SetMultiply(multiply).SetBlend(blend).SetSize(miniSize);
            }

            if (Bars.WXHB.Exists)
            {
                Bars.WXHB.LL.SelectBG.SetMultiply(multiply).SetBlend(blend).SetSize(normSize);
                Bars.WXHB.LL.MiniSelect.SetMultiply(multiply).SetBlend(blend).SetSize(miniSize);

                Bars.WXHB.RR.SelectBG.SetMultiply(multiply).SetBlend(blend).SetSize(normSize);
                Bars.WXHB.RR.MiniSelect.SetMultiply(multiply).SetBlend(blend).SetSize(miniSize);
            }

            if (Bars.ActionContents.Exists)
            {
                var dutyActionColor = Preset.White;
                if (!reset)
                {
                    var saturation = System.Drawing.Color
                        .FromArgb((int)(multiply.X * 255), (int)(multiply.Y * 255), (int)(multiply.Z * 255))
                        .GetSaturation();

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

            PluginLog.LogVerbose(
                $"Selection Color Set: {multiply}, {displayType switch { 0 => "Normal", 1 => "Hide", _ => "Dodge" }}");
        }

        /// <summary>Set/Reset colors of pressed buttons</summary>
        public static void SetPulse(bool reset = false)
        {
            var glowA = reset ? Preset.White : Profile.GlowA;
            var glowB = reset ? Preset.White : Profile.GlowB;

            if (Bars.Cross.Exists)
            {
                for (var set = 0; set < 4; set++)
                for (uint bID = 2; bID <= 5; bID++)
                {
                    var iconNode = Bars.Cross.Buttons[set][bID][2u];
                    iconNode[8u].SetColor(glowA);
                    iconNode[4u].SetColor(glowB);
                }
            }

            if (Bars.WXHB.Exists)
            {
                for (var set = 0; set < 4; set++)
                for (uint bID = 2; bID <= 5; bID++)
                {
                    var iconNode = Bars.WXHB.Buttons[set][bID][2u];
                    iconNode[8u].SetColor(glowA);
                    iconNode[4u].SetColor(glowB);
                }
            }

            if (Layout.SeparateEx.Ready && Bars.LR.Exists && Bars.RL.Exists)
            {
                for (var i = 0; i < 12; i++)
                {
                    var iconNodeLR = Bars.LR.Buttons[i][3u][2u];
                    iconNodeLR[8u].SetColor(glowA);
                    iconNodeLR[4u].SetColor(glowB);

                    var iconNodeRL = Bars.RL.Buttons[i][3u][2u];
                    iconNodeRL[8u].SetColor(glowA);
                    iconNodeRL[4u].SetColor(glowB);
                }
            }

            PluginLog.LogVerbose($"Button Colors Set; Glow: {glowA}, Pulse: {glowB}");
        }

        /// <summary>Set/Reset Text and border colors</summary>
        public static void SetText(bool reset = false)
        {
            if (!Bars.Cross.Exists) return;

            var color = reset ? Preset.White : Profile.TextColor;
            var glow = reset ? Preset.TextGlow : Profile.TextGlow;
            var border = reset ? Preset.White : Profile.BorderColor;

            Bars.Cross.LTtext.SetTextColor(color, glow);
            Bars.Cross.RTtext.SetTextColor(color, glow);
            Bars.Cross.SetText.SetTextColor(color, glow);
            Bars.Cross.SetNum.SetTextColor(color, glow);
            Bars.Cross.SetButton.SetTextColor(color, glow);

            foreach (var num in Bars.Cross.ChangeSetDisplay.Nums)
            {
                num[3u].SetTextColor(color, glow);
                num[4u].SetColor(border);
            }

            Bars.Cross.ChangeSetDisplay.Text.SetTextColor(color, glow);
            Bars.Cross.VertLine.SetColor(border);
            Bars.Cross.SetBorder.SetColor(border);
        }

        public static void SetAll(bool reset = false)
        {
            SetPulse(reset);
            SetText(reset);
            SetSelectBG(reset);
        }
    }
}