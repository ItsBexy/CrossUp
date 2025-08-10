using CrossUp.Commands;
using CrossUp.Features.Layout;
using CrossUp.Game;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using static CrossUp.CrossUp;

namespace CrossUp.UI.Tabs;

internal class SeparateEx
{
    public static void DrawTab()
    {
        using var ti = ImRaii.TabItem(Strings.SeparateEx.TabTitle);
        if (!ti) return;

        var sepExBar = Profile.SepExBar;
        var lrX = (int)Profile.LRpos.X;
        var lrY = (int)Profile.LRpos.Y;
        var rlX = (int)Profile.RLpos.X;
        var rlY = (int)Profile.RLpos.Y;
        var onlyOne = Profile.OnlyOneEx;

        bool[] borrowBars =
        [
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
        ];

        var borrowCount = 0;
        for (var i = 1; i < 10; i++) if (borrowBars[i]) borrowCount++;

        Helpers.Spacing(2);
        ImGui.Indent(10);

        using (var table = ImRaii.Table("BarBorrowDesc", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX))
        {
            if (table.Success)
            {
                ImGui.TableSetupColumn("leftCol", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("gap", ImGuiTableColumnFlags.WidthFixed, 20f * Helpers.Scale);
                ImGui.TableSetupColumn("rightCol", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                using (ImRaii.PushColor(ImGuiCol.Text, Helpers.HighlightColor))
                {
                    if (ImGui.Checkbox(Strings.SeparateEx.DisplayExSeparately, ref sepExBar)) InternalCmd.ExBarOn(sepExBar);
                }

                using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey2))
                {
                    ImGui.TextWrapped(Strings.SeparateEx.SepExWarning);
                }

                Helpers.BumpCursorY(20f * Helpers.Scale);

                if (sepExBar)
                {
                    using (ImRaii.PushColor(ImGuiCol.Text, Helpers.HighlightColor))
                    {
                        if (ImGui.RadioButton(Strings.SeparateEx.ShowOnlyOneBar, onlyOne))
                            InternalCmd.ExBarOnlyOne(true);
                        if (ImGui.RadioButton(Strings.SeparateEx.ShowBoth, !onlyOne)) InternalCmd.ExBarOnlyOne(false);
                    }

                    Helpers.BumpCursorY(20f * Helpers.Scale);

                    using (ImRaii.PushColor(ImGuiCol.Text, Helpers.HighlightColor))
                    {
                        ImGui.Text(onlyOne ? Strings.SeparateEx.BarPosition() : Strings.SeparateEx.BarPosition("L→R"));
                    }

                    ImGui.SameLine();
                    Helpers.BumpCursorX((onlyOne ? 35f : 5f) * Helpers.Scale);

                    if (ImGuiComponents.IconButton("resetLRpos", FontAwesomeIcon.UndoAlt)) InternalCmd.LRpos(-214, -88);

                    ImGui.SameLine();

                    using (var gr = ImRaii.Group())
                    {
                        if (gr.Success)
                        {
                            ImGui.SetNextItemWidth(100 * Helpers.Scale);
                            if (ImGui.InputInt("##LRX", ref lrX)) InternalCmd.LRpos(lrX, lrY);

                            Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);

                            ImGui.SetNextItemWidth(100 * Helpers.Scale);
                            if (ImGui.InputInt("##LRY", ref lrY)) InternalCmd.LRpos(lrX, lrY);

                            ImGui.SameLine();
                            Helpers.BumpCursorX(4f * Helpers.Scale);
                            Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltV, true);
                        }
                    }

                    if (!onlyOne)
                    {
                        using (ImRaii.PushColor(ImGuiCol.Text, Helpers.HighlightColor))
                        {
                            ImGui.Text(Strings.SeparateEx.BarPosition("R→L"));
                        }

                        ImGui.SameLine();
                        Helpers.BumpCursorX(5f * Helpers.Scale);
                        if (ImGuiComponents.IconButton("resetRLpos", FontAwesomeIcon.UndoAlt))
                            InternalCmd.RLpos(214, -88);

                        ImGui.SameLine();
                        using (var gr = ImRaii.Group())
                        {
                            if (gr.Success)
                            {
                                ImGui.SetNextItemWidth(100 * Helpers.Scale);
                                if (ImGui.InputInt("##RLX", ref rlX)) InternalCmd.RLpos(rlX, rlY);
                                Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);
                                ImGui.SetNextItemWidth(100 * Helpers.Scale);
                                if (ImGui.InputInt("##RLY", ref rlY)) InternalCmd.RLpos(rlX, rlY);

                                ImGui.SameLine();
                                Helpers.BumpCursorX(4f * Helpers.Scale);
                                Helpers.WriteIcon(FontAwesomeIcon.ArrowsAltV, true);
                            }
                        }
                    }

                    ImGui.TableNextColumn();

                    ImGui.TableNextColumn();
                    ImGui.Spacing();
                    ImGui.Text(Strings.SeparateEx.SelectTwoBars.ToUpper());
                    ImGui.Spacing();

                    for (var i = 1; i < 10; i++)
                    {
                        if (borrowBars[i] || borrowCount < 2)
                        {
                            if (ImGui.Checkbox($"##using{i + 1}", ref borrowBars[i]))
                            {
                                if (borrowBars[i])
                                {
                                    if (Config.LRborrow <= 0) Config.LRborrow = i;
                                    else if (Config.RLborrow <= 0) Config.RLborrow = i;

                                    Features.Layout.SeparateEx.Reset();
                                    Features.Layout.SeparateEx.EnableIfReady();
                                }
                                else
                                {
                                    if (Config.LRborrow == i) Config.LRborrow = -1;
                                    else if (Config.RLborrow == i) Config.RLborrow = -1;

                                    Features.Layout.SeparateEx.Reset();
                                    Layout.Update();
                                }

                                Config.Save();
                            }
                        }
                        else
                        {
                            var disabled = false;
                            ImGui.Checkbox("##disabled", ref disabled);
                        }

                        ImGui.SameLine();
                        var visible = GameConfig.Hotbar.GetVis(i);
                        var labelColor = visible ? ImGuiColors.DalamudWhite : ImGuiColors.DalamudGrey3;
                        ImGui.TextColored(labelColor, Strings.SeparateEx.HotbarN(i + 1));
                    }


                }
            }
        }

        HudOptions.ProfileIndicator();
    }
}
