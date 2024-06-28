using CrossUp.Game.Hotbar;
using Dalamud.Interface.Utility;
using System;
using FFXIVClientStructs.FFXIV.Client.UI;
using static CrossUp.CrossUp;
using static CrossUp.Utility.Service;
// ReSharper disable RedundantEnumCaseLabelForDefaultSection

namespace CrossUp.Features.Layout
{
    /// <summary>Methods for rearranging the main Cross Hotbar</summary>
    internal static class CrossLayout
    {
        /// <summary>Arranges all elements of the main Cross Hotbar based on current selection status and other factors</summary>
        public static void Arrange(ActionCrossSelect select, ActionCrossSelect previous, float scale, int split, bool mixBar, bool arrangeEx, (int, int, int, int) coords, bool forceArrange, bool resetAll)
        {
            if (Profile.SplitOn) Recenter(scale);

            Bars.Cross.Root.SetPos(Bars.Cross.Base.X - split * scale, Bars.Cross.Base.Y)
                           .SetSize((ushort)(float)(588 + split * 2), 210);

            ushort? lineSize = split != 0 || SeparateEx.Ready && !resetAll ? 0 : null;
            Bars.Cross.VertLine.SetSize(lineSize, lineSize);
            Bars.Cross.Padlock.SetRelativePos(Profile.PadlockOffset.X + split, Profile.PadlockOffset.Y);
            Bars.Cross.Padlock[2u].SetVis(!Profile.HidePadlock);
            Bars.Cross.SetDisplay.SetVis(!Profile.HideSetText)
                                 .SetRelativePos(Profile.SetTextOffset.X + split, Profile.SetTextOffset.Y);
            Bars.Cross.ChangeSetDisplay.ChangeSetContainer.SetRelativePos(Profile.ChangeSetOffset.X + split, Profile.ChangeSetOffset.Y);
            Bars.Cross.LTtext.SetScale(Profile.HideTriggerText ? 0F : 1F);
            Bars.Cross.RTtext.SetScale(Profile.HideTriggerText ? 0F : 1F);

            UnassignedSlotVis(resetAll || !Profile.HideUnassigned);

            if (!forceArrange && select == previous) return;

            var (lrX, lrY, rlX, rlY) = coords;
            var miniSize = (ushort)(Profile.SelectStyle == 2 || (mixBar && split > 0) ? 0 : 166);
            switch (select)
            {
                case ActionCrossSelect.None:
                case ActionCrossSelect.DoubleCrossLeft:
                case ActionCrossSelect.DoubleCrossRight:
                default:
                    Bars.Cross.Container.SetRelativePos();
                    Bars.Cross.LTtext.SetRelativePos();
                    Bars.Cross.RTtext.SetRelativePos(split * 2);

                    Bars.Cross.Buttons[0].ChildVis(true).SetRelativePos();
                    Bars.Cross.Buttons[1].ChildVis(true).SetRelativePos();
                    Bars.Cross.Buttons[2].ChildVis(true).SetRelativePos(split * 2);
                    Bars.Cross.Buttons[3].ChildVis(true).SetRelativePos(split * 2);
                    break;
                case ActionCrossSelect.Left:
                    Bars.Cross.Container.SetRelativePos();
                    Bars.Cross.LTtext.SetRelativePos();
                    Bars.Cross.RTtext.SetRelativePos(split * 2);

                    var fromLR = previous == ActionCrossSelect.LR && arrangeEx;
                    Bars.Cross.Buttons[0].ChildVis(true).SetRelativePos();
                    Bars.Cross.Buttons[1].ChildVis(!fromLR || !mixBar).SetRelativePos();
                    Bars.Cross.Buttons[2].ChildVis(!fromLR || mixBar).SetRelativePos(split * 2);
                    Bars.Cross.Buttons[3].ChildVis(!fromLR).SetRelativePos(split * 2);

                    Bars.Cross.MiniSelectL.SetSize(miniSize, 140);
                    Bars.Cross.MiniSelectR.SetSize(miniSize, 140);
                    break;

                case ActionCrossSelect.Right:
                    Bars.Cross.Container.SetRelativePos(split * 2);
                    Bars.Cross.LTtext.SetRelativePos(-split * 2);
                    Bars.Cross.RTtext.SetRelativePos();

                    var fromRL = previous == ActionCrossSelect.RL && arrangeEx;
                    Bars.Cross.Buttons[0].ChildVis(!fromRL).SetRelativePos(-split * 2);
                    Bars.Cross.Buttons[1].ChildVis(!fromRL || mixBar).SetRelativePos(-split * 2);
                    Bars.Cross.Buttons[2].ChildVis(!fromRL || !mixBar).SetRelativePos();
                    Bars.Cross.Buttons[3].ChildVis(true).SetRelativePos();

                    Bars.Cross.MiniSelectL.SetSize(miniSize, 140);
                    Bars.Cross.MiniSelectR.SetSize(miniSize, 140);
                    break;

                case ActionCrossSelect.LR:
                    Bars.Cross.Container.SetRelativePos(lrX + split, lrY);
                    Bars.Cross.LTtext.SetRelativePos(-lrX - split, -lrY);
                    Bars.Cross.RTtext.SetRelativePos(-lrX + split, -lrY);

                    Bars.Cross.Buttons[0].ChildVis(false).SetRelativePos();
                    Bars.Cross.Buttons[1].ChildVis(true).SetRelativePos();
                    Bars.Cross.Buttons[2].ChildVis(true).SetRelativePos();
                    Bars.Cross.Buttons[3].ChildVis(false).SetRelativePos();
                    break;

                case ActionCrossSelect.RL:
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
                for (uint bID = 2; bID <= 5; bID++)
                {
                    Bars.Cross.Buttons[set][bID][3u].SetScale(scale);
                    Bars.WXHB.Buttons[set][bID][3u].SetScale(scale);
                }
        }

        /// <summary>Overrides HUD settings to horizontally recenter Cross Hotbar at user-selected position</summary>
        private static void Recenter(float scale)
        {
            var baseX = (short)((ImGuiHelpers.MainViewport.Size.X - 588 * scale) / 2 + Profile.CenterPoint);
            var misalign = Bars.Cross.Base.X - baseX;
            if (Math.Abs(misalign) < 1) return;

            Bars.Cross.Base.X = baseX;
            Log.Verbose($"Realigning Cross Hotbar to Center Point {Profile.CenterPoint} (was off by {misalign})");
        }

        /// <summary>Restores everything back to default</summary>
        public static void Reset()
        {
            Bars.Cross.VertLine.SetSize();
            Bars.Cross.Padlock.SetRelativePos();
            Bars.Cross.Padlock[2u].SetVis(true);
            Bars.Cross.SetDisplay.SetVis(true).SetRelativePos();
            Bars.Cross.ChangeSetDisplay.ChangeSetContainer.SetRelativePos();
            Bars.Cross.LTtext.SetScale();
            Bars.Cross.RTtext.SetScale();
            Bars.Cross.MiniSelectL.SetSize();
            Bars.Cross.MiniSelectR.SetSize();
        }
    }
}