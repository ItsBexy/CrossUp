using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable UnusedMember.Global

namespace CrossUp;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public Vector2 ConfigWindowSize { get; set; } = new(500, 450);
    public Vector2 DebugWindowSize { get; set; } = new(600, 800);

    // unique configs per HUD Slot
    public bool UniqueHud { get; set; }
    public Profile[] Profiles { get; set; } = { new(), new(), new(), new(), new() };

    // Separate Expanded Hold Configs
    public int LRborrow { get; set; } = -1;
    public int RLborrow { get; set; } = -1;

    // Remapping Configs
    public bool RemapEx { get; set; }
    public bool RemapW { get; set; }
    public int[,] MappingsEx { get; set; } = { { 0, 0, 0, 0, 0, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1 } };
    public int[,] MappingsW { get; set; } = { { 0, 0, 0, 0, 0, 0, 0, 0 }, { 1, 1, 1, 1, 1, 1, 1, 1 } };

    [NonSerialized] private DalamudPluginInterface? PluginInterface;
    public void Initialize(DalamudPluginInterface pluginInterface) => PluginInterface = pluginInterface;
    public void Save() => PluginInterface!.SavePluginConfig(this);

    //DEPRECATED
    public bool? SepExBar { get; set; }
    public Vector2? LRpos { get; set; }
    public Vector2? RLpos { get; set; }
    public bool? OnlyOneEx { get; set; }
    public int? Split { get; set; }
    public bool? LockCenter { get; set; }
    public Vector2? PadlockOffset { get; set; }
    public Vector2? SetTextOffset { get; set; }
    public Vector2? ChangeSetOffset { get; set; }
    public bool? HidePadlock { get; set; }
    public bool? HideSetText { get; set; }
    public bool? HideTriggerText { get; set; }
    public bool? HideUnassigned { get; set; }
    public bool? HideSelect { get; set; } = null;
    public short? DisposeBaseX { get; set; }
    public float? DisposeRootX { get; set; }
    public Vector3? SelectColorMultiply { get; set; }
    public int? SelectDisplayType { get; set; }
    public Vector3? GlowA { get; set; }
    public Vector3? GlowB { get; set; }
    public Vector3? TextColor { get; set; }
    public Vector3? TextGlow { get; set; }
    public Vector3? BorderColor { get; set; }
    public bool? CombatFadeInOut { get; set; }
    public int? TranspOutOfCombat { get; set; }
    public int? TranspInCombat { get; set; }
}

public class Profile
{
    public Profile() { }
    public Profile(Profile original) //copy constructor
    {
        SplitOn = original.SplitOn;
        SplitDist = original.SplitDist;
        PadlockOffset = original.PadlockOffset;
        SetTextOffset = original.SetTextOffset;
        ChangeSetOffset = original.ChangeSetOffset;
        HidePadlock = original.HidePadlock;
        HideSetText = original.HideSetText;
        HideTriggerText = original.HideTriggerText;
        HideUnassigned = original.HideUnassigned;
        SelectColorMultiply = original.SelectColorMultiply;
        SelectBlend = original.SelectBlend;
        SelectStyle = original.SelectStyle;
        GlowA = original.GlowA;
        GlowB = original.GlowB;
        TextColor = original.TextColor;
        TextGlow = original.TextGlow;
        BorderColor = original.BorderColor;
        SepExBar = original.SepExBar;
        LRpos = original.LRpos;
        RLpos = original.RLpos;
        OnlyOneEx = original.OnlyOneEx;
        CombatFadeInOut = original.CombatFadeInOut;
        TranspOutOfCombat = original.TranspOutOfCombat;
        TranspInCombat = original.TranspInCombat;
    }
    public bool SplitOn { get; set; }
    public int SplitDist { get; set; } = 100;
    public int CenterPoint { get; set; }
    public Vector2 PadlockOffset { get; set; } = new(0);
    public Vector2 SetTextOffset { get; set; } = new(0);
    public Vector2 ChangeSetOffset { get; set; } = new(0);
    public bool HidePadlock { get; set; }
    public bool HideSetText { get; set; }
    public bool HideTriggerText { get; set; }
    public bool HideUnassigned { get; set; }
    public Vector3 SelectColorMultiply { get; set; } = CrossUp.Color.Preset.MultiplyNeutral;
    public int SelectBlend { get; set; } //0: Normal, 2: Dodge
    public int SelectStyle { get; set; } //0: Solid, 1: Frame, 2: Hide
    public Vector3 GlowA { get; set; } = CrossUp.Color.Preset.White;
    public Vector3 GlowB { get; set; } = CrossUp.Color.Preset.White;
    public Vector3 TextColor { get; set; } = CrossUp.Color.Preset.White;
    public Vector3 TextGlow { get; set; } = CrossUp.Color.Preset.TextGlow;
    public Vector3 BorderColor { get; set; } = CrossUp.Color.Preset.White;
    public bool SepExBar { get; set; }
    public Vector2 LRpos { get; set; } = new(-214, -88);
    public Vector2 RLpos { get; set; } = new(214, -88);
    public bool OnlyOneEx { get; set; }
    public bool CombatFadeInOut { get; set; }
    public int TranspOutOfCombat { get; set; } = 100;
    public int TranspInCombat { get; set; }
}