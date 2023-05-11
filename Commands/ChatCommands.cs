using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using CrossUp.Features;
using Dalamud.Logging;
using static System.Int32;

namespace CrossUp.Commands
{
    internal class ChatCommands
    {
        public static void Remove() => CrossUp.CommandManager.RemoveHandler(CrossUp.MainCommand);

        public static void OnMainCommand(string command, string args)
        {
            try
            {
                Parse.MainCommand(args);
            }
            catch (Exception ex)
            {
                PluginLog.LogWarning($"Couldn't execute CrossUp command: {command} {args}");
                PluginLog.LogError($"{ex}");
                CrossUp.UI.SettingsWindow.Show = !CrossUp.UI.SettingsWindow.Show;
            }
        }

        private static class Parse
        {
            public static void MainCommand(string args)
            {
                IReadOnlyList<string> argList = args.Split(" ");

                switch (argList[0])
                {
                    case "split":
                    case "splitdistance":
                        SplitBar(argList);
                        break;

                    case "center":
                    case "centre":
                        Center(argList);
                        break;

                    case "padlock":
                    case "padlockicon":
                        Padlock(argList);
                        break;

                    case "setnum":
                    case "setnumber":
                    case "settext":
                        SetNumText(argList);
                        break;

                    case "changeset":
                        ChangeSet(argList);
                        break;

                    case "triggertext":
                        TriggerText(argList);
                        break;

                    case "emptyslots":
                    case "unassigned":
                    case "unassignedslots":
                        EmptySlots(argList);
                        break;

                    case "selectbg":
                    case "bgcolor":
                    case "bgcolour":
                        SelectBG(argList);
                        break;

                    case "buttonglow":
                    case "glowcolor":
                    case "glowcolour":
                    case "glow":
                        ButtonGlow(argList);
                        break;

                    case "buttonpulse":
                    case "pulsecolor":
                    case "pulsecolour":
                    case "pulse":
                        ButtonPulse(argList);
                        break;

                    case "text":
                    case "textcolor":
                        TextColor(argList);
                        break;

                    case "border":
                    case "bordercolor":
                    case "bordercolour":
                        BorderColor(argList);
                        break;

                    case "exhb":
                    case "expandedhold":
                    case "exbar":
                        ExBar(argList);
                        break;

                    case "exonlyone":
                    case "onlyone":
                        OnlyOne(argList);
                        break;

                    case "lr":
                    case "lrpos":
                    case "exlr":
                        LRpos(argList);
                        break;

                    case "rl":
                    case "rlpos":
                    case "exrl":
                        RLpos(argList);
                        break;

                    case "combatfade":
                    case "fade":
                    case "fader":
                        CombatFade(argList);
                        break;

                    case "debug":
                        Internal.ToggleDebug(CrossUp.UI);
                        break;

                    default:
                        Internal.ToggleSettings(CrossUp.UI);
                        break;
                }
            }

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

            private static void SplitBar(IReadOnlyList<string> argList)
            {
                var ct = argList.Count;
                if (ct < 2)
                {
                    Internal.SplitOn(!CrossUp.Profile.SplitOn);
                }
                else
                {
                    Internal.SplitOn(TextToBool(argList[1]));
                    if (ct >= 3 && TryParse(argList[2], out var d)) Internal.SplitDist(d);
                    if (ct >= 4 && TryParse(argList[3], out var c)) Internal.Center(c);
                }
            }

            private static void Center(IReadOnlyList<string> argList) => Internal.Center(argList.Count >= 2 && TryParse(argList[1], out var c) ? c : 0);

            private static void Padlock(IReadOnlyList<string> argList)
            {
                var ct = argList.Count;
                if (ct < 2)
                {
                    Internal.Padlock(CrossUp.Profile.HidePadlock);
                    return;
                }

                var show = TextToBool(argList[1], !CrossUp.Profile.HidePadlock);
                var x = 0;
                var y = 0;

                if (ct >= 3) TryParse(argList[2], out x);
                if (ct >= 4) TryParse(argList[3], out y);

                Internal.Padlock(show, x, y);
            }

            private static void SetNumText(IReadOnlyList<string> argList)
            {
                var ct = argList.Count;
                if (ct < 2)
                {
                    Internal.SetNumText(CrossUp.Profile.HideSetText);
                    return;
                }

                var show = TextToBool(argList[1], !CrossUp.Profile.HideSetText);
                var x = 0;
                var y = 0;

                if (ct >= 3) TryParse(argList[2], out x);
                if (ct >= 4) TryParse(argList[3], out y);

                Internal.SetNumText(show, x, y);
            }

