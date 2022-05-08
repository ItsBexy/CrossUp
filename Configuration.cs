using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace CrossUp
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool SepExBar { get; set; } = false;
        public int lX { get; set; } = -214;
        public int lY { get; set; } = -88;
        public int rX { get; set; } = 214;
        public int rY { get; set; } = -88;
        public int borrowBarL { get; set; } = 8;
        public int borrowBarR { get; set; } = 9;
        public int Split { get; set; } = 0;
        public Vector3 selectColor { get; set; } = new(1, 1, 1);
        public bool selectHide { get; set; } = false;
        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
        public Vector2 PadlockOffset { get; set; } = new(0f, 0f);
        public Vector2 SetTextOffset { get; set; } = new(0f, 0f);
        public Vector2 ChangeSetOffset { get; set; } = new(0f, 0f);


        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
