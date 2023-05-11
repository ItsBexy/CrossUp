using CrossUp.Game;
using CrossUp.Game.Hotbar;
using CrossUp.Utility;
using Dalamud.Logging;
using static CrossUp.CrossUp;

namespace CrossUp.Features;

/// <summary>Feature enabling the player to change the mapping for WXHB or Expanded Hold based on which Cross Hotbar set is currently selected</summary>
internal class SetSwitching
{
    /// <summary>Responds to the player changing the Cross Hotbar set</summary>
    public static unsafe void HandleSetChange(AddonActionBarBase* barBase)
    {
        PluginLog.LogDebug($"Switched to Cross Hotbar Set {barBase->HotbarID - 9}");
        if (Config.RemapEx || Config.RemapW) Override(barBase->HotbarID);
    }

    /// <summary>Overrides the Character Configuration settings for WXHB / Expanded Hold mappings</summary>
    private static void Override(int barID)
    {
        var pvp = GameConfig.SepPvP && Service.ClientState.IsPvP ? 1 : 0;
        var set = barID - 10;
            
        if (Config.RemapEx) OverrideEx(set, pvp);
        if (Config.RemapW) OverrideW(set, pvp);
    }

    /// <summary>Overrides WXHB mapping based on CrossUp settings</summary>
    private static void OverrideEx(int set, int pvp)
    {
        var overrideLR = Config.MappingsEx[0, set];
        var overrideRL = Config.MappingsEx[1, set];

        var configLR = GameConfig.ExtraBarMaps.LR[pvp];
        var configRL = GameConfig.ExtraBarMaps.RL[pvp];

        if (configLR != overrideLR) configLR.Set(overrideLR);
        if (configRL != overrideRL) configRL.Set(overrideRL);
    }

    /// <summary>Overrides Expanded Hold mapping based on CrossUp settings</summary>
    private static void OverrideW(int set, int pvp)
    {
        var overrideLL = Config.MappingsW[0, set];
        var overrideRR = Config.MappingsW[1, set];

        var configLL = GameConfig.ExtraBarMaps.LL[pvp];
        var configRR = GameConfig.ExtraBarMaps.RR[pvp];

        if (configLL != overrideLL) configLL.Set(overrideLL);
        if (configRR != overrideRR) configRR.Set(overrideRR);
    }
}