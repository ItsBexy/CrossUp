using CrossUp.Features.Layout;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static CrossUp.Utility.Service;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace CrossUp.Game.Hotbar;

/// <summary>Methods pertaining to hotbar actions</summary>
internal static unsafe class Actions
{
    public static readonly RaptureHotbarModule* RaptureModule = Framework.Instance()->GetUIModule()->GetRaptureHotbarModule();

    /// <summary>An action that can be assigned to a hotbar. Can include a reference to a specific Hotbar slot.</summary>
    internal readonly struct Action
    {
        internal readonly uint CommandId;
        internal readonly HotbarSlotType CommandType;
        private readonly HotbarSlot? Slot;

        private Action(uint id, HotbarSlotType type, HotbarSlot? slot = null)
        {
            CommandId = id;
            CommandType = type;
            Slot = slot;
        }

        internal bool Matches(HotbarSlot slot) => CommandId == slot.CommandId && CommandType == slot.CommandType;
        internal bool Matches(SavedHotbarSlot slot) => CommandId == slot.CommandId && CommandType == slot.CommandType;

        public static implicit operator Action(SavedHotbarSlot s) => new(s.CommandId, s.CommandType);
        public static implicit operator Action(HotbarSlot h) => new(h.CommandId, h.CommandType, h);
        public static implicit operator HotbarSlot(Action a) => new() { CommandId = a.CommandId, CommandType = a.CommandType };
        public static implicit operator HotbarSlot*(Action a)
        {
            var h = a.Slot ?? a;
            return (HotbarSlot*)Unsafe.AsPointer(ref h);
        }
    }

    /// <summary>Indicates the type of expanded hold input</summary>
    internal enum ExSide { LR = 0, RL = 1 }

    /// <summary>Gets the current actions on a specific Hotbar</summary>
    internal static Action[] GetByBarID(int barID, int slotCount, int fromSlot = 0)
    {
        var contents = new Action[slotCount];
        try
        {
            ref var hotbar = ref barID == 19 ? ref RaptureModule->PetCrossHotbar : ref RaptureModule->Hotbars[barID];
            var span = hotbar.Slots;

            if (span == null) return contents;

            for (var i = 0; i < slotCount; i++)
            {
                ref var slot = ref span[i + fromSlot];
                contents[i] = slot;
            }

            return contents;
        }
        catch (Exception ex)
        {
            Log.Error($"{ex}");
            return contents;
        }
    }

    private static Span<SavedHotbarSlot> GetSavedSpan(int job, int barID)
    {
        var adjustedJob = Job.IsPvP ? Job.PvpID(job) : job;
        var savedBars = RaptureModule->SavedHotbars;
        ref var savedBar = ref savedBars[adjustedJob].Hotbars[barID];
        return savedBar.Slots;
    }

    /// <summary>Retrieves the saved hotbar contents for a specific job</summary>
    internal static Action[] GetSaved(int job, int barID, int slotCount = 12)
    {
        var contents = new Action[slotCount];

        try
        {
            var span = GetSavedSpan(job, barID);

            for (var i = 0; i < slotCount; i++)
            {
                ref var savedSlot = ref span[i];
                contents[i] = savedSlot;
            }

            return contents;
        }
        catch (Exception ex)
        {
            Log.Error($"{ex}");
            return contents;
        }
    }

