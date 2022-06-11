using System;
using Dalamud.Logging;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    private delegate byte ActionBarReceiveEventDel(AddonActionBarBase* barBase, uint eventID, void* a3, void* a4, NumberArrayData** numberArrayData);
    private HookWrapper<ActionBarReceiveEventDel>? ActionBarReceiveEventHook;

    private delegate byte ActionBarBaseUpdateDel(AddonActionBarBase* barBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData);
    private HookWrapper<ActionBarBaseUpdateDel>? ActionBarBaseUpdateHook;

    private delegate uint SetHudLayoutDel(IntPtr filePtr, uint hudLayout, byte unk0, byte unk1);
    private HookWrapper<SetHudLayoutDel>? SetHudLayoutHook;

    private readonly AgentHudLayout* hudLayout = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentHudLayout();

    /// <summary>Sets up all CrossUp's hooks</summary>
    private void EnableHooks()
    {
        Service.Framework.Update += FrameworkUpdate;

        ActionBarBaseUpdateHook ??= Common.Hook<ActionBarBaseUpdateDel>("E8 ?? ?? ?? ?? 83 BB ?? ?? ?? ?? ?? 75 09", ActionBarBaseUpdateDetour);
        ActionBarBaseUpdateHook?.Enable();

        ActionBarReceiveEventHook ??= Common.Hook<ActionBarReceiveEventDel>("E8 ?? ?? ?? FF 66 83 FB ?? ?? ?? ?? BF 3F", ActionBarReceiveEventDetour);
        ActionBarReceiveEventHook?.Enable();

        SetHudLayoutHook ??= Common.Hook<SetHudLayoutDel>("E8 ?? ?? ?? ?? 33 C0 EB 15", SetHudLayoutDetour);
        SetHudLayoutHook?.Enable();
    }

    /// <summary>Removes all CrossUp's hooks</summary>
    private void DisableHooks()
    {
        Service.Framework.Update -= FrameworkUpdate;
        ActionBarBaseUpdateHook?.Disable();
        ActionBarReceiveEventHook?.Disable();
        SetHudLayoutHook?.Disable();
    }
    
    /// <summary>Runs every frame. Checks if conditions are right to initialize the plugin, or (once initialized) if it needs to be disabled again. Also calls animation tween function when needed.</summary>
    private void FrameworkUpdate(Framework framework)
    {
        try
        {
            if (IsSetUp)
            {
                if (Bars.Cross.Exists)
                {
                    // animate button sizes if needed
                    if (Layout.SeparateEx.MetaSlots.TweensExist) Layout.SeparateEx.MetaSlots.TweenAll();

                    // if HUD layout editor is open, perform this fix once:
                    DoneHudCheck = hudLayout->AgentInterface.IsAgentActive() && (DoneHudCheck || AdjustHudEditorNode());
                }
                else
                {
                    PluginLog.LogDebug("Cross Hotbar nodes not found; disabling plugin features");
                    IsSetUp = false;
                }
            }
            else if (Bars.AllExist && Job.Current != 0)
            {
                PluginLog.LogDebug("Cross Hotbar nodes found; setting up plugin features");
                Setup();
            }
        } catch (Exception ex) { PluginLog.LogError($"Exception: Framework Update Failed!\n{ex}"); }
    }

    /// <summary>ActionBarReceiveEventDetour() will set this to true if it detects a drag/drop change, signalling the next ActionBarBaseUpdate() to handle it.</summary>
    private bool DragDrop;

    /// <summary>Called when mouse events occur on hotbars. Most importantly checking for eventTypes 50/54, which are drag/drop events.</summary>
    private byte ActionBarReceiveEventDetour(AddonActionBarBase* barBase, uint eventType, void* a3, void* a4, NumberArrayData** numberArrayData)
    {
        try
        {
            switch (eventType)
            {
                case 50 or 54 when Layout.SeparateEx.Ready:
                {
                    var barID = barBase->HotbarID;
                    PluginLog.LogDebug($"Drag/Drop Event on Bar #{barID} ({(barID > 9 ? $"Cross Hotbar Set {barID - 9}" : $"Hotbar {barID + 1}")}); Handling on next ActionBarBase Update");
                    Layout.Cross.UnassignedSlotVis(!Config.HideUnassigned);
                    DragDrop = true;
                    break;
                }
                case 47 when IsSetUp:
                    Layout.Cross.UnassignedSlotVis(true);
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
        if (!IsSetUp) return ret;

        try
        {
            if (DragDrop) HandleDragDrop();
            if (Job.HasChanged) HandleJobChange();

            if (barBase->HotbarID == 1) Layout.Update(Bars.Cross.EnableStateChanged, true);
            else if (barBase->HotbarSlotCount == 16 && Bars.Cross.SetChanged(barBase)) HandleSetChange(barBase);
        }
        catch (Exception ex) { PluginLog.LogError($"Exception: ActionBarBaseUpdateDetour Failed!\n{ex}"); }

        return ret;
    }

    /// <summary>Takes any changes made to the borrowed Expanded Hold Bars, and writes them to the corresponding Cross Hotbar set instead</summary>
    private void HandleDragDrop()
    {
        PluginLog.LogDebug("Handling Drag/Drop Event");
        DragDrop = false;

        if (!Layout.SeparateEx.Ready || (int)Bars.Cross.Selection.Current > 2) return;

        var lr = (id: Bars.LR.BorrowBar.BarID, target: Actions.LRMap, actions: Bars.LR.BorrowBar.Actions);
        var rl = (id: Bars.RL.BorrowBar.BarID, target: Actions.RLMap, actions: Bars.RL.BorrowBar.Actions);

        var shared = CharConfig.Hotbar.Shared;
        var stored = Bars.StoredActions;
        var job = Job.Current;

        Actions.Copy(lr.actions, 0, lr.target.barID, lr.target.useLeft ? 0 : 8, 8);
        Actions.Save(lr.actions, 0, lr.target.barID, lr.target.useLeft ? 0 : 8, 8, shared[lr.target.barID] ? 0 : job);
        if (stored[lr.id] != null && stored[lr.id]!.Length != 0) Actions.Save(stored[lr.id]!, 0, lr.id, 0, 12, shared[lr.id] ? 0 : job);

        Actions.Copy(rl.actions, 0, rl.target.barID, rl.target.useLeft ? 0 : 8, 8);
        Actions.Save(rl.actions, 0, rl.target.barID, rl.target.useLeft ? 0 : 8, 8, shared[rl.target.barID] ? 0 : job);
        if (stored[rl.id] != null && stored[rl.id]!.Length != 0) Actions.Save(stored[rl.id]!, 0, rl.id, 0, 12, shared[rl.id] ? 0 : job);
    }

    /// <summary>Updates the stored bars when the player changes jobs</summary>
    private static void HandleJobChange()
    {
        PluginLog.LogDebug("Job Change: " + Job.Abbr);
        if (!Layout.SeparateEx.Ready) return;
        
        Actions.Store(Config.LRborrow);
        Actions.Store(Config.RLborrow);
    }

    /// <summary>Responds to the player changing the Cross Hotbar set</summary>
    private static void HandleSetChange(AddonActionBarBase* barBase)
    {
        PluginLog.LogDebug($"Switched to Cross Hotbar Set {barBase->HotbarID - 9}");
        if (Config.RemapEx || Config.RemapW) Remap.Override(barBase->HotbarID);
    }

    /// <summary>Responds to the HUD layout being changed/set/saved</summary>
    private uint SetHudLayoutDetour(IntPtr filePtr, uint layout, byte unk0, byte unk1)
    {
        if (IsSetUp) Layout.ScheduleNudges(2, 10, false);
        return SetHudLayoutHook!.Original(filePtr, layout, unk0, unk1);
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

        for (var i = 1; i < hudScreen->UldManager.NodeListCount; i++)
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