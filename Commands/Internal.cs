using System;
using System.Numerics;
using CrossUp.Features;
using CrossUp.Features.Layout;
using CrossUp.Game;
using CrossUp.Game.Hooks;
using CrossUp.UI;
using static CrossUp.CrossUp;

// ReSharper disable MemberCanBePrivate.Global

namespace CrossUp.Commands;

/// <summary>Methods that perform actions within CrossUp. Can be called by the plugin UI, chat commands, or IPC</summary>
internal class InternalCmd
{
    public static void ToggleDebug(CrossUpUI ui) => ui.DebugWindow.Show = !ui.DebugWindow.Show;
    public static void ToggleSettings(CrossUpUI ui) => ui.SettingsWindow.Show = !ui.SettingsWindow.Show;
    public static void ToggleSettings(CrossUpUI ui, bool show) => ui.SettingsWindow.Show = show;

    internal static void SplitOn(bool on)
    {
        Profile.SplitOn = on;
        ApplyLayout();
    }

    internal static void SplitDist(int dist)
    {
        Profile.SplitDist = dist;
        ApplyLayout();
    }

    internal static void Center(int c)
    {
        Profile.CenterPoint = c;
        ApplyLayout();
    }

    internal static void Padlock(bool show, int x, int y)
    {
        Profile.HidePadlock = !show;
        Profile.PadlockOffset = new(x, y);
        ApplyLayout();
    }

    internal static void Padlock(bool show)
    {
        Profile.HidePadlock = !show;
        ApplyLayout();
    }

    internal static void SetNumText(bool show, int x, int y)
    {
        Profile.HideSetText = !show;
        Profile.SetTextOffset = new(x, y);
        ApplyLayout();
    }

    internal static void SetNumText(bool show)
    {
        Profile.HideSetText = !show;
        ApplyLayout();
    }

    internal static void ChangeSet(int x, int y)
    {
        Profile.ChangeSetOffset = new(x, y);
        ApplyLayout();
    }

    internal static void TriggerText(bool show)
    {
        Profile.HideTriggerText = !show;
        ApplyLayout();
    }

    internal static void EmptySlots(bool show)
    {
        Profile.HideUnassigned = !show;
        ApplyLayout();
    }

    internal static void SelectBG(int style, int blend, Vector3 color)
    {
        Profile.SelectStyle = style;
        Profile.SelectBlend = blend;
        Profile.SelectColorMultiply = color;
        ApplyColor();
    }

    internal static void ButtonGlow(Vector3 glow)
    {
        Profile.GlowA = glow;
        ApplyColor();
    }

    internal static void ButtonPulse(Vector3 pulse)
    {
        Profile.GlowB = pulse;
        ApplyColor();
    }

    internal static void TextColor(Vector3 color1, Vector3 color2)
    {
        Profile.TextColor = color1;
        Profile.TextGlow = color2;
        ApplyColor();
    }

    internal static void BorderColor(Vector3 color)
    {
        Profile.BorderColor = color;
        ApplyColor();
    }

    internal static void ExBarOn(bool show)
    {
        Profile.SepExBar = show;
        ApplyExBar();
    }

    internal static void ExBar(bool show, int lr, int rl, bool onlyOne=false)
    {
        Profile.SepExBar = show;
        Profile.OnlyOneEx = onlyOne;

        if (lr is > 1 and <= 10 && rl is > 1 and <= 10 && lr != rl)
        {
            Config.LRborrow = lr - 1;
            Config.RLborrow = rl - 1;
        }
        ApplyExBar();
    }

    internal static void ExBarOnlyOne(bool val)
    {
        Profile.OnlyOneEx = val;
        ApplyExBar();
    }

    internal static void LRpos(int x, int y)
    {
        Profile.LRpos = new(x, y);
        ApplyLayout();
    }

    internal static void RLpos(int x, int y)
    {
        Profile.RLpos = new(x, y);
        ApplyLayout();
    }

    internal static void CombatFade(bool active, int inCombat, int outCombat)
    {
        Profile.CombatFadeInOut = active;
        Profile.TranspInCombat = Math.Min(100,Math.Max(0,inCombat));
        Profile.TranspOutOfCombat = Math.Min(100, Math.Max(0, outCombat));
        if (!active) GameConfig.Cross.Transparency.Standard.Set(0);
        else Events.OnConditionChange();
        Config.Save();
    }

    internal static void CombatFade(bool active)
    {
        Profile.CombatFadeInOut = active;
        if (!active) GameConfig.Cross.Transparency.Standard.Set(0);
        else Events.OnConditionChange();
        Config.Save();
    }

    private static void ApplyExBar()
    {
        Config.Save();
        SeparateEx.EnableIfReady();
    }

    private static void ApplyLayout()
    {
        Config.Save();
        Layout.Update(true);
    }

    private static void ApplyColor()
    {
        Config.Save();
        Color.SetAll();
    }

    // tuple overloads (used for IPC):

    internal static void SplitBar((bool split, int distance, int center) t)
    {
        SplitOn(t.split);
        SplitDist(t.distance);
        Center(t.center);
    }

    internal static void Padlock((int x, int y, bool hide) t) => Padlock(!t.hide, t.x, t.y);

    internal static void SetNumText((int x, int y, bool hide) t) => SetNumText(!t.hide, t.x, t.y);

    internal static void SelectBG((int style, int blend, Vector3 color) s) => SelectBG(s.style, s.blend, s.color);

    internal static void ButtonGlow((Vector3 glow, Vector3 pulse) t)
    {
        ButtonGlow(t.glow);
        ButtonPulse(t.pulse);
    }

    internal static void TextAndBorders((Vector3 text, Vector3 glow, Vector3 border) t)
    {
        TextColor(t.text, t.glow);
        BorderColor(t.border);
    }

    internal static void LRpos((int x, int y) t) => LRpos(t.x, t.y);

    internal static void RLpos((int x, int y) t) => RLpos(t.x, t.y);

    internal static void ChangeSet((int x, int y) t) => ChangeSet(t.x, t.y);

    internal static void ExBar((bool show, bool onlyOne) t)
    {
        ExBarOn(t.show);
        ExBarOnlyOne(t.onlyOne);
    }


}