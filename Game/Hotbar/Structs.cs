using System.Runtime.InteropServices;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBeInternal

namespace CrossUp.Game.Hotbar
{
    [StructLayout(LayoutKind.Explicit, Size = 0x248)]
    [Addon("_ActionBar02", "_ActionBar03", "_ActionBar04", "_ActionBar05", "_ActionBar06", "_ActionBar07", "_ActionBar08", "_ActionBar09")]
    public struct AddonActionBarBase
    {
        [FieldOffset(0x23C)] public byte HotbarID;
        [FieldOffset(0x23E)] public byte HotbarSlotCount;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x710)]
    [Addon("_ActionCross")]
    public struct AddonActionCross
    {
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
        [FieldOffset(0x2E0)] public bool Selected;
    }

    [StructLayout(LayoutKind.Explicit, Size = 160376)]
    public struct RaptureHotbarModule
    {
        [FieldOffset(144)] public HotBars HotBar;
        [FieldOffset(72052)] public SavedHotBars SavedClassJob;
    }

    [StructLayout(LayoutKind.Sequential, Size = 64512)]
    public struct HotBars
    {
        public unsafe fixed byte data[64512];

        public readonly unsafe HotBar* this[int i]
        {
            get
            {
                // upper limit is 17 in ClientStructs, but we need up to 19 for the pet cross bar
                if (i is < 0 or > 19) return null;
                fixed (byte* numPtr = data) return (HotBar*)(numPtr + sizeof(HotBar) * i);
            }
        }
    }
}