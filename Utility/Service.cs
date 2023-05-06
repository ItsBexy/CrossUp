using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable CS8618

namespace CrossUp;

internal class Service
{
    [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] internal static ClientState ClientState { get; private set; }
    [PluginService] internal static Framework Framework { get; private set; }
    [PluginService] internal static Condition Condition { get; private set; }
    [PluginService] internal static GameGui GameGui { get; private set; }
    [PluginService] internal static SigScanner SigScanner { get; private set; }
}