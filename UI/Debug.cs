using System;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CrossUp;

internal sealed partial class CrossUpUI
{
#pragma warning disable CS8618
    public class Debug
    {
        public static void DrawWindow()
        {

            ImGui.SetNextWindowSizeConstraints(new Vector2(750 * Scale,400 * Scale), new Vector2(750 * Scale, 1200 * Scale));
            ImGui.SetNextWindowSize(Config.DebugWindowSize, ImGuiCond.Always);
            if (ImGui.Begin("CrossUp Debug"))
            {
                if (ImGui.BeginTabBar("DebugTabs"))
                {
                    GameConfigs.DrawTab();
                    ImGui.EndTabBar();
                }

                Config.DebugWindowSize = ImGui.GetWindowSize();
                ImGui.End();
            }
        }

        public class GameConfigs
        {
            private static int StartIndex;
            private static int EndIndex = 702;
            public static void DrawTab()
            {
                var startIndex = StartIndex;
                var endIndex = EndIndex;

                if (ImGui.BeginTabItem("Config Values"))
                {
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("##startIndex", ref startIndex))
                        StartIndex = Math.Min(701, Math.Max(0, startIndex));

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


                    ImGui.BeginTable("configTable", 5, ImGuiTableFlags.Borders, new Vector2(645 * Scale, 1));

                    ImGui.TableSetupColumn("ind", ImGuiTableColumnFlags.WidthFixed, 50 * Scale);
                    ImGui.TableSetupColumn("id", ImGuiTableColumnFlags.WidthFixed, 50 * Scale);
                    ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthFixed, 300 * Scale);
                    ImGui.TableSetupColumn("int", ImGuiTableColumnFlags.WidthFixed, 100 * Scale);
                    ImGui.TableSetupColumn("uint", ImGuiTableColumnFlags.WidthFixed, 100 * Scale);
                    ImGui.TableSetupColumn("hex", ImGuiTableColumnFlags.WidthFixed, 200 * Scale);

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

                        var hex = CharConfig.UintToHex((uint)conf.Get());

                        ImGui.TextColored(HexToColor(hex), "");
                        ImGui.SameLine();
                        ImGui.Text(hex);
                    }

                    ImGui.EndTable();
                    ImGui.EndTabItem();
                }
            }

            private static Vector4 HexToColor(string hex)
            {
                static float ToFloat(string hex, int start) => (float)int.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber) / 255;

                var r = ToFloat(hex, 0);
                var g = ToFloat(hex, 2);
                var b = ToFloat(hex, 4);
                var a = ToFloat(hex, 6);

                return new Vector4(r, g, b, a);
            }

            
        }

    }
}