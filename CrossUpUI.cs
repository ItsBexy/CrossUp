using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;
using System;
using System.Numerics;
using ImGui = ImGuiNET.ImGui;

// ReSharper disable NotAccessedField.Local
// ReSharper disable InvertIf

#pragma warning disable CS8618

namespace CrossUp;

internal sealed partial class CrossUpUI : IDisposable
{
    private static Configuration Config;
    private static CrossUp CrossUp;

    private bool settingsVisible;
    public bool SettingsVisible
    {
        get => settingsVisible;
        set => settingsVisible = value;
    }

    public CrossUpUI(Configuration config, CrossUp crossup)
    {
        Config = config;
        CrossUp = crossup;
    }

    public void Dispose() { }

    public void Draw() => DrawSettingsWindow();
    private static void ColumnCentredText(string text)
    {
        var colWidth = ImGui.GetColumnWidth();
        var textWidth = ImGui.CalcTextSize(text).X;
        var indentSize = (colWidth - textWidth) * 0.5f;

        ImGui.Indent(indentSize);
        ImGui.Text(text);
        ImGui.Indent(-indentSize);
    }
    private static void IndentedText(string text, float indent)
    {
        ImGui.Indent(indent);
        ImGui.Text(text);
        ImGui.Indent(-indent);
    }

    private void DrawSettingsWindow()
    {
        if (!SettingsVisible || !CrossUp.IsSetUp) return;

        var scale = ImGuiHelpers.GlobalScale;

        ImGui.SetNextWindowSizeConstraints(new Vector2(500 * scale, 380 * scale), new Vector2(600 * scale, 480 * scale));
        ImGui.SetNextWindowSize(Config.ConfigWindowSize, ImGuiCond.Always);
        if (ImGui.Begin(Strings.WindowTitle, ref settingsVisible))
        {
            if (ImGui.BeginTabBar("Nav"))
            {
                LookAndFeel.DrawTab();
                SeparateEx.DrawTab();
                Mapping.DrawTab();

                ImGui.EndTabBar();
            }
            Config.ConfigWindowSize = ImGui.GetWindowSize();
        }
        ImGui.End();
    }

    public class LookAndFeel
    {
        public class Rows
        {
            public static void LRsplit()
            {
                var split = Config.Split;
                var lockCenter = Config.LockCenter;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.LeftRightSplit);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(2, FontAwesomeIcon.UndoAlt))
                {
                    Config.Split = 0;
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("##Distance", ref split))
                {
                    split = Math.Max(split, 0);
                    Config.Split = split;
                    Config.Save();
                    CrossUp.Layout.Update(true);
                    CrossUp.Layout.Cross.StoreXPos();
                }

                ImGui.SameLine();
                ImGui.Indent(111);

                if (ImGui.Checkbox(Strings.LookAndFeel.LockCenter+"##Center", ref lockCenter))
                {
                    Config.LockCenter = lockCenter;
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }
                ImGui.Indent(-111);
                ImGui.TableNextColumn();

