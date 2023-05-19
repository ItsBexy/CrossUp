using System.Collections.Generic;
using CrossUp.Features.Layout;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using static CrossUp.Utility.Service;
using CSFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace CrossUp.Game.Hotbar;

/// <summary>Methods pertaining to hotbar actions</summary>
internal static unsafe class Actions
{
    private static readonly RaptureHotbarModule* RaptureModule = (RaptureHotbarModule*)CSFramework.Instance()->GetUiModule()->GetRaptureHotbarModule();

    /// <summary>An action that can be assigned to a hotbar</summary>
    internal struct Action
    {
        internal uint CommandId;
        internal HotbarSlotType CommandType;
    }

    /// <summary>Indicates the type of expanded hold input</summary>
    internal enum ExSide { LR = 0, RL = 1 }

    /// <summary>Checks if the player is in a PvP match or in the Wolves' Den</summary>
    private static bool IsPvP => ClientState.IsPvP || ClientState.TerritoryType == 250;

    /// <summary>Gets the current actions on a specific Hotbar</summary>
    internal static Action[] GetByBarID(int barID, int slotCount, int fromSlot = 0)
    {
        var contents = new Action[slotCount];
        var hotbar = RaptureModule->HotBar[barID];

        for (var i = 0; i < slotCount; i++)
        {
            var slot = hotbar->Slot[i + fromSlot];
            if (slot == null) continue;
            contents[i].CommandType = slot->CommandType;
            contents[i].CommandId = slot->CommandId;
        }

        return contents;
    }

    /// <summary>Retrieves the saved hotbar contents for a specific job</summary>
    internal static Action[] GetSaved(int job, int barID, int slotCount = 12)
    {
        if (IsPvP) job = Job.PvpID(job);

        var contents = new Action[slotCount];
        var saveBar = RaptureModule->SavedClassJob[job]->Bar[barID];

        for (var i = 0; i < slotCount; i++)
        {
            var savedSlot = saveBar->Slot[i];
            contents[i].CommandType = savedSlot->Type;
            contents[i].CommandId = savedSlot->ID;
        }

        return contents;
    }

    /// <summary>Writes a list of actions to the user's saved hotbar settings</summary>
    private static void Save(IList<Action> sourceButtons, int sourceStart, int targetID, int targetStart, int count, int job)
    {
        if (IsPvP) job = Job.PvpID(job);

        var saveBar = RaptureModule->SavedClassJob[job]->Bar[targetID];

        for (var i = 0; i < count; i++)
        {
            var saveSlot = saveBar->Slot[i + targetStart];
            var sourceSlot = sourceButtons[i + sourceStart];
            if (saveSlot->ID == sourceSlot.CommandId && saveSlot->Type == sourceSlot.CommandType) continue;

            PluginLog.LogVerbose($"Saving {sourceSlot.CommandType} {sourceSlot.CommandId} to Bar #{targetID} ({(targetID > 9 ? $"Cross Hotbar Set {targetID - 9}" : $"Hotbar {targetID + 1}")}) Slot {i + targetStart}");
            saveSlot->Type = sourceSlot.CommandType;
            saveSlot->ID = sourceSlot.CommandId;
        }
    }

    /// <summary>Copies a list of actions to a hotbar (without permanently saving them to that bar)</summary>
    internal static void Copy(IReadOnlyList<Action> sourceButtons, int sourceStart, int targetBarID,
        int targetStart, int count)
    {
        var targetBar = RaptureModule->HotBar[targetBarID];
        for (var i = 0; i < count; i++)
        {
            var targetSlot = targetBar->Slot[i + targetStart];
            var sourceSlot = sourceButtons[i + sourceStart];

            if (targetSlot->CommandId != sourceSlot.CommandId || targetSlot->CommandType != sourceSlot.CommandType)
                targetSlot->Set(sourceSlot.CommandType, sourceSlot.CommandId);
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
        int conf = (side == ExSide.LR ? GameConfig.Cross.ExMaps.LR : GameConfig.Cross.ExMaps.RL)[GameConfig.Cross.SepPvP && IsPvP ? 1 : 0];

        var barID = conf < 16 ? (conf >> 1) + 10 : (Bars.Cross.SetID.Current + (conf < 18 ? -1 : 1) - 2) % 8 + 10;
        var useLeft = conf % 2 == (conf < 16 ? 0 : 1);

        return (barID, useLeft);
    }

    /// <summary>Interprets a drag/drop action involving the "borrowed" hotbars that form the plugin's Expanded Hold bars, and redirects the action to the appropriate Cross Hotbar set.</summary>
    public static void HandleDragDrop()
    {
        PluginLog.LogDebug("Handling Drag/Drop Event");

        if (!SeparateEx.Ready || (int)Bars.Cross.Selection.Current > 2) return;

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