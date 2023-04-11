using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

namespace CrossUp;

/// <summary>Class for retrieving/setting character configuration options</summary>
public class CharConfig
{
    private static readonly unsafe ConfigModule* ConfModule = ConfigModule.Instance();

    /// <summary>
    /// Represents a Character Configuration value<br/><br/>
    /// <term cref="Set">Set()</term> Updates the value in-game<br/>
    /// <term>(int)</term> Returns the value itself<br/>
    /// <term>(bool)</term> Returns true if value > 0
    /// </summary>
    public sealed class Config
    {
        public Config(uint index) => Index = index;
        public Config(string name, uint offset = 0) => Index = IndexFromName(name) + offset;
        private readonly uint Index;
        public unsafe short ID => (short)ConfModule->GetOption(Index)->OptionID;
        public unsafe string Name => ConfModule->GetOption(Index)->GetName();
        public unsafe int Get() => ConfModule->GetIntValue(Index);
        public unsafe bool Set(int val) => ConfModule->SetOption(Index, val, 1);
        public static implicit operator int(Config cfg) => cfg.Get();
        public static implicit operator bool(Config cfg) => cfg.Get() > 0;
        public byte IntToAlpha => (byte)((100 - (int)this) * 2.55);

        /// <returns>Index | ID | Name | Value</returns>
        public override string ToString() =>
            Index + " | " + ID + " | " + Name + " | " + (uint)Get() + " | " + UintToHex((uint)Get());
    }


    public uint HexToUint(string hex) => BitConverter.ToUInt32(Convert.FromHexString(hex));
    public static string UintToHex(uint u) => Convert.ToHexString(BitConverter.GetBytes(u));

    private static unsafe uint IndexFromName(string name)
    {
        for (uint i = 0; i < 701U; ++i)
            if (ConfModule->GetOption(i)->GetName() == name)
                return i;
        return 0;
    }

    public static class Cross
    {
        /// <summary><term>Checkbox</term> The Cross Hotbar is Enabled</summary>
        public static readonly Config Enabled = new("HotbarCrossDispType");

        /// <summary><term>Checkbox</term> The Cross Hotbar is visible in the HUD</summary>
        public static readonly Config Visible = new("HotbarCrossDispAlways");
    }

    /// <summary><term>Checkbox</term> User has enabled different settings for PvP vs PvE</summary>
    public static readonly Config SepPvP = new("HotbarCrossSetPvpModeActive");

    /// <summary>
    /// <term>Radio Button</term> Cross Hotbar Display Type<br/><br/>
    /// <term>0</term> D-Pad / Buttons / D-Pad / Buttons<br/>
    /// <term>1</term> D-Pad / D-Pad / Buttons / Buttons
    /// </summary>
    public static readonly Config MixBar = new("HotbarCrossDisp");

    /// <summary><term>Dropdowns</term> Mappings for Additional Cross Hotbars<br/>Index: [<term>0</term> PvE, <term>1</term> PvP] <br/><br/>
    /// Returns:<br/>
    /// <term>0</term> Cross Hotbar 1 (Left)<br/>
    /// <term>1</term> Cross Hotbar 1 (Right)<br/>
    /// <term>2</term> Cross Hotbar 2 (Left)<br/>
    /// <term>3</term> Cross Hotbar 3 (Right)<br/>
    /// <term>4</term> Cross Hotbar 3 (Left)<br/>
    /// <term>5</term> Cross Hotbar 3 (Right)<br/>
    /// <term>6</term> Cross Hotbar 4 (Left)<br/>
    /// <term>7</term> Cross Hotbar 4 (Right)<br/>
    /// <term>8</term> Cross Hotbar 5 (Left)<br/>
    /// <term>9</term> Cross Hotbar 5 (Right)<br/>
    /// <term>10</term> Cross Hotbar 6 (Left)<br/>
    /// <term>11</term> Cross Hotbar 6 (Right)<br/>
    /// <term>12</term> Cross Hotbar 7 (Left)<br/>
    /// <term>13</term> Cross Hotbar 7 (Right)<br/>
    /// <term>14</term> Cross Hotbar 8 (Left)<br/>
    /// <term>15</term> Cross Hotbar 8 (Right)<br/>
    /// <term>16</term> Cycle Up (Right)<br/>
    /// <term>17</term> Cycle Up (Left)<br/>
    /// <term>18</term> Cycle Down (Right)<br/>
    /// <term>19</term> Cycle Down (Left)<br/>
    /// </summary>
    public static class ExtraBarMaps
    {
        /// <summary>L->R Expanded Hold Controls</summary>
        public static readonly Config[] LR =
            { new("HotbarCrossAdvancedSettingRight"), new("HotbarCrossAdvancedSettingRightPvp") };

