using System.Collections.Generic;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    private static readonly RaptureHotbarModule* RaptureModule =
        (RaptureHotbarModule*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
            GetRaptureHotbarModule();

    /// <summary>An action that can be assigned to a hotbar</summary>
    internal struct Command
    {
        internal uint CommandId;
        internal HotbarSlotType CommandType;
    }

    internal enum ExSide
    {
        LR = 0,
        RL = 1
    }

    /// <summary>Methods pertaining to hotbar actions</summary>
    public static class Actions
    {
        /// <summary>Gets the actions saved to a specific Hotbar</summary>
        internal static Command[] GetByBarID(int barID, int slotCount, int fromSlot = 0)
        {
            var contents = new Command[slotCount];
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

        /// <summary>Retrieves the saved contents of a hotbar</summary>
        internal static Command[] GetSaved(int job, int barID, int slotCount = 12)
        {
            if (IsPvP) job = Job.PvpID(job);

            var contents = new Command[slotCount];
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
        internal static void Save(IList<Command> sourceButtons, int sourceStart, int targetID, int targetStart,
            int count, int job)
        {
            if (IsPvP) job = Job.PvpID(job);

            var saveBar = RaptureModule->SavedClassJob[job]->Bar[targetID];

            for (var i = 0; i < count; i++)
            {
                var saveSlot = saveBar->Slot[i + targetStart];
                var sourceSlot = sourceButtons[i + sourceStart];
                if (saveSlot->ID == sourceSlot.CommandId && saveSlot->Type == sourceSlot.CommandType) continue;

                PluginLog.LogDebug(
                    $"Saving {sourceSlot.CommandType} {sourceSlot.CommandId} to Bar #{targetID} ({(targetID > 9 ? $"Cross Hotbar Set {targetID - 9}" : $"Hotbar {targetID + 1}")}) Slot {i + targetStart}");
                saveSlot->Type = sourceSlot.CommandType;
                saveSlot->ID = sourceSlot.CommandId;
            }
        }

        /// <summary>Copies a list of actions to a hotbar (without permanently saving them to that bar)</summary>
        internal static void Copy(IReadOnlyList<Command> sourceButtons, int sourceStart, int targetBarID,
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
        internal static void Store(int barID) => Bars.StoredActions[barID] =
            GetSaved(CharConfig.Hotbar.Shared[barID] ? 0 : Job.Current, barID);

        internal static Command[] GetExHoldActions(ExSide exSide)
        {
            var map = GetExMap(exSide);
            return GetByBarID(map.barID, 8, map.useLeft ? 0 : 8);
        }

        internal static (int barID, bool useLeft) GetExMap(ExSide side)
        {
            int conf = (side == ExSide.LR ? CharConfig.ExtraBarMaps.LR : CharConfig.ExtraBarMaps.RL)[CharConfig.SepPvP && IsPvP ? 1 : 0];

            return (barID: conf < 16 ? (conf >> 1) + 10 : (Bars.Cross.SetID.Current + (conf < 18 ? -1 : 1) - 2) % 8 + 10,
                    useLeft: conf % 2 == (conf < 16 ? 0 : 1));
        }
    }
}