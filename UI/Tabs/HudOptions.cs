using System;
using CrossUp.Features;
using CrossUp.Features.Layout;
using CrossUp.Game;
using CrossUp.Utility;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using static CrossUp.CrossUp;
using static CrossUp.Utility.Service;

namespace CrossUp.UI.Tabs;

internal class HudOptions
{
    private static int CopyFrom;
    private static int CopyTo;

    public static void DrawTab()
    {
        using var ti = ImRaii.TabItem(Strings.Hud.TabTitle);
        if (!ti) return;

        Helpers.Spacing(2);
        ImGui.Indent(10);

        var uniqueHud = Config.UniqueHud;
        var from = CopyFrom;
        var to = CopyTo;

        if (ImGui.RadioButton(Strings.Hud.HudAllSame, !uniqueHud))
        {
            Config.UniqueHud = false;

            if (!Features.Layout.SeparateEx.Ready) Features.Layout.SeparateEx.Disable();
            Layout.Update(true);
            Color.SetAll();

            Config.Save();
        }

        if (ImGui.RadioButton(Strings.Hud.HudUnique, uniqueHud))
        {
            Config.UniqueHud = true;

            if (!Features.Layout.SeparateEx.Ready) Features.Layout.SeparateEx.Disable();
            Layout.Update(true);
            Color.SetAll();
            Config.Save();
        }

        Helpers.BumpCursorY(20f * Helpers.Scale);
        ImGui.Text(Strings.Hud.CurrentHudSlot);

        ImGui.SameLine();

        for (var i = 1; i <= 4; i++)
        {
            using (var col = ImRaii.PushColor(ImGuiCol.ButtonHovered, Helpers.ColorSchemes[i, 2])
                                   .Push(ImGuiCol.ButtonActive, Helpers.ColorSchemes[i, 1]))
            {

                var current = HudData.CurrentSlot == i;

                if (current) col.Push(ImGuiCol.Button, Helpers.ColorSchemes[i, 1])
                                .Push(ImGuiCol.Text, Helpers.ColorSchemes[i, 0]);

                if (ImGui.Button(Strings.NumSymbols[i]))
                {
                    ChatHelper.SendMessage($"/hudlayout {i}");

                    if (!Features.Layout.SeparateEx.Ready) Features.Layout.SeparateEx.Disable();
                    Layout.Update(true);
                    Color.SetAll();
                }
            }

            if (i != 4) ImGui.SameLine();
        }

        Helpers.BumpCursorY(20f * Helpers.Scale);

        if (uniqueHud)
        {
            var msg = Strings.Hud.HighlightMsg.Split("|");
            ImGui.Text(msg[0]);
            ImGui.SameLine();
            Helpers.BumpCursorX(-5f * Helpers.Scale);
            ImGui.TextColored(Helpers.HighlightColor, msg[1]);
            ImGui.SameLine();
            Helpers.BumpCursorX(-6f * Helpers.Scale);
            ImGui.Text(msg[2]);
        }
        else
        {
            ImGui.Text("");
        }

        Helpers.BumpCursorY(20f * Helpers.Scale);

        HudCopyTable(from, to);

        ProfileIndicator();
    }

    private static void HudCopyTable(int from, int to)
    {
        using var table = ImRaii.Table("hudcopy", 2, ImGuiTableFlags.SizingFixedFit);
        if (!table.Success) return;

        ImGui.TableSetupColumn("fromTo", ImGuiTableColumnFlags.WidthFixed, 120F * Helpers.Scale);
        ImGui.TableSetupColumn("hudControls", ImGuiTableColumnFlags.WidthFixed);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey3);
        Helpers.ColumnCentredText(Strings.Hud.CopySettings.ToUpper());
        ImGui.PopStyleColor(1);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        Helpers.ColumnRightAlignText($"{Strings.Hud.From.ToUpper()} ");
        ImGui.TableNextColumn();

        using (var gr = ImRaii.Group())
        {
            if (gr.Success)
            {
                for (var i = 0; i <= 4; i++)
                {
                    using (var col = ImRaii.PushColor(ImGuiCol.ButtonHovered, Helpers.ColorSchemes[i, 2])
                               .Push(ImGuiCol.ButtonActive, Helpers.ColorSchemes[i, 1]))
                    {
                        if (from == i)
                            col.Push(ImGuiCol.Text, Helpers.ColorSchemes[i, 0])
                                .Push(ImGuiCol.Button, Helpers.ColorSchemes[i, 1]);

                        if (ImGui.Button($"{Strings.NumSymbols[i]}##From{i}")) CopyFrom = i;
                    }

                    if (i != 4) ImGui.SameLine();
                }
            }
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        Helpers.ColumnCentredText("");

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        Helpers.ColumnRightAlignText($"{Strings.Hud.To.ToUpper()} ");
        ImGui.TableNextColumn();

        using (var gr = ImRaii.Group())
        {
            if (gr.Success)
            {
                for (var i = 0; i <= 4; i++)
                {
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Helpers.ColorSchemes[i, 2]);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, Helpers.ColorSchemes[i, 1]);
                    if (to == i) ImGui.PushStyleColor(ImGuiCol.Button, Helpers.ColorSchemes[i, 1]);
                    if (to == i) ImGui.PushStyleColor(ImGuiCol.Text, Helpers.ColorSchemes[i, 0]);

                    if (ImGui.Button($"{Strings.NumSymbols[i]}##To{i}")) CopyTo = i;

                    ImGui.PopStyleColor(to == i ? 4 : 2);
                    if (i != 4) ImGui.SameLine();
                }
            }
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();

        ImGui.Spacing();
        var label = $"   {Strings.Hud.Copy.ToUpper()}   ";

        if (!ImGui.Button(label) || CopyTo == CopyFrom) return;

        Config.Profiles[CopyTo] = new(Config.Profiles[CopyFrom]);

        Log.Info($"Copying configs from Profile {Strings.NumSymbols[CopyFrom]} to Profile {Strings.NumSymbols[CopyTo]}");

        if (!Features.Layout.SeparateEx.Ready) Features.Layout.SeparateEx.Disable();
        Layout.Update(true);
        Color.SetAll();
    }

    public static void ProfileIndicator()
    {
        var p = Config.UniqueHud ? HudData.CurrentSlot : 0;
        var text = $"{(p == 0 ? Strings.Hud.AllHudSlots : Strings.Hud.HudSlot)} {Strings.NumSymbols[p]}";

        var textSize = ImGui.CalcTextSize(text);
        var windowSize = ImGui.GetWindowContentRegionMax();

        ImGui.SetCursorPosX(Math.Max(30f * Helpers.Scale, windowSize.X - 30f * Helpers.Scale - textSize.X));
        ImGui.SetCursorPosY(Math.Max(400 * Helpers.Scale, windowSize.Y - 30f * Helpers.Scale - textSize.Y));

        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, Helpers.ColorSchemes[p, 1])
                     .Push(ImGuiCol.ButtonActive, Helpers.ColorSchemes[p, 1])
                     .Push(ImGuiCol.Button, Helpers.ColorSchemes[p, 1])
                     .Push(ImGuiCol.Text, Helpers.ColorSchemes[p, 0]))
        {
            ImGui.Button(text);
        }
    }
}
