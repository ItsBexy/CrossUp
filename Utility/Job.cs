namespace CrossUp;

public sealed partial class CrossUp
{
    /// <summary>Checks if the player is in a PvP match or in the Wolves' Den</summary>
    private static bool IsPvP => Service.ClientState.IsPvP || Service.ClientState.TerritoryType == 250;

    public class Job
    {
        /// <summary>Gets player's current Job ID</summary>
        internal static int Current
        {
            get
            {
                var job = Service.ClientState.LocalPlayer?.ClassJob.Id;
                return LastKnown = job != null ? (int)job : 0;
            }
        }

        /// <summary>Gets player's Job abbreviation</summary>
        internal static string Abbr => Service.ClientState.LocalPlayer?.ClassJob.GameData != null
            ? Service.ClientState.LocalPlayer?.ClassJob.GameData.Abbreviation ?? "???"
            : "???";

        private static int LastKnown;

        /// <summary>True if the player's Job has just changed</summary>
        internal static bool HasChanged => LastKnown != Current;

        /// <summary>Retrieves the ID of a job's PvP hotbar sets (NOT future-proofed for more jobs being added)</summary>
        internal static int PvpID(int job) => job switch
        {
            0 => 41,
            19 => 42,
            20 => 43,
            21 => 44,
            22 => 45,
            23 => 46,
            24 => 47,
            25 => 48,
            27 => 49,
            28 => 50,
            30 => 51,
            31 => 52,
            32 => 53,
            33 => 54,
            34 => 55,
            35 => 56,
            37 => 57,
            38 => 58,
            39 => 59,
            40 => 60,
            _ => job
        };
    }
}