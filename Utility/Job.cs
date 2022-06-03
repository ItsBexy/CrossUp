using Dalamud.Logging;

namespace CrossUp;

public sealed partial class CrossUp
{
    /// <summary>Gets player's current Job ID</summary>
    private static int PlayerJob
    {
        get
        {
            var job = Service.ClientState.LocalPlayer?.ClassJob.Id;
            return LastKnownJob = job != null ? (int)job : 0;
        }
    }

    private static int LastKnownJob;
    /// <summary>True if the player's Job has just changed</summary>
    private static bool JobChanged => LastKnownJob != PlayerJob;

    /// <summary>Checks if the player is in a PvP match or in the Wolves' Den</summary>
    private static bool PvpArea => Service.ClientState.IsPvP || Service.ClientState.TerritoryType == 250;

    /// <summary>Retrieves the ID of a job's PvP hotbar sets (NOT future-proofed for more jobs being added)</summary>
    private static int PvpJob(int job) => job switch { 0 => 41,
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
                                                       _ => job };
}