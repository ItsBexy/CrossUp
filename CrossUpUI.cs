using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace CrossUp;
internal class CrossUpUI : IDisposable
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

    public void Dispose() {}
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
        var split = Config.Split;
        var padlockX = (int)Config.PadlockOffset.X;
        var padlockY = (int)Config.PadlockOffset.Y;
        var setTextX = (int)Config.SetTextOffset.X;
        var setTextY = (int)Config.SetTextOffset.Y;
        var changeSetX = (int)Config.ChangeSetOffset.X;
        var changeSetY = (int)Config.ChangeSetOffset.Y;


        ImGui.Spacing();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "REPOSITION BAR ELEMENTS");
        ImGui.Spacing();
        ImGui.BeginTable("Reposition", 4, ImGuiTableFlags.SizingFixedFit);


        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text("Left/Right Separation");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("##Distance", ref split))
        {
            split = Math.Max(split, 0);
            Config.Split = split;
            Config.Save();
            CrossUp.ResetHud();
            CrossUp.UpdateBarState(true);
        }
        

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text("Padlock Icon");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("X##PadX", ref padlockX))
        {
            Config.PadlockOffset = Config.PadlockOffset with { X = padlockX };
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("Y##PadY", ref padlockY))
        {
            Config.PadlockOffset = Config.PadlockOffset with { Y = padlockY };
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.TableNextColumn();
        if (ImGui.Checkbox("Hide##HidePad", ref hidePadlock))
        {
            Config.HidePadlock = hidePadlock;
            CrossUp.UpdateBarState(true, true);
            Config.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text("SET # Text");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("X##SetX", ref setTextX))
        {
            Config.SetTextOffset = Config.SetTextOffset with { X = setTextX };
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("Y##SetY", ref setTextY))
        {
            Config.SetTextOffset = Config.SetTextOffset with { Y = setTextY };
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.TableNextColumn();
        if (ImGui.Checkbox("Hide##HideSetText", ref hideSetText))
        {
            Config.HideSetText = hideSetText;
            CrossUp.UpdateBarState(true, true);
            Config.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text("CHANGE SET Display");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("X##ChangeSetX", ref changeSetX))
        {
            Config.ChangeSetOffset = Config.ChangeSetOffset with { X = changeSetX };
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(90 * scale);
        if (ImGui.InputInt("Y##ChangeSetY", ref changeSetY))
        {
            Config.ChangeSetOffset = Config.ChangeSetOffset with { Y = changeSetY };
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.EndTable();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Spacing();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "CUSTOMIZE COLORS");
        ImGui.Spacing();

        ImGui.SetNextItemWidth(200 * scale);
        if (ImGui.ColorEdit3("Bar Highlight", ref selectColor))
        {
            Config.selectColor = selectColor;
            Config.Save();
            CrossUp.SetSelectColor();
        }

        ImGui.SameLine();
        if (ImGui.Checkbox("Hide##HideHighlight", ref hideSelect))
        {
            Config.selectHide = hideSelect;
            Config.Save();
            CrossUp.SetSelectColor();
        }

        ImGui.SetNextItemWidth(200 * scale);
        if (ImGui.ColorEdit3("Button Glow", ref glowA))
        {
            Config.GlowA = glowA;
            Config.Save();
            CrossUp.SetPulseColor();
        }

        ImGui.SetNextItemWidth(200 * scale);
        if (ImGui.ColorEdit3("Button Pulse", ref glowB))
        {
            Config.GlowB = glowB;
            Config.Save();
            CrossUp.SetPulseColor();
        }

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

        if (ImGui.Checkbox("Display Expanded Hold Controls Separately", ref sepExBar))
        {
            Config.SepExBar = sepExBar;
            if (sepExBar && Config.borrowBarR > 0 && Config.borrowBarL > 0) CrossUp.EnableEx();
            else CrossUp.DisableEx();
            Config.Save();
        }
        ImGui.Spacing();

        if (!Config.SepExBar) return;
        
        ImGui.BeginTable("BarBorrowDesc", 2, ImGuiTableFlags.SizingStretchProp);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "NOTE: This feature functions by borrowing\nthe buttons from two of your standard\nmouse/kb hotbars. The hotbars you choose\nwill not be overwritten, but they will be\nunavailable while the feature is active.\n\n");

        ImGui.BeginTable("Coords", 3, ImGuiTableFlags.SizingFixedFit);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        ImGui.Indent(10);
        if (ImGui.RadioButton("Show Only One Bar", onlyOneEx))
        {
            Config.OnlyOneEx = true;
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.TableNextColumn();
        if (ImGui.RadioButton("Show Both", !onlyOneEx))
        {
            Config.OnlyOneEx = false;
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.TableNextRow();
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.Indent(-5);
        ImGui.Text(onlyOneEx?"EXHB Position":"Left EXHB Position");

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(100*scale);
        if (ImGui.InputInt("X##LX", ref lX))
        {
            Config.lX = lX;
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(100 * scale);
        if (ImGui.InputInt("Y##LY", ref lY))
        {
            Config.lY = lY;
            Config.Save();
            CrossUp.UpdateBarState(true, true);
        }

        if (!onlyOneEx)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.Text("Right EXHB Position");

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(100 * scale);
            if (ImGui.InputInt("X##RX", ref rX))
            {
                Config.rX = rX;
                Config.Save();
                CrossUp.UpdateBarState(true, true);
            }

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(100 * scale);
            if (ImGui.InputInt("Y##RY", ref rY))
            {
                Config.rY = rY;
                Config.Save();
                CrossUp.UpdateBarState(true, true);
            }
        }

        ImGui.EndTable();

        ImGui.TableNextColumn();

        ImGui.Text("SELECT 2 BARS:");
        ImGui.BeginTable("BarBorrow", 2, ImGuiTableFlags.SizingFixedFit);

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

                        CrossUp.ResetHud();
                        Config.Save();
                        CrossUp.EnableEx();
                    }
                    else
                    {
                        if (Config.borrowBarL == i) Config.borrowBarL = -1;
                        else if (Config.borrowBarR == i) Config.borrowBarR = -1;

                        CrossUp.ResetHud();
                        CrossUp.UpdateBarState();
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

        ImGui.EndTable();
        ImGui.EndTable();
    }

    private void ColumnCentredText(string text)
    {
        var colWidth = ImGui.GetColumnWidth();
        var textWidth = ImGui.CalcTextSize(text).X;
        var indentSize = (colWidth - textWidth) * 0.5f;
        ImGui.Indent(indentSize);
        ImGui.Text(text);
        ImGui.Indent(-indentSize);
    }

    private void MappingTab(bool type)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var featureOn = type ? Config.RemapEx : Config.RemapW;
        var mappings = type ? Config.MappingsEx : Config.MappingsW;
        var optionString = "";

        for (var i = 1; i <= 8; i++) optionString += $"Cross Hotbar {i} - Left\0Cross Hotbar {i} - Right\0";

        ImGui.Spacing();
        if (ImGui.Checkbox($"Use set-specific assignments for {(type ? "Expanded Hold Controls" : "WXHB")}",ref featureOn))
        {
            if (type) Config.RemapEx = featureOn;
            else      Config.RemapW  = featureOn;
        }

        if (!featureOn) return;
        
        ImGui.BeginTable((type?"EXHB":"WXHB")+" Remap",3,ImGuiTableFlags.SizingStretchProp);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(" If Using...");

        ImGui.TableNextColumn();
        ColumnCentredText($"Map {(type ? "L→R" : "L Double Tap")} to...");

        ImGui.TableNextColumn();
        ColumnCentredText($"Map {(type ? "R→L" : "R Double Tap")} to...");

        for (var i = 0; i < 8; i++)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ColumnCentredText("SET "+(i+1));

            for (var c = 0; c <= 1; c++)
            {
                ImGui.TableNextColumn();

                var indent = (ImGui.GetColumnWidth()-170f*scale) / 2;
                if (indent>0) {ImGui.Indent(indent);}
                ImGui.SetNextItemWidth(170f*scale);
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
        if (!SettingsVisible) return;

        var scale = ImGuiHelpers.GlobalScale;

        ImGui.SetNextWindowSizeConstraints(new Vector2(450 * scale, 200 * scale), new Vector2(550 * scale, 500 * scale));
        ImGui.SetNextWindowSize(Config.ConfigWindowSize, ImGuiCond.Always);
        if (ImGui.Begin("CrossUp Settings", ref settingsVisible))
        {
            if (ImGui.BeginTabBar("Nav")) {

                if (ImGui.BeginTabItem("Look & Feel"))
                {
                    StyleTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Separate EXHB"))
                {
                    SepExTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Bar Assignments"))
                {
                    if (ImGui.BeginTabBar("MapTabs"))
                    {
                        if (ImGui.BeginTabItem("WXHB"))
                        {
                            MappingTab(false);
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Expanded Hold Controls"))
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