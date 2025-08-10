using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using static CrossUp.CrossUp;

namespace CrossUp.UI.Tabs;

internal static class SetSwitching
{
    public static void DrawTab()
    {
        using var ti = ImRaii.TabItem(Strings.SetSwitching.TabTitle);
        if (!ti)
            return;

        ImGui.Spacing();

        using var tb = ImRaii.TabBar("MapTabs");
        if (!tb.Success) return;

        SetSwitchingSubTab(false);
        SetSwitchingSubTab(true);
    }

    private static void SetSwitchingSubTab(bool type)
    {
        var title = type ? Strings.SetSwitching.ExpandedHoldControls : Strings.SetSwitching.WXHB;

        using var ti = ImRaii.TabItem(title);
        if (!ti) return;

        var featureOn = type ? Config.RemapEx : Config.RemapW;


        ImGui.Spacing();
        ImGui.Indent(10);
        if (ImGui.Checkbox(Strings.SetSwitching.AutoSwitch(title), ref featureOn))
        {
            if (type)
                Config.RemapEx = featureOn;
            else
                Config.RemapW = featureOn;
        }

        ImGui.Indent(-10);

        if (!featureOn) return;

        ImGui.Spacing();
        ImGui.Indent(10);

        SetSwitchingTable(type);
    }

    private static void SetSwitchingTable(bool type)
    {
        var optionString = "";

        for (var i = 1; i <= 8; i++)
        {
            optionString += $"{Strings.SetSwitching.MenuText(i, Strings.SetSwitching.Left)}\0{Strings.SetSwitching.MenuText(i, Strings.SetSwitching.Right)}\0";
        }

        int[,] mappings = type ? Config.MappingsEx : Config.MappingsW;
        using var table = ImRaii.Table($"{( type ? "EXHB" : "WXHB" )} Remap", 5, ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.ScrollX);
        if (!table.Success) return;

        ImGui.TableSetupColumn("sets", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize(Strings.SetSwitching.IfUsing).X + 20f * Helpers.Scale);

        ImGui.TableSetupColumn("gap1", ImGuiTableColumnFlags.WidthFixed, 10f * Helpers.Scale);
        ImGui.TableSetupColumn("l", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("gap2", ImGuiTableColumnFlags.WidthFixed, 10f * Helpers.Scale);

        ImGui.TableSetupColumn("r", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        Helpers.ColumnCentredText(Strings.SetSwitching.IfUsing);

        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        Helpers.ColumnCentredText(Strings.SetSwitching.MapTo(type ? "L→R" : Strings.SetSwitching.DoubleTap("L")));

        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        Helpers.ColumnCentredText(Strings.SetSwitching.MapTo(type ? "R→L" : Strings.SetSwitching.DoubleTap("R")));

        for (var i = 0; i < 8; i++)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            Helpers.ColumnCentredText($"{Strings.SetSwitching.Set.ToUpper()} {i + 1}");

            for (var c = 0; c <= 1; c++)
            {
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();

                var indent = (ImGui.GetColumnWidth() - 170f * Helpers.Scale) / 2;
                if (indent > 0) ImGui.Indent(indent);

                ImGui.SetNextItemWidth(170f * Helpers.Scale);
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
    }
}