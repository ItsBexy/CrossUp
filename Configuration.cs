using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace CrossUp;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool SepExBar { get; set; }
    public int lX { get; set; } = -214;
    public int lY { get; set; } = -88;
    public int rX { get; set; } = 214;
    public int rY { get; set; } = -88;
    public int borrowBarL { get; set; } = 8;
    public int borrowBarR { get; set; } = 9;
    public int Split { get; set; }
    public Vector3 selectColor { get; set; } = new(1, 1, 1);
    public bool selectHide { get; set; }
    public bool HidePadlock { get; set; }
    public bool HideSetText { get; set; }
    public Vector2 PadlockOffset { get; set; } = new(0f, 0f);
    public Vector2 SetTextOffset { get; set; } = new(0f, 0f);
    public Vector2 ChangeSetOffset { get; set; } = new(0f, 0f);
    public bool RemapEx { get; set; }
    public int[,] MappingsEx { get; set; } = { { 0, 0, 0, 0, 0, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1 } };
    public bool RemapW { get; set; }
    public int[,] MappingsW { get; set; } = { { 0, 0, 0, 0, 0, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1 } };
    public Vector2 ConfigWindowSize { get; set; } = new(450f,390f);
    public Vector3 GlowA { get; set; } = new(1, 1, 1);
    public Vector3 GlowB { get; set; } = new(1, 1, 1);
    public bool OnlyOneEx { get; set; }

    [NonSerialized]
    private DalamudPluginInterface? PluginInterface;
    public void Initialize(DalamudPluginInterface pluginInterface) => PluginInterface = pluginInterface;
    public void Save() => PluginInterface!.SavePluginConfig(this);

}