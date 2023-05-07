using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace CrossUp;

internal sealed partial class CrossUpUI
{
    private bool OldConfigsChecked;
    private bool ShowUpdateWarning;
    private void DrawMsgWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(320 * XupGui.Scale, 240 * XupGui.Scale), ImGuiCond.Always);
        if (!ImGui.Begin("CrossUp Notice v0.4.2.0", ref ShowUpdateWarning, ImGuiWindowFlags.NoResize)) return;

        ImGui.Text(Strings.UpdateWarning0420);

        XupGui.BumpCursorY(40f * XupGui.Scale);
        ImGui.Text("Open CrossUp Settings: ");
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
        {
            SettingsVisible = true;
            ShowUpdateWarning = false;
        }

        ImGui.End();
    }
    private void PortOldConfigs()
    {
        OldConfigsChecked = true;
        if (!Config.Split.HasValue) return;

        ShowUpdateWarning = true;

        Profile old = new() // build a new profile out of the user's legacy settings
        {
            SplitDist = Config.Split ?? 0,
            SplitOn = (Config.Split ?? 0) > 0,
            CenterPoint = 0,
            PadlockOffset = Config.PadlockOffset ?? new(0),
            SetTextOffset = Config.SetTextOffset ?? new(0),
            ChangeSetOffset = Config.ChangeSetOffset ?? new(0),
            HidePadlock = Config.HidePadlock ?? false,
            HideSetText = Config.HideSetText ?? false,
            HideTriggerText = Config.HideTriggerText ?? false,
            HideUnassigned = Config.HideUnassigned ?? false,
            SelectColorMultiply = Config.SelectColorMultiply ?? CrossUp.Color.Preset.MultiplyNeutral,
            SelectBlend = Config.SelectDisplayType ?? 0,
            SelectStyle = Config.SelectDisplayType == 1 ? 2 : 1,
            GlowA = Config.GlowA ?? CrossUp.Color.Preset.White,
            GlowB = Config.GlowB ?? CrossUp.Color.Preset.White,
            TextColor = Config.TextColor ?? CrossUp.Color.Preset.White,
            TextGlow = Config.TextGlow ?? CrossUp.Color.Preset.TextGlow,
            BorderColor = Config.BorderColor ?? CrossUp.Color.Preset.White,
            SepExBar = Config.SepExBar ?? false,
            LRpos = Config.LRpos ?? new(-214, -88),
            RLpos = Config.RLpos ?? new(214, -88),
            OnlyOneEx = Config.OnlyOneEx ?? false,
            CombatFadeInOut = Config.CombatFadeInOut ?? false,
            TranspOutOfCombat = Config.TranspOutOfCombat ?? 100,
            TranspInCombat = Config.TranspInCombat ?? 0
        };

        for (var i = 0; i < 5; i++) Config.Profiles[i] = new(old); // copy those old settings to each HUD profile

        // remove legacy settings
        Config.Split = null;
        Config.LockCenter = null;
        Config.PadlockOffset = null;
        Config.SetTextOffset = null;
        Config.ChangeSetOffset = null;
        Config.HidePadlock = null;
        Config.HideSetText = null;
        Config.HideTriggerText = null;
        Config.HideUnassigned = null;
        Config.SelectColorMultiply = null;
        Config.SelectDisplayType = null;
        Config.GlowA = null;
        Config.GlowB = null;
        Config.TextColor = null;
        Config.TextGlow = null;
        Config.BorderColor = null;
        Config.SepExBar = null;
        Config.LRpos = null;
        Config.RLpos = null;
        Config.OnlyOneEx = null;
        Config.CombatFadeInOut = null;
        Config.TranspOutOfCombat = null;
        Config.TranspInCombat = null;

        Config.Save();
    }

}