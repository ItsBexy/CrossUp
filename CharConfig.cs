using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    private static readonly ConfigModule* CharConfigs = ConfigModule.Instance();
    private static int GetCharConfig(uint configIndex) => CharConfigs->GetIntValue(configIndex);
    private static int GetCharConfig(short configID) => CharConfigs->GetIntValue(configID);
    private static void SetCharConfig(uint configIndex, int value) => CharConfigs->SetOption(configIndex, value, 1);
    private static void SetCharConfig(short configID, int value)
    {
        var option = (ConfigOption)configID;
        for (uint index = 0; index < 683U; ++index)
        {
            if (CharConfigs->GetOption(index)->OptionID != option) continue;
            SetCharConfig(index, value);
            break;
        }
    }

    // relevant character configuration lookups
    // NOTE: (uint) will get/set by index, (short) will get/set by ID. NOT the same thing
    public readonly struct ConfigID
    {
        public const short CrossEnabled = 378; // checkbox -- Cross Hotbar Enabled
        public const short SepPvP = 400; // checkbox -- Enable PvP Settings
        public const short MixBar = 370; // radio -- Cross Hotbar Display Type
                                         //          0 = dpad / button / dpad / button
                                         //          1 = dpad / dpad / button / button

        // dropdowns -- selected sets for EXHB and WXHB [PvE, PvP]
        // returns 0-19 representing the left/right sides of each bar, and the four cycle options
        public static readonly short[] LRset = { 399, 422 };
        public static readonly short[] RLset = { 398, 421 };
        public static readonly short[] LLset = { 424, 427 };
        public static readonly short[] RRset = { 425, 428 };

        public struct Transparency // slider -- transparency for cross hotbar buttons
        {
            public const short Standard = 435;
            public const short Active = 436;
            public const short Inactive = 437;
        }

        public struct Hotbar // kb/mouse hotbars 1-10 (0-9 internally)
        {
            public static readonly short[] Shared = { 350, 351, 352, 353, 354, 355, 356, 357, 358, 359 }; //checkbox -- bar is shared between jobs

                // can only seem to find these by index
            public static readonly uint[] Visible = { 485, 486, 487, 488, 489, 490, 491, 492, 493, 494 }; //checkbox -- bar is visible
            public static readonly uint[] GridType = { 501, 502, 503, 504, 505, 506, 507, 508, 509, 510 }; //radio    -- bar grid type (0-5)
        }
    }

        // For debugging
    // ReSharper disable once UnusedMember.Global
    public void LogCharConfigs(uint start, uint end = 0)
    {
        if (end < start) end = start;
        for (var i = start; i <= end; i++) PluginLog.Log(i + " " + GetCharConfig(i));
    }

    // ReSharper disable once UnusedMember.Global
    public void LogCharConfigs(short start, short end = 0)
    {
        if (end < start) end = start;
        for (var i = start; i <= end; i++) PluginLog.Log(i + " " + GetCharConfig(i));
    }

}