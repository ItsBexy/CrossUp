using System;
using System.Numerics;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    public struct ButtonAction
    {
        public uint Id;
        public HotbarSlotType CommandType;
    }

    public class BarContents
    {
        public static ButtonAction[] XHB => GetCrossbarContents();
        public static ButtonAction[] LR => GetExBarContents(true);
        public static ButtonAction[] RL => GetExBarContents(false);
    }

        // main func for retrieving hotbar actions. most others point to this one
    private static ButtonAction[] GetBarContentsByID(int barID, int slotCount, int fromSlot = 0) 
    {
        var contents = new ButtonAction[slotCount];
        var hotbar = raptureModule->HotBar[barID];

        for (var i = 0; i < slotCount; i++)
        {
            var slotStruct = hotbar->Slot[i + fromSlot];
            if (slotStruct == null) continue;
            contents[i].CommandType = slotStruct->CommandType;
            contents[i].Id = slotStruct->CommandType == HotbarSlotType.Action
                ? ActionManager->GetAdjustedActionId(slotStruct->CommandId)
                : slotStruct->CommandId;
        }

        return contents;
    }

    private static ButtonAction[] GetSavedBar(int job, int barID, int slotCount = 12) // retrieve saved bar contents
    {
        var contents = new ButtonAction[slotCount];
        var savedBar = raptureModule->SavedClassJob[job]->Bar[barID];

        for (var i = 0; i < slotCount; i++)
        {
            contents[i].CommandType = savedBar->Slot[i]->Type;
            contents[i].Id = savedBar->Slot[i]->ID;
        }

        return contents;
    }

        //get whatever's on the cross hotbar
    private static ButtonAction[] GetCrossbarContents() 
    {
        var barBaseXHB = (AddonActionBarBase*)UnitBases.Cross;
        var xBar = (AddonActionCross*)UnitBases.Cross;
        if (xBar->PetBar)
        {
            // FIX STILL NEEDED
            // I can identify if the pet hotbar is active, but I haven't worked out how to retrieve its contents.
            // Grabbing the same ol regular bar contents for now, even though it looks odd.
            // this does not affect gameplay, only visuals
            
            return GetBarContentsByID(barBaseXHB->HotbarID, 16);
        }
        else
        {
            return GetBarContentsByID(barBaseXHB->HotbarID, 16);
        }
    }

        //get whatever's mapped to the Ex cross hotbars
    private static ButtonAction[] GetExBarContents(bool leftOrRight) 
    {
        var usePvP = GetCharConfig(ConfigID.SepPvP) == 1 && Service.ClientState.IsPvP ? 1 : 0;
        var exConf = GetCharConfig(leftOrRight ? ConfigID.LRset[usePvP] : ConfigID.RLset[usePvP]);
        int exBarTarget;
        bool useLeft;
        if (exConf < 16)
        {
            exBarTarget = (exConf >> 1) + 10;
            useLeft = exConf % 2 == 0;
        }
        else
        {
            var barBaseXHB = (AddonActionBarBase*)UnitBases.Cross;
            exBarTarget = (barBaseXHB->HotbarID + ((exConf < 18) ? -1 : 1) - 2) % 8 + 10;
            useLeft = exConf % 2 == 1;
        }

        var contents = GetBarContentsByID(exBarTarget, 8, useLeft ? 0 : 8);
        return contents;
    }

        // copy a set of actions onto part/all of a bar
    private static void CopyButtons(ButtonAction[] sourceButtons, int sourceSlot, int targetBarID, int targetSlot,int count)
    {
        var targetBar = raptureModule->HotBar[targetBarID];
        for (var i = 0; i < count; i++)
        {
            var tButton = targetBar->Slot[i + targetSlot];
            var sButton = sourceButtons[i + sourceSlot];
            if (tButton->CommandId != sButton.Id || tButton->CommandType != sButton.CommandType)
            {
                tButton->Set(sButton.CommandType, sButton.Id);
            }
        }
    }
}