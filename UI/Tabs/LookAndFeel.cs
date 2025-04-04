﻿using System;
using CrossUp.Commands;
using CrossUp.Features;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using static CrossUp.CrossUp;

namespace CrossUp.UI.Tabs;

internal class LookAndFeel
{
    public static void DrawTab()
    {
        using var ti = ImRaii.TabItem(Strings.LookAndFeel.TabTitle);

        if (!ti) return;

        ImGui.Spacing();

        using (var tb = ImRaii.TabBar("LookFeelSubTabs"))
        {
            if (tb.Success)
            {
                LayoutSubTab();
                ColorSubTab();
            }
        }

        HudOptions.ProfileIndicator();
    }

    private static void ColorSubTab()
    {
        using var ti = ImRaii.TabItem(Strings.LookAndFeel.Colors);
        if (!ti) return;

        ImGui.Spacing();
        ImGui.Indent(10);

        using var table = ImRaii.Table("Colors", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX);
        if (!table.Success) return;

        ImGui.TableSetupColumn("colorLabels", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("colorControls", ImGuiTableColumnFlags.WidthFixed);

        Rows.BarHighlightColor();
        Rows.ButtonColor();
        Rows.TextColor();
    }

    private static void LayoutSubTab()
    {
        using var ti = ImRaii.TabItem(Strings.LookAndFeel.CrossHotbarLayout);
        if (!ti) return;

        ImGui.Spacing();
        ImGui.Indent(10);

        using var table = ImRaii.Table("Layout", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX);
        if (!table.Success) return;

        ImGui.TableSetupColumn("layoutLabels", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("reset", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("layoutControls", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("hide", ImGuiTableColumnFlags.WidthFixed);

        Rows.LRsplit();
        Rows.Padlock();
        Rows.SetNumText();
        Rows.ChangeSetDisplay();
        Rows.TriggerText();
        Rows.UnassignedSlots();
        Rows.CombatFade();
    }

    private static class Rows
    {
        public static void LRsplit()
        {
            var splitOn = Profile.SplitOn;
            var splitDist = Profile.SplitDist;
            var centerPoint = Profile.CenterPoint;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.DimColor, Strings.LookAndFeel.BarSeparation.ToUpper());

            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.SeparateLR);

            ImGui.TableNextColumn();
            if (ImGui.Checkbox("##SplitOn", ref splitOn)) InternalCmd.SplitOn(splitOn);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            if (splitOn)
            {
                ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.SplitDistance);
                ImGui.TableNextColumn();

                if (ImGuiComponents.IconButton("resetSplit", FontAwesomeIcon.UndoAlt)) InternalCmd.SplitDist(100);

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * Helpers.Scale);
                if (ImGui.InputInt("##Distance", ref splitDist)) InternalCmd.SplitDist(Math.Max(splitDist, -142));

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.CenterPoint);

                ImGui.TableNextColumn();

                if (ImGuiComponents.IconButton("resetCenter", FontAwesomeIcon.UndoAlt)) InternalCmd.Center(0);

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * Helpers.Scale);
                if (ImGui.InputInt("##CenterPoint", ref centerPoint)) InternalCmd.Center(centerPoint);

                ImGuiComponents.HelpMarker(Strings.LookAndFeel.CenterNote);
            }

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.DimColor, Strings.LookAndFeel.BarElements.ToUpper());
        }

        public static void Padlock()
        {
            var x = (int)Profile.PadlockOffset.X;
            var y = (int)Profile.PadlockOffset.Y;
            var hide = Profile.HidePadlock;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.PadlockIcon);

            ImGui.TableNextColumn();

            ImGui.PushID("resetPadlock");
            if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) InternalCmd.Padlock(true, 0, 0);
            ImGui.PopID();

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(90 * Helpers.Scale);
            if (ImGui.InputInt("##PadX", ref x)) Apply();

            Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(90 * Helpers.Scale);
            if (ImGui.InputInt("##PadY", ref y)) Apply();

            Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltV, true);

            ImGui.TableNextColumn();
            using (ImRaii.PushColor(ImGuiCol.Text, Helpers.HighlightColor))
            {
                if (ImGui.Checkbox($"{Strings.LookAndFeel.Hide}##HidePad", ref hide)) Apply();
            }
            return;

            void Apply() => InternalCmd.Padlock(!hide, x, y);
        }

        public static void SetNumText()
        {
            var x = (int)Profile.SetTextOffset.X;
            var y = (int)Profile.SetTextOffset.Y;
            var hide = Profile.HideSetText;
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.SetNumText);

            ImGui.TableNextColumn();

            if (ImGuiComponents.IconButton("resetSetText", FontAwesomeIcon.UndoAlt)) InternalCmd.SetNumText(true, 0, 0);

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(90 * Helpers.Scale);
            if (ImGui.InputInt("##SetX", ref x)) Apply();

            Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(90 * Helpers.Scale);
            if (ImGui.InputInt("##SetY", ref y)) Apply();

            Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltV, true);

            ImGui.TableNextColumn();

            using (ImRaii.PushColor(ImGuiCol.Text, Helpers.HighlightColor))
            {
                if (ImGui.Checkbox($"{Strings.LookAndFeel.Hide}##HideSetText", ref hide)) Apply();
            }
            return;

            void Apply() => InternalCmd.SetNumText(!hide, x, y);
        }

        public static void ChangeSetDisplay()
        {
            var x = (int)Profile.ChangeSetOffset.X;
            var y = (int)Profile.ChangeSetOffset.Y;
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.ChangeSetDisplay);

            ImGui.TableNextColumn();
            if (ImGuiComponents.IconButton("resetChangeSet", FontAwesomeIcon.UndoAlt)) InternalCmd.ChangeSet(0, 0);

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(90 * Helpers.Scale);
            if (ImGui.InputInt("##ChangeSetX", ref x)) Apply();

            Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(90 * Helpers.Scale);
            if (ImGui.InputInt("##ChangeSetY", ref y)) Apply();

            Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltV, true);
            return;

            void Apply() => InternalCmd.ChangeSet(x, y);
        }

        public static void TriggerText()
        {
            var hide = Profile.HideTriggerText;
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.HideTriggerText);

            ImGui.TableNextColumn();
            if (ImGui.Checkbox("##HideTriggerText", ref hide)) InternalCmd.TriggerText(!hide);
        }

        public static void UnassignedSlots()
        {
            var hide = Profile.HideUnassigned;
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.HideUnassignedSlots);

            ImGui.TableNextColumn();
            if (ImGui.Checkbox("##HideUnassigned", ref hide)) InternalCmd.EmptySlots(!hide);
        }

        public static void CombatFade()
        {
            var fade = Profile.CombatFadeInOut;
            var tOut = Profile.TranspOutOfCombat;
            var tIn = Profile.TranspInCombat;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.FadeOutsideCombat);

            ImGui.TableNextColumn();
            if (ImGui.Checkbox("##Fade", ref fade)) InternalCmd.CombatFade(fade);

            if (!fade) return;

            ImGui.TableNextColumn();

            using (var gr = ImRaii.Group())
            {
                if (gr.Success)
                {
                    ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.InCombat);
                    ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.OutOfCombat);
                }
            }

            ImGui.SameLine();

            using (var gr = ImRaii.Group())
            {
                if (!gr.Success) return;

                ImGui.SetNextItemWidth(100 * Helpers.Scale);
                if (ImGui.SliderInt("##CombatTransparency", ref tIn, 0, 100)) InternalCmd.CombatFade(fade, tIn, tOut);
                ImGui.SetNextItemWidth(100 * Helpers.Scale);
                if (ImGui.SliderInt("##NonCombatTransparency", ref tOut, 0, 100)) InternalCmd.CombatFade(fade, tIn, tOut);
            }
        }

        public static void BarHighlightColor()
        {
            var color = Profile.SelectColorMultiply;
            var blend = Profile.SelectBlend;
            var style = Profile.SelectStyle;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.DimColor, Strings.LookAndFeel.SelectedBar.ToUpper());

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.BackdropColor);
            ImGui.TableNextColumn();

            if (ImGuiComponents.IconButton("resetBG", FontAwesomeIcon.UndoAlt)) InternalCmd.SelectBG(0, 0, Color.Preset.MultiplyNeutral);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(100 * Helpers.Scale);
            if (ImGui.ColorEdit3("##BarMultiply", ref color, Helpers.PickerFlags)) InternalCmd.SelectBG(style, blend, color);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.BackdropStyle);

            ImGui.TableNextColumn();
            if (ImGui.RadioButton(Strings.LookAndFeel.StyleSolid, style == 0)) InternalCmd.SelectBG(0, blend, color);
            ImGui.SameLine();
            if (ImGui.RadioButton(Strings.LookAndFeel.StyleFrame, style == 1)) InternalCmd.SelectBG(1, blend, color);
            ImGui.SameLine();
            if (ImGui.RadioButton(Strings.LookAndFeel.StyleHidden, style == 2)) InternalCmd.SelectBG(2, blend, color);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.ColorBlending);

            ImGui.TableNextColumn();
            if (ImGui.RadioButton(Strings.LookAndFeel.BlendNormal, blend == 0)) InternalCmd.SelectBG(style, 0, color);
            ImGui.SameLine();
            if (ImGui.RadioButton(Strings.LookAndFeel.BlendDodge, blend == 2)) InternalCmd.SelectBG(style, 2, color);
        }

        public static void ButtonColor()
        {
            var glow = Profile.GlowA;
            var pulse = Profile.GlowB;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.DimColor, Strings.LookAndFeel.Buttons.ToUpper());
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.ButtonGlow);

            ImGui.TableNextColumn();

            if (ImGuiComponents.IconButton("resetGlowA", FontAwesomeIcon.UndoAlt)) InternalCmd.ButtonGlow(Color.Preset.White);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(100 * Helpers.Scale);
            if (ImGui.ColorEdit3("##ButtonGlow", ref glow, Helpers.PickerFlags)) InternalCmd.ButtonGlow(glow);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.ButtonPulse);

            ImGui.TableNextColumn();

            if (ImGuiComponents.IconButton("resetGlowB", FontAwesomeIcon.UndoAlt)) InternalCmd.ButtonPulse(Color.Preset.White);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(100 * Helpers.Scale);
            if (ImGui.ColorEdit3("##ButtonPulse", ref pulse, Helpers.PickerFlags)) InternalCmd.ButtonPulse(pulse);
        }

        public static void TextColor()
        {
            var textColor = Profile.TextColor;
            var textGlow = Profile.TextGlow;
            var border = Profile.BorderColor;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.DimColor, Strings.LookAndFeel.TextAndBorders.ToUpper());
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.TextColor);

            ImGui.TableNextColumn();

            if (ImGuiComponents.IconButton("resetTextColor", FontAwesomeIcon.UndoAlt)) InternalCmd.TextColor(Color.Preset.White, textGlow);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(100 * Helpers.Scale);
            if (ImGui.ColorEdit3("##TextColor", ref textColor, Helpers.PickerFlags)) InternalCmd.TextColor(textColor, textGlow);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.TextGlowColor);

            ImGui.TableNextColumn();

            if (ImGuiComponents.IconButton("resetTextGlow", FontAwesomeIcon.UndoAlt)) InternalCmd.TextColor(textColor, Color.Preset.TextGlow);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(100 * Helpers.Scale);
            if (ImGui.ColorEdit3("##TextGlow", ref textGlow, Helpers.PickerFlags)) InternalCmd.TextColor(textColor, textGlow);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(Helpers.HighlightColor, Strings.LookAndFeel.BorderColor);

            ImGui.TableNextColumn();

            if (ImGuiComponents.IconButton("resetBorder", FontAwesomeIcon.UndoAlt)) InternalCmd.BorderColor(Color.Preset.White);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(100 * Helpers.Scale);
            if (ImGui.ColorEdit3("##Border", ref border, Helpers.PickerFlags)) InternalCmd.BorderColor(border);
        }
    }

}