using System;
using CrossUp.UI.Tabs;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using static CrossUp.CrossUp;

namespace CrossUp.UI;

internal sealed class CrossUpUI : IDisposable
{
    private readonly WindowSystem WindowSystem = new("CrossUp");

    internal readonly SettingsWindow SettingsWindow = new();

    public CrossUpUI()
    {
        WindowSystem.AddWindow(SettingsWindow);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += SettingsWindow.Toggle;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= SettingsWindow.Toggle;
    }
}

internal sealed class SettingsWindow : Window
{
    public SettingsWindow(string name = "CrossUp##Settings", ImGuiWindowFlags flags = ImGuiWindowFlags.NoScrollbar, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(500f, 450f),
            MaximumSize = new(1000f)
        };
    }

    public override void Draw()
    {
        if (!ImGui.BeginTabBar("Nav")) return;

        LookAndFeel.DrawTab();
        SeparateEx.DrawTab();
        SetSwitching.DrawTab();
        HudOptions.DrawTab();
        TextCommands.DrawTab();

        ImGui.EndTabBar();
    }
}