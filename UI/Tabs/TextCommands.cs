using System;
using System.Collections.Generic;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace CrossUp.UI.Tabs;

internal class TextCommands
{
    internal static void DrawTab()
    {
        if (!Helpers.BeginIconTabItem("textcommands", FontAwesomeIcon.Terminal, 23f * Helpers.Scale, "Text Commands")) return;

        ImGui.BeginTable("textCommandList", 1, ImGuiTableFlags.ScrollY);
        ImGui.TableNextColumn();

        Helpers.Spacing(2);
        ImGui.Indent(10);

        ImGui.TextColored(ImGuiColors.DalamudGrey3, Strings.TextCommands.Header.ToUpper());

        Helpers.Spacing(2);

        foreach (var command in CommandList) command.Describe();

        ImGui.EndTable();
        ImGui.EndTabItem();
    }

    private readonly struct TextCommand
    {
        private readonly string Input;
        private readonly string Description;
        private readonly Argument[] Arguments;
        public TextCommand(string input, string description, Argument[]? arguments = null)
        {
            Input = input;
            Description = description;
            Arguments = arguments ?? Array.Empty<Argument>();
        }
        public void Describe()
        {
            ImGui.Spacing();
            ImGui.Text(Input);
            foreach (var arg in Arguments) arg.AddLabel();
            ImGui.TextColored(ImGuiColors.DalamudGrey3, Description);
            ImGui.Spacing();
        }
    }

    private readonly struct Argument
    {
        private readonly string Label;
        private readonly string Tooltip;
        public Argument(string label, string tooltip)
        {
            Label = label;
            Tooltip = tooltip;
        }
        public void AddLabel()
        {
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DalamudGrey, $"[{Label}]");
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(Tooltip);
        }
    }

    private static IEnumerable<TextCommand> CommandList => new TextCommand[] {
        new(input: "/xup", description: Strings.TextCommands.OpenClose),
        new(input: "/xup split",description: Strings.TextCommands.Split, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.OnOff,        tooltip: Strings.TextCommands.TogglesFeat),
            new(label: Strings.TextCommands.ArgLabels.Distance,     tooltip: Strings.TextCommands.SetsSeparationDistance),
            new(label: Strings.TextCommands.ArgLabels.Center,       tooltip: Strings.TextCommands.SetsCenterPoint)
        }),
        new(input: "/xup padlock",
            description: Strings.TextCommands.Padlock,
            arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.ShowHide,     tooltip: Strings.TextCommands.TogglesVis),
            new(label: "x",                                         tooltip: Strings.TextCommands.SetsX),
            new(label: "y",                                         tooltip: Strings.TextCommands.SetsY)
        }),
        new(input: "/xup setnum", description: Strings.TextCommands.SetNum, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.ShowHide,     tooltip: Strings.TextCommands.TogglesVis),
            new(label: "x",                                         tooltip: Strings.TextCommands.SetsX),
            new(label: "y",                                         tooltip: Strings.TextCommands.SetsY)
        }),
        new(input: "/xup changeset", description: Strings.TextCommands.ChangeSet, arguments: new Argument[]
        {
            new(label: "x",                                         tooltip: Strings.TextCommands.SetsX),
            new(label: "y",                                         tooltip: Strings.TextCommands.SetsY)
        }),
        new(input: "/xup triggertext", description: Strings.TextCommands.TriggerText, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.ShowHide,     tooltip: Strings.TextCommands.TogglesVis)
        }),
        new(input: "/xup emptyslots", description: Strings.TextCommands.UnassignedSlots, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.ShowHide,     tooltip: Strings.TextCommands.TogglesVis)
        }),
        new(input: "/xup selectbg", description: Strings.TextCommands.SelectBg, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.Style,        tooltip: Strings.TextCommands.BgStyle),
            new(label: Strings.TextCommands.ArgLabels.Blend,        tooltip: Strings.TextCommands.BgBlend),
            new(label: Strings.TextCommands.ArgLabels.Color,        tooltip: Strings.TextCommands.BgColor)
        }),
        new(input: "/xup buttonglow", description: Strings.TextCommands.ButtonColor, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.GlowColor,    tooltip: Strings.TextCommands.ButtonGlow),
            new(label: Strings.TextCommands.ArgLabels.PulseColor,   tooltip: Strings.TextCommands.ButtonPulse)
        }),
        new(input: "/xup textcolor", description: Strings.TextCommands.TextAndBorders, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.TextColor,    tooltip: Strings.TextCommands.TextColor),
            new(label: Strings.TextCommands.ArgLabels.GlowColor,    tooltip: Strings.TextCommands.TextGlow),
            new(label: Strings.TextCommands.ArgLabels.BorderColor,  tooltip: Strings.TextCommands.BorderColor)
        }),
        new(input: "/xup exhb", description: Strings.TextCommands.SepEx, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.OnOff,        tooltip: Strings.TextCommands.TogglesFeat),
            new(label: Strings.TextCommands.ArgLabels.BorrowBar,    tooltip: Strings.TextCommands.BorrowBar),
            new(label: Strings.TextCommands.ArgLabels.BorrowBar,    tooltip: Strings.TextCommands.BorrowBar)
        }),
        new(input: "/xup exonlyone", description: Strings.TextCommands.OnlyOne, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.TrueFalse,    tooltip: Strings.TextCommands.OnlyOneTip)
        }),
        new(input: "/xup lr", description: Strings.TextCommands.ExPos(s: "L→R"), arguments: new Argument[]
        {
            new(label: "x",                                         tooltip: Strings.TextCommands.SetsX),
            new(label: "y",                                         tooltip: Strings.TextCommands.SetsY)
        }),
        new(input: "/xup rl", description: Strings.TextCommands.ExPos(s: "R→L"), arguments: new Argument[]
        {
            new(label: "x",                                         tooltip: Strings.TextCommands.SetsX),
            new(label: "y",                                         tooltip: Strings.TextCommands.SetsY)
        }),
        new(input: "/xup combatfade", description: Strings.TextCommands.Fader, arguments: new Argument[]
        {
            new(label: Strings.TextCommands.ArgLabels.OnOff,        tooltip: Strings.TextCommands.TogglesFeat),
            new(label: Strings.TextCommands.ArgLabels.CombatTransp, tooltip: Strings.TextCommands.SetsTransparency),
            new(label: Strings.TextCommands.ArgLabels.IdleTransp,   tooltip: Strings.TextCommands.SetsTransparency)
        })
    };
}