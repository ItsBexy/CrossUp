using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dalamud.Logging;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    /// <summary>Methods regarding the overall layout of the Cross Hotbar</summary>
    public static class Layout
    {
        /// <summary>Checks/updates the Cross Hotbar selection and calls the main arrangement function</summary>
        public static void Update(bool forceArrange = false, bool hudFixCheck = false)
        {
            Arrange(Bars.Cross.Selection, Bars.Cross.LastKnownSelection, forceArrange, hudFixCheck);
            Bars.Cross.LastKnownSelection = Bars.Cross.Selection;
        }

        /// <summary>Arranges all elements of the Cross Hotbar based on current selection state</summary>
        public static void Arrange(int select, int prevSelect = 0, bool forceArrange = false, bool hudFixCheck = true, bool resetAll = false)
        {
            if (!Bars.Cross.Enabled)
            {
                Reset();
                return;
            }

            if (Bars.Cross.Base == null || Bars.Cross.Base->UldManager.NodeListSize == 0) return;

            var scale = Bars.Cross.Root.Node->ScaleX;
            var split = resetAll ? 0 : Config.Split;
            bool mixBar = CharConfig.MixBar;

            if (hudFixCheck) HudOffsetFix(split, scale);

            Bars.Cross.Root.SetPos(Bars.Cross.Base->X - split * scale, Bars.Cross.Base->Y)
                .SetSize((ushort)(588 + split * 2),210);

            var anchorX = (int)(Bars.Cross.Root.Node->X + 146 * scale);
            var anchorY = (int)(Bars.Cross.Root.Node->Y + 70 * scale);

            // arrange the EXHB if that feature is turned on and two borrowed bars are selected
            var arrangeEx = SeparateEx.Ready && !resetAll;
            if (arrangeEx) SeparateEx.Arrange(select, prevSelect, scale, anchorX, anchorY, forceArrange);

            var lX = arrangeEx ? Config.lX : 0;
            var lY = arrangeEx ? Config.lY : 0;
            var rX = arrangeEx ? Config.OnlyOneEx ? Config.lX : Config.rX : 0;
            var rY = arrangeEx ? Config.OnlyOneEx ? Config.lY : Config.rY : 0;

            // vertical bar looks odd if certain CrossUp features are turned on, so hiding if necessary
            var hideDivider = split > 0 || Config.SepExBar && !resetAll;

            Bars.Cross.VertLine.SetSize(hideDivider ? 0 : null, hideDivider ? 0 : null);
            Bars.Cross.Padlock.SetRelativePos(Config.PadlockOffset.X + split, Config.PadlockOffset.Y);
            Bars.Cross.PadlockIcon.SetVis(!Config.HidePadlock);
            Bars.Cross.SetDisplay.SetVis(!Config.HideSetText)
                              .SetRelativePos(Config.SetTextOffset.X + split, Config.SetTextOffset.Y);
            Bars.Cross.ChangeSet.SetRelativePos(Config.ChangeSetOffset.X + split, Config.ChangeSetOffset.Y);
            Bars.Cross.LTtext.SetScale(Config.HideTriggerText ? 0F : 1F);
            Bars.Cross.RTtext.SetScale(Config.HideTriggerText ? 0F : 1F);

            UnassignedCrossSlotVis(resetAll || !Config.HideUnassigned);

            if (!forceArrange && select == prevSelect) return;
            switch (select)
            {
                case 0: // NONE
                case 5: // LEFT WXHB
                case 6: // RIGHT WXHB
                    Bars.Cross.Component.SetRelativePos();
                    Bars.Cross.LTtext.SetRelativePos();
                    Bars.Cross.RTtext.SetRelativePos(split * 2);

                    Bars.Cross.Left.GroupL.ChildVis(true).SetRelativePos();
                    Bars.Cross.Left.GroupR.ChildVis(true).SetRelativePos();
                    Bars.Cross.Right.GroupL.ChildVis(true).SetRelativePos(split * 2);
                    Bars.Cross.Right.GroupR.ChildVis(true).SetRelativePos(split * 2);
                    break;
                case 1: //LEFT BAR
                    Bars.Cross.Component.SetRelativePos();
                    Bars.Cross.LTtext.SetRelativePos();
                    Bars.Cross.RTtext.SetRelativePos(split * 2);

                    Bars.Cross.Left.GroupL.ChildVis(true).SetRelativePos();
                    Bars.Cross.Left.GroupR.ChildVis(!(prevSelect == 3 && arrangeEx && mixBar)).SetRelativePos();
                    Bars.Cross.Right.GroupL.ChildVis(!(prevSelect == 3 && arrangeEx && !mixBar)).SetRelativePos(split * 2);
                    Bars.Cross.Right.GroupR.ChildVis(!(prevSelect == 3 && arrangeEx)).SetRelativePos(split * 2);

                    Bars.Cross.MiniSelectL.SetSize((ushort)(Config.selectHide || (mixBar && split > 0) ? 0 : 166), 140);
                    Bars.Cross.MiniSelectR.SetSize((ushort)(Config.selectHide || (mixBar && split > 0) ? 0 : 166), 140);
                    break;
                case 2: // RIGHT BAR
                    Bars.Cross.Component.SetRelativePos(split * 2);
                    Bars.Cross.LTtext.SetRelativePos(-split * 2);
                    Bars.Cross.RTtext.SetRelativePos();

                    Bars.Cross.Left.GroupL.ChildVis(!(prevSelect == 4 && arrangeEx)).SetRelativePos(-split * 2);
                    Bars.Cross.Left.GroupR.ChildVis(!(prevSelect == 4 && arrangeEx && !mixBar)).SetRelativePos(-split * 2);
                    Bars.Cross.Right.GroupL.ChildVis(!(prevSelect == 4 && arrangeEx && mixBar)).SetRelativePos();
                    Bars.Cross.Right.GroupR.ChildVis(true).SetRelativePos();

                    Bars.Cross.MiniSelectL.SetSize((ushort)(Config.selectHide || (mixBar && split > 0) ? 0 : 166), 140);
                    Bars.Cross.MiniSelectR.SetSize((ushort)(Config.selectHide || (mixBar && split > 0) ? 0 : 166), 140);
                    break;
                case 3: // L->R BAR
                    Bars.Cross.Component.SetRelativePos(lX + split, lY);
                    Bars.Cross.LTtext.SetRelativePos(-lX - split, -lY);
                    Bars.Cross.RTtext.SetRelativePos(-lX + split, -lY);

                    Bars.Cross.Left.GroupL.ChildVis(false).SetRelativePos();
                    Bars.Cross.Left.GroupR.ChildVis(true).SetRelativePos();
                    Bars.Cross.Right.GroupL.ChildVis(true).SetRelativePos();
                    Bars.Cross.Right.GroupR.ChildVis(false).SetRelativePos();
                    break;
                case 4: // R->L BAR
                    Bars.Cross.Component.SetRelativePos(rX + split, rY);
                    Bars.Cross.LTtext.SetRelativePos(-rX - split, -rY);
                    Bars.Cross.RTtext.SetRelativePos(-rX + split, -rY);

                    Bars.Cross.Left.GroupL.ChildVis(false).SetRelativePos();
                    Bars.Cross.Left.GroupR.ChildVis(true).SetRelativePos();
                    Bars.Cross.Right.GroupL.ChildVis(true).SetRelativePos();
                    Bars.Cross.Right.GroupR.ChildVis(false).SetRelativePos();
                    break;
            }
        }

        /// <summary>Sets the visibility of empty slots on the Cross Hotbar</summary>
        public static void UnassignedCrossSlotVis(bool show)
        {
            if (!Initialized || !Bars.Cross.Exist) return;
            var scale = show ? 1F : 0F;
            try
            {
                for (var i = 0; i < 4; i++)
                {
                    Bars.Cross.Left.GroupL.ChildNode(i,1).SetScale(scale);
                    Bars.Cross.Left.GroupR.ChildNode(i,1).SetScale(scale);
                    Bars.Cross.Right.GroupL.ChildNode(i,1).SetScale(scale);
                    Bars.Cross.Right.GroupR.ChildNode(i,1).SetScale(scale);
                    Bars.WXLL.GroupL.ChildNode(i,1).SetScale(scale);
                    Bars.WXLL.GroupR.ChildNode(i,1).SetScale(scale);
                    Bars.WXRR.GroupL.ChildNode(i,1).SetScale(scale);
                    Bars.WXRR.GroupR.ChildNode(i,1).SetScale(scale);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Log(Initialized+" "+Bars.Cross.Exist);
                PluginLog.LogError(ex+"");
            }
        }

        /// <summary>Run the UpdateBarState() function a few times on login (hopefully ensures bars look right by the time user sees them)</summary>
        public static void ScheduleNudges(int c=5)
        {
            for (var i = 1; i <= c; i++)
            {
                var n = i;
                Task.Delay(500 * i).ContinueWith(delegate
                {
                    if (!Initialized) return;
                    PluginLog.LogDebug($"Nudging Nodes Post-Login {n}/{c}");

                    if (Config.DisposeBaseX != null && Config.DisposeRootX != null) RestoreDisposalXPos();
                    Update(true);
                });
            }
        }

        /// <summary>Fix for misalignment after entering/exiting HUD Layout Interface</summary>
        private static void HudOffsetFix(int split, float scale)
        {
            if (Bars.Cross.Base->X - Bars.Cross.Root.Node->X - Math.Round(split * scale) >= 0) return;

            PluginLog.LogDebug($"HUD FIX: {Bars.Cross.Base->X} - {Bars.Cross.Root.Node->X} - {Math.Round(split * scale)} = {Bars.Cross.Base->X - Bars.Cross.Root.Node->X - Math.Round(split * scale)}");
            Bars.Cross.Base->X += (short)(split * scale);
        }

        /// <summary>Reset all the node properties we've messed with and reset any borrowed bars</summary>
        public static void Reset()
        {
            if (!Bars.Cross.Exist) return;

            Bars.Cross.VertLine.SetSize();
            Bars.Cross.Padlock.SetRelativePos();
            Bars.Cross.PadlockIcon.SetVis(true);
            Bars.Cross.SetDisplay.SetVis(true).SetRelativePos();
            Bars.Cross.ChangeSet.SetRelativePos();
            Bars.Cross.LTtext.SetScale();
            Bars.Cross.RTtext.SetScale();
            Bars.Cross.MiniSelectL.SetSize();
            Bars.Cross.MiniSelectR.SetSize();

            for (var barID = 1; barID <= 9; barID++)
            {
                Actions.Copy(Actions.GetSaved(CharConfig.Hotbar.Shared[barID] ? 0 : Job.Current, barID), 0, barID, 0, 12);
                ResetBar(barID);
            }
        }

        /// <summary>Put a borrowed hotbar back the way we found it based on HUD layout settings</summary>
        private static void ResetBar(int barID)
        {
            if (!Bars.ActionBars[barID].Exist) return;
            var bar = Bars.ActionBars[barID];

            bar.Root.SetPos(bar.Base->X, bar.Base->Y)
                    .SetSize()
                    .SetScale(bar.Base->Scale);

            bar.BarNumText.SetScale();

            for (var i = 0; i < 12; i++) bar.Button[i].SetRelativePos()
                                                      .SetVis(true)
                                                      .SetScale();

            if (CharConfig.Hotbar.Visible[barID] && Bars.WasHidden[barID] && ((barID != Bars.LR.BorrowID && barID != Bars.RL.BorrowID) || !SeparateEx.Ready))
            {
                CharConfig.Hotbar.Visible[barID].Set(0);
            }

            foreach (var button in bar.Button) button.ChildNode(1).SetVis(true);
        }
    }
}

