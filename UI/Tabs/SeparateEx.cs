using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace CrossUp;

internal sealed partial class CrossUpUI
{
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
            for (var i = 1; i < 10; i++)
                if (borrowBars[i])
                    borrowCount++;

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Indent(10);

            if (ImGui.BeginTable("BarBorrowDesc", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX))
            {
                ImGui.TableSetupColumn("leftCol", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("gap", ImGuiTableColumnFlags.WidthFixed, 20f * XupGui.Scale);
                ImGui.TableSetupColumn("rightCol", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();


                ImGui.PushStyleColor(ImGuiCol.Text, XupGui.HighlightColor);
                if (ImGui.Checkbox(Strings.SeparateEx.DisplayExSeparately, ref sepExBar))
                    CrossUp.Commands.ExBarOn(sepExBar);
                ImGui.PopStyleColor(1);

                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey2);
                ImGui.PushTextWrapPos();
                ImGui.TextWrapped(Strings.SeparateEx.SepExWarning);
                ImGui.PopTextWrapPos();
                ImGui.PopStyleColor();

                XupGui.BumpCursorY(20f * XupGui.Scale);


                if (sepExBar)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, XupGui.HighlightColor);
                    if (ImGui.RadioButton(Strings.SeparateEx.ShowOnlyOneBar, onlyOne))
                        CrossUp.Commands.ExBarOnlyOne(true);
                    if (ImGui.RadioButton(Strings.SeparateEx.ShowBoth, !onlyOne))
                        CrossUp.Commands.ExBarOnlyOne(false);
                    ImGui.PopStyleColor(1);


                    XupGui.BumpCursorY(20f * XupGui.Scale);
                    ImGui.PushStyleColor(ImGuiCol.Text, XupGui.HighlightColor);
                    ImGui.Text(onlyOne
                        ? Strings.SeparateEx.BarPosition
                        : Strings.SeparateEx.BarPositionSpecific("L→R"));
                    ImGui.PopStyleColor(1);

                    ImGui.SameLine();
                    XupGui.BumpCursorX((onlyOne ? 35f : 5f) * XupGui.Scale);
                    ImGui.PushID("resetLRpos");
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.LRpos(-214, -88);
                    ImGui.PopID();

                    ImGui.SameLine();
                    ImGui.BeginGroup();
                    {
                        ImGui.SetNextItemWidth(100 * XupGui.Scale);
                        if (ImGui.InputInt("##LX", ref lrX)) CrossUp.Commands.LRpos(lrX, lrY);

                        XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);

                        ImGui.SetNextItemWidth(100 * XupGui.Scale);
                        if (ImGui.InputInt("##LY", ref lrY)) CrossUp.Commands.LRpos(lrX, lrY);

                        ImGui.SameLine();
                        XupGui.BumpCursorX(4f * XupGui.Scale);
                        XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltV);
                    }
                    ImGui.EndGroup();

                    if (!onlyOne)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, XupGui.HighlightColor);
                        ImGui.Text(Strings.SeparateEx.BarPositionSpecific("R→L"));
                        ImGui.PopStyleColor(1);

                        ImGui.SameLine();
                        XupGui.BumpCursorX(5f * XupGui.Scale);
                        ImGui.PushID("resetRLpos");
                        if (ImGuiComponents.IconButton(FontAwesomeIcon.UndoAlt)) CrossUp.Commands.RLpos(214, -88);
                        ImGui.PopID();

                        ImGui.SameLine();
                        ImGui.BeginGroup();
                        {
                            ImGui.SetNextItemWidth(100 * XupGui.Scale);
                            if (ImGui.InputInt("##RX", ref rlX)) CrossUp.Commands.LRpos(rlX, rlY);
                            XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltH, true);
                            ImGui.SetNextItemWidth(100 * XupGui.Scale);
                            if (ImGui.InputInt("##RY", ref rlY)) CrossUp.Commands.LRpos(rlX, rlY);

                            ImGui.SameLine();
                            XupGui.BumpCursorX(4f * XupGui.Scale);
                            XupGui.WriteIcon(FontAwesomeIcon.ArrowsAltV);
                        }
                        ImGui.EndGroup();
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

                                    CrossUp.Layout.ResetBorrowed();
                                    CrossUp.Layout.SeparateEx.EnableIfReady();
                                    Config.Save();
                                }
                                else
                                {
                                    if (Config.LRborrow == i) Config.LRborrow = -1;
                                    else if (Config.RLborrow == i) Config.RLborrow = -1;

                                    CrossUp.Layout.ResetBorrowed();
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


                        ImGui.SameLine();
                        var labelColor = GameConfig.Hotbar.Visible[i]
                            ? ImGuiColors.DalamudWhite
                            : ImGuiColors.DalamudGrey3;
                        ImGui.TextColored(labelColor, Strings.SeparateEx.HotbarN(i + 1));
                    }
                }

                ImGui.EndTable();
            }

            HudOptions.ProfileIndicator();
            ImGui.EndTabItem();
        }
    }
}