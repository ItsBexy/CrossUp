using System;
using System.Numerics;
using CrossUp.Features.Layout;
using CrossUp.Game.Hotbar;
using static CrossUp.CrossUp;
using static CrossUp.Utility.Service;

namespace CrossUp.Features;

internal class Color
{
    public struct Preset
    {
        public static readonly Vector3 White = new(1F);
        public static readonly Vector3 TextGlow = new(157f / 255f, 131f / 255f, 91f / 255f);
        public static readonly Vector3 MultiplyNeutral = new(100f / 255f);
    }

    /// <summary>Apply selected highlight colour to all XHB and WXHB highlight ninegrids</summary>
    private static void SetSelectBG(bool reset = false)
    {
        var multiply = reset ? Preset.MultiplyNeutral : Profile.SelectColorMultiply;
        var blend = (uint)(reset ? 0 : Profile.SelectBlend);
        var style = reset ? 0 : Profile.SelectStyle;
        var hide = style == 2;
        var uvwh = BGStyles[style, 0];
        var offset = BGStyles[style, 1];
        var scale = style == 1 ? 1.02f : 1f;

        Vector2 normSize = hide ? new(0) : new(300, 140);
        Vector2 miniSize = hide ? new(0) : new(166, 140);

        if (Bars.Cross.Exists)
        {
            Bars.Cross.SelectBG
                .SetMultiply(multiply)
                .SetBlend(blend)
                .SetSize(normSize)
                .SetOrigin(160,67)
                .SetScale(scale)
                .SetBGCoords(uvwh, offset);

            Bars.Cross.MiniSelectR
                .SetMultiply(multiply)
                .SetBlend(blend)
                .SetSize(miniSize)
                .SetBGCoords(uvwh, offset);

            Bars.Cross.MiniSelectL
                .SetMultiply(multiply)
                .SetBlend(blend)
                .SetSize(miniSize)
                .SetBGCoords(uvwh, offset);
        }

        if (Bars.WXHB.Exists)
        {
            Bars.WXHB.LL.SelectBG
                .SetMultiply(multiply)
                .SetBlend(blend)
                .SetSize(normSize)
                .SetOrigin(160, 67)
                .SetScale(scale)
                .SetBGCoords(uvwh, offset);

            Bars.WXHB.LL.MiniSelect
                .SetMultiply(multiply)
                .SetBlend(blend)
                .SetSize(miniSize)
                .SetBGCoords(uvwh, offset);

            Bars.WXHB.RR.SelectBG
                .SetMultiply(multiply)
                .SetBlend(blend)
                .SetSize(normSize)
                .SetOrigin(160, 67)
                .SetScale(scale)
                .SetBGCoords(uvwh, offset);

            Bars.WXHB.RR.MiniSelect
                .SetMultiply(multiply)
                .SetBlend(blend)
                .SetSize(miniSize)
                .SetBGCoords(uvwh, offset);
        }

        if (Bars.ActionContents.Exists)
        {
            SetDutyActionBG(reset, multiply);
        }

        Log.Debug($"Selection Color Set: {multiply}, {(reset ? 0 : Profile.SelectBlend) switch { 0 => "Normal", 1 => "Hide", _ => "Dodge" }}");
    }

    /// <summary>Presets for the BG texture options</summary>
    private static readonly Vector4[,] BGStyles =
    {
        {new(0,0,104,104), new(48)},
        {new(284, 28, 40, 40), new(9)},
        {new(0,0,104,104), new(48)}
    };

    /// <summary>Set/Reset colors of pressed buttons</summary>
    private static void SetPulse(bool reset = false)
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

        if (SeparateEx.Ready && Bars.LR.Exists && Bars.RL.Exists)
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

        Log.Debug($"Button Colors Set; Glow: {glowA}, Pulse: {glowB}");
    }

    /// <summary>Set/Reset Text and border colors</summary>
    private static void SetText(bool reset = false)
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

    /// <summary>Set the BG color for duty-specific actions</summary>
    private static void SetDutyActionBG(bool reset, Vector3 multiply)
    {
        var dutyActionColor = Preset.White;

        if (!reset)
        {
            var saturation = System.Drawing.Color.FromArgb((int)(multiply.X * 255), (int)(multiply.Y * 255), (int)(multiply.Z * 255)).GetSaturation();

            float Adjust(float val) => val * saturation + Math.Min(1f, val * 2.55f) * (1f - saturation);

            dutyActionColor = new Vector3
            {
                X = Adjust(multiply.X),
                Y = Adjust(multiply.Y),
                Z = Adjust(multiply.Z)
            };
        }

        Bars.ActionContents.BG1.SetColor(dutyActionColor);
        Bars.ActionContents.BG2.SetColor(dutyActionColor);
        Bars.ActionContents.BG3.SetColor(dutyActionColor);
        Bars.ActionContents.BG4.SetColor(dutyActionColor);
    }

    public static void SetAll(bool reset = false)
    {
        SetPulse(reset);
        SetText(reset);
        SetSelectBG(reset);
    }
}