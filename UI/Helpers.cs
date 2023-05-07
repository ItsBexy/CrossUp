using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace CrossUp;

internal sealed partial class CrossUpUI
{
    public class XupGui
    {
        public static readonly Vector4[,] ColorSchemes =
        {
            {
                new(1f),
                new(0.6f, 0.6f, 0.6f, 1f),
                new(0.6f, 0.6f, 0.6f, 0.9f)
            },
            {
                new(0.996f, 0.639f, 0.620f, 1f),
                new(0.522f, 0.004f, 0.165f, 1f),
                new(0.522f, 0.004f, 0.165f, 0.9f)
            },
            {
                new(0.835f, 0.737f, 0.345f, 1f),
                new(0.471f, 0.141f, 0.004f, 1f),
                new(0.471f, 0.141f, 0.004f, 0.9f)
            },
            {
                new(0.216f, 0.831f, 0.929f, 1f),
                new(0f, 0.267f, 0.502f, 1f),
                new(0f, 0.267f, 0.502f, 0.9f)
            },
            {
                new(0.984f, 0.6f, 1f, 1f),
                new(0.357f, 0.1495f, 0.549f, 1f),
                new(0.357f, 0.1490f, 0.549f, 0.9f)
            }
        };

        public static Vector4 HighlightColor =>
            Config.UniqueHud ? ColorSchemes[CrossUp.HudSlot, 0] : ImGuiColors.DalamudWhite;

        public static Vector4 DimColor =>
            Config.UniqueHud ? ColorSchemes[CrossUp.HudSlot, 1] : ImGuiColors.DalamudGrey3;

        public static void ColumnCentredText(string text)
        {
            var colWidth = ImGui.GetColumnWidth();
            var textWidth = ImGui.CalcTextSize(text).X;
            var indentSize = (colWidth - textWidth) * 0.5f;

            ImGui.Indent(indentSize);
            ImGui.Text(text);
            ImGui.Indent(-indentSize);
        }

        public static void ColumnRightAlignText(string text)
        {
            var colWidth = ImGui.GetColumnWidth();
            var textWidth = ImGui.CalcTextSize(text).X;
            var indentSize = colWidth - textWidth;

            ImGui.Indent(indentSize);
            ImGui.Text(text);
            ImGui.Indent(-indentSize);
        }

        public static void BumpCursorX(float val) => ImGui.SetCursorPosX(ImGui.GetCursorPosX() + val);
        public static void BumpCursorY(float val) => ImGui.SetCursorPosY(ImGui.GetCursorPosY() + val);

        public static void WriteIcon(FontAwesomeIcon icon, bool sameLine = false)
        {
            if (sameLine) ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.Text($"{icon.ToIconString()}");
            ImGui.PopFont();
        }

        public static float Scale => ImGuiHelpers.GlobalScale;
        public const ImGuiColorEditFlags PickerFlags = ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.DisplayHex;
    }
}