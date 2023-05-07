using System;
using System.Globalization;
using System.Numerics;
using Dalamud.Logging;
using static System.Int32;

namespace CrossUp;

public sealed partial class CrossUp
{
    private void OnMainCommand(string command, string args)
    {
        try
        {
            ParseTextCommand(args);
        }
        catch (Exception ex)
        {
            PluginLog.LogWarning($"Couldn't execute CrossUp command: {command} {args}");
            PluginLog.LogError($"{ex}");
            CrossUpUI.SettingsVisible = !CrossUpUI.SettingsVisible;
        }
    }
    private void ParseTextCommand(string args)
    {
        var argList = args.Split(" ");

        switch (argList[0])
        {
            case "split":
            case "splitdistance":
                Commands.Parse.SplitBar(argList);
                break;

            case "center":
            case "centre":
                Commands.Parse.Center(argList);
                break;

            case "padlock":
            case "padlockicon":
                Commands.Parse.Padlock(argList);
                break;

            case "setnum":
            case "setnumber":
            case "settext":
                Commands.Parse.SetNumText(argList);
                break;

            case "changeset":
                Commands.Parse.ChangeSet(argList);
                break;

            case "triggertext":
                Commands.Parse.TriggerText(argList);
                break;

            case "emptyslots":
            case "unassigned":
            case "unassignedslots":
                Commands.Parse.EmptySlots(argList);
                break;

            case "selectbg":
            case "bgcolor":
            case "bgcolour":
                Commands.Parse.SelectBG(argList);
                break;

            case "buttonglow":
            case "glowcolor":
            case "glowcolour":
            case "glow":
                Commands.Parse.ButtonGlow(argList);
                break;

            case "buttonpulse":
            case "pulsecolor":
            case "pulsecolour":
            case "pulse":
                Commands.Parse.ButtonPulse(argList);
                break;

            case "text":
            case "textcolor":
                Commands.Parse.TextColor(argList);
                break;

            case "border":
            case "bordercolor":
            case "bordercolour":
                Commands.Parse.BorderColor(argList);
                break;

            case "exhb":
            case "expandedhold":
            case "exbar":
                Commands.Parse.ExBar(argList);
                break;

            case "exonlyone":
            case "onlyone":
                Commands.Parse.OnlyOne(argList);
                break;

            case "lr":
            case "lrpos":
            case "exlr":
                Commands.Parse.LRpos(argList);
                break;

            case "rl":
            case "rlpos":
            case "exrl":
                Commands.Parse.RLpos(argList);
                break;

            case "combatfade":
            case "fade":
            case "fader":
                Commands.Parse.CombatFade(argList);
                break;

            case "debug":
                CrossUpUI.DebugVisible = !CrossUpUI.DebugVisible;
                break;

            default:
                CrossUpUI.SettingsVisible = !CrossUpUI.SettingsVisible;
                break;
        }
    }

    internal partial class Commands
    {
        internal class Parse
        {
            private static bool TextToBool(string str, bool toggleRef = false)
            {
                return str is "true" or "on" or "show" ||
                       TryParse(str, out var val) && val > 0 ||
                       str is not ("false" or "off" or "hide" or "hidden" or "0")
                       && !toggleRef;
            }

            private static Vector3 HexToColor3(string hex)
            {
                if (hex[0].ToString() == "#") hex = hex[1..];

                static float ToFloat(string hex, int start) =>
                    (float)Parse(hex.Substring(start, 2), NumberStyles.HexNumber) / 255;

                var r = ToFloat(hex, 0);
                var g = ToFloat(hex, 2);
                var b = ToFloat(hex, 4);

                return new Vector3(r, g, b);
            }

