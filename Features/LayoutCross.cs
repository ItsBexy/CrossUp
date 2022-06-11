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
        public static class Cross
        {
            /// <summary>Arranges all elements of the main Cross Hotbar based on current selection status and other factors</summary>
            public static void Arrange(Select select, Select previous, float scale, int split, bool mixBar, bool arrangeEx, (int, int, int, int) coords, bool forceArrange, bool resetAll)
            {
                Bars.Cross.Root.SetPos(Bars.Cross.Base->X - split * scale, Bars.Cross.Base->Y)
                               .SetSize((ushort)(float)(588 + split * 2), 210);

                ushort? lineSize = split > 0 || Config.SepExBar && !resetAll ? 0 : null;
                Bars.Cross.VertLine.SetSize(lineSize, lineSize);
                Bars.Cross.Padlock.SetRelativePos(Config.PadlockOffset.X + split, Config.PadlockOffset.Y);
                Bars.Cross.PadlockIcon.SetVis(!Config.HidePadlock);
                Bars.Cross.SetDisplay.SetVis(!Config.HideSetText)
                                     .SetRelativePos(Config.SetTextOffset.X + split, Config.SetTextOffset.Y);
                Bars.Cross.ChangeSetDisplay.Container.SetRelativePos(Config.ChangeSetOffset.X + split, Config.ChangeSetOffset.Y);
                Bars.Cross.LTtext.SetScale(Config.HideTriggerText ? 0F : 1F);
                Bars.Cross.RTtext.SetScale(Config.HideTriggerText ? 0F : 1F);

                UnassignedSlotVis(resetAll || !Config.HideUnassigned);

                if (!forceArrange && select == previous) return;

                var (lrX, lrY, rlX, rlY) = coords;
                switch (select)
                {
                    case Select.None:
                    case Select.LL:
                    case Select.RR:
                    default:
                        Bars.Cross.Container.SetRelativePos();
                        Bars.Cross.LTtext.SetRelativePos();
                        Bars.Cross.RTtext.SetRelativePos(split * 2);

                        Bars.Cross.Left.GroupL.ChildVis(true).SetRelativePos();
                        Bars.Cross.Left.GroupR.ChildVis(true).SetRelativePos();
                        Bars.Cross.Right.GroupL.ChildVis(true).SetRelativePos(split * 2);
                        Bars.Cross.Right.GroupR.ChildVis(true).SetRelativePos(split * 2);
                        break;

                    case Select.Left:
                        Bars.Cross.Container.SetRelativePos();
                        Bars.Cross.LTtext.SetRelativePos();
                        Bars.Cross.RTtext.SetRelativePos(split * 2);

                        Bars.Cross.Left.GroupL.ChildVis(true).SetRelativePos();
                        Bars.Cross.Left.GroupR.ChildVis(!(previous == Select.LR && arrangeEx && mixBar)).SetRelativePos();
                        Bars.Cross.Right.GroupL.ChildVis(!(previous == Select.LR && arrangeEx && !mixBar)).SetRelativePos(split * 2);
                        Bars.Cross.Right.GroupR.ChildVis(!(previous == Select.LR && arrangeEx)).SetRelativePos(split * 2);

                        Bars.Cross.MiniSelectL.SetSize((ushort)(Config.HideSelect || (mixBar && split > 0) ? 0 : 166), 140);
                        Bars.Cross.MiniSelectR.SetSize((ushort)(Config.HideSelect || (mixBar && split > 0) ? 0 : 166), 140);
                        break;

                    case Select.Right:
                        Bars.Cross.Container.SetRelativePos(split * 2);
                        Bars.Cross.LTtext.SetRelativePos(-split * 2);
                        Bars.Cross.RTtext.SetRelativePos();

                        Bars.Cross.Left.GroupL.ChildVis(!(previous == Select.RL && arrangeEx)).SetRelativePos(-split * 2);
                        Bars.Cross.Left.GroupR.ChildVis(!(previous == Select.RL && arrangeEx && !mixBar)).SetRelativePos(-split * 2);
                        Bars.Cross.Right.GroupL.ChildVis(!(previous == Select.RL && arrangeEx && mixBar)).SetRelativePos();
                        Bars.Cross.Right.GroupR.ChildVis(true).SetRelativePos();

                        Bars.Cross.MiniSelectL.SetSize((ushort)(Config.HideSelect || (mixBar && split > 0) ? 0 : 166), 140);
                        Bars.Cross.MiniSelectR.SetSize((ushort)(Config.HideSelect || (mixBar && split > 0) ? 0 : 166), 140);
                        break;

                    case Select.LR:
                        Bars.Cross.Container.SetRelativePos(lrX + split, lrY);
                        Bars.Cross.LTtext.SetRelativePos(-lrX - split, -lrY);
                        Bars.Cross.RTtext.SetRelativePos(-lrX + split, -lrY);

                        Bars.Cross.Left.GroupL.ChildVis(false).SetRelativePos();
                        Bars.Cross.Left.GroupR.ChildVis(true).SetRelativePos();
                        Bars.Cross.Right.GroupL.ChildVis(true).SetRelativePos();
                        Bars.Cross.Right.GroupR.ChildVis(false).SetRelativePos();
                        break;

                    case Select.RL:
                        Bars.Cross.Container.SetRelativePos(rlX + split, rlY);
                        Bars.Cross.LTtext.SetRelativePos(-rlX - split, -rlY);
                        Bars.Cross.RTtext.SetRelativePos(-rlX + split, -rlY);

                        Bars.Cross.Left.GroupL.ChildVis(false).SetRelativePos();
                        Bars.Cross.Left.GroupR.ChildVis(true).SetRelativePos();
                        Bars.Cross.Right.GroupL.ChildVis(true).SetRelativePos();
                        Bars.Cross.Right.GroupR.ChildVis(false).SetRelativePos();
                        break;
                }
            }

            /// <summary>Sets the visibility of empty slots on the Cross Hotbar</summary>
            public static void UnassignedSlotVis(bool show)
            {
                var scale = show ? 1F : 0F;
                for (var i = 0; i < 4; i++)
                {
                    Bars.Cross.Left.GroupL.ChildNode(i, 1).SetScale(scale);
                    Bars.Cross.Left.GroupR.ChildNode(i, 1).SetScale(scale);
                    Bars.Cross.Right.GroupL.ChildNode(i, 1).SetScale(scale);
                    Bars.Cross.Right.GroupR.ChildNode(i, 1).SetScale(scale);
                    Bars.WXLL.GroupL.ChildNode(i, 1).SetScale(scale);
                    Bars.WXLL.GroupR.ChildNode(i, 1).SetScale(scale);
                    Bars.WXRR.GroupL.ChildNode(i, 1).SetScale(scale);
                    Bars.WXRR.GroupR.ChildNode(i, 1).SetScale(scale);
                }
            }

            /// <summary>Overrides HUD settings to force the Cross Hotbar to be horizontally centered</summary>
            public static void Recenter(float scale)
            {
                var baseX = (short)((ImGuiHelpers.MainViewport.Size.X - 588 * scale) / 2);
                if (Math.Abs(Bars.Cross.Base->X - baseX) < 0.9) return;

                PluginLog.LogDebug("Re-centering Cross Hotbar");
                Bars.Cross.Base->X = baseX;
                StoreCrossXPos();
            }

            /// <summary>Fix for misalignment after entering/exiting HUD Layout Interface</summary>
            public static void HudOffsetFix(int split, float scale)
            {
                var misalign = Bars.Cross.Base->X - Bars.Cross.Root.Node->X - Math.Round(split * scale);
                if (misalign >= 0) return;
              
                PluginLog.LogDebug($"HUD FIX: Misaligned by {misalign}");
                Bars.Cross.Base->X -= (short)misalign;
            }
        }
    }
}