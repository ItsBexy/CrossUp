using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Dalamud.Logging;
using ClientStructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace CrossUp;
public sealed unsafe partial class CrossUp
{

    private static readonly RaptureHotbarModule* RaptureModule = (RaptureHotbarModule*)ClientStructsFramework.Instance()->GetUiModule()->GetRaptureHotbarModule();
    /// <summary>An action that can be assigned to a hotbar</summary>
    public struct Action
    {
        public uint ID;
        public HotbarSlotType Type;
    }
    /// <summary>Represents the portion of a Cross Hotbar set that an additional bar (WXHB or Expanded Hold) is mapped to</summary>
    public struct MappedSet
    {
        public int BarID;
        public bool UseLeft;
    }
    public static class Actions
    {
        /// <summary>Gets the actions saved to a specific Hotbar</summary>
        public static Action[] GetByBarID(int barID, int slotCount, int fromSlot = 0)
        {
            var contents = new Action[slotCount];
            var hotbar = RaptureModule->HotBar[barID];

            for (var i = 0; i < slotCount; i++)
            {
                var slot = hotbar->Slot[i + fromSlot];
                if (slot == null) continue;
                contents[i].Type = slot->CommandType;
                contents[i].ID = slot->CommandId;
            }
            return contents;
        }

        /// <summary>Retrieves the saved contents of a hotbar</summary>
        public static Action[] GetSaved(int job, int barID, int slotCount = 12)
        {
            if (PvpArea) job = PvpJob(job);
            var contents = new Action[slotCount];
            var saveBar = RaptureModule->SavedClassJob[job]->Bar[barID];

            for (var i = 0; i < slotCount; i++)
            {
                var savedSlot = saveBar->Slot[i];
                contents[i].Type = savedSlot->Type;
                contents[i].ID = savedSlot->ID;
            }

            return contents;
        }

        /// <summary>Writes a list of actions to the user's saved hotbar settings</summary>
        public static void Save(IList<Action> sourceButtons, int sourceStart, int targetID, int targetStart, int count, int job)
        {
            if (sourceButtons == null) return;
            if (PvpArea) job = PvpJob(job);

            var saveBar = RaptureModule->SavedClassJob[job]->Bar[targetID];

            for (var i = 0; i < count; i++)
            {

                var saveSlot = saveBar->Slot[i + targetStart];
                var sourceSlot = sourceButtons[i + sourceStart];
                if (saveSlot->ID == sourceSlot.ID && saveSlot->Type == sourceSlot.Type) continue;

                PluginLog.LogDebug($"Saving {sourceSlot.Type} {sourceSlot.ID} to Bar #{targetID} ({(targetID > 9 ? $"Cross Hotbar Set {targetID - 9}" : $"Hotbar {targetID + 1}")}) Slot {i + targetStart}");
                saveSlot->Type = sourceSlot.Type;
                saveSlot->ID = sourceSlot.ID;

            }
        }

        /// <summary>Copies a list of actions to a hotbar (without permanently saving them to that bar)</summary>
        public static void Copy(IReadOnlyList<Action> sourceButtons, int sourceStart, int targetBarID, int targetStart, int count)
        {
            var targetBar = RaptureModule->HotBar[targetBarID];
            for (var i = 0; i < count; i++)
            {
                var targetSlot = targetBar->Slot[i + targetStart];
                var sourceSlot = sourceButtons[i + sourceStart];

                if (targetSlot->CommandId != sourceSlot.ID || targetSlot->CommandType != sourceSlot.Type) targetSlot->Set(sourceSlot.Type, sourceSlot.ID);
            }
        }

        /// <summary>Stores a set of hotbar actions for the plugin to reference later</summary>
        public static void Store(int barID) => Bars.StoredActions[barID] = GetSaved(CharConfig.Hotbar.Shared[barID] ? 0 : PlayerJob, barID);

        public static MappedSet LRMap => GetExMap(true);
        public static MappedSet RLMap => GetExMap(false);
        private static MappedSet GetExMap(bool lr)
        {
            int conf = (lr ? CharConfig.ExtraBarMaps.LR : CharConfig.ExtraBarMaps.RL)[CharConfig.SepPvP && PvpArea ? 1 : 0];
            return new MappedSet
            {
                BarID = conf < 16 ? (conf >> 1) + 10 : (Bars.Cross.LastKnownSetID + (conf < 18 ? -1 : 1) - 2) % 8 + 10,
                UseLeft = conf % 2 == (conf < 16 ? 0 : 1)
            };
        }
    }
}