            internal static void SplitBar(string[] argList)
            {
                var len = argList.Length;
                if (len < 2)
                {
                    SplitOn(!Profile.SplitOn);
                }
                else
                {
                    SplitOn(TextToBool(argList[1]));
                    if (len >= 3 && TryParse(argList[2], out var d)) SplitDist(d);
                    if (len >= 4 && TryParse(argList[3], out var c)) Commands.Center(c);
                }
            }
            internal static void Center(string[] argList) =>
                Commands.Center(argList.Length >= 2 && TryParse(argList[1], out var c) ? c : 0);
            internal static void Padlock(string[] argList)
            {
                var len = argList.Length;
                if (len < 2)
                {
                    Commands.Padlock(Profile.HidePadlock);
                    return;
                }

                var show = TextToBool(argList[1], !Profile.HidePadlock);
                var x = 0;
                var y = 0;

                if (len >= 3) TryParse(argList[2], out x);
                if (len >= 4) TryParse(argList[3], out y);

                Commands.Padlock(show, x, y);
            }
            internal static void SetNumText(string[] argList)
            {
                var len = argList.Length;
                if (len < 2)
                {
                    Commands.SetNumText(Profile.HideSetText);
                    return;
                }

                var show = TextToBool(argList[1], !Profile.HideSetText);
                var x = 0;
                var y = 0;

                if (len >= 3) TryParse(argList[2], out x);
                if (len >= 4) TryParse(argList[3], out y);

                Commands.SetNumText(show, x, y);
            }
            internal static void ChangeSet(string[] argList)
            {
                if (argList.Length >= 3)
                {
                    TryParse(argList[1], out var x);
                    TryParse(argList[2], out var y);
                    Commands.ChangeSet(x, y);
                }
                else
                {
                    Commands.ChangeSet(0, 0);
                }
            }
            internal static void TriggerText(string[] argList) => Commands.TriggerText(argList.Length >= 2
                ? TextToBool(argList[1], !Profile.HideTriggerText)
                : Profile.HideTriggerText);
            internal static void EmptySlots(string[] argList) => Commands.EmptySlots(argList.Length >= 2
                ? TextToBool(argList[1], !Profile.HideUnassigned)
                : Profile.HideUnassigned);
            internal static void SelectBG(string[] argList)
            {
                if (argList.Length >= 4)
                {
                    var style = argList[1] is "frame" or "1"         ? 1 :
                                argList[1] is "hide"  or "hidden" or "2" or "off" ? 2 :
                                                                       0;
                    var blend = argList[2] is "dodge" or "2" ? 2 : 0;
                    var color = HexToColor3(argList[3]);

                    Commands.SelectBG(style, blend, color);
                }
                else
                {
                    Commands.SelectBG(0, 0, Color.Preset.MultiplyNeutral);
                }
            }
            internal static void ButtonGlow(string[] argList)
            {
                var len = argList.Length;
                if (len >= 2)
                {
                    Commands.ButtonGlow(HexToColor3(argList[1]));
                    if (len >= 3) Commands.ButtonPulse(HexToColor3(argList[2]));
                }
                else
                {
                    Commands.ButtonGlow(Color.Preset.White);
                }
            }
            internal static void ButtonPulse(string[] argList) => Commands.ButtonPulse(argList.Length >= 2 ? HexToColor3(argList[1]) : Color.Preset.White);
            internal static void TextColor(string[] argList)
            {
                var len = argList.Length;
                if (len >= 3)
                {
                    Commands.TextColor(HexToColor3(argList[1]), HexToColor3(argList[2]));
                    if (len >= 4) Commands.BorderColor(HexToColor3(argList[3]));
                }
                else
                {
                    Commands.TextColor(Color.Preset.White, Color.Preset.TextGlow);
                    Commands.BorderColor(Color.Preset.White);
                }
            }
            internal static void BorderColor(string[] argList) => Commands.BorderColor(argList.Length >= 2 ? HexToColor3(argList[1]) : Color.Preset.White);
            internal static void ExBar(string[] argList)
            {
                var len = argList.Length;
                if (len < 2)
                {
                    ExBarOn(!Profile.SepExBar);
                }
                else
                {
                    var show = TextToBool(argList[1], Profile.SepExBar);

                    if (len >= 4)
                    {
                        TryParse(argList[2], out var lr);
                        TryParse(argList[3], out var rl);
                        var onlyOne = len >= 5 && TextToBool(argList[4], Profile.OnlyOneEx);
                        Commands.ExBar(show, lr, rl, onlyOne);
                    }
                    else
                    {
                        ExBarOn(show);
                    }
                }
            }
            internal static void OnlyOne(string[] argList) => ExBarOnlyOne(argList.Length >= 2 ? TextToBool(argList[1]) : !Profile.OnlyOneEx);
            internal static void LRpos(string[] argList)
            {
                if (argList.Length < 3)
                {
                    Commands.LRpos(-214,-88);
                }
                else
                {
                    TryParse(argList[1], out var x);
                    TryParse(argList[2], out var y);
                    Commands.LRpos(x, y);
                }
            }
            internal static void RLpos(string[] argList)
            {
                if (argList.Length < 3)
                {
                    Commands.RLpos(214,-88);
                }
                else
                {
                    TryParse(argList[1], out var x);
                    TryParse(argList[2], out var y);
                    Commands.RLpos(x, y);
                }
            }
            internal static void CombatFade(string[] argList)
            {
                var len = argList.Length;
                if (len >= 2)
                {
                    var active = TextToBool(argList[1]);

                    if (len >= 4)
                    {
                        TryParse(argList[2], out var inCombat);
                        TryParse(argList[3], out var outCombat);
                        Commands.CombatFade(active, inCombat, outCombat);
                    }
                    else
                    {
                        Commands.CombatFade(active);
                    }
                }
                else
                {
                    Commands.CombatFade(!Profile.CombatFadeInOut);
                }
            }
        }
    }

    
}