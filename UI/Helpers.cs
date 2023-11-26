using System.Numerics;
using CrossUp.Game;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using static CrossUp.CrossUp;
using static Dalamud.Interface.Colors.ImGuiColors;

namespace CrossUp.UI;

internal class Helpers
{
    internal static readonly Vector4[,] ColorSchemes =
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

    internal static Vector4 HighlightColor => Config.UniqueHud ? ColorSchemes[HudData.CurrentSlot, 0] : DalamudWhite;
    internal static Vector4 DimColor => Config.UniqueHud ? ColorSchemes[HudData.CurrentSlot, 1] : DalamudGrey3;

    internal static void ColumnCentredText(string text)
    {
        var colWidth = ImGui.GetColumnWidth();
        var textWidth = ImGui.CalcTextSize(text).X;
        var indentSize = (colWidth - textWidth) * 0.5f;

        ImGui.Indent(indentSize);
        ImGui.Text(text);
        ImGui.Indent(-indentSize);
    }

    internal static void ColumnRightAlignText(string text)
    {
        var colWidth = ImGui.GetColumnWidth();
        var textWidth = ImGui.CalcTextSize(text).X;
        var indentSize = colWidth - textWidth;

        ImGui.Indent(indentSize);
        ImGui.Text(text);
        ImGui.Indent(-indentSize);
    }

    internal static void BumpCursorX(float val) => ImGui.SetCursorPosX(ImGui.GetCursorPosX() + val);
    internal static void BumpCursorY(float val) => ImGui.SetCursorPosY(ImGui.GetCursorPosY() + val);

    internal static void WriteIcon(FontAwesomeIcon icon, bool sameLine = false)
    {
        if (sameLine) ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text($"{icon.ToIconString()}");
        ImGui.PopFont();
    }

    internal static bool BeginIconTabItem(string label, FontAwesomeIcon icon, float width, string tooltip)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.SetNextItemWidth(width);
        var result = ImGui.BeginTabItem($"{icon.ToIconString()}##{label}");
        ImGui.PopFont();
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltip);
        return result;
    }

    internal static void Spacing(int n)
    {
        for (var i = 0; i < n; i++) ImGui.Spacing();
    }

    internal static float Scale => ImGuiHelpers.GlobalScale;
    internal const ImGuiColorEditFlags PickerFlags = ImGuiColorEditFlags.PickerMask | ImGuiColorEditFlags.DisplayHex;
}