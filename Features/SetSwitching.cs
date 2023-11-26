using CrossUp.Game;
using static CrossUp.CrossUp;

namespace CrossUp.Features;

/// <summary>Feature enabling the player to change the mapping for WXHB or Expanded Hold based on which Cross Hotbar set is currently selected</summary>
internal class SetSwitching
{
    /// <summary>Responds to the player changing the Cross Hotbar set</summary>
    public static void HandleSetChange(byte id)
    {
        if (Config.RemapEx || Config.RemapW) Override(id);
    }

    /// <summary>Overrides the Character Configuration settings for WXHB / Expanded Hold mappings</summary>
    private static void Override(int barID)
    {
        var mode = GameConfig.Cross.SepPvP && Job.IsPvP ? 1 : 0;
        var set = barID - 10;

        if (Config.RemapEx) OverrideEx(set, mode);
        if (Config.RemapW) OverrideW(set, mode);
    }

    /// <summary>Overrides WXHB mapping based on CrossUp settings</summary>
    private static void OverrideEx(int set, int mode)
    {
        var overrideLR = Config.MappingsEx[0, set];
        var overrideRL = Config.MappingsEx[1, set];

        var configLR = GameConfig.Cross.ExMaps.LR[mode];
        var configRL = GameConfig.Cross.ExMaps.RL[mode];

        if (configLR != overrideLR) configLR.Set(overrideLR);
        if (configRL != overrideRL) configRL.Set(overrideRL);
    }

    /// <summary>Overrides Expanded Hold mapping based on CrossUp settings</summary>
    private static void OverrideW(int set, int mode)
    {
        var overrideLL = Config.MappingsW[0, set];
        var overrideRR = Config.MappingsW[1, set];

        var configLL = GameConfig.Cross.ExMaps.LL[mode];
        var configRR = GameConfig.Cross.ExMaps.RR[mode];

        if (configLL != overrideLL) configLL.Set(overrideLL);
        if (configRR != overrideRR) configRR.Set(overrideRR);
    }
}