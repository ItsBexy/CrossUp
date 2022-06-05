namespace CrossUp;

public sealed partial class CrossUp
{

    /// <summary>Feature enabling the player to change the mapping for WXHB or Expanded Hold based on which Cross Hotbar set is currently selected</summary>
    public class Remap
    {
        /// <summary>Overwrites the relevant Character Configuration settings</summary>
        public static void Override(int barID)
        {
            var pvp = CharConfig.SepPvP && Service.ClientState.IsPvP ? 1 : 0;
            var set = barID - 10;

            if (Config.RemapEx)
            {
                var overrideLR = Config.MappingsEx[0, set];
                var overrideRL = Config.MappingsEx[1, set];
            
                var configLR = CharConfig.ExtraBarMaps.LR[pvp];
                var configRL = CharConfig.ExtraBarMaps.RL[pvp];

                if (configLR != overrideLR) configLR.Set(overrideLR);
                if (configRL != overrideRL) configRL.Set(overrideRL);
            }

            if (Config.RemapW)
            {
                var overrideLL = Config.MappingsW[0, set];
                var overrideRR = Config.MappingsW[1, set];

                var configLL = CharConfig.ExtraBarMaps.LL[pvp];
                var configRR = CharConfig.ExtraBarMaps.RR[pvp];

                if (configLL != overrideLL) configLL.Set(overrideLL);
                if (configRR != overrideRR) configRR.Set(overrideRR);
            }
        }
    }
}