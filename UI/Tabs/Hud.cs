using System;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;

namespace CrossUp;

internal sealed partial class CrossUpUI
{
    public class HudOptions
    {
        private static int CopyFrom;
        private static int CopyTo;
        public static void DrawTab()
        {
            if (!ImGui.BeginTabItem(Strings.Hud.TabTitle)) return;

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Indent(10);

            var uniqueHud = Config.UniqueHud;
            var from = CopyFrom;
            var to = CopyTo;

            if (ImGui.RadioButton(Strings.Hud.HudAllSame, !uniqueHud))
            {
                Config.UniqueHud = false;

                if (!CrossUp.Layout.SeparateEx.Ready) CrossUp.Layout.SeparateEx.Disable();
                CrossUp.Layout.Update(true);
                CrossUp.Color.SetAll();

                Config.Save();
            }

            if (ImGui.RadioButton(Strings.Hud.HudUnique, uniqueHud))
            {
                Config.UniqueHud = true;

                if (!CrossUp.Layout.SeparateEx.Ready) CrossUp.Layout.SeparateEx.Disable();
                CrossUp.Layout.Update(true);
                CrossUp.Color.SetAll();
                Config.Save();
            }

            XupGui.BumpCursorY(20f * XupGui.Scale);
            ImGui.Text(Strings.Hud.CurrentHudSlot);

            ImGui.SameLine();
            for (var i = 1; i <= 4; i++)
            {
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, XupGui.ColorSchemes[i, 2]);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, XupGui.ColorSchemes[i, 1]);

                var current = CrossUp.HudSlot == i;
                if (current) ImGui.PushStyleColor(ImGuiCol.Button, XupGui.ColorSchemes[i, 1]);
                if (current) ImGui.PushStyleColor(ImGuiCol.Text, XupGui.ColorSchemes[i, 0]);


                if (ImGui.Button(Strings.NumSymbols[i]))
                {
                    ChatHelper.SendMessage($"/hudlayout {i}");

                    if (!CrossUp.Layout.SeparateEx.Ready) CrossUp.Layout.SeparateEx.Disable();
                    CrossUp.Layout.Update(true);
                    CrossUp.Color.SetAll();
                }

                ImGui.PopStyleColor(current ? 4 : 2);

                if (i != 4) ImGui.SameLine();
            }

            XupGui.BumpCursorY(20f * XupGui.Scale);

            if (uniqueHud)
            {
                var msg = Strings.Hud.HighlightMsg.Split("|");
                ImGui.Text(msg[0]);
                ImGui.SameLine();
                XupGui.BumpCursorX(-5f * XupGui.Scale);
                ImGui.TextColored(XupGui.HighlightColor, msg[1]);
                ImGui.SameLine();
                XupGui.BumpCursorX(-6f * XupGui.Scale);
                ImGui.Text(msg[2]);
            }
            else
            {
                ImGui.Text("");
            }

            XupGui.BumpCursorY(20f * XupGui.Scale);


            if (ImGui.BeginTable("hudcopy", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("fromTo", ImGuiTableColumnFlags.WidthFixed, 120F * XupGui.Scale);
                ImGui.TableSetupColumn("controls", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();

                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey3);
                XupGui.ColumnCentredText(Strings.Hud.CopySettings.ToUpper());
                ImGui.PopStyleColor(1);


                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                XupGui.ColumnRightAlignText($"{Strings.Hud.From.ToUpper()} ");
                ImGui.TableNextColumn();

                ImGui.BeginGroup();
                for (var i = 0; i <= 4; i++)
                {
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, XupGui.ColorSchemes[i, 2]);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, XupGui.ColorSchemes[i, 1]);
                    if (from == i) ImGui.PushStyleColor(ImGuiCol.Text, XupGui.ColorSchemes[i, 0]);
                    if (from == i) ImGui.PushStyleColor(ImGuiCol.Button, XupGui.ColorSchemes[i, 1]);

                    if (ImGui.Button($"{Strings.NumSymbols[i]}##From{i}")) CopyFrom = i;

                    ImGui.PopStyleColor(from == i ? 4 : 2);
                    if (i != 4) ImGui.SameLine();
                }

                ImGui.EndGroup();

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                XupGui.ColumnCentredText("");

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                XupGui.ColumnRightAlignText($"{Strings.Hud.To.ToUpper()} ");
                ImGui.TableNextColumn();
                ImGui.BeginGroup();
                for (var i = 0; i <= 4; i++)
                {
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, XupGui.ColorSchemes[i, 2]);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, XupGui.ColorSchemes[i, 1]);
                    if (to == i) ImGui.PushStyleColor(ImGuiCol.Button, XupGui.ColorSchemes[i, 1]);
                    if (to == i) ImGui.PushStyleColor(ImGuiCol.Text, XupGui.ColorSchemes[i, 0]);


                    if (ImGui.Button($"{Strings.NumSymbols[i]}##To{i}")) CopyTo = i;

                    ImGui.PopStyleColor(to == i ? 4 : 2);
                    if (i != 4) ImGui.SameLine();
                }

                ImGui.EndGroup();

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();

                ImGui.Spacing();
                var label = $"   {Strings.Hud.Copy.ToUpper()}   ";

                if (ImGui.Button(label))
                {
                    if (CopyTo != CopyFrom)
                    {
                        Config.Profiles[CopyTo] = new(Config.Profiles[CopyFrom]);

                        PluginLog.Log(
                            $"Copying configs from Profile {Strings.NumSymbols[CopyFrom]} to Profile {Strings.NumSymbols[CopyTo]}");

                        if (!CrossUp.Layout.SeparateEx.Ready) CrossUp.Layout.SeparateEx.Disable();
                        CrossUp.Layout.Update(true);
                        CrossUp.Color.SetAll();
                    }
                }

                ImGui.EndTable();
            }

            ProfileIndicator();
            ImGui.EndTabItem();
        }
        public static void ProfileIndicator()
        {
            var p = Config.UniqueHud ? CrossUp.HudSlot : 0;
            var text = $"{(p == 0 ? Strings.Hud.AllHudSlots : Strings.Hud.HudSlot)} {Strings.NumSymbols[p]}";

            var textSize = ImGui.CalcTextSize(text);
            var windowSize = ImGui.GetWindowContentRegionMax();

            ImGui.SetCursorPosX(Math.Max(30f* XupGui.Scale,windowSize.X - 30f * XupGui.Scale - textSize.X));
            ImGui.SetCursorPosY(Math.Max(400 * XupGui.Scale, windowSize.Y - 30f * XupGui.Scale - textSize.Y));

            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, XupGui.ColorSchemes[p, 1]);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, XupGui.ColorSchemes[p, 1]);
            ImGui.PushStyleColor(ImGuiCol.Button, XupGui.ColorSchemes[p, 1]);
            ImGui.PushStyleColor(ImGuiCol.Text, XupGui.ColorSchemes[p, 0]);

            ImGui.Button(text);
            ImGui.PopStyleColor(4);
        }
    }
}