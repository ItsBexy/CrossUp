using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Dalamud.Logging;
// ReSharper disable UnusedMember.Global

namespace CrossUp;

/// <summary>Class for retrieving/setting character configuration options</summary>
public class CharConfig
{
    /// <summary>
    /// Represents a Character Configuration value<br/><br/>
    /// Set() Updates the value in-game<br/>
    /// (int) Returns the value itself<br/>
    /// (bool) Returns true if value > 0<br/>
    /// </summary>
    public class Config
    {
        public short? ID;
        public uint Index;
        private int Get() => ID == null ? Get(Index) : Get((short)ID);
        private static unsafe int Get(uint index) => Instance->GetIntValue(index);
        private static unsafe int Get(short id) => Instance->GetIntValue(id);
        public bool Set(int val) => ID == null ? Set(Index, val) : Set((short)ID, val);
        private static unsafe bool Set(uint configIndex, int value) => Instance->SetOption(configIndex, value, 1);
        private static unsafe bool Set(short configID, int value)
        {
            for (uint index = 0; index < 683U; ++index) if (Instance->GetOption(index)->OptionID == (ConfigOption)configID) return Set(index, value);
            return false;
        }
        public static implicit operator int(Config cfg) => cfg.Get();
        public static implicit operator bool(Config cfg) => cfg.Get() > 0;
    }

    private static readonly unsafe ConfigModule* Instance = ConfigModule.Instance();

    /// <summary>Checkbox: The Cross Hotbar is Enabled</summary>
    public static readonly Config CrossEnabled = new() { ID = 378 }; // checkbox -- Cross Hotbar Enabled

    /// <summary>Checkbox: User has enabled different settings for PvP vs PvE</summary>
    public static readonly Config SepPvP = new() { ID = 400 };

    /// <summary>
    /// Radio: Cross Hotbar Display Type<br/>
    /// 0 = D-Pad / Buttons / D-Pad / Buttons<br/>
    /// 1 = D-Pad / D-Pad / Buttons / Buttons
    /// </summary>
    public static readonly Config MixBar = new() { ID = 370 };

    /// <summary>Dropdowns: Mappings for Additional Cross Hotbars [0: PvE, 1: PvP] <br/>
    /// 0 = Set 1 (Left)<br/>
    /// 1 = Set 1 (Right)<br/>
    /// 2 = Set 2 (Left)<br/>
    /// 3 = Set 3 (Right)<br/>
    /// 4 = Set 3 (Left)<br/>
    /// 5 = Set 3 (Right)<br/>
    /// 6 = Set 4 (Left)<br/>
    /// 7 = Set 4 (Right)<br/>
    /// 8 = Set 5 (Left)<br/>
    /// 9 = Set 5 (Right)<br/>
    /// 10 = Set 6 (Left)<br/>
    /// 11 = Set 6 (Right)<br/>
    /// 12 = Set 7 (Left)<br/>
    /// 13 = Set 7 (Right)<br/>
    /// 14 = Set 8 (Left)<br/>
    /// 15 = Set 8 (Right)<br/>
    /// 16 = Cycle Up (Right)<br/>
    /// 17 = Cycle Up (Left)<br/>
    /// 18 = Cycle Down (Right)<br/>
    /// 19 = Cycle Down (Left)<br/>
    /// </summary>
    public static class ExtraBarMaps
    {
        /// <summary>L->R Expanded Hold Controls</summary>
        public static readonly Config[] LR = { new() { ID = 399 }, new() { ID = 422 } };
        /// <summary>R->L Expanded Hold Controls</summary>
        public static readonly Config[] RL = { new() { ID = 398 }, new() { ID = 421 } };
        /// <summary>Left WXHB</summary>
        public static readonly Config[] LL = { new() { ID = 424 }, new() { ID = 427 } };
        /// <summary>Right WXHB</summary>
        public static readonly Config[] RR = { new() { ID = 425 }, new() { ID = 428 } };
    }

    /// <summary>Sliders: Transparency settings for Cross Hotbar<br/><br/>0-100 (Converted by game to alpha 0-255)</summary>
    public class Transparency
    {
        public static readonly Config Standard = new() { ID = 435 };
        public static readonly Config Active = new() { ID = 436 };
        public static readonly Config Inactive = new() { ID = 437 };
    }
    /// <summary>Per-bar configuration settings, by [int BarID]</summary>
    public class Hotbar
    {
        /// <summary>Checkbox: Whether the hotbar is shared between all jobs<br/><br/>
        /// 0 = Job-specific<br/>
        /// 1 = Shared</summary>
        public static readonly Config[] Shared =
        {   new() { ID = 350 },
            new() { ID = 351 },
            new() { ID = 352 },
            new() { ID = 353 },
            new() { ID = 354 },
            new() { ID = 355 },
            new() { ID = 356 },
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
            new() { ID = 367 }
        };
        /// <summary>Checkbox: Whether the bar is set to visible<br/><br/>
        /// 0 = Hidden<br/>
        /// 1 = Visible</summary>
        public static readonly Config[] Visible =
        {   
            new() { Index = 485 },
            new() { Index = 486 },
            new() { Index = 487 },
            new() { Index = 488 },
            new() { Index = 489 },
            new() { Index = 490 },
            new() { Index = 491 },
            new() { Index = 492 },
            new() { Index = 493 },
            new() { Index = 494 }
        };
        /// <summary>Radio: The bar's grid layout setting<br/><br/>
        /// 0 = 12x1<br/>
        /// 2 = 6x2<br/>
        /// 3 = 4x3<br/>
        /// 4 = 3x4<br/>
        /// 5 = 2x6<br/>
        /// 6 = 1x12<br/>
        /// </summary>
        public static readonly Config[] GridType = // radio    -- bar grid type (0-5)
        {
            new() { Index = 501 },
            new() { Index = 502 },
            new() { Index = 503 },
            new() { Index = 504 },
            new() { Index = 505 },
            new() { Index = 506 },
            new() { Index = 507 },
            new() { Index = 508 },
            new() { Index = 509 },
            new() { Index = 510 }
        };
    }
}