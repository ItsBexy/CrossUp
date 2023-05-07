using ImGuiNET;
using System;
using System.Numerics;
using static CrossUp.CrossUp;
using ImGui = ImGuiNET.ImGui;
// ReSharper disable NotAccessedField.Local
#pragma warning disable CS8618

namespace CrossUp;

internal sealed partial class CrossUpUI : IDisposable
{
    private static Configuration Config;
    private static CrossUp CrossUp;
    private static Profile Profile => Config.Profiles[Config.UniqueHud ? HudSlot : 0];

    private bool settingsVisible;
    public bool SettingsVisible
    {
        get => settingsVisible;
        set => settingsVisible = value;
    }
    public CrossUpUI(Configuration config, CrossUp crossup)
    {
        Config = config;
        CrossUp = crossup;
    }

    public void Dispose() { }
    public void Draw()
    {
        if (!OldConfigsChecked) PortOldConfigs();
        if (ShowUpdateWarning) DrawMsgWindow();

        if (SettingsVisible && IsSetUp) DrawSettingsWindow();
        if (DebugVisible) DrawDebugWindow();
    }
    private void DrawSettingsWindow()
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(500 * XupGui.Scale, 450 * XupGui.Scale), new Vector2(9999f));
        ImGui.SetNextWindowSize(Config.ConfigWindowSize, ImGuiCond.Always);
        if (!ImGui.Begin("CrossUp", ref settingsVisible, ImGuiWindowFlags.NoScrollbar)) return;

        if (ImGui.BeginTabBar("Nav"))
        {
            LookAndFeel.DrawTab();
            SeparateEx.DrawTab();
            SetSwitching.DrawTab();
            HudOptions.DrawTab();
            TextCommands.DrawTab();

            ImGui.EndTabBar();
        }

        Config.ConfigWindowSize = ImGui.GetWindowSize();

        ImGui.End();
    }
}