            private static void ChangeSet(IReadOnlyList<string> argList)
            {
                if (argList.Count >= 3)
                {
                    TryParse(argList[1], out var x);
                    TryParse(argList[2], out var y);
                    Internal.ChangeSet(x, y);
                }
                else
                {
                    Internal.ChangeSet(0, 0);
                }
            }

            private static void TriggerText(IReadOnlyList<string> argList) => Internal.TriggerText(argList.Count >= 2
                ? TextToBool(argList[1], !CrossUp.Profile.HideTriggerText)
                : CrossUp.Profile.HideTriggerText);

            private static void EmptySlots(IReadOnlyList<string> argList) => Internal.EmptySlots(argList.Count >= 2
                ? TextToBool(argList[1], !CrossUp.Profile.HideUnassigned)
                : CrossUp.Profile.HideUnassigned);

            private static void SelectBG(IReadOnlyList<string> argList)
            {
                if (argList.Count >= 4)
                {
                    var style = argList[1] is "frame" or "1"         ? 1 :
                        argList[1] is "hide"  or "hidden" or "2" or "off" ? 2 :
                        0;
                    var blend = argList[2] is "dodge" or "2" ? 2 : 0;
                    var color = HexToColor3(argList[3]);

                    Internal.SelectBG(style, blend, color);
                }
                else
                {
                    Internal.SelectBG(0, 0, Color.Preset.MultiplyNeutral);
                }
            }

            private static void ButtonGlow(IReadOnlyList<string> argList)
            {
                var ct = argList.Count;
                if (ct >= 2)
                {
                    Internal.ButtonGlow(HexToColor3(argList[1]));
                    if (ct >= 3) Internal.ButtonPulse(HexToColor3(argList[2]));
                }
                else
                {
                    Internal.ButtonGlow(Color.Preset.White);
                }
            }

            private static void ButtonPulse(IReadOnlyList<string> argList) => Internal.ButtonPulse(argList.Count >= 2 ? HexToColor3(argList[1]) : Color.Preset.White);

            private static void TextColor(IReadOnlyList<string> argList)
            {
                var ct = argList.Count;
                if (ct >= 3)
                {
                    Internal.TextColor(HexToColor3(argList[1]), HexToColor3(argList[2]));
                    if (ct >= 4) Internal.BorderColor(HexToColor3(argList[3]));
                }
                else
                {
                    Internal.TextColor(Color.Preset.White, Color.Preset.TextGlow);
                    Internal.BorderColor(Color.Preset.White);
                }
            }

            private static void BorderColor(IReadOnlyList<string> argList) => Internal.BorderColor(argList.Count >= 2 ? HexToColor3(argList[1]) : Color.Preset.White);

            private static void ExBar(IReadOnlyList<string> argList)
            {
                var ct = argList.Count;
                if (ct < 2)
                {
                    Internal.ExBarOn(!CrossUp.Profile.SepExBar);
                }
                else
                {
                    var show = TextToBool(argList[1], CrossUp.Profile.SepExBar);

                    if (ct >= 4)
                    {
                        TryParse(argList[2], out var lr);
                        TryParse(argList[3], out var rl);
                        var onlyOne = ct >= 5 && TextToBool(argList[4], CrossUp.Profile.OnlyOneEx);
                        Internal.ExBar(show, lr, rl, onlyOne);
                    }
                    else
                    {
                        Internal.ExBarOn(show);
                    }
                }
            }

            private static void OnlyOne(IReadOnlyList<string> argList) => Internal.ExBarOnlyOne(argList.Count >= 2 ? TextToBool(argList[1]) : !CrossUp.Profile.OnlyOneEx);

            private static void LRpos(IReadOnlyList<string> argList)
            {
                if (argList.Count < 3)
                {
                    Internal.LRpos(-214,-88);
                }
                else
                {
                    TryParse(argList[1], out var x);
                    TryParse(argList[2], out var y);
                    Internal.LRpos(x, y);
                }
            }

            private static void RLpos(IReadOnlyList<string> argList)
            {
                if (argList.Count < 3)
                {
                    Internal.RLpos(214,-88);
                }
                else
                {
                    TryParse(argList[1], out var x);
                    TryParse(argList[2], out var y);
                    Internal.RLpos(x, y);
                }
            }

            private static void CombatFade(IReadOnlyList<string> argList)
            {
                var ct = argList.Count;
                if (ct >= 2)
                {
                    var active = TextToBool(argList[1]);

                    if (ct >= 4)
                    {
                        TryParse(argList[2], out var inCombat);
                        TryParse(argList[3], out var outCombat);
                        Internal.CombatFade(active, inCombat, outCombat);
                    }
                    else
                    {
                        Internal.CombatFade(active);
                    }
                }
                else
                {
                    Internal.CombatFade(!CrossUp.Profile.CombatFadeInOut);
                }
            }
        }
    }
}