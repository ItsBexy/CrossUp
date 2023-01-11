using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

namespace CrossUp;

/// <summary>Class for retrieving/setting character configuration options</summary>
public class CharConfig
{
    /// <summary>
    /// Represents a Character Configuration value<br/><br/>
    /// <term cref="Set">Set()</term> Updates the value in-game<br/>
    /// <term>(int)</term> Returns the value itself<br/>
    /// <term>(bool)</term> Returns true if value > 0
    /// </summary>
    public sealed class Config
    {
        public short? ID;
        public uint Index;
        public int Get() => ID == null ? Get(Index) : Get((short)ID);
        public static unsafe int Get(uint index) => Instance->GetIntValue(index);
        public static unsafe int Get(short id) => Instance->GetIntValue(id);
        public bool Set(int val) => ID == null ? Set(Index, val) : Set((short)ID, val);
        private static unsafe bool Set(uint configIndex, int value) => Instance->SetOption(configIndex, value, 1);
        private static unsafe bool Set(short configID, int value)
        {
            for (uint index = 0; index < 683U; ++index) if (Instance->GetOption(index)->OptionID == (ConfigOption)configID) return Set(index, value);
            return false;
        }
        public static implicit operator int(Config cfg) => cfg.Get();
        public static implicit operator bool(Config cfg) => cfg.Get() > 0;
        public byte IntToAlpha => (byte)((100 - (int)this) * 2.55);
    }

    private static readonly unsafe ConfigModule* Instance = ConfigModule.Instance();

    public static class Cross
    {
        /// <summary><term>Checkbox</term> The Cross Hotbar is Enabled</summary>
        public static readonly Config Enabled = new() { ID = 384 };

        /// <summary><term>Checkbox</term> The Cross Hotbar is visible in the HUD</summary>
        public static readonly Config Visible = new() { Index = 546 };
    }

    /// <summary><term>Checkbox</term> User has enabled different settings for PvP vs PvE</summary>
    public static readonly Config SepPvP = new() { ID = 406 };

    /// <summary>
    /// <term>Radio Button</term> Cross Hotbar Display Type<br/><br/>
    /// <term>0</term> D-Pad / Buttons / D-Pad / Buttons<br/>
    /// <term>1</term> D-Pad / D-Pad / Buttons / Buttons
    /// </summary>
    public static readonly Config MixBar = new() { ID = 376 };

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
        public static readonly Config[] LR = { new() { ID = 405 }, new() { ID = 428 } };
        /// <summary>R->L Expanded Hold Controls</summary>
        public static readonly Config[] RL = { new() { ID = 404 }, new() { ID = 427 } };
        /// <summary>Left WXHB</summary>
        public static readonly Config[] LL = { new() { ID = 430 }, new() { ID = 433 } };
        /// <summary>Right WXHB</summary>
        public static readonly Config[] RR = { new() { ID = 431 }, new() { ID = 434 } };
    }

    /// <summary><term>Sliders</term> Transparency settings for Cross Hotbar buttons<br/><br/>Returns 0-100 (Converted by game to alpha 0-255)</summary>
    public class Transparency
    {
        public static readonly Config Standard = new() { ID = 441 };
        public static readonly Config Active = new() { ID = 442 };
        public static readonly Config Inactive = new() { ID = 443 };
    }
    /// <summary>Per-bar configuration settings, by [int BarID]</summary>
    public class Hotbar
    {
        /// <summary><term>CheckBox</term> Whether the hotbar is shared between all jobs<br/><br/>
        /// <term>0</term> Job-specific<br/>
        /// <term>1</term> Shared</summary>
        public static readonly Config[] Shared =
        {   new() { ID = 356 },
            new() { ID = 357 },
            new() { ID = 358 },
            new() { ID = 359 },
            new() { ID = 360 },
            new() { ID = 361 },
            new() { ID = 362 },
            new() { ID = 363 },
            new() { ID = 364 },
            new() { ID = 365 },

            new() { ID = 366 },
            new() { ID = 367 },
            new() { ID = 368 },
            new() { ID = 369 },
            new() { ID = 370 },
            new() { ID = 371 },
            new() { ID = 372 },
            new() { ID = 373 }
        };
        /// <summary><term>CheckBox</term> Whether the bar is set to visible<br/><br/>
        /// <term>0</term> Hidden<br/>
        /// <term>1</term> Visible</summary>
        public static readonly Config[] Visible =
        {   
            new() { Index = 494 },
            new() { Index = 495 },
            new() { Index = 496 },
            new() { Index = 497 },
            new() { Index = 498 },
            new() { Index = 499 },
            new() { Index = 500 },
            new() { Index = 501 },
            new() { Index = 502 },
            new() { Index = 503 }
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
            new() { Index = 510 },
            new() { Index = 511 },
            new() { Index = 512 },
            new() { Index = 513 },
            new() { Index = 514 },
            new() { Index = 515 },
            new() { Index = 516 },
            new() { Index = 517 },
            new() { Index = 518 },
            new() { Index = 519 }
        };
    }

}