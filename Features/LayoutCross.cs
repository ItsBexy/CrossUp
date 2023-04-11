using System;
using Dalamud.Interface;
using Dalamud.Logging;
using static CrossUp.CrossUp.Bars.Cross.Selection;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    public static partial class Layout
    {
        /// <summary>Methods for rearranging the main Cross Hotbar</summary>
        internal static class Cross
        {
            /// <summary>Arranges all elements of the main Cross Hotbar based on current selection status and other factors</summary>
            public static void Arrange(Select select, Select previous, float scale, int split, bool mixBar,
                bool arrangeEx, (int, int, int, int) coords, bool forceArrange, bool resetAll)
            {
                Bars.Cross.Root.SetPos(Bars.Cross.Base.X - split * scale, Bars.Cross.Base.Y)
                    .SetSize((ushort)(float)(588 + split * 2), 210);

                ushort? lineSize = split > 0 || Profile.SepExBar && !resetAll ? 0 : null;
                Bars.Cross.VertLine.SetSize(lineSize, lineSize);
                Bars.Cross.Padlock.SetRelativePos(Profile.PadlockOffset.X + split, Profile.PadlockOffset.Y);
                Bars.Cross.Padlock[2u].SetVis(!Profile.HidePadlock);
                Bars.Cross.SetDisplay.SetVis(!Profile.HideSetText)
                                     .SetRelativePos(Profile.SetTextOffset.X + split, Profile.SetTextOffset.Y);
                Bars.Cross.ChangeSetDisplay.Container.SetRelativePos(Profile.ChangeSetOffset.X + split, Profile.ChangeSetOffset.Y);
                Bars.Cross.LTtext.SetScale(Profile.HideTriggerText ? 0F : 1F);
                Bars.Cross.RTtext.SetScale(Profile.HideTriggerText ? 0F : 1F);

                UnassignedSlotVis(resetAll || !Profile.HideUnassigned);

                if (!forceArrange && select == previous) return;

                var (lrX, lrY, rlX, rlY) = coords;
                var miniSize = Profile.SelectDisplayType == 1 || (mixBar && split > 0) ? 0 : 166;
                switch (select)
                {
                    case Select.None:
                    case Select.LL:
                    case Select.RR:
                    default:
                        Bars.Cross.Container.SetRelativePos();
                        Bars.Cross.LTtext.SetRelativePos();
                        Bars.Cross.RTtext.SetRelativePos(split * 2);

                        Bars.Cross.Buttons[0].ChildVis(true).SetRelativePos();
                        Bars.Cross.Buttons[1].ChildVis(true).SetRelativePos();
                        Bars.Cross.Buttons[2].ChildVis(true).SetRelativePos(split * 2);
                        Bars.Cross.Buttons[3].ChildVis(true).SetRelativePos(split * 2);
                        break;
                    case Select.Left:
                        Bars.Cross.Container.SetRelativePos();
                        Bars.Cross.LTtext.SetRelativePos();
                        Bars.Cross.RTtext.SetRelativePos(split * 2);

                        var fromLR = previous == Select.LR && arrangeEx;
                        Bars.Cross.Buttons[0].ChildVis(true).SetRelativePos();
                        Bars.Cross.Buttons[1].ChildVis(!fromLR || !mixBar).SetRelativePos();
                        Bars.Cross.Buttons[2].ChildVis(!fromLR || mixBar).SetRelativePos(split * 2);
                        Bars.Cross.Buttons[3].ChildVis(!fromLR).SetRelativePos(split * 2);

                        Bars.Cross.MiniSelectL.SetSize((ushort)miniSize, 140);
                        Bars.Cross.MiniSelectR.SetSize((ushort)miniSize, 140);
                        break;

                    case Select.Right:
                        Bars.Cross.Container.SetRelativePos(split * 2);
                        Bars.Cross.LTtext.SetRelativePos(-split * 2);
                        Bars.Cross.RTtext.SetRelativePos();

                        var fromRL = previous == Select.RL && arrangeEx;
                        Bars.Cross.Buttons[0].ChildVis(!fromRL).SetRelativePos(-split * 2);
                        Bars.Cross.Buttons[1].ChildVis(!fromRL || mixBar).SetRelativePos(-split * 2);
                        Bars.Cross.Buttons[2].ChildVis(!fromRL || !mixBar).SetRelativePos();
                        Bars.Cross.Buttons[3].ChildVis(true).SetRelativePos();

                        Bars.Cross.MiniSelectL.SetSize((ushort)miniSize, 140);
                        Bars.Cross.MiniSelectR.SetSize((ushort)miniSize, 140);
                        break;

                    case Select.LR:
                        Bars.Cross.Container.SetRelativePos(lrX + split, lrY);
                        Bars.Cross.LTtext.SetRelativePos(-lrX - split, -lrY);
                        Bars.Cross.RTtext.SetRelativePos(-lrX + split, -lrY);

                        Bars.Cross.Buttons[0].ChildVis(false).SetRelativePos();
                        Bars.Cross.Buttons[1].ChildVis(true).SetRelativePos();
                        Bars.Cross.Buttons[2].ChildVis(true).SetRelativePos();
                        Bars.Cross.Buttons[3].ChildVis(false).SetRelativePos();
                        break;

                    case Select.RL:
                        Bars.Cross.Container.SetRelativePos(rlX + split, rlY);
                        Bars.Cross.LTtext.SetRelativePos(-rlX - split, -rlY);
                        Bars.Cross.RTtext.SetRelativePos(-rlX + split, -rlY);

                        Bars.Cross.Buttons[0].ChildVis(false).SetRelativePos();
                        Bars.Cross.Buttons[1].ChildVis(true).SetRelativePos();
                        Bars.Cross.Buttons[2].ChildVis(true).SetRelativePos();
                        Bars.Cross.Buttons[3].ChildVis(false).SetRelativePos();
                        break;
                }
            }

            /// <summary>Sets the visibility of empty slots on the Cross Hotbar</summary>
            public static void UnassignedSlotVis(bool show)
            {
                var scale = show ? 1F : 0F;

                for (var set = 0; set < 4; set++)
                {
                    for (uint bID = 2; bID <= 5; bID++)
                    {
                        Bars.Cross.Buttons[set][bID][3u].SetScale(scale);
                        Bars.WXHB.Buttons[set][bID][3u].SetScale(scale);
                    }
                }
            }

            /// <summary>Overrides HUD settings to force the Cross Hotbar to be horizontally centered</summary>
            public static void Recenter(float scale)
            {
                var baseX = (short)((ImGuiHelpers.MainViewport.Size.X - 588 * scale) / 2);
                if (Math.Abs(Bars.Cross.Base.X - baseX) < 0.9) return;

                PluginLog.LogDebug("Re-centering Cross Hotbar");
                Bars.Cross.Base.X = baseX;
                StoreXPos();
            }

            /// <summary>Fix for misalignment after entering/exiting HUD Layout Interface</summary>
            public static void HudOffsetFix(int split, float scale)
            {
                var misalign = Bars.Cross.Base.X - Bars.Cross.Root.Node->X - Math.Round(split * scale);
                if (misalign >= 0) return;

                PluginLog.LogDebug($"HUD FIX: Misaligned by {misalign}");
                Bars.Cross.Base.X -= (short)misalign;
            }

            /// <summary>Records the X coordinates of the Cross Hotbar's AtkUnitBase and root node on disable/dispose</summary>
            internal static void StoreXPos()
            {
                if (!Bars.Cross.Exists)
                {
                    return;
                }

                PluginLog.LogDebug($"Storing Cross Hotbar X Position; UnitBase: {Bars.Cross.Base.X}, Root Node: {Bars.Cross.Root.Node->X}");

                Config.DisposeBaseX = Bars.Cross.Base.X;
                Config.DisposeRootX = Bars.Cross.Root.Node->X;
                Config.Save();
            }

            /// <summary>Restores the last known X coordinates of the Cross Hotbar's AtkUnitBase and root node</summary>
            public static void RestoreXPos()
            {
                try
                {
                    if (!Bars.Cross.Exists || Profile.LockCenter) return;
                    if (Bars.Cross.Base.X != (short)Config.DisposeBaseX! ||
                        Math.Abs(Bars.Cross.Root.Node->X - (float)Config.DisposeRootX!) > 0.5F)
                        PluginLog.LogDebug("Correcting Cross Hotbar X Position");

                    Bars.Cross.Base.X = (short)Config.DisposeBaseX!;
                    Bars.Cross.Root.Node->X = (float)Config.DisposeRootX!;
                } catch (Exception ex) { PluginLog.LogWarning("Exception: Couldn't restore Cross Hotbar X Position!\n" + ex); }
            }
        }
    }
}