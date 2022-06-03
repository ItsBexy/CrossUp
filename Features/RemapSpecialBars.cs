namespace CrossUp;

public sealed partial class CrossUp
{
    // EXHB/WXHB CUSTOM MAPPING

    /// <summary>Set Character Configs for EXHB/WXHB to match user's override preferences</summary>
    private void OverrideMappings(int barID)
    {
        var pvp = CharConfig.SepPvP && Service.ClientState.IsPvP ? 1 : 0;
        var index = barID - 10;

        if (Config.RemapEx)
        {
            var overrideLR = Config.MappingsEx[0, index];
            var overrideRL = Config.MappingsEx[1, index];
            
            var configLR = CharConfig.ExtraBarMaps.LR[pvp];
            var configRL = CharConfig.ExtraBarMaps.RL[pvp];

            if (configLR != overrideLR) configLR.Set(overrideLR);
            if (configRL != overrideRL) configRL.Set(overrideRL);
        }

        if (Config.RemapW)
        {
            var overrideLL = Config.MappingsW[0, index];
            var overrideRR = Config.MappingsW[1, index];

            var configLL = CharConfig.ExtraBarMaps.LL[pvp];
            var configRR = CharConfig.ExtraBarMaps.RR[pvp];

            if (configLL != overrideLL) configLL.Set(overrideLL);
            if (configRR != overrideRR) configRR.Set(overrideRR);
        }
    }
}