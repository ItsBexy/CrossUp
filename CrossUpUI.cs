﻿using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;
using System;
using System.Numerics;
using ImGui = ImGuiNET.ImGui;
using PluginLog = Dalamud.Logging.PluginLog;

// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable InvertIf
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

#pragma warning disable CS8618

namespace CrossUp;

internal sealed partial class CrossUpUI : IDisposable
{
    private static Configuration Config;
    private static CrossUp CrossUp;
    private static Profile Profile => Config.Profiles[Config.UniqueHud ? CrossUp.HudSlot : 0];

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

    private bool OldConfigsChecked;
    private bool ShowUpdateWarning;
    public void Draw()
    {
        if (!OldConfigsChecked) { PortOldConfigs(); }
        DrawSettingsWindow();
        DrawMsgWindow();
    }

    private void DrawMsgWindow()
    {
        if (!ShowUpdateWarning) return;
        
        ImGui.SetNextWindowSize(new Vector2(320 * Scale, 240 * Scale), ImGuiCond.Always);
        if (ImGui.Begin("CrossUp Notice v0.4.2.0",ref ShowUpdateWarning,ImGuiWindowFlags.NoResize))
        {
            ImGui.Text(Strings.UpdateWarning);

            BumpCursorY(40f*Scale);
            ImGui.Text("Open CrossUp Config: ");
            ImGui.SameLine();
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
            {
                SettingsVisible = true;
                ShowUpdateWarning = false;
            }
            ImGui.End();
        }
    }

