using System;
using CrossUp.UI.Tabs;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
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
        PluginInterface.UiBuilder.OpenMainUi += SettingsWindow.Toggle;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= SettingsWindow.Toggle;
        PluginInterface.UiBuilder.OpenMainUi -= SettingsWindow.Toggle;
    }
}

internal sealed class SettingsWindow : Window
{
    public SettingsWindow(string name = "CrossUp##CrossUpSettings", ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow) =>
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(500f, 450f),
            MaximumSize = new(1000f)
        };

    public override void Draw()
    {
        using var tb = ImRaii.TabBar("Nav");
        if (!tb.Success) return;

        LookAndFeel.DrawTab();
        SeparateEx.DrawTab();
        SetSwitching.DrawTab();
        HudOptions.DrawTab();
        TextCommands.DrawTab();
    }
}