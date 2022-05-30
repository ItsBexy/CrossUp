namespace CrossUp;

public sealed partial class CrossUp
{
    // EXHB/WXHB CUSTOM MAPPING

    // set Character Configs to match user's override prefs
    private void OverrideMappings(int barID)
    {
        var usePvP = CharConfig.SepPvP && Service.ClientState.IsPvP ? 1 : 0;
        var index = barID - 10;

        if (Config.RemapEx)
        {
            var overrideLR = Config.MappingsEx[0, index];
            var overrideRL = Config.MappingsEx[1, index];
            
            var configLR = CharConfig.LRset[usePvP];
            var configRL = CharConfig.RLset[usePvP];

            if (configLR != overrideLR) configLR.Set(overrideLR);
            if (configRL != overrideRL) configRL.Set(overrideRL);
        }

        if (Config.RemapW)
        {
            var overrideLL = Config.MappingsW[0, index];
            var overrideRR = Config.MappingsW[1, index];

            var configLL = CharConfig.LLset[usePvP];
            var configRR = CharConfig.RRset[usePvP];

            if (configLL != overrideLL) configLL.Set(overrideLL);
            if (configRR != overrideRR) configRR.Set(overrideRR);
        }
    }
}