    /// <summary>Writes a list of actions to the user's saved hotbar settings</summary>
    private static void Save(IList<Action> sourceActions, int sourceStart, int targetID, int targetStart, int count, int job)
    {
        try
        {
            var span = GetSavedSpan(job, targetID);

            for (var i = 0; i < count; i++)
            {
                ref var savedSlot = ref span[i + targetStart];
                var source = sourceActions[i + sourceStart];

                if (source.Matches(savedSlot)) continue;

                RaptureModule->WriteSavedSlot((uint)job, (uint)targetID, (uint)(i + targetStart), source, false, Job.IsPvP);

                Log.Verbose($"Saving {source.CommandType} {source.CommandId} to Bar #{targetID} ({(targetID > 9 ? $"Cross Hotbar Set {targetID - 9}" : $"Hotbar {targetID + 1}")}) Slot {i + targetStart}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"{ex}");
        }
    }

    /// <summary>Copies a list of actions to a hotbar (without permanently saving them to that bar)</summary>
    internal static void Copy(IReadOnlyList<Action> sourceButtons, int sourceStart, int targetBarID, int targetStart, int count)
    {
        try
        {
            ref var targetBar = ref RaptureModule->Hotbars[targetBarID];

            for (var i = 0; i < count; i++)
            {
                ref var targetSlot = ref targetBar.Slots[i + targetStart];
                var source = sourceButtons[i + sourceStart];

                if (source.Matches(targetSlot)) continue;

                targetSlot.Set(source.CommandType, source.CommandId);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"{ex}");
        }
    }

    /// <summary>Stores a set of hotbar actions for the plugin to reference later</summary>
    internal static void Store(int barID) => Bars.StoredActions[barID] = GetSaved(GameConfig.Hotbar.Shared[barID] ? 0 : Job.Current, barID);

    /// <summary>Gets the actions that correspond to the selected Expanded Hold bar</summary>
    internal static Action[] GetExHoldActions(ExSide exSide)
    {
        var map = GetExMap(exSide);
        return GetByBarID(map.barID, 8, map.useLeft ? 0 : 8);
    }

    /// <summary>Parses the game configuration to identify the mapped bar for an Expanded Hold input</summary>
    private static (int barID, bool useLeft) GetExMap(ExSide side)
    {
        var conf = (side == ExSide.LR ? GameConfig.Cross.ExMaps.LR : GameConfig.Cross.ExMaps.RL)[GameConfig.Cross.SepPvP && Job.IsPvP ? 1 : 0];

        var barID = conf < 16 ? (conf >> 1) + 10 : (Bars.Cross.SetID.Current + (conf < 18 ? 1 : -1) - 2) % 8 + 10;
        var useLeft = conf % 2 == 0;

        return (barID, useLeft);
    }

    /// <summary>Interprets a drag/drop action involving the "borrowed" hotbars that form the plugin's Expanded Hold bars, and redirects the action to the appropriate Cross Hotbar set.</summary>
    public static void HandleDragDrop()
    {
        Log.Debug("Handling Drag/Drop Event");

        if (!SeparateEx.Ready || (int)Bars.Cross.AddonCross->Selected > 2) return;

        var lr = (id: Bars.LR.ID, map: GetExMap(ExSide.LR), actions: Bars.LR.BorrowBar.Actions);
        var rl = (id: Bars.RL.ID, map: GetExMap(ExSide.RL), actions: Bars.RL.BorrowBar.Actions);

        var shared = GameConfig.Hotbar.Shared;
        var stored = Bars.StoredActions;
        var job = Job.Current;

        Copy(lr.actions, 0, lr.map.barID, lr.map.useLeft ? 0 : 8, 8);
        Save(lr.actions, 0, lr.map.barID, lr.map.useLeft ? 0 : 8, 8, shared[lr.map.barID] ? 0 : job);
        if (stored[lr.id] != null && stored[lr.id]!.Length != 0) Save(stored[lr.id]!, 0, lr.id, 0, 12, shared[lr.id] ? 0 : job);

        Copy(rl.actions, 0, rl.map.barID, rl.map.useLeft ? 0 : 8, 8);
        Save(rl.actions, 0, rl.map.barID, rl.map.useLeft ? 0 : 8, 8, shared[rl.map.barID] ? 0 : job);
        if (stored[rl.id] != null && stored[rl.id]!.Length != 0) Save(stored[rl.id]!, 0, rl.id, 0, 12, shared[rl.id] ? 0 : job);
    }

}