                ImGuiComponents.HelpMarker(Strings.LookAndFeel.HelpText);
           
            }
            public static void Padlock()
            {
                var padlockX = (int)Config.PadlockOffset.X;
                var padlockY = (int)Config.PadlockOffset.Y;
                var hidePadlock = Config.HidePadlock;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.PadlockIcon);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(3, FontAwesomeIcon.UndoAlt))
                {
                    Config.PadlockOffset = new(0);
                    Config.HidePadlock = false;
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("X##PadX", ref padlockX))
                {
                    Config.PadlockOffset = Config.PadlockOffset with { X = padlockX };
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("Y##PadY", ref padlockY))
                {
                    Config.PadlockOffset = Config.PadlockOffset with { Y = padlockY };
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }

                ImGui.TableNextColumn();
                if (ImGui.Checkbox(Strings.LookAndFeel.Hide+"##HidePad", ref hidePadlock))
                {
                    Config.HidePadlock = hidePadlock;
                    CrossUp.Layout.Update(true, true);
                    Config.Save();
                }
            }
            public static void SetNumText()
            {
                var setTextX = (int)Config.SetTextOffset.X;
                var setTextY = (int)Config.SetTextOffset.Y;
                var hideSetText = Config.HideSetText;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.SetNumText);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(4, FontAwesomeIcon.UndoAlt))
                {
                    Config.SetTextOffset = new(0);
                    Config.HideSetText = false;
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("X##SetX", ref setTextX))
                {
                    Config.SetTextOffset = Config.SetTextOffset with { X = setTextX };
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("Y##SetY", ref setTextY))
                {
                    Config.SetTextOffset = Config.SetTextOffset with { Y = setTextY };
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }

                ImGui.TableNextColumn();
                if (ImGui.Checkbox(Strings.LookAndFeel.Hide + "##HideSetText", ref hideSetText))
                {
                    Config.HideSetText = hideSetText;
                    CrossUp.Layout.Update(true, true);
                    Config.Save();
                }
            }
            public static void ChangeSetDisplay()
            {
                var changeSetX = (int)Config.ChangeSetOffset.X;
                var changeSetY = (int)Config.ChangeSetOffset.Y;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.ChangeSetText);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(5, FontAwesomeIcon.UndoAlt))
                {
                    Config.ChangeSetOffset = new(0);
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }


                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("X##ChangeSetX", ref changeSetX))
                {
                    Config.ChangeSetOffset = Config.ChangeSetOffset with { X = changeSetX };
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("Y##ChangeSetY", ref changeSetY))
                {
                    Config.ChangeSetOffset = Config.ChangeSetOffset with { Y = changeSetY };
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }
            }
            public static void TriggerText()
            {
                var hideTriggerText = Config.HideTriggerText;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.Text(Strings.LookAndFeel.LRTriggerText);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HideTriggerText", ref hideTriggerText))
                {
                    Config.HideTriggerText = hideTriggerText;
                    CrossUp.Layout.Update(true, true);
                    Config.Save();
                }
            }
            public static void UnassignedSlots()
            {
                var hideUnassigned = Config.HideUnassigned;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.UnassignedSlots);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HideUnassigned", ref hideUnassigned))
                {
                    Config.HideUnassigned = hideUnassigned;
                    CrossUp.Layout.Update(true, true);
                    Config.Save();
                }
            }
            public static void CombatFade()
            {
                var fade = Config.CombatFadeInOut;
                var tOut = Config.TranspOutOfCombat;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Fade Outside Combat");

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##Fade", ref fade))
                {
                    Config.CombatFadeInOut = fade;
                    if (!fade) CharConfig.Transparency.Standard.Set(0);
                    else CrossUp.OnConditionChange();
                    Config.Save();
                }

                if (!fade) return;

                ImGui.TableNextColumn();
                
                ImGui.SetNextItemWidth(200*ImGuiHelpers.GlobalScale);
                if (ImGui.SliderInt("##NonCombatTransparency", ref tOut, 0, 100))
                {
                    if (tOut > 100) tOut = 100;
                    if (tOut < 0) tOut = 0;
                    Config.TranspOutOfCombat = tOut;
                    Config.Save();
                    CrossUp.OnConditionChange();
                }

            }
            public static void BarHighlightColor()
            {
                var multiply = Config.SelectColorMultiply;
                var displayType = Config.SelectDisplayType;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(ImGuiColors.DalamudGrey3, Strings.LookAndFeel.ColorSubheadBar);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.ColorBarHighlight);
                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(9, FontAwesomeIcon.UndoAlt))
                {
                    Config.SelectColorMultiply = CrossUp.Color.Preset.MultiplyNeutral;
                    Config.SelectDisplayType = 0;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * ImGuiHelpers.GlobalScale);
                if (ImGui.ColorEdit3("##BarMultiply", ref multiply))
                {
                    Config.SelectColorMultiply = multiply;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.ColorBarBlend);

                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                if (ImGui.RadioButton(Strings.LookAndFeel.BlendNormal,displayType == 0))
                {
                    Config.SelectDisplayType = 0;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }

                ImGui.SameLine();

                if (ImGui.RadioButton(Strings.LookAndFeel.BlendDodge, displayType == 2))
                {
                    Config.SelectDisplayType = 2;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }
                ImGui.SameLine();

                if (ImGui.RadioButton(Strings.LookAndFeel.BlendHide, displayType == 1))
                {
                    Config.SelectDisplayType = 1;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Spacing();


            }
            public static void ButtonColor()
            {
                var glowA = Config.GlowA;
                var glowB = Config.GlowB;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(ImGuiColors.DalamudGrey3, Strings.LookAndFeel.ColorSubheadButtons);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.Text(Strings.LookAndFeel.ColorGlow);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(7, FontAwesomeIcon.UndoAlt))
                {
                    Config.GlowA = CrossUp.Color.Preset.White;
                    Config.Save();
                    CrossUp.Color.SetPulse();
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * ImGuiHelpers.GlobalScale);
                if (ImGui.ColorEdit3("##ButtonGlow", ref glowA))
                {
                    Config.GlowA = glowA;
                    Config.Save();
                    CrossUp.Color.SetPulse();
                }
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.ColorPulse);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(8, FontAwesomeIcon.UndoAlt))
                {
                    Config.GlowB = CrossUp.Color.Preset.White;
                    Config.Save();
                    CrossUp.Color.SetPulse();
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * ImGuiHelpers.GlobalScale);
                if (ImGui.ColorEdit3("##ButtonPulse", ref glowB))
                {
                    Config.GlowB = glowB;
                    Config.Save();
                    CrossUp.Color.SetPulse();
                }
            }
            public static void TextColor()
            {
                var textColor = Config.TextColor;
                var textGlow = Config.TextGlow;
                var border = Config.BorderColor;


                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(ImGuiColors.DalamudGrey3, Strings.LookAndFeel.ColorSubheadTextBorders);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.Text(Strings.LookAndFeel.TextColor);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(10, FontAwesomeIcon.UndoAlt))
                {
                    Config.TextColor = CrossUp.Color.Preset.White;
                    Config.Save();
                    CrossUp.Color.SetText();
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * ImGuiHelpers.GlobalScale);
                if (ImGui.ColorEdit3("##TextColor", ref textColor))
                {
                    Config.TextColor = textColor;
                    Config.Save();
                    CrossUp.Color.SetText();
                }
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.TextGlow);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(11, FontAwesomeIcon.UndoAlt))
                {
                    Config.TextGlow = CrossUp.Color.Preset.TextGlow;
                    Config.Save();
                    CrossUp.Color.SetText();
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * ImGuiHelpers.GlobalScale);
                if (ImGui.ColorEdit3("##TextGlow", ref textGlow))
                {
                    Config.TextGlow = textGlow;
                    Config.Save();
                    CrossUp.Color.SetText();
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(Strings.LookAndFeel.BorderColor);

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(12, FontAwesomeIcon.UndoAlt))
                {
                    Config.BorderColor = CrossUp.Color.Preset.White;
                    Config.Save();
                    CrossUp.Color.SetText();
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * ImGuiHelpers.GlobalScale);
                if (ImGui.ColorEdit3("##Border", ref border))
                {
                    Config.BorderColor = border;
                    Config.Save();
                    CrossUp.Color.SetText();
                }
            }
        }

        public static void DrawTab()
        {
            if (!ImGui.BeginTabItem(Strings.LookAndFeel.TabTitle)) return;
            var columnSize = new[] { 140, 22, 215, 60 };

            if (ImGui.BeginTabBar("LookFeelSubTabs"))
            {
                if (ImGui.BeginTabItem(Strings.LookAndFeel.LayoutHeader))
                {
                    ImGui.Spacing();
                    ImGui.Indent(10);

                    if (ImGui.BeginTable("Reposition", 5, ImGuiTableFlags.SizingFixedFit))
                    {
                        ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed, columnSize[0] * ImGuiHelpers.GlobalScale);
                        ImGui.TableSetupColumn("reset", ImGuiTableColumnFlags.WidthFixed, columnSize[1] * ImGuiHelpers.GlobalScale);
                        ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed, columnSize[2] * ImGuiHelpers.GlobalScale);
                        ImGui.TableSetupColumn("hide", ImGuiTableColumnFlags.WidthFixed, columnSize[3] * ImGuiHelpers.GlobalScale);

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

                if (ImGui.BeginTabItem(Strings.LookAndFeel.ColorHeader))
                {
                    ImGui.Spacing();
                    ImGui.Indent(10);

                    if (ImGui.BeginTable("Colors", 3, ImGuiTableFlags.SizingFixedFit))
                    {
                        ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed, columnSize[0] * ImGuiHelpers.GlobalScale);
                        ImGui.TableSetupColumn("reset", ImGuiTableColumnFlags.WidthFixed, columnSize[1] * ImGuiHelpers.GlobalScale);
                        ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed, (columnSize[2] + columnSize[3]) * ImGuiHelpers.GlobalScale);

                        Rows.BarHighlightColor();
                        Rows.ButtonColor();
                        Rows.TextColor();

                        ImGui.EndTable();
                    }
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }


           

            ImGui.EndTabItem();
        }
    }

    public class SeparateEx
    {
        public static void DrawTab()
        {
            if (!ImGui.BeginTabItem(Strings.SeparateEx.TabTitle)) return;
            var scale = ImGuiHelpers.GlobalScale;
            var sepExBar = Config.SepExBar;
            var lrX = (int)Config.LRpos.X;
            var lrY = (int)Config.LRpos.Y;
            var rlX = (int)Config.RLpos.X;
            var rlY = (int)Config.RLpos.Y;
            var onlyOneEx = Config.OnlyOneEx;

            bool[] borrowBars =
            {
                false,
                Config.LRborrow == 1 || Config.RLborrow == 1,
                Config.LRborrow == 2 || Config.RLborrow == 2,
                Config.LRborrow == 3 || Config.RLborrow == 3,
                Config.LRborrow == 4 || Config.RLborrow == 4,
                Config.LRborrow == 5 || Config.RLborrow == 5,
                Config.LRborrow == 6 || Config.RLborrow == 6,
                Config.LRborrow == 7 || Config.RLborrow == 7,
                Config.LRborrow == 8 || Config.RLborrow == 8,
                Config.LRborrow == 9 || Config.RLborrow == 9
            };

            var borrowCount = 0;
            for (var i = 1; i < 10; i++) if (borrowBars[i]) borrowCount++;

            ImGui.Spacing();

            ImGui.Indent(10);
            if (ImGui.Checkbox(Strings.SeparateEx.ToggleText, ref sepExBar))
            {
                Config.SepExBar = sepExBar;
                if (CrossUp.Layout.SeparateEx.Ready) CrossUp.Layout.SeparateEx.Enable();
                else CrossUp.Layout.SeparateEx.Disable();
                Config.Save();
            }
            ImGui.Spacing();

            if (ImGui.BeginTable("BarBorrowDesc", 2))
            {
                ImGui.TableSetupColumn("leftCol", ImGuiTableColumnFlags.WidthFixed, 330 * scale);
                ImGui.TableSetupColumn("rightCol", ImGuiTableColumnFlags.WidthFixed, 115 * scale);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey2);
                ImGui.PushTextWrapPos();
                ImGui.TextWrapped(Strings.SeparateEx.Warning);
                ImGui.PopTextWrapPos();
                ImGui.PopStyleColor();

                if (Config.SepExBar)
                {
                    ImGui.SetCursorPosY(128f * scale + 87f);

                    ImGui.Indent(20);
                    if (ImGui.RadioButton(Strings.SeparateEx.ShowOnlyOne, onlyOneEx))
                    {
                        Config.OnlyOneEx = true;
                        Config.Save();
                        CrossUp.Layout.Update(true, true);
                    }

                    ImGui.SameLine();
                    if (ImGui.RadioButton(Strings.SeparateEx.ShowBoth, !onlyOneEx))
                    {
                        Config.OnlyOneEx = false;
                        Config.Save();
                        CrossUp.Layout.Update(true, true);
                    }
                    ImGui.Indent(-10);

                    ImGui.Spacing();

                    if (ImGui.BeginTable("Coords", 3, ImGuiTableFlags.SizingFixedFit))
                    {
                        ImGui.TableSetupColumn("coordText", ImGuiTableColumnFlags.WidthFixed, 120 * scale);
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        ImGui.Indent(-5);
                        ColumnCentredText((onlyOneEx ? "" : Strings.Terms.LRinput + " ") + Strings.SeparateEx.BarPosition);

                        ImGui.TableNextColumn();

                        if (ImGuiComponents.IconButton(0, FontAwesomeIcon.UndoAlt))
                        {
                            Config.LRpos = new(-214,-88);
                            CrossUp.Layout.Update(true);
                        }

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(100 * scale);
                        if (ImGui.InputInt("X##LX", ref lrX))
                        {
                            Config.LRpos = Config.LRpos with { X = lrX };
                            Config.Save();
                            CrossUp.Layout.Update(true, true);
                        }

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(100 * scale);
                        if (ImGui.InputInt("Y##LY", ref lrY))
                        {
                            Config.LRpos = Config.LRpos with { Y = lrY };
                            Config.Save();
                            CrossUp.Layout.Update(true, true);
                        }

                        if (!onlyOneEx)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();

                            ColumnCentredText(Strings.Terms.RLinput + " " + Strings.SeparateEx.BarPosition);
                            ImGui.TableNextColumn();
                            if (ImGuiComponents.IconButton(1, FontAwesomeIcon.UndoAlt))
                            {
                                Config.RLpos = new(214,-88);
                                CrossUp.Layout.Update(true);
                            }

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(100 * scale);
                            if (ImGui.InputInt("X##RX", ref rlX))
                            {
                                Config.RLpos = Config.RLpos with { X = rlX };
                                Config.Save();
                                CrossUp.Layout.Update(true, true);
                            }

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(100 * scale);
                            if (ImGui.InputInt("Y##RY", ref rlY))
                            {
                                Config.RLpos = Config.RLpos with { Y = rlY };
                                Config.Save();
                                CrossUp.Layout.Update(true, true);
                            }
                        }

                        ImGui.EndTable();
                    }


                    ImGui.TableNextColumn();
                    ImGui.Spacing();
                    ColumnCentredText(Strings.SeparateEx.PickTwo);

                    if (ImGui.BeginTable("BarBorrow", 2, ImGuiTableFlags.SizingFixedFit))
                    {
                        ImGui.Indent(14);
                        for (var i = 1; i < 10; i++)
                        {
                            ImGui.TableNextColumn();

                            if (borrowBars[i] || borrowCount < 2)
                            {
                                if (ImGui.Checkbox("##using" + (i + 1), ref borrowBars[i]))
                                {
                                    if (borrowBars[i])
                                    {
                                        if (Config.LRborrow <= 0) Config.LRborrow = i;
                                        else if (Config.RLborrow <= 0) Config.RLborrow = i;

                                        CrossUp.Layout.ResetBars();
                                        Config.Save();
                                        if (CrossUp.Layout.SeparateEx.Ready) CrossUp.Layout.SeparateEx.Enable();
                                    }
                                    else
                                    {
                                        if (Config.LRborrow == i) Config.LRborrow = -1;
                                        else if (Config.RLborrow == i) Config.RLborrow = -1;

                                        CrossUp.Layout.ResetBars();
                                        CrossUp.Layout.Update();
                                        Config.Save();
                                    }
                                }
                            }
                            else
                            {
                                var disabled = false;
                                ImGui.Checkbox("##disabled", ref disabled);
                            }

                            ImGui.TableNextColumn();
                            ImGui.Text(Strings.Terms.Hotbar+" " + (i + 1));
                        }
                        ImGui.TableNextRow();
                        ImGui.Indent(-14);

                        ImGui.EndTable();
                    }
                }

                ImGui.EndTable();
            }
            ImGui.EndTabItem();
        }
    }

    public static class Mapping
    {
        public static void DrawTab()
        {
            if (!ImGui.BeginTabItem(Strings.BarMapping.TabTitle)) return;

            if (ImGui.BeginTabBar("MapTabs"))
            {
                SubTab(false);
                SubTab(true);

                ImGui.EndTabBar();
            }
            ImGui.EndTabItem();
        }
        private static void SubTab(bool type)
        {
            var title = type ? Strings.Terms.ExpandedHold : Strings.Terms.WXHB;
            if (!ImGui.BeginTabItem(title)) return;

            var scale = ImGuiHelpers.GlobalScale;
            var featureOn = type ? Config.RemapEx : Config.RemapW;
            var mappings = type ? Config.MappingsEx : Config.MappingsW;
            var optionString = "";

            for (var i = 1; i <= 8; i++) optionString += $"{Strings.Terms.CrossHotbar} {i} - {Strings.Terms.Left}\0{Strings.Terms.CrossHotbar} {i} - {Strings.Terms.Right}\0";

            ImGui.Spacing();
            ImGui.Indent(10);
            if (ImGui.Checkbox($"{Strings.BarMapping.UseSetSpecific} {title}", ref featureOn))
            {
                if (type) Config.RemapEx = featureOn;
                else Config.RemapW = featureOn;
            }
            ImGui.Indent(-10);

            if (featureOn)
            {
                ImGui.Spacing();
                ImGui.Indent(10);

                if (ImGui.BeginTable((type ? "EXHB" : "WXHB") + " Remap", 3, ImGuiTableFlags.SizingStretchProp))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(" " + Strings.BarMapping.IfUsing);

                    ImGui.TableNextColumn();
                    ColumnCentredText(Strings.BarMapping.MapTo(type ? Strings.Terms.LRinput : Strings.Terms.LLinput));

                    ImGui.TableNextColumn();
                    ColumnCentredText(Strings.BarMapping.MapTo(type ? Strings.Terms.RLinput : Strings.Terms.RRinput));

                    for (var i = 0; i < 8; i++)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        IndentedText(Strings.Terms.Set + " " + (i + 1), 13.5F);

                        for (var c = 0; c <= 1; c++)
                        {
                            ImGui.TableNextColumn();

                            var indent = (ImGui.GetColumnWidth() - 170f * scale) / 2;
                            if (indent > 0) ImGui.Indent(indent);

                            ImGui.SetNextItemWidth(170f * scale);
                            if (ImGui.Combo($"##{(type ? c == 0 ? "LR" : "RL" : c == 0 ? "LL" : "RR")}{i + 1}",
                                    ref mappings[c, i], optionString, 16))
                            {
                                if (type)
                                    Config.MappingsEx[c, i] = mappings[c, i];
                                else
                                    Config.MappingsW[c, i] = mappings[c, i];
                                Config.Save();
                            }

                            if (indent > 0) ImGui.Indent(-indent);
                        }
                    }
                    ImGui.EndTable();
                }
            }
            ImGui.EndTabItem();
        }
    }
}