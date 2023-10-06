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

    /// <summary>The Job abbreviation</summary>
    internal static string Abbr => ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation ?? "???";

    /// <summary>The current PvP state</summary>
    public static bool IsPvP => WasPvP = ClientState.IsPvP || ClientState.TerritoryType == 250;

    /// <summary>Previous PvP state</summary>
    private static bool WasPvP;

    /// <summary>True if the player's Job has just changed</summary>
    internal static bool HasChanged => LastKnown != Current || WasPvP != IsPvP;

    /// <summary>Retrieves the ID of a job's PvP hotbar sets (NOT future-proofed for more jobs being added)</summary>
    internal static int PvpID(int job) => job switch
    {
        0 => 43, // shared
        19 => 44, // PLD
        20 => 45, // MNK
        21 => 46, // WAR
        22 => 47, // DRG
        23 => 48, // BRD
        24 => 49, // WHM
        25 => 50, // BLM
        27 => 51, // SMN
        28 => 52, // SCH
        30 => 53, // NIN
        31 => 54, // MCH
        32 => 55, // DRK
        33 => 56, // AST
        34 => 57, // SAM
        35 => 58, // RDM
        37 => 59, // GNB
        38 => 60, // DNC
        39 => 61, // RPR
        40 => 62, // SGE
        _ => job
    };

    /// <summary>Updates the stored bars when the player changes jobs</summary>
    public static void HandleJobChange()
    {
        Log.Debug($"Job Update: {ClientState.LocalPlayer?.ClassJob.Id} {Abbr} ({(IsPvP ? "PvP" : "PvE")})");
        if (!SeparateEx.Ready) return;
        
        Actions.Store(Bars.LR.ID);
        Actions.Store(Bars.RL.ID);
    }
}