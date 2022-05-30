using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Dalamud.Logging;

namespace CrossUp;
public sealed unsafe partial class CrossUp
{
    public struct Action
    {
        public uint ID;
        public HotbarSlotType Type;
    }
    public struct MappedSet
    {
        public int BarID;
        public bool UseLeft;
    }
    private static int GetPlayerJob()
    {
        var job = Service.ClientState.LocalPlayer?.ClassJob.Id;
        return job != null ? (int)job : 0;
    }
    private static Action[] GetBarContentsByID(int barID, int slotCount, int fromSlot = 0)
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
    private static Action[] GetExBarContents(bool lr)
    {
        var mapSet = GetExHoldMapping(lr);
        return GetBarContentsByID(mapSet.BarID, 8, mapSet.UseLeft ? 0 : 8);
    }
    private static Action[] GetSavedBar(int job, int barID, int slotCount = 12) 
    {
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
    private static void SetSavedBar(IList<Action> sourceButtons, int sourceStart, int targetID, int targetStart, int count, int job)
    {
        if (sourceButtons == null) return;

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
    private static void CopyButtons(IReadOnlyList<Action> sourceButtons, int sourceStart, int targetBarID, int targetStart, int count)
    {
        var targetBar = RaptureModule->HotBar[targetBarID];
        for (var i = 0; i < count; i++)
        {
            var targetSlot = targetBar->Slot[i + targetStart];
            var sourceSlot = sourceButtons[i + sourceStart];

            if (targetSlot->CommandId != sourceSlot.ID || targetSlot->CommandType != sourceSlot.Type) targetSlot->Set(sourceSlot.Type, sourceSlot.ID);
        }
    }
    private static MappedSet GetExHoldMapping(bool lr)
    {
        int conf = (lr ? CharConfig.LRset : CharConfig.RLset)[CharConfig.SepPvP && Service.ClientState.IsPvP ? 1 : 0];
        return new MappedSet
        {
            BarID = conf < 16 ? (conf >> 1) + 10 : (Bars.Cross.LastKnownSetID + (conf < 18 ? -1 : 1) - 2) % 8 + 10,
            UseLeft = conf % 2 == (conf < 16 ? 0 : 1)
        };
    }
    private static void OnDragDropEx(bool lr)
    {
        if (Bars.Cross.Selection > 2) return;

        var borrow = lr ? Bars.LR.BorrowBar : Bars.RL.BorrowBar;
        var mapSet = GetExHoldMapping(lr);

        CopyButtons(borrow.Actions, 0, mapSet.BarID, mapSet.UseLeft ? 0 : 8, 8);
        SetSavedBar(borrow.Actions, 0, mapSet.BarID, mapSet.UseLeft ? 0 : 8, 8, CharConfig.Hotbar.Shared[mapSet.BarID] ? 0 : GetPlayerJob());

        if (Bars.StoredActions[borrow.BarID] == null || Bars.StoredActions[borrow.BarID]!.Length == 0) return;
        SetSavedBar(Bars.StoredActions[borrow.BarID]!, 0, borrow.BarID, 0, 12, CharConfig.Hotbar.Shared[borrow.BarID] ? 0 : GetPlayerJob());
    }
}