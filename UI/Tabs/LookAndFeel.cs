using System;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace CrossUp;

internal sealed partial class CrossUpUI
{
    public class LookAndFeel
    {
        public static void DrawTab()
        {
            if (!ImGui.BeginTabItem(Strings.LookAndFeel.TabTitle)) return;

            ImGui.Spacing();

            if (ImGui.BeginTabBar("LookFeelSubTabs"))
            {
                if (ImGui.BeginTabItem(Strings.LookAndFeel.CrossHotbarLayout))
                {
                    ImGui.Spacing();
                    ImGui.Indent(10);

                    if (ImGui.BeginTable("Layout", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX))
                    {
                        ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("reset", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("hide", ImGuiTableColumnFlags.WidthFixed);

                        Rows.LRsplit();
                        Rows.Padlock();
                        Rows.SetNumText();
                        Rows.ChangeSetDisplay();
                        Rows.TriggerText();
                        Rows.UnassignedSlots();
                        Rows.CombatFade();

                        ImGui.EndTable();
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Strings.LookAndFeel.Colors))
                {
                    ImGui.Spacing();
                    ImGui.Indent(10);

                    if (ImGui.BeginTable("Colors", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX))
                    {
                        ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed);

                        Rows.BarHighlightColor();
                        Rows.ButtonColor();
                        Rows.TextColor();

                        ImGui.EndTable();
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            HudOptions.ProfileIndicator();
            ImGui.EndTabItem();
        }
        public class Rows
        {
            public static void LRsplit()
            {
                var splitOn = Profile.SplitOn;
                var splitDist = Profile.SplitDist;
                var centerPoint = Profile.CenterPoint;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.DimColor, Strings.LookAndFeel.BarSeparation.ToUpper());

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.SeparateLR);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##SplitOn", ref splitOn)) CrossUp.Commands.SplitOn(splitOn);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                if (splitOn)
                {
                    ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.SplitDistance);
                    ImGui.TableNextColumn();

                    ImGui.PushID("resetSplit");
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.SplitDist(100);
                    ImGui.PopID();

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(90 * XupGui.Scale);
                    if (ImGui.InputInt("##Distance", ref splitDist))
                        CrossUp.Commands.SplitDist(Math.Max(splitDist, -142));

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();

                    ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.CenterPoint);

                    ImGui.TableNextColumn();

                    ImGui.PushID("resetCenter");
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.Center(0);
                    ImGui.PopID();

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(90 * XupGui.Scale);
                    if (ImGui.InputInt("##CenterPoint", ref centerPoint)) CrossUp.Commands.Center(centerPoint);

                    ImGuiComponents.HelpMarker(Strings.LookAndFeel.CenterNote);
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.DimColor, Strings.LookAndFeel.BarElements.ToUpper());
            }
            public static void Padlock()
            {
                var y = (int)Profile.PadlockOffset.Y;
                var x = (int)Profile.PadlockOffset.X;
                var hide = Profile.HidePadlock;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.PadlockIcon);

                ImGui.TableNextColumn();

                ImGui.PushID("resetPadlock");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.Padlock(true, 0, 0);
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * XupGui.Scale);
                if (ImGui.InputInt("##PadX", ref x)) Apply();

                XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * XupGui.Scale);
                if (ImGui.InputInt("##PadY", ref y)) Apply();

                XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltV, true);

                ImGui.TableNextColumn();
                ImGui.PushStyleColor(ImGuiCol.Text, XupGui.HighlightColor);
                if (ImGui.Checkbox($"{Strings.LookAndFeel.Hide}##HidePad", ref hide)) Apply();
                ImGui.PopStyleColor(1);

                void Apply() => CrossUp.Commands.Padlock(!hide, x, y);
            }
            public static void SetNumText()
            {
                var x = (int)Profile.SetTextOffset.X;
                var y = (int)Profile.SetTextOffset.Y;
                var hide = Profile.HideSetText;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.SetNumText);

                ImGui.TableNextColumn();

                ImGui.PushID("resetSetText");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.SetNumText(true, 0, 0);
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * XupGui.Scale);
                if (ImGui.InputInt("##SetX", ref x)) Apply();

                XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * XupGui.Scale);
                if (ImGui.InputInt("##SetY", ref y)) Apply();

                XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltV, true);

                ImGui.TableNextColumn();
                ImGui.PushStyleColor(ImGuiCol.Text, XupGui.HighlightColor);
                if (ImGui.Checkbox($"{Strings.LookAndFeel.Hide}##HideSetText", ref hide)) Apply();
                ImGui.PopStyleColor(1);

                void Apply() => CrossUp.Commands.SetNumText(!hide, x, y);
            }
            public static void ChangeSetDisplay()
            {
                var x = (int)Profile.ChangeSetOffset.X;
                var y = (int)Profile.ChangeSetOffset.Y;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.ChangeSetDisplay);

                ImGui.TableNextColumn();
                ImGui.PushID("resetChangeSet");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.ChangeSet(0, 0);
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * XupGui.Scale);
                if (ImGui.InputInt("##ChangeSetX", ref x)) Apply();

                XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * XupGui.Scale);
                if (ImGui.InputInt("##ChangeSetY", ref y)) Apply();

                XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltV, true);

                void Apply() => CrossUp.Commands.ChangeSet(x, y);
            }
            public static void TriggerText()
            {
                var hide = Profile.HideTriggerText;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.HideTriggerText);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HideTriggerText", ref hide)) CrossUp.Commands.TriggerText(!hide);
            }
            public static void UnassignedSlots()
            {
                var hide = Profile.HideUnassigned;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.HideUnassignedSlots);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HideUnassigned", ref hide)) CrossUp.Commands.EmptySlots(!hide);
            }
            public static void CombatFade()
            {
                var fade = Profile.CombatFadeInOut;
                var tOut = Profile.TranspOutOfCombat;
                var tIn = Profile.TranspInCombat;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.FadeOutsideCombat);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##Fade", ref fade)) CrossUp.Commands.CombatFade(fade);

                if (!fade) return;

                ImGui.TableNextColumn();

                ImGui.BeginGroup();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.InCombat);
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.OutOfCombat);
                ImGui.EndGroup();

                ImGui.SameLine();

                ImGui.BeginGroup();
                ImGui.SetNextItemWidth(100 * XupGui.Scale);
                if (ImGui.SliderInt("##CombatTransparency", ref tIn, 0, 100)) CrossUp.Commands.CombatFade(fade, tIn, tOut);
                ImGui.SetNextItemWidth(100 * XupGui.Scale);
                if (ImGui.SliderInt("##NonCombatTransparency", ref tOut, 0, 100)) CrossUp.Commands.CombatFade(fade, tIn, tOut);
                ImGui.EndGroup();
            }
            public static void BarHighlightColor()
            {
                var color = Profile.SelectColorMultiply;
                var blend = Profile.SelectBlend;
                var style = Profile.SelectStyle;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.DimColor, Strings.LookAndFeel.SelectedBar.ToUpper());

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.BackdropColor);
                ImGui.TableNextColumn();

                ImGui.PushID("resetBG");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.SelectBG(0, 0, CrossUp.Color.Preset.MultiplyNeutral);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * XupGui.Scale);
                if (ImGui.ColorEdit3("##BarMultiply", ref color, XupGui.PickerFlags)) CrossUp.Commands.SelectBG(style, blend, color);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.BackdropStyle);

                ImGui.TableNextColumn();
                if (ImGui.RadioButton(Strings.LookAndFeel.StyleSolid, style == 0)) CrossUp.Commands.SelectBG(0, blend, color);
                ImGui.SameLine();
                if (ImGui.RadioButton(Strings.LookAndFeel.StyleFrame, style == 1)) CrossUp.Commands.SelectBG(1, blend, color);
                ImGui.SameLine();
                if (ImGui.RadioButton(Strings.LookAndFeel.StyleHidden, style == 2)) CrossUp.Commands.SelectBG(2, blend, color);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.ColorBlending);

                ImGui.TableNextColumn();
                if (ImGui.RadioButton(Strings.LookAndFeel.BlendNormal, blend == 0)) CrossUp.Commands.SelectBG(style, 0, color);
                ImGui.SameLine();
                if (ImGui.RadioButton(Strings.LookAndFeel.BlendDodge, blend == 2)) CrossUp.Commands.SelectBG(style, 2, color);
            }
            public static void ButtonColor()
            {
                var glow = Profile.GlowA;
                var pulse = Profile.GlowB;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.DimColor, Strings.LookAndFeel.Buttons.ToUpper());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.ButtonGlow);

                ImGui.TableNextColumn();

                ImGui.PushID("resetGlowA");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.ButtonGlow(CrossUp.Color.Preset.White);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * XupGui.Scale);
                if (ImGui.ColorEdit3("##ButtonGlow", ref glow, XupGui.PickerFlags)) CrossUp.Commands.ButtonGlow(glow);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.ButtonPulse);

                ImGui.TableNextColumn();

                ImGui.PushID("resetGlowB");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.ButtonPulse(CrossUp.Color.Preset.White);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * XupGui.Scale);
                if (ImGui.ColorEdit3("##ButtonPulse", ref pulse, XupGui.PickerFlags)) CrossUp.Commands.ButtonPulse(pulse);
            }
            public static void TextColor()
            {
                var textColor = Profile.TextColor;
                var textGlow = Profile.TextGlow;
                var border = Profile.BorderColor;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.DimColor, Strings.LookAndFeel.TextAndBorders.ToUpper());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.TextColor);

                ImGui.TableNextColumn();

                ImGui.PushID("resetTextColor");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.TextColor(CrossUp.Color.Preset.White, textGlow);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * XupGui.Scale);
                if (ImGui.ColorEdit3("##TextColor", ref textColor, XupGui.PickerFlags)) CrossUp.Commands.TextColor(textColor, textGlow);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.TextGlowColor);

                ImGui.TableNextColumn();

                ImGui.PushID("resetTextGlow");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.TextColor(textColor, CrossUp.Color.Preset.TextGlow);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * XupGui.Scale);
                if (ImGui.ColorEdit3("##TextGlow", ref textGlow, XupGui.PickerFlags)) CrossUp.Commands.TextColor(textColor, textGlow);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(XupGui.HighlightColor, Strings.LookAndFeel.BorderColor);

                ImGui.TableNextColumn();

                ImGui.PushID("resetBorder");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.BorderColor(CrossUp.Color.Preset.White);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * XupGui.Scale);
                if (ImGui.ColorEdit3("##Border", ref border, XupGui.PickerFlags)) CrossUp.Commands.BorderColor(border);
            }
        }
    }
}