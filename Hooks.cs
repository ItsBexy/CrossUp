using System;
using Dalamud.Logging;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using NodeTools;
using System.Runtime.InteropServices;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    private delegate byte ActionBarReceiveEventDel(AddonActionBarBase* barBase, uint eventID, void* a3, void* a4, NumberArrayData** numberArrayData);
    private Hook<ActionBarReceiveEventDel>? ActionBarReceiveEventHook;

    private delegate byte ActionBarBaseUpdateDel(AddonActionBarBase* barBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData);
    private Hook<ActionBarBaseUpdateDel>? ActionBarBaseUpdateHook;

    private delegate uint SetHudLayoutDel(IntPtr filePtr, uint hudLayout, byte unk0, byte unk1);
    private Hook<SetHudLayoutDel>? SetHudLayoutHook;

    private delegate IntPtr GetFilePointerDelegate(byte index);
    private static GetFilePointerDelegate? GetFilePointer;

    private readonly AgentHudLayout* hudLayout = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentHudLayout();

    /// <summary>Sets up all of CrossUp's hooks</summary>
    private void EnableHooks()
    {
        Service.Framework.Update += FrameworkUpdate;
        Service.Condition.ConditionChange += OnConditionChange;

        ActionBarReceiveEventHook ??= Hook<ActionBarReceiveEventDel>.FromAddress(Service.SigScanner.ScanText("E8 ?? ?? ?? FF 66 83 FB ?? ?? ?? ?? BF 3F"), ActionBarReceiveEventDetour); ActionBarReceiveEventHook?.Enable();

        ActionBarBaseUpdateHook ??= Hook<ActionBarBaseUpdateDel>.FromAddress(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 83 BB ?? ?? ?? ?? ?? 75 09"), ActionBarBaseUpdateDetour); ActionBarBaseUpdateHook?.Enable();

        SetHudLayoutHook ??= Hook<SetHudLayoutDel>.FromAddress(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 33 C0 EB 15"), SetHudLayoutDetour);
        SetHudLayoutHook?.Enable();

        var getFilePointerPtr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 85 C0 74 14 83 7B 44 00");
        if (getFilePointerPtr != IntPtr.Zero) { GetFilePointer = Marshal.GetDelegateForFunctionPointer<GetFilePointerDelegate>(getFilePointerPtr); }
    }

    private const int SlotOffset = 0x6378;
    internal static int HudSlot;

    private static int GetHudSlot()
    {
        var filePtr = GetFilePointer?.Invoke(0) ?? IntPtr.Zero;
        var dataPtr = filePtr + 0x50;
        return (int)*(uint*)(Marshal.ReadIntPtr(dataPtr) + SlotOffset) + 1;
    }

    /// <summary>Removes all of CrossUp's hooks</summary>
    private void DisableHooks()
    {
        Service.Framework.Update -= FrameworkUpdate;
        Service.Condition.ConditionChange -= OnConditionChange;
        ActionBarReceiveEventHook?.Disable();
        ActionBarBaseUpdateHook?.Disable();
        SetHudLayoutHook?.Disable();
    }

    private bool LogFrameCatch = true;



    /// <summary>Runs every frame. Checks if conditions are right to initialize the plugin, or (once initialized) if it needs to be disabled again.<br/><br/>
    /// Also calls animation function (<see cref="Layout.SeparateEx.MetaSlots.TweenAll"/>) when relevant.</summary>
    private void FrameworkUpdate(Framework framework)
    {
        try
        {
            if (IsSetUp)
            {
                if (Bars.Cross.Exists)
                {
                    if (Layout.SeparateEx.MetaSlots.TweensExist) Layout.SeparateEx.MetaSlots.TweenAll(); // animate button sizes if needed
                    if (Profile.CombatFadeInOut && FadeTween.Active) { FadeTween.Run(); } // run fader animation if needed
                    DoneHudCheck = hudLayout->AgentInterface.IsAgentActive() && (DoneHudCheck || AdjustHudEditorNode()); // if HUD layout editor is open, perform this fix once

                    if (Layout.SeparateEx.Ready && Bars.MainMenu.Exists) Layout.SeparateEx.HideForMenu();
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
                HudSlot = GetHudSlot();
                PluginLog.LogDebug("Hud Layout " + HudSlot + " is currently active");
                Setup();
            }
        }
        catch (Exception ex)
        {
            if (LogFrameCatch)
            {
                LogFrameCatch = false;
                PluginLog.LogError($"Exception: Framework Update Failed!\n{ex}");
                Task.Delay(5000).ContinueWith(delegate { LogFrameCatch = true; }); // So we aren't spamming if something goes terribly wrong
            }
        }
    }

    /// <summary>Set to true when <see cref="ActionBarReceiveEventDetour"/> detects a drag/drop change, signalling the next <see cref="ActionBarBaseUpdateDetour"/> to handle it.</summary>
    private bool DragDrop;

    /// <summary>Called when mouse events occur on hotbar slots.<br/><br/>
    /// Relevant Event Types:<br/>
    ///<term>47</term> Initiate drag<br/>
    ///<term>50</term> Drag/Drop onto a slot<br/>
    ///<term>54</term> Drag/Discard from a slot
    /// </summary>
    private byte ActionBarReceiveEventDetour(AddonActionBarBase* barBase, uint eventType, void* a3, void* a4,
        NumberArrayData** numberArrayData)
    {
        try
        {
            switch (eventType)
            {
                case 50 or 54 when Layout.SeparateEx.Ready:
                {
                    var barID = barBase->HotbarID;
                    PluginLog.LogDebug($"Drag/Drop Event on Bar #{barID} ({(barID > 9 ? $"Cross Hotbar Set {barID - 9}" : $"Hotbar {barID + 1}")}); Handling on next ActionBarBase Update");
                    Layout.Cross.UnassignedSlotVis(Profile.HideUnassigned);
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

    /// <summary>Called whenever a change occurs on any hotbar. Calls the plugin's main arrangement functions.</summary>
    private byte ActionBarBaseUpdateDetour(AddonActionBarBase* barBase, NumberArrayData** numberArrayData,
        StringArrayData** stringArrayData)
    {
        var ret = ActionBarBaseUpdateHook!.Original(barBase, numberArrayData, stringArrayData);
        if (!IsSetUp) return ret;

        try
        {
            if (DragDrop) HandleDragDrop();
            if (Job.HasChanged) HandleJobChange();

            if (barBase->HotbarID == 1) Layout.Update(Bars.Cross.EnableStateChanged, true);
            else if (barBase->HotbarSlotCount == 16 && Bars.Cross.SetID.HasChanged(barBase)) HandleSetChange(barBase);
        }
        catch (Exception ex) { PluginLog.LogError($"Exception: ActionBarBaseUpdateDetour Failed!\n{ex}"); }

        return ret;
    }

    /// <summary>Takes any drag/drop changes that have just been made to the borrowed Expanded Hold Bars, and writes them to the corresponding Cross Hotbar set instead</summary>
    private void HandleDragDrop()
    {
        PluginLog.LogDebug("Handling Drag/Drop Event");
        DragDrop = false;

        if (!Layout.SeparateEx.Ready || (int)Bars.Cross.Selection.Current > 2) return;

        var lr = (id: Bars.LR.ID, map: Actions.GetExMap(ExSide.LR), actions: Bars.LR.BorrowBar.Actions);
        var rl = (id: Bars.RL.ID, map: Actions.GetExMap(ExSide.RL), actions: Bars.RL.BorrowBar.Actions);

        var shared = CharConfig.Hotbar.Shared;
        var stored = Bars.StoredActions;
        var job = Job.Current;

        Actions.Copy(lr.actions, 0, lr.map.barID, lr.map.useLeft ? 0 : 8, 8);
        Actions.Save(lr.actions, 0, lr.map.barID, lr.map.useLeft ? 0 : 8, 8, shared[lr.map.barID] ? 0 : job);
        if (stored[lr.id] != null && stored[lr.id]!.Length != 0) Actions.Save(stored[lr.id]!, 0, lr.id, 0, 12, shared[lr.id] ? 0 : job);

        Actions.Copy(rl.actions, 0, rl.map.barID, rl.map.useLeft ? 0 : 8, 8);
        Actions.Save(rl.actions, 0, rl.map.barID, rl.map.useLeft ? 0 : 8, 8, shared[rl.map.barID] ? 0 : job);
        if (stored[rl.id] != null && stored[rl.id]!.Length != 0) Actions.Save(stored[rl.id]!, 0, rl.id, 0, 12, shared[rl.id] ? 0 : job);
    }

    /// <summary>Updates the stored bars when the player changes jobs</summary>
    private static void HandleJobChange()
    {
        PluginLog.LogDebug("Job Change: " + Job.Abbr);
        if (!Layout.SeparateEx.Ready) return;

        Actions.Store(Bars.LR.ID);
        Actions.Store(Bars.RL.ID);
    }

    /// <summary>Responds to the player changing the Cross Hotbar set</summary>
    private static void HandleSetChange(AddonActionBarBase* barBase)
    {
        PluginLog.LogDebug($"Switched to Cross Hotbar Set {barBase->HotbarID - 9}");
        if (Config.RemapEx || Config.RemapW) Remap.Override(barBase->HotbarID);
    }

    /// <summary>Responds to the HUD layout being changed/set/saved</summary>
    private uint SetHudLayoutDetour(IntPtr filePtr, uint hudSlot, byte unk0, byte unk1)
    {
        PluginLog.LogDebug($"Loaded Hud Layout {hudSlot+1}");
        HudSlot = (int)(hudSlot + 1);
        if (IsSetUp) Layout.ScheduleNudges(2, 10, false);
        return SetHudLayoutHook!.Original(filePtr, hudSlot, unk0, unk1);
    }

    /// <summary>Whether <see cref="AdjustHudEditorNode"/> has already run and completed</summary>
    private static bool DoneHudCheck;

    /// <summary>Fix for misaligned frame around XHB when using the HUD Layout Interface</summary>
    private static bool AdjustHudEditorNode()
    {
        var hudScreen = new BaseWrapper("_HudLayoutScreen");
        var root = Bars.Cross.Root.Node;

        if (hudScreen == null || root == null) return false;

        var scale = root->ScaleX;
        var hudNodes = hudScreen.NodeList;

        for (var i = 1; i < hudScreen.NodeListCount; i++)
        {
            if (!hudNodes[i]->IsVisible ||
                Math.Abs(hudNodes[i]->Y - root->Y) > 1 ||
                Math.Abs(hudNodes[i]->Width - root->Width * scale) > 1 ||
                Math.Abs(hudNodes[i]->Height - root->Height * scale) > 1 ||
                Math.Abs(hudNodes[i]->X - root->X) < 1) continue;
            hudNodes[i]->X = root->X;
            hudNodes[i]->Flags_2 |= 0xD;
            return true;
        }

        return false;
    }

    /// <summary>Runs the fader feature when the player's condition changes</summary>
    internal static void OnConditionChange(ConditionFlag flag = 0, bool value = true)
    {
        if (Profile.CombatFadeInOut) FadeTween.Begin(Service.Condition[ConditionFlag.InCombat]);
    }
}