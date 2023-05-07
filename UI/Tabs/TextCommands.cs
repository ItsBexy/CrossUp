using System;
using System.Collections.Generic;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace CrossUp;

internal sealed partial class CrossUpUI
{
    public class TextCommands
    {
        public static void DrawTab()
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.SetNextItemWidth(23f * XupGui.Scale);

            if (ImGui.BeginTabItem(FontAwesomeIcon.Terminal.ToIconString()))
            {
                ImGui.PopFont();

                ImGui.BeginTable("textCommandList", 1, ImGuiTableFlags.ScrollY);
                ImGui.TableNextColumn();

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Indent(10);

                ImGui.TextColored(ImGuiColors.DalamudGrey3, Strings.TextCommands.Header.ToUpper());

                ImGui.Spacing();
                ImGui.Spacing();
                
                foreach (var command in CommandList) command.Describe();

                ImGui.EndTable();
                ImGui.EndTabItem();
            }
            else
            {
                ImGui.PopFont();
            }
        }

        public readonly struct TextCommand
        {
            private readonly string Input;
            private readonly string Description;
            private readonly Argument[] Arguments;
            public TextCommand(string input, string description, Argument[] arguments)
            {
                Input = input;
                Description = description;
                Arguments = arguments;
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
        public readonly struct Argument
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
            new(input: "/xup", description: Strings.TextCommands.OpenClose, arguments: Array.Empty<Argument>()),
            new(input: "/xup split", description: Strings.TextCommands.Split, arguments: new Argument[] 
            {
                new("on/off", Strings.TextCommands.TogglesFeat),
                new("distance", Strings.TextCommands.SetsSeparationDistance),
                new("center point", Strings.TextCommands.SetsCenterPoint)
            }),
            new(input: "/xup padlock", description: Strings.TextCommands.Padlock, arguments: new Argument[] 
            {
                new("on/off", Strings.TextCommands.TogglesVis),
                new("x", Strings.TextCommands.SetsX),
                new("y", Strings.TextCommands.SetsY)
            }),
            new(input: "/xup setnum", description: Strings.TextCommands.SetNum, arguments: new Argument[]
            {
                new("on/off", Strings.TextCommands.TogglesVis),
                new("x", Strings.TextCommands.SetsX),
                new("y", Strings.TextCommands.SetsY)
            }),
            new(input: "/xup changeset", description: Strings.TextCommands.ChangeSet, arguments: new Argument[]
            {
                new("x", Strings.TextCommands.SetsX),
                new("y", Strings.TextCommands.SetsY)
            }),
            new(input: "/xup triggertext", description: Strings.TextCommands.TriggerText, arguments: new Argument[]
            {
                new("show/hide", Strings.TextCommands.TogglesVis)
            }),
            new(input: "/xup emptyslots", description: Strings.TextCommands.UnassignedSlots, arguments: new Argument[]
            {
                new("show/hide", Strings.TextCommands.TogglesVis)
            }),
            new(input: "/xup selectbg", description: Strings.TextCommands.SelectBg, arguments: new Argument[]
            {
                new("style", Strings.TextCommands.BgStyle),
                new("blend", Strings.TextCommands.BgBlend),
                new("color", Strings.TextCommands.BgColor)
            }),
            new(input: "/xup buttonglow", description: Strings.TextCommands.ButtonColor, arguments: new Argument[]
            {
                new("glow color", Strings.TextCommands.ButtonGlow),
                new("pulse color", Strings.TextCommands.ButtonPulse)
            }),
            new(input: "/xup textcolor", description: Strings.TextCommands.TextAndBorders, arguments: new Argument[]
            {
                new("text color", Strings.TextCommands.TextColor),
                new("glow color", Strings.TextCommands.TextGlow),
                new("border color", Strings.TextCommands.BorderColor)
            }),
            new(input: "/xup exhb", description: Strings.TextCommands.SepEx, arguments: new Argument[]
            {
                new("on/off", Strings.TextCommands.TogglesFeat),
                new("borrowed hotbar", Strings.TextCommands.BorrowBar),
                new("borrowed hotbar", Strings.TextCommands.BorrowBar)
            }),
            new(input: "/xup exonlyone", description: Strings.TextCommands.OnlyOne, arguments: new Argument[]
            {
                new("true/false", Strings.TextCommands.OnlyOneTip)
            }),
            new(input: "/xup lr", description: Strings.TextCommands.ExPos("L→R"), arguments: new Argument[]
            {
                new("x", Strings.TextCommands.SetsX),
                new("y", Strings.TextCommands.SetsY)
            }),
            new(input: "/xup rl", description: Strings.TextCommands.ExPos("R→L"), arguments: new Argument[]
            {
                new("x", Strings.TextCommands.SetsX),
                new("y", Strings.TextCommands.SetsY)
            }),
            new(input: "/xup combatfade", description: Strings.TextCommands.Fader, arguments: new Argument[]
            {
                new("on/off", Strings.TextCommands.TogglesFeat),
                new("combat transparency", Strings.TextCommands.SetsTransparency),
                new("idle transparency", Strings.TextCommands.SetsTransparency)
            })
        };
    }
}