    private void PortOldConfigs()
    {
        OldConfigsChecked = true;
        if (Config.Split.HasValue)
        {
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


    private static readonly Vector4[,] ColorSchemes = {
        {
            new(1f,1f,1f,1f),
            new(0.6f,0.6f,0.6f,1f),
            new(0.6f,0.6f,0.6f,0.9f)
        },
        {
            new(0.996f,0.639f,0.620f,1f),
            new(0.522f,0.004f,0.165f,1f),
            new(0.522f,0.004f,0.165f,0.9f)
        },
        {
            new(0.835f,0.737f,0.345f,1f),
            new(0.471f,0.141f,0.004f,1f),
            new(0.471f,0.141f,0.004f,0.9f)
        },
        {
            new(0.216f,0.831f,0.929f,1f),
            new(0f,0.267f,0.502f,1f),
            new(0f,0.267f,0.502f,0.9f)
        },
        {
            new(0.984f,0.6f,1f,1f),
            new(0.357f,0.1495f,0.549f,1f),
            new(0.357f,0.1490f,0.549f,0.9f)
        }
    };

    private static float Scale => ImGuiHelpers.GlobalScale;

    private static Vector4 HighlightColor => Config.UniqueHud ? ColorSchemes[CrossUp.HudSlot, 0] : ImGuiColors.DalamudWhite;
    private static Vector4 DimColor => Config.UniqueHud ? ColorSchemes[CrossUp.HudSlot, 1] : ImGuiColors.DalamudGrey3;

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

    private static void BumpCursorX(float val) => ImGui.SetCursorPosX(ImGui.GetCursorPosX() + val);
    private static void BumpCursorY(float val) => ImGui.SetCursorPosY(ImGui.GetCursorPosY() + val);
    private static void BumpCursor(float x, float y)
    {
        BumpCursorX(x);
        BumpCursorY(y);
    }

    

    private void DrawSettingsWindow()
    {
        if (!SettingsVisible || !CrossUp.IsSetUp) return;

        ImGui.SetNextWindowSizeConstraints(new Vector2(500 * Scale, 450 * Scale), new Vector2(550 * Scale, 500 * Scale));
        ImGui.SetNextWindowSize(Config.ConfigWindowSize, ImGuiCond.Always);
        if (ImGui.Begin(Strings.WindowTitle, ref settingsVisible))
        {
            if (ImGui.BeginTabBar("Nav"))
            {
                LookAndFeel.DrawTab();
                SeparateEx.DrawTab();
                Mapping.DrawTab();
                HudOptions.DrawTab();

#if DEBUG
                ConfigDebug.DrawTab();
#endif

                ImGui.EndTabBar();
            }

            ProfileIndicator();

            Config.ConfigWindowSize = ImGui.GetWindowSize();

            ImGui.End();
        }
    }

    public class LookAndFeel
    {
        public class Rows
        {
            public static void LRsplit()
            {
                var splitOn = Profile.SplitOn;
                var splitDist = Profile.SplitDist;
                var centerPoint = Profile.CenterPoint;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(DimColor, Strings.LookAndFeel.Split);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.SplitOn);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##SplitOn", ref splitOn))
                {
                    Profile.SplitOn = splitOn;
                    CrossUp.Layout.Update(true);
                    Config.Save();
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                if (splitOn)
                {
                    ImGui.TextColored(HighlightColor, Strings.LookAndFeel.SplitDistance);
                    ImGui.TableNextColumn();

                    ImGui.PushID("resetSplit");
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                    {
                        Profile.SplitDist = 100;
                        Config.Save();
                        CrossUp.Layout.Update(true);
                    }
                    ImGui.PopID();

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(90 * Scale);
                    if (ImGui.InputInt("##Distance", ref splitDist))
                    {
                        splitDist = Math.Max(splitDist, -142);
                        Profile.SplitDist = splitDist;
                        Config.Save();
                        CrossUp.Layout.Update(true);
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    

                    ImGui.TextColored(HighlightColor, Strings.LookAndFeel.SplitCenter);

                    ImGui.TableNextColumn();

                    ImGui.PushID("resetCenter");
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                    {
                        Profile.CenterPoint = 0;
                        Config.Save();
                        CrossUp.Layout.Update(true);
                    }
                    ImGui.PopID();

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(90 * Scale);
                    if (ImGui.InputInt("##CenterPoint", ref centerPoint))
                    {
                        Profile.CenterPoint = centerPoint;
                        Config.Save();
                        CrossUp.Layout.Update(true);
                    }
                    
                    ImGuiComponents.HelpMarker(Strings.LookAndFeel.SplitNote);
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(DimColor, Strings.LookAndFeel.BarElements);

            }
            public static void Padlock()
            {
                var padlockX = (int)Profile.PadlockOffset.X;
                var padlockY = (int)Profile.PadlockOffset.Y;
                var hidePadlock = Profile.HidePadlock;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.PadlockIcon);

                ImGui.TableNextColumn();

                ImGui.PushID("resetPadlock");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.PadlockOffset = new(0);
                    Profile.HidePadlock = false;
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("X##PadX", ref padlockX))
                {
                    Profile.PadlockOffset = Profile.PadlockOffset with { X = padlockX };
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("Y##PadY", ref padlockY))
                {
                    Profile.PadlockOffset = Profile.PadlockOffset with { Y = padlockY };
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }

                ImGui.TableNextColumn();
                ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
                if (ImGui.Checkbox($"{Strings.LookAndFeel.Hide}##HidePad", ref hidePadlock))
                {
                    Profile.HidePadlock = hidePadlock;
                    CrossUp.Layout.Update(true);
                    Config.Save();
                }
                ImGui.PopStyleColor(1);
            }
            public static void SetNumText()
            {
                var setTextX = (int)Profile.SetTextOffset.X;
                var setTextY = (int)Profile.SetTextOffset.Y;
                var hideSetText = Profile.HideSetText;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.SetNumText);

                ImGui.TableNextColumn();

                ImGui.PushID("resetSetText");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.SetTextOffset = new(0);
                    Profile.HideSetText = false;
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("X##SetX", ref setTextX))
                {
                    Profile.SetTextOffset = Profile.SetTextOffset with { X = setTextX };
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("Y##SetY", ref setTextY))
                {
                    Profile.SetTextOffset = Profile.SetTextOffset with { Y = setTextY };
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }

                ImGui.TableNextColumn();
                ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
                if (ImGui.Checkbox($"{Strings.LookAndFeel.Hide}##HideSetText", ref hideSetText))
                {
                    Profile.HideSetText = hideSetText;
                    CrossUp.Layout.Update(true);
                    Config.Save();
                }
                ImGui.PopStyleColor(1);
            }
            public static void ChangeSetDisplay()
            {
                var changeSetX = (int)Profile.ChangeSetOffset.X;
                var changeSetY = (int)Profile.ChangeSetOffset.Y;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ChangeSetText);

                ImGui.TableNextColumn();
                ImGui.PushID("resetChangeSet");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.ChangeSetOffset = new(0);
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }
                ImGui.PopID();


                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("X##ChangeSetX", ref changeSetX))
                {
                    Profile.ChangeSetOffset = Profile.ChangeSetOffset with { X = changeSetX };
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("Y##ChangeSetY", ref changeSetY))
                {
                    Profile.ChangeSetOffset = Profile.ChangeSetOffset with { Y = changeSetY };
                    Config.Save();
                    CrossUp.Layout.Update(true);
                }
            }
            public static void TriggerText()
            {
                var hideTriggerText = Profile.HideTriggerText;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.LRTriggerText);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HideTriggerText", ref hideTriggerText))
                {
                    Profile.HideTriggerText = hideTriggerText;
                    CrossUp.Layout.Update(true);
                    Config.Save();
                }
            }
            public static void UnassignedSlots()
            {
                var hideUnassigned = Profile.HideUnassigned;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.UnassignedSlots);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HideUnassigned", ref hideUnassigned))
                {
                    Profile.HideUnassigned = hideUnassigned;
                    CrossUp.Layout.Update(true);
                    Config.Save();
                }
            }
            public static void CombatFade()
            {
                var fade = Profile.CombatFadeInOut;
                var tOut = Profile.TranspOutOfCombat;
                var tIn = Profile.TranspInCombat;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.FadeOutsideCombat);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##Fade", ref fade))
                {
                    Profile.CombatFadeInOut = fade;
                    if (!fade) CharConfig.Transparency.Standard.Set(0);
                    else CrossUp.OnConditionChange();
                    Config.Save();
                }

                if (!fade) return;

                ImGui.TableNextColumn();

                ImGui.TextColored(HighlightColor,"Combat");
                ImGui.SameLine();

                ImGui.SetCursorPosX(258f * Scale);
                ImGui.SetNextItemWidth(140 * Scale);
                if (ImGui.SliderInt("##CombatTransparency", ref tIn, 0, 100))
                {
                    if (tIn > 100) tIn = 100;
                    if (tIn < 0) tIn = 0;
                    Profile.TranspInCombat = tIn;
                    Config.Save();
                    CrossUp.OnConditionChange();
                }

                ImGui.TextColored(HighlightColor, "Idle");
                ImGui.SameLine();


                ImGui.SetCursorPosX(258f * Scale);
                ImGui.SetNextItemWidth(140 * Scale);
                if (ImGui.SliderInt("##NonCombatTransparency", ref tOut, 0, 100))
                {
                    if (tOut > 100) tOut = 100;
                    if (tOut < 0) tOut = 0;
                    Profile.TranspOutOfCombat = tOut;
                    Config.Save();
                    CrossUp.OnConditionChange();
                }
            }
            public static void BarHighlightColor()
            {
                var multiply = Profile.SelectColorMultiply;
                var displayType = Profile.SelectBlend;
                var tex = Profile.SelectStyle;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(DimColor, Strings.LookAndFeel.ColorSubheadBar);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorBarHighlight);
                ImGui.TableNextColumn();

                ImGui.PushID("resetBG");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.SelectColorMultiply = CrossUp.Color.Preset.MultiplyNeutral;
                    Profile.SelectBlend = 0;
                    Profile.SelectStyle = 0;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * Scale);
                if (ImGui.ColorEdit3("##BarMultiply", ref multiply))
                {
                    Profile.SelectColorMultiply = multiply;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }



                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorBarStyle);

                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                if (ImGui.RadioButton(Strings.LookAndFeel.StyleSolid, tex == 0))
                {
                    Profile.SelectStyle = 0;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }

                ImGui.SameLine();

                if (ImGui.RadioButton(Strings.LookAndFeel.StyleFrame, tex == 1))
                {
                    Profile.SelectStyle = 1;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }

                ImGui.SameLine();

                if (ImGui.RadioButton(Strings.LookAndFeel.StyleHide, tex == 2))
                {
                    Profile.SelectStyle = 2;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }


                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorBarBlend);

                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                if (ImGui.RadioButton(Strings.LookAndFeel.BlendNormal, displayType == 0))
                {
                    Profile.SelectBlend = 0;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }

                ImGui.SameLine();

                if (ImGui.RadioButton(Strings.LookAndFeel.BlendDodge, displayType == 2))
                {
                    Profile.SelectBlend = 2;
                    Config.Save();
                    CrossUp.Color.SetSelectBG();
                }






                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Spacing();




            }
            public static void ButtonColor()
            {
                var glowA = Profile.GlowA;
                var glowB = Profile.GlowB;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(DimColor, Strings.LookAndFeel.ColorSubheadButtons);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorGlow);

                ImGui.TableNextColumn();


                ImGui.PushID("resetGlowA");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.GlowA = CrossUp.Color.Preset.White;
                    Config.Save();
                    CrossUp.Color.SetPulse();
                }
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * Scale);
                if (ImGui.ColorEdit3("##ButtonGlow", ref glowA))
                {
                    Profile.GlowA = glowA;
                    Config.Save();
                    CrossUp.Color.SetPulse();
                }
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorPulse);

                ImGui.TableNextColumn();

                ImGui.PushID("resetGlowB");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.GlowB = CrossUp.Color.Preset.White;
                    Config.Save();
                    CrossUp.Color.SetPulse();
                }
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * Scale);
                if (ImGui.ColorEdit3("##ButtonPulse", ref glowB))
                {
                    Profile.GlowB = glowB;
                    Config.Save();
                    CrossUp.Color.SetPulse();
                }
            }
            public static void TextColor()
            {
                var textColor = Profile.TextColor;
                var textGlow = Profile.TextGlow;
                var border = Profile.BorderColor;


                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(DimColor, Strings.LookAndFeel.ColorSubheadTextBorders);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.TextColor);

                ImGui.TableNextColumn();

                ImGui.PushID("resetTextColor");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.TextColor = CrossUp.Color.Preset.White;
                    Config.Save();
                    CrossUp.Color.SetText();
                }
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * Scale);
                if (ImGui.ColorEdit3("##TextColor", ref textColor))
                {
                    Profile.TextColor = textColor;
                    Config.Save();
                    CrossUp.Color.SetText();
                }
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.TextGlow);

                ImGui.TableNextColumn();

                ImGui.PushID("resetTextGlow");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.TextGlow = CrossUp.Color.Preset.TextGlow;
                    Config.Save();
                    CrossUp.Color.SetText();
                }
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * Scale);
                if (ImGui.ColorEdit3("##TextGlow", ref textGlow))
                {
                    Profile.TextGlow = textGlow;
                    Config.Save();
                    CrossUp.Color.SetText();
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.BorderColor);

                ImGui.TableNextColumn();

                ImGui.PushID("resetBorder");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                {
                    Profile.BorderColor = CrossUp.Color.Preset.White;
                    Config.Save();
                    CrossUp.Color.SetText();
                }
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(240 * Scale);
                if (ImGui.ColorEdit3("##Border", ref border))
                {
                    Profile.BorderColor = border;
                    Config.Save();
                    CrossUp.Color.SetText();
                }
            }
        }
        public static void DrawTab()
        {
            if (!ImGui.BeginTabItem(Strings.LookAndFeel.TabTitle)) return;
            var columnSize = new[] { 140, 22, 215, 60 };

            ImGui.Spacing();

            if (ImGui.BeginTabBar("LookFeelSubTabs"))
            {

                if (ImGui.BeginTabItem(Strings.LookAndFeel.LayoutHeader))
                {
                    ImGui.Spacing();
                    ImGui.Indent(10);

                    if (ImGui.BeginTable("Reposition", 5, ImGuiTableFlags.SizingFixedFit))
                    {
                        ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed, columnSize[0] * Scale);
                        ImGui.TableSetupColumn("reset", ImGuiTableColumnFlags.WidthFixed, columnSize[1] * Scale);
                        ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed, columnSize[2] * Scale);
                        ImGui.TableSetupColumn("hide", ImGuiTableColumnFlags.WidthFixed, columnSize[3] * Scale);

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
                        ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed, columnSize[0] * Scale);
                        ImGui.TableSetupColumn("reset", ImGuiTableColumnFlags.WidthFixed, columnSize[1] * Scale);
                        ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed, (columnSize[2] + columnSize[3]) * Scale);

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
            var sepExBar = Profile.SepExBar;
            var lrX = (int)Profile.LRpos.X;
            var lrY = (int)Profile.LRpos.Y;
            var rlX = (int)Profile.RLpos.X;
            var rlY = (int)Profile.RLpos.Y;
            var onlyOneEx = Profile.OnlyOneEx;

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


            ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
            if (ImGui.Checkbox(Strings.SeparateEx.ToggleText, ref sepExBar))
            {
                Profile.SepExBar = sepExBar;
                if (CrossUp.Layout.SeparateEx.Ready) CrossUp.Layout.SeparateEx.Enable();
                else CrossUp.Layout.SeparateEx.Disable();
                Config.Save();
            }
            ImGui.PopStyleColor(1);
            ImGui.Spacing();

            if (ImGui.BeginTable("BarBorrowDesc", 2))
            {
                ImGui.TableSetupColumn("leftCol", ImGuiTableColumnFlags.WidthFixed, 330 * Scale);
                ImGui.TableSetupColumn("rightCol", ImGuiTableColumnFlags.WidthFixed, 115 * Scale);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey2);
                ImGui.PushTextWrapPos();
                ImGui.TextWrapped(Strings.SeparateEx.Warning);
                ImGui.PopTextWrapPos();
                ImGui.PopStyleColor();

                if (Profile.SepExBar)
                {
                    ImGui.SetCursorPosY(128f * Scale + 87f);

                    ImGui.Indent(20);
                    ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
                    if (ImGui.RadioButton(Strings.SeparateEx.ShowOnlyOne, onlyOneEx))
                    {
                        Profile.OnlyOneEx = true;
                        Config.Save();
                        CrossUp.Layout.Update(true);
                    }

                    ImGui.SameLine();
                    if (ImGui.RadioButton(Strings.SeparateEx.ShowBoth, !onlyOneEx))
                    {
                        Profile.OnlyOneEx = false;
                        Config.Save();
                        CrossUp.Layout.Update(true);
                    }
                    ImGui.PopStyleColor(1);
                    ImGui.Indent(-10);

                    ImGui.Spacing();

                    if (ImGui.BeginTable("Coords", 3, ImGuiTableFlags.SizingFixedFit))
                    {
                        ImGui.TableSetupColumn("coordText", ImGuiTableColumnFlags.WidthFixed, 120 * Scale);
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        ImGui.Indent(-5);

                        ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
                        ColumnCentredText((onlyOneEx ? "" : $"{Strings.Terms.LRinput} ") + Strings.SeparateEx.BarPosition);
                        ImGui.PopStyleColor(1);

                        ImGui.TableNextColumn();

                        ImGui.PushID("resetLRpos");
                        if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                        {
                            Profile.LRpos = new(-214, -88);
                            CrossUp.Layout.Update(true);
                        }

                        ImGui.PopID();

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(100 * Scale);
                        if (ImGui.InputInt("X##LX", ref lrX))
                        {
                            Profile.LRpos = Profile.LRpos with { X = lrX };
                            Config.Save();
                            CrossUp.Layout.Update(true);
                        }

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(100 * Scale);
                        if (ImGui.InputInt("Y##LY", ref lrY))
                        {
                            Profile.LRpos = Profile.LRpos with { Y = lrY };
                            Config.Save();
                            CrossUp.Layout.Update(true);
                        }

                        if (!onlyOneEx)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();

                            ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
                            ColumnCentredText($"{Strings.Terms.RLinput} {Strings.SeparateEx.BarPosition}");
                            ImGui.PopStyleColor(1);

                            ImGui.TableNextColumn();


                            ImGui.PushID("resetRLpos");
                            if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt))
                            {
                                Profile.RLpos = new(214, -88);
                                CrossUp.Layout.Update(true);
                            }
                            ImGui.PopID();

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(100 * Scale);
                            if (ImGui.InputInt("X##RX", ref rlX))
                            {
                                Profile.RLpos = Profile.RLpos with { X = rlX };
                                Config.Save();
                                CrossUp.Layout.Update(true);
                            }

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(100 * Scale);
                            if (ImGui.InputInt("Y##RY", ref rlY))
                            {
                                Profile.RLpos = Profile.RLpos with { Y = rlY };
                                Config.Save();
                                CrossUp.Layout.Update(true);
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

                                    if (ImGui.Checkbox($"##using{i + 1}", ref borrowBars[i]))
                                    {
                                        if (borrowBars[i])
                                        {
                                            if (Config.LRborrow <= 0) Config.LRborrow = i;
                                            else if (Config.RLborrow <= 0) Config.RLborrow = i;


                                            CrossUp.Layout.ResetBars();

                                        if (CrossUp.Layout.SeparateEx.Ready) CrossUp.Layout.SeparateEx.Enable();
                                            Config.Save();
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
                            var labelColor = CharConfig.Hotbar.Visible[i] ? ImGuiColors.DalamudWhite : ImGuiColors.DalamudGrey3;
                            ImGui.TextColored(labelColor, $"{Strings.Terms.Hotbar} {i + 1}");
                            
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

                if (ImGui.BeginTable($"{(type ? "EXHB" : "WXHB")} Remap", 3, ImGuiTableFlags.SizingStretchProp))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text($" {Strings.BarMapping.IfUsing}");

                    ImGui.TableNextColumn();
                    ColumnCentredText(Strings.BarMapping.MapTo(type ? Strings.Terms.LRinput : Strings.Terms.LLinput));

                    ImGui.TableNextColumn();
                    ColumnCentredText(Strings.BarMapping.MapTo(type ? Strings.Terms.RLinput : Strings.Terms.RRinput));

                    for (var i = 0; i < 8; i++)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();

                        IndentedText($"{Strings.Terms.Set} {i + 1}", 13.5F);

                        for (var c = 0; c <= 1; c++)
                        {
                            ImGui.TableNextColumn();

                            var indent = (ImGui.GetColumnWidth() - 170f * Scale) / 2;
                            if (indent > 0) ImGui.Indent(indent);

                            ImGui.SetNextItemWidth(170f * Scale);
                            if (ImGui.Combo($"##{(type ? c == 0 ? "LR" : "RL" : c == 0 ? "LL" : "RR")}{i + 1}",
                                    ref mappings[c, i], optionString, 16))
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
            }
            ImGui.EndTabItem();
        }
    }

    private static void ProfileIndicator()
    {
        var p = Config.UniqueHud ? CrossUp.HudSlot : 0;
        var symbol = Strings.NumSymbols[p];
        ImGui.SetCursorPosX((p==0?351:385) * Scale);
        ImGui.SetCursorPosY(400 * Scale);

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorSchemes[p, 1]);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorSchemes[p, 1]);
        ImGui.PushStyleColor(ImGuiCol.Button, ColorSchemes[p, 1]);
        ImGui.PushStyleColor(ImGuiCol.Text, ColorSchemes[p, 0]);

        ImGui.Button(p == 0 ? Strings.Hud.AllSlots : $"{Strings.Terms.HudSlot} {Strings.NumSymbols[p]}");
        ImGui.PopStyleColor(4);
    }

    public class HudOptions
    {
        private static int CopyFrom;
        private static int CopyTo;
        public static void DrawTab()
        {
            if (!ImGui.BeginTabItem(Strings.Hud.TabTitle)) return;

            ImGui.Spacing();
            ImGui.Indent(10);

            var uniqueHud = Config.UniqueHud;
            var from = CopyFrom;
            var to = CopyTo;

            if (ImGui.RadioButton(Strings.Hud.AllSame, !uniqueHud))
            {
                Config.UniqueHud = false;
                if (!Profile.SepExBar) { CrossUp.Layout.SeparateEx.Disable(); }
                CrossUp.Layout.Update(true);
                Config.Save();
            }

            if (ImGui.RadioButton(Strings.Hud.Unique, uniqueHud))
            {
                Config.UniqueHud = true;
                if (!Profile.SepExBar) { CrossUp.Layout.SeparateEx.Disable(); }
                CrossUp.Layout.Update(true);
                Config.Save();
            }

            BumpCursorY(20f*Scale);
            ImGui.Text(Strings.Hud.Current);

            ImGui.SameLine();
            BumpCursorX(74f * Scale);
            for (var i = 1; i <= 4; i++)
            {
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorSchemes[i, 2]);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorSchemes[i, 1]);

                var current = CrossUp.HudSlot == i;
                if (current) ImGui.PushStyleColor(ImGuiCol.Button, ColorSchemes[i, 1]);
                if (current) ImGui.PushStyleColor(ImGuiCol.Text, ColorSchemes[i, 0]);


                if (ImGui.Button(Strings.NumSymbols[i]))
                {
                    ChatHelper.SendMessage($"/hudlayout {i}");

                    if (!Profile.SepExBar) { CrossUp.Layout.SeparateEx.Disable(); }
                    CrossUp.Layout.Update(true);
                    CrossUp.Color.SetAll();
                }

                ImGui.PopStyleColor(current ? 4 : 2);

                if (i!=4) ImGui.SameLine();
            }

            BumpCursorY(20f * Scale);

            if (uniqueHud)
            {
                ImGui.Text(Strings.Hud.HighlightMsg1);
                ImGui.SameLine();
                BumpCursorX(-5f * Scale);
                ImGui.TextColored(HighlightColor, Strings.Hud.HighlightMsg2);
                ImGui.SameLine();
                BumpCursorX(-6f * Scale);
                ImGui.Text(Strings.Hud.HighlightMsg3);
            }
            else { ImGui.Text(""); }
            
            BumpCursorY(30f*Scale);

            ImGui.SetCursorPosX(195 * Scale);
            ImGui.TextColored(ImGuiColors.DalamudGrey3,Strings.Hud.CopyProfile);

            BumpCursorY(6f * Scale);
            
            ImGui.SetCursorPosX(120* Scale);
            ImGui.TextColored(ImGuiColors.DalamudGrey3, Strings.Hud.From);
            ImGui.SameLine();
            ImGui.SetCursorPosX(170 * Scale);

            for (var i = 0; i <= 4; i++)
            {

                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorSchemes[i, 2]);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorSchemes[i, 1]);
                if (from == i) ImGui.PushStyleColor(ImGuiCol.Text, ColorSchemes[i, 0]);
                if (from == i) ImGui.PushStyleColor(ImGuiCol.Button, ColorSchemes[i, 1]);

                if (ImGui.Button($"{Strings.NumSymbols[i]}##From{i}")) CopyFrom = i;

                ImGui.PopStyleColor(from == i ? 4 : 2);
                if (i != 4) ImGui.SameLine();
            }
            
            IndentedText("", 215 * Scale);

            ImGui.SetCursorPosX(138 * Scale);
            ImGui.TextColored(ImGuiColors.DalamudGrey3, Strings.Hud.To);
            ImGui.SameLine();
            ImGui.SetCursorPosX(170 * Scale);

            for (var i = 0; i <= 4; i++)
            {

                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorSchemes[i, 2]);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorSchemes[i, 1]);
                if (to == i) ImGui.PushStyleColor(ImGuiCol.Button, ColorSchemes[i, 1]);
                if (to == i) ImGui.PushStyleColor(ImGuiCol.Text, ColorSchemes[i, 0]);

                if (ImGui.Button($"{Strings.NumSymbols[i]}##To{i}")) CopyTo = i;

                ImGui.PopStyleColor(to == i ? 4 : 2);
                if (i != 4) ImGui.SameLine();
            }

            BumpCursorY(10f*Scale);
            ImGui.SetCursorPosX(210*Scale);

            if (ImGui.Button($"   {Strings.Hud.Copy}   "))
            {
                if (CopyTo != CopyFrom)
                {
                    Config.Profiles[CopyTo] = new(Config.Profiles[CopyFrom]);

                    PluginLog.Log($"Copying configs from Profile {Strings.NumSymbols[CopyFrom]} to Profile {Strings.NumSymbols[CopyTo]}");

                    if (!Profile.SepExBar) { CrossUp.Layout.SeparateEx.Disable(); }
                    CrossUp.Layout.Update(true);
                    CrossUp.Color.SetAll();

                }
            }



        }
    }

    public class ConfigDebug
    {
        private static int StartIndex;
        private static int EndIndex = 702;

        public static void DrawTab()
        {
            var startIndex = StartIndex;
            var endIndex = EndIndex;

            if (!ImGui.BeginTabItem("Debug: Config")) return;

            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("##startIndex", ref startIndex)) StartIndex = Math.Min(701, Math.Max(0, startIndex));

            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("##endIndex", ref endIndex)) EndIndex = Math.Max(0, Math.Min(701, endIndex));

            ImGui.SameLine();
            if (ImGui.Button("Log Config Indexes", new Vector2(150, 20)))
            {
                PluginLog.Log("Index\tID\tName\tValue");
                for (var i = (uint)startIndex; i <= endIndex; i++)
                {
                    if (i is < 0 or > 700) break;
                    PluginLog.Log(new CharConfig.Config(i).ToString());
                }
            }


            ImGui.BeginTable("configTable", 5, ImGuiTableFlags.Borders, new Vector2(620 * Scale, 1));

            ImGui.TableSetupColumn("ind", ImGuiTableColumnFlags.WidthFixed, 50 * Scale);
            ImGui.TableSetupColumn("id", ImGuiTableColumnFlags.WidthFixed, 50 * Scale);
            ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthFixed, 300 * Scale);
            ImGui.TableSetupColumn("int", ImGuiTableColumnFlags.WidthFixed, 100 * Scale);
            ImGui.TableSetupColumn("uint", ImGuiTableColumnFlags.WidthFixed, 100 * Scale);
            ImGui.TableSetupColumn("hex", ImGuiTableColumnFlags.WidthFixed, 100 * Scale);

            ImGui.TableNextColumn();
            ImGui.TableHeader("Index");
            ImGui.TableNextColumn();
            ImGui.TableHeader("ID");
            ImGui.TableNextColumn();
            ImGui.TableHeader("Name");
            ImGui.TableNextColumn();
            ImGui.TableHeader("uint");
            ImGui.TableNextColumn();
            ImGui.TableHeader("hex");


            for (var i = (uint)startIndex; i <= endIndex; i++)
            {
                if (i is < 0 or > 700) break;

                var conf = new CharConfig.Config(i);

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{i}");
                ImGui.TableNextColumn();

                ImGui.Text($"{conf.ID}");
                ImGui.TableNextColumn();
                ImGui.Text($"{conf.Name}");
                ImGui.SetNextItemWidth(100);
                ImGui.TableNextColumn();
                ImGui.Text($"{(uint)conf.Get()}");
                ImGui.TableNextColumn();
                ImGui.Text($"{CharConfig.UintToHex((uint)conf.Get())}");
            }

            ImGui.EndTable();

            ImGui.EndTabItem();
        }
    }

}