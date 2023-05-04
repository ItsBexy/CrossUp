using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;
using System;
using System.Numerics;
using static CrossUp.CrossUp;
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
    private static Profile Profile => Config.Profiles[Config.UniqueHud ? HudSlot : 0];

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

#if DEBUG
        Debug.DrawWindow();
#endif

    }

    private void DrawMsgWindow()
    {
        if (!ShowUpdateWarning) return;

        ImGui.SetNextWindowSize(new Vector2(320 * Scale, 240 * Scale), ImGuiCond.Always);
        if (ImGui.Begin("CrossUp Notice v0.4.2.0", ref ShowUpdateWarning, ImGuiWindowFlags.NoResize))
        {
            ImGui.Text(Strings.UpdateWarning);

            BumpCursorY(40f * Scale);
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
                SelectColorMultiply = Config.SelectColorMultiply ?? Color.Preset.MultiplyNeutral,
                SelectBlend = Config.SelectDisplayType ?? 0,
                SelectStyle = Config.SelectDisplayType == 1 ? 2 : 1,
                GlowA = Config.GlowA ?? Color.Preset.White,
                GlowB = Config.GlowB ?? Color.Preset.White,
                TextColor = Config.TextColor ?? Color.Preset.White,
                TextGlow = Config.TextGlow ?? Color.Preset.TextGlow,
                BorderColor = Config.BorderColor ?? Color.Preset.White,
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

    private static float Scale => ImGuiHelpers.GlobalScale;

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

    private static Vector4 HighlightColor => Config.UniqueHud ? ColorSchemes[HudSlot, 0] : ImGuiColors.DalamudWhite;
    private static Vector4 DimColor => Config.UniqueHud ? ColorSchemes[HudSlot, 1] : ImGuiColors.DalamudGrey3;

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
        if (!SettingsVisible || !IsSetUp) return;

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

                ImGui.EndTabBar();
            }

            ProfileIndicator();

            Config.ConfigWindowSize = ImGui.GetWindowSize();

            ImGui.End();
        }
    }

    public class LookAndFeel
    {
        const ImGuiColorEditFlags PickerFlags = ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.DisplayHex;
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
                if (ImGui.Checkbox("##SplitOn", ref splitOn)) Commands.SplitOn(splitOn);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                if (splitOn)
                {
                    ImGui.TextColored(HighlightColor, Strings.LookAndFeel.SplitDistance);
                    ImGui.TableNextColumn();

                    ImGui.PushID("resetSplit");
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.SplitDist(100);
                    ImGui.PopID();

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(90 * Scale);
                    if (ImGui.InputInt("##Distance", ref splitDist)) Commands.SplitDist(Math.Max(splitDist, -142));

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();

                    ImGui.TextColored(HighlightColor, Strings.LookAndFeel.SplitCenter);

                    ImGui.TableNextColumn();

                    ImGui.PushID("resetCenter");
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.Center(0);
                    ImGui.PopID();

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(90 * Scale);
                    if (ImGui.InputInt("##CenterPoint", ref centerPoint)) Commands.Center(centerPoint);

                    ImGuiComponents.HelpMarker(Strings.LookAndFeel.SplitNote);
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(DimColor, Strings.LookAndFeel.BarElements);

            }
            public static void Padlock()
            {
                var x = (int)Profile.PadlockOffset.X;
                var y = (int)Profile.PadlockOffset.Y;
                var hide = Profile.HidePadlock;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.PadlockIcon);

                ImGui.TableNextColumn();

                ImGui.PushID("resetPadlock");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.Padlock(true, 0, 0);
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("X##PadX", ref x)) Apply();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("Y##PadY", ref y)) Apply();

                ImGui.TableNextColumn();
                ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
                if (ImGui.Checkbox($"{Strings.LookAndFeel.Hide}##HidePad", ref hide)) Apply();
                ImGui.PopStyleColor(1);

                void Apply() => Commands.Padlock(!hide, x, y);
            }
            public static void SetNumText()
            {
                var x = (int)Profile.SetTextOffset.X;
                var y = (int)Profile.SetTextOffset.Y;
                var hide = Profile.HideSetText;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.SetNumText);

                ImGui.TableNextColumn();

                ImGui.PushID("resetSetText");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.SetNumText(true, 0, 0);
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("X##SetX", ref x)) Apply();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("Y##SetY", ref y)) Apply();

                ImGui.TableNextColumn();
                ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
                if (ImGui.Checkbox($"{Strings.LookAndFeel.Hide}##HideSetText", ref hide)) Apply();
                ImGui.PopStyleColor(1);

                void Apply() => Commands.SetNumText(!hide, x, y);
            }
            public static void ChangeSetDisplay()
            {
                var x = (int)Profile.ChangeSetOffset.X;
                var y = (int)Profile.ChangeSetOffset.Y;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ChangeSetText);

                ImGui.TableNextColumn();
                ImGui.PushID("resetChangeSet");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.ChangeSet(0, 0);
                ImGui.PopID();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("X##ChangeSetX", ref x)) Apply();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * Scale);
                if (ImGui.InputInt("Y##ChangeSetY", ref y)) Apply();

                void Apply() => Commands.ChangeSet(x, y);
            }
            public static void TriggerText()
            {
                var hide = Profile.HideTriggerText;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.LRTriggerText);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HideTriggerText", ref hide)) Commands.TriggerText(!hide);
            }
            public static void UnassignedSlots()
            {
                var hide = Profile.HideUnassigned;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.UnassignedSlots);

                ImGui.TableNextColumn();
                if (ImGui.Checkbox("##HideUnassigned", ref hide)) Commands.EmptySlots(!hide);
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
                if (ImGui.Checkbox("##Fade", ref fade)) Commands.CombatFade(fade);

                if (!fade) return;

                ImGui.TableNextColumn();

                ImGui.TextColored(HighlightColor, "Combat");
                ImGui.SameLine();

                ImGui.SetCursorPosX(258f * Scale);
                ImGui.SetNextItemWidth(140 * Scale);
                if (ImGui.SliderInt("##CombatTransparency", ref tIn, 0, 100)) Commands.CombatFade(fade, tIn, tOut);

                ImGui.TextColored(HighlightColor, "Idle");
                ImGui.SameLine();

                ImGui.SetCursorPosX(258f * Scale);
                ImGui.SetNextItemWidth(140 * Scale);
                if (ImGui.SliderInt("##NonCombatTransparency", ref tOut, 0, 100)) Commands.CombatFade(fade, tIn, tOut);
            }
            public static void BarHighlightColor()
            {
                var color = Profile.SelectColorMultiply;
                var blend = Profile.SelectBlend;
                var style = Profile.SelectStyle;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(DimColor, Strings.LookAndFeel.ColorSubheadBar);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorBarHighlight);
                ImGui.TableNextColumn();

                ImGui.PushID("resetBG");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.SelectBG(0, 0, Color.Preset.MultiplyNeutral);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * Scale);
                if (ImGui.ColorEdit3("##BarMultiply", ref color, PickerFlags)) Commands.SelectBG(style, blend, color);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorBarStyle);

                ImGui.TableNextColumn();
                if (ImGui.RadioButton(Strings.LookAndFeel.StyleSolid, style == 0)) Commands.SelectBG(0, blend, color);
                ImGui.SameLine();
                if (ImGui.RadioButton(Strings.LookAndFeel.StyleFrame, style == 1)) Commands.SelectBG(1, blend, color);
                ImGui.SameLine();
                if (ImGui.RadioButton(Strings.LookAndFeel.StyleHide, style == 2)) Commands.SelectBG(2, blend, color);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorBarBlend);

                ImGui.TableNextColumn();
                if (ImGui.RadioButton(Strings.LookAndFeel.BlendNormal, blend == 0)) Commands.SelectBG(style, 0, color);
                ImGui.SameLine();
                if (ImGui.RadioButton(Strings.LookAndFeel.BlendDodge, blend == 2)) Commands.SelectBG(style, 2, color);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Spacing();

            }
            public static void ButtonColor()
            {
                var glow = Profile.GlowA;
                var pulse = Profile.GlowB;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(DimColor, Strings.LookAndFeel.ColorSubheadButtons);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorGlow);

                ImGui.TableNextColumn();

                ImGui.PushID("resetGlowA");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.ButtonGlow(Color.Preset.White);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * Scale);
                if (ImGui.ColorEdit3("##ButtonGlow", ref glow, PickerFlags)) Commands.ButtonGlow(glow);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.ColorPulse);

                ImGui.TableNextColumn();

                ImGui.PushID("resetGlowB");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.ButtonPulse(Color.Preset.White);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * Scale);
                if (ImGui.ColorEdit3("##ButtonPulse", ref pulse, PickerFlags)) Commands.ButtonPulse(pulse);
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
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.TextColor(Color.Preset.White, textGlow);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * Scale);
                if (ImGui.ColorEdit3("##TextColor", ref textColor, PickerFlags)) Commands.TextColor(textColor, textGlow);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.TextGlow);

                ImGui.TableNextColumn();

                ImGui.PushID("resetTextGlow");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.TextColor(textColor, Color.Preset.TextGlow);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * Scale);
                if (ImGui.ColorEdit3("##TextGlow", ref textGlow, PickerFlags)) Commands.TextColor(textColor, textGlow);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(HighlightColor, Strings.LookAndFeel.BorderColor);

                ImGui.TableNextColumn();

                ImGui.PushID("resetBorder");
                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.BorderColor(Color.Preset.White);
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100 * Scale);
                if (ImGui.ColorEdit3("##Border", ref border, PickerFlags)) Commands.BorderColor(border);
            }
        }
        public static void DrawTab()
        {
            if (ImGui.BeginTabItem(Strings.LookAndFeel.TabTitle))
            {
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

                        if (ImGui.BeginTable("Colors", 2, ImGuiTableFlags.SizingFixedFit))
                        {
                            ImGui.TableSetupColumn("labels", ImGuiTableColumnFlags.WidthFixed, columnSize[0] * Scale);
                            ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed, (columnSize[1] + columnSize[2] + columnSize[3]) * Scale);

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
    }

    public class SeparateEx
    {
        public static void DrawTab()
        {
            if (ImGui.BeginTabItem(Strings.SeparateEx.TabTitle))
            {
                var sepExBar = Profile.SepExBar;
                var lrX = (int)Profile.LRpos.X;
                var lrY = (int)Profile.LRpos.Y;
                var rlX = (int)Profile.RLpos.X;
                var rlY = (int)Profile.RLpos.Y;
                var onlyOne = Profile.OnlyOneEx;

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
                if (ImGui.Checkbox(Strings.SeparateEx.ToggleText, ref sepExBar)) Commands.ExBarOn(sepExBar);

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
                        if (ImGui.RadioButton(Strings.SeparateEx.ShowOnlyOne, onlyOne)) Commands.ExBarOnlyOne(true);

                        ImGui.SameLine();
                        if (ImGui.RadioButton(Strings.SeparateEx.ShowBoth, !onlyOne)) Commands.ExBarOnlyOne(false);

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
                            ColumnCentredText((onlyOne ? "" : $"{Strings.Terms.LRinput} ") + Strings.SeparateEx.BarPosition);
                            ImGui.PopStyleColor(1);

                            ImGui.TableNextColumn();

                            ImGui.PushID("resetLRpos");
                            if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.LRpos(-214, -88);

                            ImGui.PopID();

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(100 * Scale);
                            if (ImGui.InputInt("X##LX", ref lrX)) Commands.LRpos(lrX, lrY);

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(100 * Scale);
                            if (ImGui.InputInt("Y##LY", ref lrY)) Commands.LRpos(lrX, lrY);

                            if (!onlyOne)
                            {
                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();

                                ImGui.PushStyleColor(ImGuiCol.Text, HighlightColor);
                                ColumnCentredText($"{Strings.Terms.RLinput} {Strings.SeparateEx.BarPosition}");
                                ImGui.PopStyleColor(1);

                                ImGui.TableNextColumn();

                                ImGui.PushID("resetRLpos");
                                if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) Commands.RLpos(214, -88);

                                ImGui.PopID();

                                ImGui.TableNextColumn();
                                ImGui.SetNextItemWidth(100 * Scale);
                                if (ImGui.InputInt("X##RX", ref rlX)) Commands.LRpos(rlX, rlY);

                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();
                                ImGui.TableNextColumn();
                                ImGui.TableNextColumn();
                                ImGui.SetNextItemWidth(100 * Scale);
                                if (ImGui.InputInt("Y##RY", ref rlY)) Commands.LRpos(rlX, rlY);
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

                                            Layout.ResetBars();

                                            if (Layout.SeparateEx.Ready) Layout.SeparateEx.Enable(true);
                                            Config.Save();
                                        }
                                        else
                                        {
                                            if (Config.LRborrow == i) Config.LRborrow = -1;
                                            else if (Config.RLborrow == i) Config.RLborrow = -1;

                                            Layout.ResetBars();
                                            Layout.Update();
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
                                var labelColor = CharConfig.Hotbar.Visible[i]
                                    ? ImGuiColors.DalamudWhite
                                    : ImGuiColors.DalamudGrey3;
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
    }

    public static class Mapping
    {
        public static void DrawTab()
        {
            if (ImGui.BeginTabItem(Strings.BarMapping.TabTitle))
            {
                if (ImGui.BeginTabBar("MapTabs"))
                {
                    SubTab(false);
                    SubTab(true);

                    ImGui.EndTabBar();
                }

                ImGui.EndTabItem();
            }
        }
        private static void SubTab(bool type)
        {
            var title = type ? Strings.Terms.ExpandedHold : Strings.Terms.WXHB;
            if (ImGui.BeginTabItem(title))
            {
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
                        ColumnCentredText(
                            Strings.BarMapping.MapTo(type ? Strings.Terms.LRinput : Strings.Terms.LLinput));

                        ImGui.TableNextColumn();
                        ColumnCentredText(
                            Strings.BarMapping.MapTo(type ? Strings.Terms.RLinput : Strings.Terms.RRinput));

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
    }

    private static void ProfileIndicator()
    {
        var p = Config.UniqueHud ? HudSlot : 0;
        ImGui.SetCursorPosX((p == 0 ? 351 : 385) * Scale);
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
            if (ImGui.BeginTabItem(Strings.Hud.TabTitle))
            {
                ImGui.Spacing();
                ImGui.Indent(10);

                var uniqueHud = Config.UniqueHud;
                var from = CopyFrom;
                var to = CopyTo;

                if (ImGui.RadioButton(Strings.Hud.AllSame, !uniqueHud))
                {
                    Config.UniqueHud = false;

                    if (!Profile.SepExBar) Layout.SeparateEx.Disable();
                    Layout.Update(true);
                    Color.SetAll();

                    Config.Save();
                }

                if (ImGui.RadioButton(Strings.Hud.Unique, uniqueHud))
                {
                    Config.UniqueHud = true;

                    if (!Profile.SepExBar) Layout.SeparateEx.Disable();
                    Layout.Update(true);
                    Color.SetAll();
                    Config.Save();
                }

                BumpCursorY(20f * Scale);
                ImGui.Text(Strings.Hud.Current);

                ImGui.SameLine();
                BumpCursorX(74f * Scale);
                for (var i = 1; i <= 4; i++)
                {
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorSchemes[i, 2]);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorSchemes[i, 1]);

                    var current = HudSlot == i;
                    if (current) ImGui.PushStyleColor(ImGuiCol.Button, ColorSchemes[i, 1]);
                    if (current) ImGui.PushStyleColor(ImGuiCol.Text, ColorSchemes[i, 0]);


                    if (ImGui.Button(Strings.NumSymbols[i]))
                    {
                        ChatHelper.SendMessage($"/hudlayout {i}");

                        if (!Profile.SepExBar) Layout.SeparateEx.Disable();
                        Layout.Update(true);
                        Color.SetAll();
                    }

                    ImGui.PopStyleColor(current ? 4 : 2);

                    if (i != 4) ImGui.SameLine();
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
                else
                {
                    ImGui.Text("");
                }

                BumpCursorY(30f * Scale);

                ImGui.SetCursorPosX(195 * Scale);
                ImGui.TextColored(ImGuiColors.DalamudGrey3, Strings.Hud.CopyProfile);

                BumpCursorY(6f * Scale);

                ImGui.SetCursorPosX(120 * Scale);
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

                BumpCursorY(10f * Scale);
                ImGui.SetCursorPosX(210 * Scale);

                if (ImGui.Button($"   {Strings.Hud.Copy}   "))
                {
                    if (CopyTo != CopyFrom)
                    {
                        Config.Profiles[CopyTo] = new(Config.Profiles[CopyFrom]);

                        PluginLog.Log($"Copying configs from Profile {Strings.NumSymbols[CopyFrom]} to Profile {Strings.NumSymbols[CopyTo]}");

                        if (!Profile.SepExBar) Layout.SeparateEx.Disable();
                        Layout.Update(true);
                        Color.SetAll();
                    }
                }
            }
        }
    }
}