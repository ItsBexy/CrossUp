using CrossUp.Features.Layout;
using CrossUp.Game.Hotbar;
using static CrossUp.Utility.Service;

namespace CrossUp.Game;

internal class Job
{
    /// <summary>Gets player's current Job ID</summary>
    internal static int Current
    {
        get
        {
            var job = ClientState.LocalPlayer?.ClassJob.Id;
            return LastKnown = job != null ? (int)job : 0;
        }
    }

    /// <summary>The player's last known job</summary>
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

    /// <summary>Updates the stored bars when the player changes jobs</summary>
    public static void HandleJobChange()
    {
        Log.Debug($"Job Change: {ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation ?? "???"}");
        if (!SeparateEx.Ready) return;

        Actions.Store(Bars.LR.ID);
        Actions.Store(Bars.RL.ID);
    }
}