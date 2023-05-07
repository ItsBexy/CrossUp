using ImGuiNET;

namespace CrossUp;

internal sealed partial class CrossUpUI
{
    public static class SetSwitching
    {
        public static void DrawTab()
        {
            if (!ImGui.BeginTabItem(Strings.SetSwitching.TabTitle)) return;

            ImGui.Spacing();
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
            var title = type ? Strings.SetSwitching.ExpandedHoldControls : Strings.SetSwitching.WXHB;
            if (!ImGui.BeginTabItem(title)) return;

            var featureOn = type ? Config.RemapEx : Config.RemapW;
            var mappings = type ? Config.MappingsEx : Config.MappingsW;
            var optionString = "";

            for (var i = 1; i <= 8; i++)
            {
                optionString += $"{Strings.SetSwitching.MenuText(i, Strings.SetSwitching.Left)}\0"
                                + $"{Strings.SetSwitching.MenuText(i, Strings.SetSwitching.Right)}\0";
            }

            ImGui.Spacing();
            ImGui.Indent(10);
            if (ImGui.Checkbox(Strings.SetSwitching.UseSetSpecific(title), ref featureOn))
            {
                if (type) Config.RemapEx = featureOn;
                else Config.RemapW = featureOn;
            }

            ImGui.Indent(-10);

            if (featureOn)
            {
                ImGui.Spacing();
                ImGui.Indent(10);

                if (ImGui.BeginTable($"{(type ? "EXHB" : "WXHB")} Remap", 5,
                        ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.ScrollX))
                {
                    ImGui.TableSetupColumn("sets", ImGuiTableColumnFlags.WidthFixed,
                        ImGui.CalcTextSize(Strings.SetSwitching.IfUsing).X + 20f * XupGui.Scale);

                    ImGui.TableSetupColumn("gap1", ImGuiTableColumnFlags.WidthFixed, 10f * XupGui.Scale);
                    ImGui.TableSetupColumn("l", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("gap2", ImGuiTableColumnFlags.WidthFixed, 10f * XupGui.Scale);

                    ImGui.TableSetupColumn("r", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    XupGui.ColumnCentredText(Strings.SetSwitching.IfUsing);

                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    XupGui.ColumnCentredText(Strings.SetSwitching.MapTo(type ? "L→R" : Strings.SetSwitching.DoubleTap("L")));

                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    XupGui.ColumnCentredText(Strings.SetSwitching.MapTo(type ? "R→L" : Strings.SetSwitching.DoubleTap("R")));

                    for (var i = 0; i < 8; i++)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();

                        XupGui.ColumnCentredText($"{Strings.SetSwitching.Set.ToUpper()} {i + 1}");

                        for (var c = 0; c <= 1; c++)
                        {
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();

                            var indent = (ImGui.GetColumnWidth() - 170f * XupGui.Scale) / 2;
                            if (indent > 0) ImGui.Indent(indent);

                            ImGui.SetNextItemWidth(170f * XupGui.Scale);
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