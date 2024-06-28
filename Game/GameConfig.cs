using Dalamud.Game.Config;
using System;
using CrossUp.Utility;
// ReSharper disable UnusedMember.Global

namespace CrossUp.Game;

/// <summary>Class for retrieving/setting character configuration options</summary>
public static class GameConfig
{
    public readonly struct GameOption
    {
        private readonly dynamic Option;

        internal GameOption(UiConfigOption option) => Option = option;
        internal GameOption(UiControlOption option) => Option = option;
        internal GameOption(SystemConfigOption option) => Option = option;

        internal void Set(int value) => Service.DalamudGameConfig.Set(Option, (uint)value);
        internal void Set(uint value) => Service.DalamudGameConfig.Set(Option, value);
        internal void Set(bool value) => Service.DalamudGameConfig.Set(Option, value);

        public static implicit operator bool(GameOption o)
        {
            Service.DalamudGameConfig.TryGet(o.Option, out bool val);
            return val;
        }

        public static implicit operator uint(GameOption o)
        {
            Service.DalamudGameConfig.TryGet(o.Option, out uint val);
            return val;
        }

        public static implicit operator int(GameOption o)
        {
            Service.DalamudGameConfig.TryGet(o.Option, out uint val);
            return (int)val;
        }
    }

    /// <summary>Converts to a hex value (relevant for color options)</summary>
    internal static string UintToHex(uint u) => Convert.ToHexString(BitConverter.GetBytes(u));

    /// <summary>Cross Hotbar options</summary>
    internal static class Cross
    {

        /// <summary><term>Checkbox</term> The Cross Hotbar is Enabled</summary>
        internal static readonly GameOption Enabled = new(UiControlOption.HotbarCrossDispType);

        /// <summary>Expanded hold controls are enabled in PvE</summary>
        private static readonly GameOption EnabledExPvE = new(UiConfigOption.HotbarCrossAdvancedSetting);

        /// <summary>Expanded hold controls are enabled in PvP</summary>
        private static readonly GameOption EnabledExPvP = new(UiConfigOption.HotbarCrossAdvancedSettingPvp);

        /// <summary>Gets Expanded hold state based on PvP state</summary>
        internal static GameOption EnabledEx => (Job.IsPvP && SepPvP) ? EnabledExPvP : EnabledExPvE;

        /// <summary><term>Checkbox</term> The Cross Hotbar is visible in the HUD</summary>
        internal static readonly GameOption Visible = new(UiControlOption.HotbarCrossDispAlways);

        /// <summary>
        /// <term>Radio Button</term> Cross Hotbar Display Type<br/><br/>
        /// <term>0</term> D-Pad / Buttons / D-Pad / Buttons<br/>
        /// <term>1</term> D-Pad / D-Pad / Buttons / Buttons
        /// </summary>
        internal static readonly GameOption MixBar = new(UiConfigOption.HotbarCrossDisp);

        /// <summary><term>Checkbox</term> User has enabled different settings for PvP vs PvE</summary>
        internal static readonly GameOption SepPvP = new(UiConfigOption.HotbarCrossSetPvpModeActive);


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
        internal static class ExMaps
        {
            /// <summary>L->R Expanded Hold Controls</summary>
            internal static readonly GameOption[] LR = { new(UiConfigOption.HotbarCrossAdvancedSettingRight), new(UiConfigOption.HotbarCrossAdvancedSettingRightPvp) };

            /// <summary>R->L Expanded Hold Controls</summary>
            internal static readonly GameOption[] RL = { new(UiConfigOption.HotbarCrossAdvancedSettingLeft), new(UiConfigOption.HotbarCrossAdvancedSettingLeftPvp) };

            /// <summary>Left WXHB</summary>
            internal static readonly GameOption[] LL = { new(UiConfigOption.HotbarWXHBSetLeft), new(UiConfigOption.HotbarWXHBSetLeftPvP) };

            /// <summary>Right WXHB</summary>
            internal static readonly GameOption[] RR = { new(UiConfigOption.HotbarWXHBSetRight), new(UiConfigOption.HotbarWXHBSetRightPvP) };
        }

        /// <summary><term>Sliders</term> Transparency settings for Cross Hotbar buttons<br/><br/>Returns 0-100 (Converted by game to alpha 0-255)</summary>
        internal class Transparency
        {
            internal static readonly GameOption Standard = new(UiConfigOption.HotbarXHBAlphaDefault);
            internal static readonly GameOption Active = new(UiConfigOption.HotbarXHBAlphaActiveSet);
            internal static readonly GameOption Inactive = new(UiConfigOption.HotbarXHBAlphaInactiveSet);
        }
    }

    /// <summary>Per-bar configuration settings, by [int BarID]</summary>
    internal class Hotbar
    {
        /// <summary><term>CheckBox</term> Whether the hotbar is shared between all jobs<br/><br/>
        /// <term>0</term> Job-specific<br/>
        /// <term>1</term> Shared</summary>
        internal static readonly GameOption[] Shared =
        {
            new(UiConfigOption.HotbarCommon01),
            new(UiConfigOption.HotbarCommon02),
            new(UiConfigOption.HotbarCommon03),
            new(UiConfigOption.HotbarCommon04),
            new(UiConfigOption.HotbarCommon05),
            new(UiConfigOption.HotbarCommon06),
            new(UiConfigOption.HotbarCommon07),
            new(UiConfigOption.HotbarCommon08),
            new(UiConfigOption.HotbarCommon09),
            new(UiConfigOption.HotbarCommon10),

            new(UiConfigOption.HotbarCrossCommon01),
            new(UiConfigOption.HotbarCrossCommon02),
            new(UiConfigOption.HotbarCrossCommon03),
            new(UiConfigOption.HotbarCrossCommon04),
            new(UiConfigOption.HotbarCrossCommon05),
            new(UiConfigOption.HotbarCrossCommon06),
            new(UiConfigOption.HotbarCrossCommon07),
            new(UiConfigOption.HotbarCrossCommon08)
        };

        /// <summary>
        /// <term>Bitmask</term> Represents the visibility setting for each action bar. Use <see cref="GetVis"/> and <see cref="SetVis"/> to read / set the visibility of individual bars.
        /// </summary>
        private static readonly GameOption VisMask = new(UiControlOption.HotbarDisp);

        /// <summary>
        /// Gets the visibility of an action bar
        /// </summary>
        /// <param name="id">The bar's ID</param>
        /// <returns></returns>
        public static bool GetVis(int id)
        {
            if (id is < 0 or > 9) return false;
            var offset = (int)Math.Pow(2, id);
            return (VisMask & offset) == offset;
        }

        /// <summary>
        /// Sets the visibility of an action bar
        /// </summary>
        /// <param name="id">The bar's ID</param>
        /// <param name="show">The visibility state to set</param>
        public static void SetVis(int id, bool show)
        {
            if (id is < 0 or > 9) return;

            var mask = (int)VisMask;
            var offset = (int)Math.Pow(2, id);

            if (show) mask |= offset;
            else mask &= ~offset;

            VisMask.Set(mask);
        }
    }

}