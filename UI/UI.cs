using CrossUp.UI.Windows;
using System;
using static CrossUp.CrossUp;

namespace CrossUp.UI;

internal sealed class CrossUpUI : IDisposable
{
    internal readonly SettingsWindow SettingsWindow = new();
    internal readonly DebugWindow DebugWindow = new();

    public CrossUpUI()
    {
        PluginInterface.UiBuilder.Draw += Draw;
        PluginInterface.UiBuilder.OpenConfigUi += Open;
    }

    private void Draw()
    {
        if (SettingsWindow.Show && IsSetUp) SettingsWindow.Draw();
        if (DebugWindow.Show) DebugWindow.Draw();
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= Open;
    }

    private void Open() => SettingsWindow.Show = true;
}