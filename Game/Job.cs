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
    private static string Abbr => ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation ?? "???";

    /// <summary>The current PvP state</summary>
    public static bool IsPvP => WasPvP = ClientState.IsPvP || ClientState.TerritoryType == 250;

    /// <summary>Previous PvP state</summary>
    private static bool WasPvP;

    /// <summary>True if the player's Job has just changed</summary>
    internal static bool HasChanged => LastKnown != Current || WasPvP != IsPvP;

    /// <summary>Retrieves the ID of a job's PvP hotbar sets (NOT future-proofed for more jobs being added)</summary>
    internal static unsafe int PvpID(int job) => Actions.RaptureModule->GetPvPSavedHotbarIndexForClassJobId((uint)job);

    /// <summary>Updates the stored bars when the player changes jobs</summary>
    public static void HandleJobChange()
    {
        Log.Debug($"Job Update: {ClientState.LocalPlayer?.ClassJob.Id} {Abbr} ({(IsPvP ? "PvP" : "PvE")})");
        if (!SeparateEx.Ready) return;

        Actions.Store(Bars.LR.ID);
        Actions.Store(Bars.RL.ID);
    }
}