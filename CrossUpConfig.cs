using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace CrossUp;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public Vector2 ConfigWindowSize { get; set; } = new(450f, 650f);

    // Separate Expanded Hold Configs
    public bool SepExBar { get; set; }
    public int LRborrow { get; set; } = 8;
    public int RLborrow { get; set; } = 9;
    public Vector2 LRpos { get; set; } = new() { X = -214, Y = -88 };
    public Vector2 RLpos { get; set; } = new() { X = 214, Y = -88 };
    public bool OnlyOneEx { get; set; }

    // Cross Hotbar Layout Configs
    public int Split { get; set; }
    public bool LockCenter { get; set; }
    public Vector2 PadlockOffset { get; set; } = new(0f, 0f);
    public Vector2 SetTextOffset { get; set; } = new(0f, 0f);
    public Vector2 ChangeSetOffset { get; set; } = new(0f, 0f);
    public bool HidePadlock { get; set; }
    public bool HideSetText { get; set; }
    public bool HideTriggerText { get; set; }
    public bool HideUnassigned { get; set; }
    public bool HideSelect { get; set; }
    public short? DisposeBaseX { get; set; }
    public float? DisposeRootX { get; set; }

    // Color Configs
    public Vector3 SelectColorMultiply { get; set; } = CrossUp.Color.Preset.MultiplyNeutral;
    public int SelectDisplayType { get; set; }
    public Vector3 GlowA { get; set; } = CrossUp.Color.Preset.White;
    public Vector3 GlowB { get; set; } = CrossUp.Color.Preset.White;
    public Vector3 TextColor { get; set; } = CrossUp.Color.Preset.White;
    public Vector3 TextGlow { get; set; } = CrossUp.Color.Preset.TextGlow;
    public Vector3 BorderColor { get; set; } = CrossUp.Color.Preset.White;

    // Remapping Configs
    public bool RemapEx { get; set; }
    public bool RemapW { get; set; }
    public int[,] MappingsEx { get; set; } = { { 0, 0, 0, 0, 0, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1 } };
    public int[,] MappingsW { get; set; } = { { 0, 0, 0, 0, 0, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1 } };

    [NonSerialized]
    private DalamudPluginInterface? PluginInterface;
    public void Initialize(DalamudPluginInterface pluginInterface) => PluginInterface = pluginInterface;
    public void Save() => PluginInterface!.SavePluginConfig(this);
}