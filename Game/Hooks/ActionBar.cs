using System;
using CrossUp.Features;
using CrossUp.Features.Layout;
using CrossUp.Game.Hotbar;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static CrossUp.CrossUp;
using static CrossUp.Utility.Service;

// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace CrossUp.Game.Hooks
{
    internal sealed unsafe class ActionBarHooks : IDisposable
    {
        private delegate byte ActionBarReceiveEventDel(AddonActionBarBase* barBase, uint eventID, void* a3, void* a4, NumberArrayData** numberArrayData);
        private static Hook<ActionBarReceiveEventDel>? ActionBarReceiveEventHook;

        private delegate byte ActionBarBaseUpdateDel(AddonActionBarBase* barBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData);
        private static Hook<ActionBarBaseUpdateDel>? ActionBarBaseUpdateHook;

        public ActionBarHooks()
        {
            ActionBarReceiveEventHook = GameInteropProvider.HookFromSignature<ActionBarReceiveEventDel>("E8 ?? ?? ?? FF 66 83 FB ?? ?? ?? ?? BF 3F", ActionBarReceiveEventDetour);
            ActionBarReceiveEventHook.Enable();

            ActionBarBaseUpdateHook = GameInteropProvider.HookFromSignature<ActionBarBaseUpdateDel>("E8 ?? ?? ?? ?? 83 BB ?? ?? ?? ?? ?? 75 09", ActionBarBaseUpdateDetour);
            ActionBarBaseUpdateHook.Enable();
        }

        public void Dispose()
        {
            ActionBarReceiveEventHook?.Dispose();
            ActionBarBaseUpdateHook?.Dispose();
        }

        /// <summary>Set to true when <see cref="ActionBarReceiveEventDetour"/> detects a drag/drop change, signalling the next <see cref="ActionBarBaseUpdateDetour"/> to handle it.</summary>
        private static bool DragDrop;

        /// <summary>Called whenever a change occurs on any hotbar. Calls the plugin's main arrangement functions.</summary>
        private static byte ActionBarBaseUpdateDetour(AddonActionBarBase* barBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData)
        {
            var ret = ActionBarBaseUpdateHook!.Original(barBase, numberArrayData, stringArrayData);
            if (!IsSetUp) return ret;

            try
            {
                if (DragDrop)
                {
                    Actions.HandleDragDrop();
                    DragDrop = false;
                }
                if (Job.HasChanged) Job.HandleJobChange();
                if (barBase->RaptureHotbarId == 1) Layout.Update(Bars.Cross.EnableStateChanged);
                if (barBase->SlotCount == 16 && Bars.Cross.SetID.HasChanged()) SetSwitching.HandleSetChange(barBase);
            }
            catch (Exception ex)
            {
                Log.Error($"Exception: ActionBarBaseUpdateDetour Failed!\n{ex}");
            }

            return ret;
        }

        /// <summary>Called when mouse events occur on hotbar slots.<br/><br/>
        /// Relevant Event Types:<br/>
        ///<term>47</term> Initiate drag<br/>
        ///<term>50</term> Drag/Drop onto a slot<br/>
        ///<term>54</term> Drag/Discard from a slot
        /// </summary>
        private static byte ActionBarReceiveEventDetour(AddonActionBarBase* barBase, uint eventType, void* a3, void* a4, NumberArrayData** numberArrayData)
        {
            try
            {
                switch (eventType)
                {
                    case 50 or 54 when SeparateEx.Ready && GameConfig.Cross.Enabled:
                    {
                        var barID = barBase->RaptureHotbarId;
                        Log.Debug($"Drag/Drop Event on Bar #{barID} ({(barID > 9 ? $"Cross Hotbar Set {barID - 9}" : $"Hotbar {barID + 1}")}); Handling on next ActionBarBaseUpdate");
                        Cross.UnassignedSlotVis(Profile.HideUnassigned);
                        DragDrop = true;
                        break;
                    }
                    case 47 when IsSetUp:
                        Cross.UnassignedSlotVis(true);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception: ActionBarReceiveEventDetour Failed!\n{ex}");
            }

            return ActionBarReceiveEventHook!.Original(barBase, eventType, a3, a4, numberArrayData);
        }
    }
}