        /// <summary>R->L Expanded Hold Controls</summary>
        public static readonly Config[] RL =
            { new("HotbarCrossAdvancedSettingLeft"), new("HotbarCrossAdvancedSettingLeftPvp") };

        /// <summary>Left WXHB</summary>
        public static readonly Config[] LL = { new("HotbarWXHBSetLeft"), new("HotbarWXHBSetLeftPvP") };

        /// <summary>Right WXHB</summary>
        public static readonly Config[] RR = { new("HotbarWXHBSetRight"), new("HotbarWXHBSetRightPvP") };
    }

    /// <summary><term>Sliders</term> Transparency settings for Cross Hotbar buttons<br/><br/>Returns 0-100 (Converted by game to alpha 0-255)</summary>
    public class Transparency
    {
        public static readonly Config Standard = new("HotbarXHBAlphaDefault");
        public static readonly Config Active = new("HotbarXHBAlphaActiveSet");
        public static readonly Config Inactive = new("HotbarXHBAlphaInactiveSet");
    }

    /// <summary>Per-bar configuration settings, by [int BarID]</summary>
    public class Hotbar
    {
        /// <summary><term>CheckBox</term> Whether the hotbar is shared between all jobs<br/><br/>
        /// <term>0</term> Job-specific<br/>
        /// <term>1</term> Shared</summary>
        public static readonly Config[] Shared =
        {
            new("HotbarCommon01"),
            new("HotbarCommon02"),
            new("HotbarCommon03"),
            new("HotbarCommon04"),
            new("HotbarCommon05"),
            new("HotbarCommon06"),
            new("HotbarCommon07"),
            new("HotbarCommon08"),
            new("HotbarCommon09"),
            new("HotbarCommon10"),

            new("HotbarCrossCommon01"),
            new("HotbarCrossCommon02"),
            new("HotbarCrossCommon03"),
            new("HotbarCrossCommon04"),
            new("HotbarCrossCommon05"),
            new("HotbarCrossCommon06"),
            new("HotbarCrossCommon07"),
            new("HotbarCrossCommon08")
        };

        /// <summary><term>CheckBox</term> Whether the bar is set to visible<br/><br/>
        /// <term>0</term> Hidden<br/>
        /// <term>1</term> Visible</summary>
        public static readonly Config[] Visible =
        {
            new("HotbarDisp"),
            new("HotbarDisp", 1),
            new("HotbarDisp", 2),
            new("HotbarDisp", 3),
            new("HotbarDisp", 4),
            new("HotbarDisp", 5),
            new("HotbarDisp", 6),
            new("HotbarDisp", 7),
            new("HotbarDisp", 8),
            new("HotbarDisp", 9)
        };

        /// <summary><term>Radio Button</term> The bar's grid layout setting<br/><br/>
        /// <term>0</term> 12x1<br/>
        /// <term>1</term> 6x2<br/>
        /// <term>2</term> 4x3<br/>
        /// <term>3</term> 3x4<br/>
        /// <term>4</term> 2x6<br/>
        /// <term>5</term> 1x12<br/>
        /// </summary>
        public static readonly Config[] GridType =
        {
            new("HotbarDispSetDragType", 2), //this is not the actual name of this setting; it has no name, so we're looking up a setting we know is nearby and offsetting it
            new("HotbarDispSetDragType", 3),
            new("HotbarDispSetDragType", 4),
            new("HotbarDispSetDragType", 5),
            new("HotbarDispSetDragType", 6),
            new("HotbarDispSetDragType", 7),
            new("HotbarDispSetDragType", 8),
            new("HotbarDispSetDragType", 9),
            new("HotbarDispSetDragType", 10),
            new("HotbarDispSetDragType", 11)
        };
    }
}