using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Logging;

namespace CrossUp;
internal partial class CrossUpUI : IDisposable
{
    private readonly Configuration Config;
    private readonly CrossUp CrossUp;

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
    private void StyleTab()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var selectColor = Config.selectColor;
        var glowA = Config.GlowA;
        var glowB = Config.GlowB;
        var hideSelect = Config.selectHide;
        var hidePadlock = Config.HidePadlock;
        var hideSetText = Config.HideSetText;
        var hideUnassigned = Config.HideUnassigned;
        var hideTriggerText = Config.HideTriggerText;
        var split = Config.Split;
        var padlockX = (int)Config.PadlockOffset.X;
        var padlockY = (int)Config.PadlockOffset.Y;
        var setTextX = (int)Config.SetTextOffset.X;
        var setTextY = (int)Config.SetTextOffset.Y;
        var changeSetX = (int)Config.ChangeSetOffset.X;
        var changeSetY = (int)Config.ChangeSetOffset.Y;

        var columnSize = new[] { 140,22,215,60 };

        ImGui.Spacing();
        ImGui.Indent(10);

        ImGui.TextColored(ImGuiColors.DalamudGrey, Strings.LookAndFeel.LayoutHeader);
        ImGui.Spacing();

        ImGui.BeginTable("Reposition", 5, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed, columnSize[0] * scale);
        ImGui.TableSetupColumn("reset", ImGuiTableColumnFlags.WidthFixed, columnSize[1] * scale);
        ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed, columnSize[2] * scale);
        ImGui.TableSetupColumn("hide", ImGuiTableColumnFlags.WidthFixed, columnSize[3] * scale);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(Strings.LookAndFeel.LeftRightSplit);

        ImGui.TableNextColumn();
        if (ImGuiComponents.IconButton(2, FontAwesomeIcon.UndoAlt))
        {
            Config.Split = 0;
            Config.Save();
            CrossUp.Layout.Reset();
            CrossUp.Layout.Update(true);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("##Distance", ref split))
        {
            split = Math.Max(split, 0);
            Config.Split = split;
            Config.Save();
            CrossUp.Layout.Reset();
            CrossUp.Layout.Update(true);
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(Strings.LookAndFeel.PadlockIcon);

        ImGui.TableNextColumn();
        if (ImGuiComponents.IconButton(3, FontAwesomeIcon.UndoAlt))
        {
            Config.PadlockOffset = new() { X=0,Y=0};
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("X##PadX", ref padlockX))
        {
            Config.PadlockOffset = Config.PadlockOffset with { X = padlockX };
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("Y##PadY", ref padlockY))
        {
            Config.PadlockOffset = Config.PadlockOffset with { Y = padlockY };
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }

        ImGui.TableNextColumn();
        if (ImGui.Checkbox("Hide##HidePad", ref hidePadlock))
        {
            Config.HidePadlock = hidePadlock;
            CrossUp.Layout.Update(true, true);
            Config.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(Strings.LookAndFeel.SetNumText);

        ImGui.TableNextColumn();
        if (ImGuiComponents.IconButton(4, FontAwesomeIcon.UndoAlt))
        {
            Config.SetTextOffset = new() { X = 0, Y = 0 };
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("X##SetX", ref setTextX))
        {
            Config.SetTextOffset = Config.SetTextOffset with { X = setTextX };
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("Y##SetY", ref setTextY))
        {
            Config.SetTextOffset = Config.SetTextOffset with { Y = setTextY };
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }

        ImGui.TableNextColumn();
        if (ImGui.Checkbox("Hide##HideSetText", ref hideSetText))
        {
            Config.HideSetText = hideSetText;
            CrossUp.Layout.Update(true, true);
            Config.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(Strings.LookAndFeel.ChangeSetText);

        ImGui.TableNextColumn();
        if (ImGuiComponents.IconButton(5, FontAwesomeIcon.UndoAlt))
        {
            Config.ChangeSetOffset = new() { X = 0, Y = 0 };
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }


        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("X##ChangeSetX", ref changeSetX))
        {
            Config.ChangeSetOffset = Config.ChangeSetOffset with { X = changeSetX };
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("Y##ChangeSetY", ref changeSetY))
        {
            Config.ChangeSetOffset = Config.ChangeSetOffset with { Y = changeSetY };
            Config.Save();
            CrossUp.Layout.Update(true, true);
        }

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
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();


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
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();

        ImGui.EndTable();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Spacing();
   
        ImGui.TextColored(ImGuiColors.DalamudGrey, Strings.LookAndFeel.ColorHeader);
        ImGui.Spacing();

        ImGui.BeginTable("Colors", 4, ImGuiTableFlags.SizingFixedFit);

        ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed, columnSize[0] * scale);
        ImGui.TableSetupColumn("reset", ImGuiTableColumnFlags.WidthFixed, columnSize[1] * scale);
        ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed, columnSize[2] * scale);
        ImGui.TableSetupColumn("hide", ImGuiTableColumnFlags.WidthFixed, columnSize[3] * scale);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(Strings.LookAndFeel.ColorBarHighlight);

        ImGui.TableNextColumn();
        if (ImGuiComponents.IconButton(6, FontAwesomeIcon.UndoAlt))
        {
            Config.selectColor = new() { X = 1, Y = 1, Z = 1 };
            Config.Save();
            CrossUp.Color.SetSelectBG();
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(201 * scale);
        if (ImGui.ColorEdit3("##BarHighlight", ref selectColor))
        {
            Config.selectColor = selectColor;
            Config.Save();
            CrossUp.Color.SetSelectBG();
        }


        

        ImGui.TableNextColumn();
        if (ImGui.Checkbox("Hide##HideHighlight", ref hideSelect))
        {
            Config.selectHide = hideSelect;
            Config.Save();
            CrossUp.Color.SetSelectBG();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        ImGui.Text(Strings.LookAndFeel.ColorGlow);

        ImGui.TableNextColumn();
        if (ImGuiComponents.IconButton(7, FontAwesomeIcon.UndoAlt))
        {
            Config.GlowA = new() { X = 1, Y = 1, Z = 1 };
            Config.Save();
            CrossUp.Color.SetPulse();
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(201 * scale);
        if (ImGui.ColorEdit3("##ButtonGlow", ref glowA))
        {
            Config.GlowA = glowA;
            Config.Save();
            CrossUp.Color.SetPulse();
        }
        ImGui.TableNextColumn();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(Strings.LookAndFeel.ColorPulse);

        ImGui.TableNextColumn();
        if (ImGuiComponents.IconButton(8, FontAwesomeIcon.UndoAlt))
        {
            Config.GlowB = new() { X = 1, Y = 1, Z = 1 };
            Config.Save();
            CrossUp.Color.SetPulse();
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(201 * scale);
        if (ImGui.ColorEdit3("##ButtonPulse", ref glowB))
        {
            Config.GlowB = glowB;
            Config.Save();
            CrossUp.Color.SetPulse();
        }
        ImGui.TableNextColumn();

        ImGui.EndTable();
    }
    private void SepExTab()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var sepExBar = Config.SepExBar;
        var lX = Config.lX;
        var lY = Config.lY;
        var rX = Config.rX;
        var rY = Config.rY;
        var onlyOneEx = Config.OnlyOneEx;

        bool[] borrowBars =
        {
            false,
            Config.borrowBarL == 1 || Config.borrowBarR == 1,
            Config.borrowBarL == 2 || Config.borrowBarR == 2,
            Config.borrowBarL == 3 || Config.borrowBarR == 3,
            Config.borrowBarL == 4 || Config.borrowBarR == 4,
            Config.borrowBarL == 5 || Config.borrowBarR == 5,
            Config.borrowBarL == 6 || Config.borrowBarR == 6,
            Config.borrowBarL == 7 || Config.borrowBarR == 7,
            Config.borrowBarL == 8 || Config.borrowBarR == 8,
            Config.borrowBarL == 9 || Config.borrowBarR == 9
        };

        var borrowCount = 0;
        for (var i = 1; i < 10; i++) if (borrowBars[i]) borrowCount++;

        ImGui.Spacing();

        ImGui.Indent(10);
        if (ImGui.Checkbox(Strings.SeparateEx.ToggleText, ref sepExBar))
        {
            Config.SepExBar = sepExBar;
            if (CrossUp.SeparateEx.Ready) CrossUp.SeparateEx.Enable();
            else CrossUp.SeparateEx.Disable();
            Config.Save();
        }
        ImGui.Spacing();

        ImGui.BeginTable("BarBorrowDesc", 2);
        ImGui.TableSetupColumn("leftCol", ImGuiTableColumnFlags.WidthFixed, 300 * scale);
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
            ImGui.SetCursorPosY(128f*scale+87f);
            ImGui.Indent(10);
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

            ImGui.BeginTable("Coords", 3, ImGuiTableFlags.SizingFixedFit);

            ImGui.TableSetupColumn("coordText", ImGuiTableColumnFlags.WidthFixed, 120 * scale);
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.Indent(-5);
            ColumnCentredText((onlyOneEx ? "" : Strings.Terms.LRinput + " ") + Strings.SeparateEx.BarPosition);

            ImGui.TableNextColumn();

            if (ImGuiComponents.IconButton(0, FontAwesomeIcon.UndoAlt))
            {
                Config.lX = -214;
                Config.lY = -88;
                CrossUp.Layout.Update(true);
            }

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(100 * scale);
            if (ImGui.InputInt("X##LX", ref lX))
            {
                Config.lX = lX;
                Config.Save();
                CrossUp.Layout.Update(true, true);
            }

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(100 * scale);
            if (ImGui.InputInt("Y##LY", ref lY))
            {
                Config.lY = lY;
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
                    Config.rX = 214;
                    Config.rY = -88;
                    CrossUp.Layout.Update(true);
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100 * scale);
                if (ImGui.InputInt("X##RX", ref rX))
                {
                    Config.rX = rX;
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100 * scale);
                if (ImGui.InputInt("Y##RY", ref rY))
                {
                    Config.rY = rY;
                    Config.Save();
                    CrossUp.Layout.Update(true, true);
                }
            }

            ImGui.EndTable();

            ImGui.TableNextColumn();
            
            ImGui.Spacing();
            ColumnCentredText(Strings.SeparateEx.PickTwo);

            ImGui.BeginTable("BarBorrow", 2, ImGuiTableFlags.SizingFixedFit);

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
                            if (Config.borrowBarL <= 0) Config.borrowBarL = i;
                            else if (Config.borrowBarR <= 0) Config.borrowBarR = i;

                            CrossUp.Layout.Reset();
                            Config.Save();
                            if (CrossUp.SeparateEx.Ready) CrossUp.SeparateEx.Enable();
                        }
                        else
                        {
                            if (Config.borrowBarL == i) Config.borrowBarL = -1;
                            else if (Config.borrowBarR == i) Config.borrowBarR = -1;

                            CrossUp.Layout.Reset();
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
                ImGui.Text("Hotbar " + (i + 1));
            }
            ImGui.TableNextRow();
            ImGui.Indent(-14);

            ImGui.EndTable();
        }

        ImGui.EndTable();
    }
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
    private void MappingTab(bool type)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var featureOn = type ? Config.RemapEx : Config.RemapW;
        var mappings = type ? Config.MappingsEx : Config.MappingsW;
        var optionString = "";

        for (var i = 1; i <= 8; i++) optionString += $"{Strings.Terms.CrossHotbar} {i} - {Strings.Terms.Left}\0{Strings.Terms.CrossHotbar} {i} - {Strings.Terms.Right}\0";

        ImGui.Spacing();
        ImGui.Indent(10);
        if (ImGui.Checkbox($"{Strings.BarMapping.UseSetSpecific} {(type ? Strings.Terms.ExpandedHold : Strings.Terms.WXHB)}", ref featureOn))
        {
            if (type) Config.RemapEx = featureOn;
            else Config.RemapW = featureOn;
        }
        ImGui.Indent(-10);

        if (!featureOn) return;

        ImGui.Spacing();
        ImGui.Indent(10);

        ImGui.BeginTable((type ? "EXHB" : "WXHB") + " Remap", 3, ImGuiTableFlags.SizingStretchProp);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(" "+ Strings.BarMapping.IfUsing);

        ImGui.TableNextColumn();
        ColumnCentredText(Strings.BarMapping.MapTo(type ? Strings.Terms.LRinput : Strings.Terms.LLinput));

        ImGui.TableNextColumn();
        ColumnCentredText(Strings.BarMapping.MapTo(type ? Strings.Terms.RLinput: Strings.Terms.RRinput));

        for (var i = 0; i < 8; i++)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            IndentedText(Strings.Terms.Set+" "+(i + 1),13.5F);

            for (var c = 0; c <= 1; c++)
            {
                ImGui.TableNextColumn();

                var indent = (ImGui.GetColumnWidth() - 170f * scale) / 2;
                if (indent > 0) { ImGui.Indent(indent); }
                ImGui.SetNextItemWidth(170f * scale);
                if (ImGui.Combo($"##{(type ? c == 0 ? "LR" : "RL" : c == 0 ? "LL" : "RR")}{i + 1}", ref mappings[c, i], optionString, 16))
                {
                    if (type) Config.MappingsEx[c, i] = mappings[c, i];
                    else Config.MappingsW[c, i] = mappings[c, i];
                    Config.Save();
                }
                if (indent > 0) ImGui.Indent(-indent);
            }
        }

        ImGui.EndTable();
    }
    private void DrawSettingsWindow()
    {
        if (!SettingsVisible || !CrossUp.Initialized) return;

        var scale = ImGuiHelpers.GlobalScale;

        ImGui.SetNextWindowSizeConstraints(new Vector2(500 * scale, 400 * scale), new Vector2(600 * scale, 500 * scale));
        ImGui.SetNextWindowSize(Config.ConfigWindowSize, ImGuiCond.Always);
        if (ImGui.Begin(Strings.WindowTitle, ref settingsVisible))
        {
            if (ImGui.BeginTabBar("Nav"))
            {
                if (ImGui.BeginTabItem(Strings.LookAndFeel.TabTitle))
                {
                    StyleTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Strings.SeparateEx.TabTitle))
                {
                    SepExTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(Strings.BarMapping.TabTitle))
                {
                    if (ImGui.BeginTabBar("MapTabs"))
                    {
                        if (ImGui.BeginTabItem(Strings.Terms.WXHB))
                        {
                            MappingTab(false);
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem(Strings.Terms.ExpandedHold))
                        {
                            MappingTab(true);
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
            Config.ConfigWindowSize = ImGui.GetWindowSize();
        }
        ImGui.End();
    }
}