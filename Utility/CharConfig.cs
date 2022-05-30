using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Dalamud.Logging;
// ReSharper disable UnusedMember.Global

namespace CrossUp;
public class CharConfig
{
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
    public static readonly Config CrossEnabled = new() { ID = 378 }; // checkbox -- Cross Hotbar Enabled
    public static readonly Config SepPvP = new() { ID = 400 };       // checkbox -- Enable PvP Settings
    public static readonly Config MixBar = new() { ID = 370 };       // radio -- Cross Hotbar Display Type
    public static readonly Config[] LRset = { new() { ID = 399 }, new() { ID = 422 } };  // dropdowns -- selected sets for EXHB and WXHB [PvE, PvP]
    public static readonly Config[] RLset = { new() { ID = 398 }, new() { ID = 421 } };  //    each setting returns  0-19
    public static Config[] LLset = { new() { ID = 424 }, new() { ID = 427 } };  //     0-15 = Left/Right (Alternating) sides of the 8 Cross Hotbar sets
    public static Config[] RRset = { new() { ID = 425 }, new() { ID = 428 } };  //    16-19 = "Cycle" options that grab Right/Left sides of next/prev bar

    public class Transparency   // sliders -- transparency for cross hotbar buttons
    {
        public static readonly Config Standard = new() { ID = 435 };
        public static readonly Config Active = new() { ID = 436 };
        public static readonly Config Inactive = new() { ID = 437 };
    }
    public class Hotbar
    {
        public static readonly Config[] Shared = //checkbox -- bar is shared between jobs
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
        public static readonly Config[] Visible = // checkbox -- bar is visible
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