using System;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    /// <summary>Checks/updates the Cross Hotbar selection and calls the main arrangement function</summary>
    public void UpdateBarState(bool forceArrange = false, bool hudFixCheck = false)
    {
        ArrangeCrossBar(Bars.Cross.Selection, Bars.Cross.LastKnownSelection, forceArrange, hudFixCheck);
        Bars.Cross.LastKnownSelection = Bars.Cross.Selection;
    }

    /// <summary>Arranges all elements of the Cross Hotbar based on current selection state</summary>
    private void ArrangeCrossBar(int select, int prevSelect = 0, bool forceArrange = false, bool hudFixCheck = true, bool resetAll = false)
    {
        //don't do anything if the cross hotbar isn't actually turned on
        if (!Bars.Cross.Enabled)
        {
            ResetHud();
            return;
        }

        if (Bars.Cross.Base == null || Bars.Cross.Base->UldManager.NodeListSize == 0) return;

        var scale = Bars.Cross.Root.Node->ScaleX;
        bool mixBar = CharConfig.MixBar;

        var split = resetAll ? 0 : Config.Split;

        // fix for misalignment after entering/exiting HUD Layout interface
        if (hudFixCheck && Bars.Cross.Base->X - Bars.Cross.Root.Node->X - Math.Round(split * scale) < 0)
        {
            PluginLog.LogDebug($"HUD FIX: {Bars.Cross.Base->X} - {Bars.Cross.Root.Node->X} - {Math.Round(split * scale)} = {Bars.Cross.Base->X - Bars.Cross.Root.Node->X - Math.Round(split * scale)}");
            Bars.Cross.Base->X += (short)(split * scale);
        }

        Bars.Cross.Root.SetPos(Bars.Cross.Base->X - split * scale, Bars.Cross.Base->Y).SetSize((ushort)(588 + split * 2),210);

        var anchorX = (int)(Bars.Cross.Root.Node->X + 146 * scale);
        var anchorY = (int)(Bars.Cross.Root.Node->Y + 70 * scale);

        // arrange the EXHB if that feature is turned on and two borrowed bars are selected
        var arrangeEx = ExBarsOk && !resetAll;
        if (arrangeEx) ArrangeExBars(select, prevSelect, scale, anchorX, anchorY, forceArrange);

        var lX = arrangeEx ? Config.lX : 0;
        var lY = arrangeEx ? Config.lY : 0;
        var rX = arrangeEx ? Config.OnlyOneEx ? Config.lX : Config.rX : 0;
        var rY = arrangeEx ? Config.OnlyOneEx ? Config.lY : Config.rY : 0;

        // vertical bar looks odd if certain CrossUp features are turned on, so hiding if necessary
        var hideDivider = split > 0 || Config.SepExBar && !resetAll;

        Bars.Cross.VertLine.SetSize(hideDivider ? 0 : null, hideDivider ? 0 : null);
        Bars.Cross.Padlock.SetRelativePos(Config.PadlockOffset.X + split, Config.PadlockOffset.Y);
        Bars.Cross.PadlockIcon.SetVis(!Config.HidePadlock);
        Bars.Cross.SetText.SetVis(!Config.HideSetText)
                          .SetRelativePos(Config.SetTextOffset.X + split, Config.SetTextOffset.Y);
        Bars.Cross.ChangeSet.SetRelativePos(Config.ChangeSetOffset.X + split, Config.ChangeSetOffset.Y);
        Bars.Cross.LTtext.SetScale(Config.HideTriggerText ? 0F : 1F);
        Bars.Cross.RTtext.SetScale(Config.HideTriggerText ? 0F : 1F);

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

    /// <summary>Reset all the node properties we've messed with and reset any borrowed bars</summary>
    public static void ResetHud()
    {
        if (!Bars.Cross.Exist) return;

        Bars.Cross.VertLine.SetSize();
        Bars.Cross.Padlock.SetRelativePos();
        Bars.Cross.PadlockIcon.SetVis(true);
        Bars.Cross.SetText.SetVis(true).SetRelativePos();
        Bars.Cross.ChangeSet.SetRelativePos();
        Bars.Cross.LTtext.SetScale();
        Bars.Cross.RTtext.SetScale();
        Bars.Cross.MiniSelectL.SetSize();
        Bars.Cross.MiniSelectR.SetSize();

        for (var barID = 1; barID <= 9; barID++)
        {
            // restore the bar's proper saved actions, including those affected by drag/drop
            Actions.Copy(Actions.GetSaved(CharConfig.Hotbar.Shared[barID] ? 0 : PlayerJob, barID), 0, barID, 0, 12);
            ResetBarPos(barID);
        }
    }

    /// <summary>Put a borrowed hotbar back the way we found it based on HUD layout settings</summary>
    private static void ResetBarPos(int barID)
    {
        var bar = Bars.ActionBars[barID];
        if (!bar.Exist) return;

        bar.Root.SetPos(bar.Base->X, bar.Base->Y)
                .SetSize()
                .SetScale(bar.Base->Scale);

        bar.BarNumText.SetScale();

        for (var i = 0; i < 12; i++) bar.Button[i].SetRelativePos()
                                                  .SetVis(true)
                                                  .SetScale();

        if (CharConfig.Hotbar.Visible[barID] && Bars.WasHidden[barID] && ((barID != Bars.LR.BorrowID && barID != Bars.RL.BorrowID) || Bars.LR.BorrowID < 1 || Bars.RL.BorrowID < 1))
        {
            CharConfig.Hotbar.Visible[barID].Set(0);
        }

        foreach (var button in bar.Button) button.ChildNode(1).SetVis(true);
    }
}

