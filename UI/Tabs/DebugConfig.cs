using System;
using System.Globalization;
using System.Numerics;
using CrossUp.Game;
using Dalamud.Logging;
using ImGuiNET;

namespace CrossUp.UI.Tabs
{
    internal class DebugConfig
    {
        private static int StartIndex;
        private static int EndIndex = 702;

        internal static void DrawTab()
        {
            var startIndex = StartIndex;
            var endIndex = EndIndex;

            if (!ImGui.BeginTabItem("Game Configurations")) return;

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
                    PluginLog.Log(new GameConfig.Option(i));
                }
            }

            ImGui.BeginTable("configTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY);

            ImGui.TableSetupColumn("ind", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("id", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("int", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("uint", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("hex", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableHeadersRow();

            for (var i = (uint)startIndex; i <= endIndex; i++)
            {
                if (i is < 0 or > 700) break;

                var conf = new GameConfig.Option(i);

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{i}");
                ImGui.TableNextColumn();

                ImGui.Text($"{conf.ID}");
                ImGui.TableNextColumn();
                ImGui.Text($"{conf.Name}");
                ImGui.SetNextItemWidth(100);
                ImGui.TableNextColumn();
                ImGui.Text($"{conf.Get()}");
                ImGui.TableNextColumn();
                ImGui.Text($"{(uint)conf.Get()}");
                ImGui.TableNextColumn();

                var hex = GameConfig.UintToHex((uint)conf.Get());

                ImGui.TextColored(HexToColor(hex), "");
                ImGui.SameLine();
                ImGui.Text(hex);
            }

            ImGui.EndTable();
            ImGui.EndTabItem();
        }

        private static Vector4 HexToColor(string hex)
        {
            static float ToFloat(string hex, int start) => (float)int.Parse(hex.Substring(start, 2), NumberStyles.HexNumber) / 255;

            var r = ToFloat(hex, 0);
            var g = ToFloat(hex, 2);
            var b = ToFloat(hex, 4);
            var a = ToFloat(hex, 6);

            return new Vector4(r, g, b, a);
        }
    }
}