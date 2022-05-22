using System.Runtime.InteropServices;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CrossUp;

// Relevant Game Structs from SimpleTweaks by Caraxi https://github.com/Caraxi/SimpleTweaksPlugin/tree/main/GameStructs

[StructLayout(LayoutKind.Explicit, Size = 0x248)]
[Addon("_ActionBar02", "_ActionBar03", "_ActionBar04", "_ActionBar05", "_ActionBar06", "_ActionBar07", "_ActionBar08", "_ActionBar09")]
public unsafe struct AddonActionBarBase {
    [FieldOffset(0x000)] public AtkUnitBase AtkUnitBase;
    [FieldOffset(0x220)] public ActionBarSlotAction* ActionBarSlotsAction;
    [FieldOffset(0x228)] public void* UnknownPtr228; // Field of 0s
    [FieldOffset(0x230)] public void* UnknownPtr230; // Points to same location as +0x228 ??
    [FieldOffset(0x238)] public int UnknownInt238;   
    [FieldOffset(0x23C)] public byte HotbarID;
    [FieldOffset(0x23D)] public sbyte HotbarIDOther;
    [FieldOffset(0x23E)] public byte HotbarSlotCount;
    [FieldOffset(0x23F)] public int UnknownInt23F;
    [FieldOffset(0x243)] public int UnknownInt243; // Flags of some kind
}

[StructLayout(LayoutKind.Explicit, Size = 0xC8)]
public unsafe struct ActionBarSlotAction {
    [FieldOffset(0x04)] public int ActionId;       // Not cleared when slot is emptied
    [FieldOffset(0x18)] public void* UnknownPtr;   // Points 34 bytes ahead ??
    [FieldOffset(0x90)] public AtkComponentNode* Icon;
    [FieldOffset(0x98)] public AtkTextNode* ControlHintTextNode;
    [FieldOffset(0xA0)] public AtkResNode* IconFrame;
    [FieldOffset(0xA8)] public AtkImageNode* ChargeIcon;
    [FieldOffset(0xB0)] public AtkResNode* RecastOverlayContainer;
    [FieldOffset(0xB8)] public byte* PopUpHelpTextPtr; // Null when slot is empty
}

[StructLayout(LayoutKind.Explicit, Size = 0x710)]
[Addon("_ActionCross")]
public struct AddonActionCross
{
    [FieldOffset(0x000)] public AddonActionBarBase ActionBarBase;
    [FieldOffset(0x6E8)] public int LRBar;
    [FieldOffset(0x6EC)] public int RLBar;
    [FieldOffset(0x701)] public bool LeftBar;
    [FieldOffset(0x702)] public bool RightBar;
    [FieldOffset(0x704)] public bool PetBar;
}

[StructLayout(LayoutKind.Explicit, Size = 0x2F8)]
[Addon("_ActionDoubleCrossL", "_ActionDoubleCrossR")]
public struct AddonActionDoubleCrossBase
{
    [FieldOffset(0x000)] public AddonActionBarBase ActionBarBase;
    [FieldOffset(0x2EC)] public byte UseLeftSide;
    [FieldOffset(0x2E8)] public byte BarTarget;
    [FieldOffset(0x2e0)] public bool Selected;
}


    // overriding getter for RaptureHotbarModule.Hotbar to allow retrieval of bar 19

[StructLayout(LayoutKind.Explicit, Size = 160376)]
public struct RaptureHotbarModule
{
    [FieldOffset(144)]
    public HotBars HotBar;

    [FieldOffset(72052)]
    public SavedHotBars SavedClassJob;
}

[StructLayout(LayoutKind.Sequential, Size = 64512)]
public struct HotBars
{
    private unsafe fixed byte data[64512];

    public unsafe HotBar* this[int i]
    {
        get
        {
            if (i is < 0 or > 19)    // upper limit is 17 in ClientStructs, but we need 19 for the pet cross bar
                return null;
            fixed (byte* numPtr = data)
                return (HotBar*)(numPtr + sizeof(HotBar) * i);
        }
    }
}