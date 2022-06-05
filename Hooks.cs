﻿using System;
using Dalamud.Logging;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    private delegate byte ActionBarReceiveEvent(AddonActionBarBase* barBase, uint eventID, void* a3, void* a4, NumberArrayData** numberArrayData);
    private readonly HookWrapper<ActionBarReceiveEvent>? ActionBarReceiveEventHook;

    private delegate byte ActionBarBaseUpdate(AddonActionBarBase* barBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData);
    private readonly HookWrapper<ActionBarBaseUpdate>? ActionBarBaseUpdateHook;

    private readonly AgentHudLayout* hudLayout = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentHudLayout();

    /// <summary>Runs every frame. Checks if conditions are right to initialize the plugin, or (once initialized) if it needs to be disabled again. Also calls animation tween function when needed.</summary>
    private void FrameworkUpdate(Framework framework)
    {
        try {
            if (Initialized)
            {
                if (Bars.Cross.Exist)
                {
                    // animate button sizes if needed
                    if (SeparateEx.MetaSlots.TweensExist) SeparateEx.MetaSlots.TweenAll();

                    // if HUD layout editor is open, perform this fix once:
                    DoneHudCheck = hudLayout->AgentInterface.IsAgentActive() && (DoneHudCheck || AdjustHudEditorNode());
                }
                else
                {
                    PluginLog.LogDebug("Cross Hotbar nodes not found; disabling plugin features");
                    Initialized = false;
                }
            }
            else if (Bars.Cross.Exist && Job.Current != 0)
            {
                PluginLog.LogDebug("Cross Hotbar nodes found; setting up plugin features");
                Initialize();
            }
        } catch (Exception ex) { PluginLog.LogError($"Exception: Framework Update Failed!\n{ex}"); }
    }

    /// <summary>ActionBarReceiveEventDetour() will set this to true if it detects a drag/drop change, signalling the next ActionBarBaseUpdate() to handle it.</summary>
    private bool DragDrop;
    /// <summary>Called when mouse events occur on hotbars. Specifically checking for eventTypes 50/54, which are drag/drop events.</summary>
    private byte ActionBarReceiveEventDetour(AddonActionBarBase* barBase, uint eventType, void* a3, void* a4, NumberArrayData** numberArrayData)
    {
        try
        {
            switch (eventType)
            {
                case 50 or 54 when SeparateEx.Ready:
                {
                    var barID = barBase->HotbarID;
                    PluginLog.LogDebug($"Drag/Drop Event on Bar #{barID} ({(barID > 9 ? $"Cross Hotbar Set {barID - 9}" : $"Hotbar {barID + 1}")}); Handling on next ActionBarBase Update");
                    Layout.UnassignedCrossSlotVis(!Config.HideUnassigned);
                    DragDrop = true;
                    break;
                }
                case 47:
                    Layout.UnassignedCrossSlotVis(true);
                    break;
            }
        }
        catch (Exception ex) { PluginLog.LogError($"Exception: ActionBarReceiveEventDetour Failed!\n{ex}"); }

        return ActionBarReceiveEventHook!.Original(barBase, eventType, a3, a4, numberArrayData);
    }

    /// <summary>Called whenever a change occurs on the Cross Hotbar. Calls the plugin's main arrangement functions.</summary>
    private byte ActionBarBaseUpdateDetour(AddonActionBarBase* barBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData)
    {
        var ret = ActionBarBaseUpdateHook!.Original(barBase, numberArrayData, stringArrayData);
        if (!Initialized) return ret;

        try
        {
            if (DragDrop) HandleDragDrop();
            if (Job.HasChanged) HandleJobChange();

            if (barBase->HotbarID == 1) // Hotbar events will always call updates for every bar, but we're piggybacking off of bar 1 because of its consistency and its place in the order
            {
                Layout.Update(Bars.Cross.EnableStateChanged, true);
            }
            else if (barBase->HotbarSlotCount == 16 && barBase->HotbarID != Bars.Cross.LastKnownSetID)  // Cross Hotbar set has changed
            {
                PluginLog.LogDebug($"Switched to Cross Hotbar Set {barBase->HotbarID - 9}");
                if (Config.RemapEx || Config.RemapW) Remap.Override(barBase->HotbarID);
                Bars.Cross.LastKnownSetID = barBase->HotbarID;
            }
        }
        catch (Exception ex) { PluginLog.LogError($"Exception: ActionBarBaseUpdateDetour Failed!\n{ex}"); }

        return ret;
    }

    /// <summary>Updates the stored bars when the player changes jobs</summary>
    private static void HandleJobChange()
    {
        PluginLog.LogDebug("Job Change: " + Job.Abbr);
        if (!SeparateEx.Ready) return;
        
        Actions.Store(Config.borrowBarL);
        Actions.Store(Config.borrowBarR);
    }

    /// <summary>Takes any changes made to the borrowed Expanded Hold Bars, and writes them to the corresponding Cross Hotbar set instead</summary>
    private void HandleDragDrop()
    {
        PluginLog.LogDebug("Handling Drag/Drop Event");
        DragDrop = false;

        if (Bars.Cross.Selection > 2 || !SeparateEx.Ready) return;

        var lrID = Bars.LR.BorrowBar.BarID;
        var rlID = Bars.RL.BorrowBar.BarID;
        var lrMap = Actions.LRMap;
        var rlMap = Actions.RLMap;
        var shared = CharConfig.Hotbar.Shared;
        var stored = Bars.StoredActions;
        var job = Job.Current;

        Actions.Copy(Bars.LR.BorrowBar.Actions, 0, lrMap.BarID, lrMap.UseLeft ? 0 : 8, 8);
        Actions.Save(Bars.LR.BorrowBar.Actions, 0, lrMap.BarID, lrMap.UseLeft ? 0 : 8, 8, shared[lrMap.BarID] ? 0 : job);
        if (stored[lrID] != null && stored[lrID]!.Length != 0) Actions.Save(stored[lrID]!, 0, lrID, 0, 12, shared[lrID] ? 0 : job);

        Actions.Copy(Bars.RL.BorrowBar.Actions, 0, rlMap.BarID, rlMap.UseLeft ? 0 : 8, 8);
        Actions.Save(Bars.RL.BorrowBar.Actions, 0, rlMap.BarID, rlMap.UseLeft ? 0 : 8, 8, shared[rlMap.BarID] ? 0 : job);
        if (stored[rlID] != null && stored[rlID]!.Length != 0) Actions.Save(stored[rlID]!, 0, rlID, 0, 12, shared[rlID] ? 0 : job);
    }

    /// <summary>Whether the Hud Interface check has already run and completed</summary>
    private static bool DoneHudCheck;
    /// <summary>Fix for misaligned frame around XHB when using the HUD Layout Interface</summary>
    private static bool AdjustHudEditorNode()
    {
        var hudScreen = (AtkUnitBase*)Service.GameGui.GetAddonByName("_HudLayoutScreen", 1);
        var root = Bars.Cross.Root.Node;
        if (hudScreen == null || root == null) return false;

        var scale = root->ScaleX;
        var hudNodes = hudScreen->UldManager.NodeList;

        for (var i = 0; i < hudScreen->UldManager.NodeListCount; i++)
        {
            if (!hudNodes[i]->IsVisible || Math.Abs(hudNodes[i]->Y - root->Y) > 1 ||
                Math.Abs(hudNodes[i]->Width - root->Width * scale) > 1 ||
                Math.Abs(hudNodes[i]->Height - root->Height * scale) > 1 ||
                Math.Abs(hudNodes[i]->X - root->X) < 1) continue;
            hudNodes[i]->X = root->X;
            hudNodes[i]->Flags_2 |= 0xD;
            return true;
        }
        return false;
    }

}