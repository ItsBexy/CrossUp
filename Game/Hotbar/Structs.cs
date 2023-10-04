using System.Runtime.InteropServices;
using FFXIVClientStructs.Attributes;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBeInternal

namespace CrossUp.Game.Hotbar
{
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
}