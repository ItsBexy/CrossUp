using System;
using CrossUp.Commands;
using CrossUp.Features;
using CrossUp.Features.Layout;
using CrossUp.Game.Hooks;
using CrossUp.Game.Hotbar;
using CrossUp.UI;
using CrossUp.UI.Localization;
using CrossUp.Utility;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using static CrossUp.Game.Hooks.HudHooks;

#pragma warning disable CS8618

namespace CrossUp;

internal sealed class CrossUp : IDalamudPlugin
{
    public string Name => "CrossUp";
    internal const string MainCommand = "/xup";
    internal static DalamudPluginInterface PluginInterface { get; private set; }
    internal static CommandManager CommandManager { get; private set; }

    internal static CrossUpConfig Config { get; private set; }
    internal static ConfigProfile Profile => Config.Profiles[Config.UniqueHud ? HudSlot : 0];

    internal static CrossUpUI UI { get; private set; }
    private static Hooks Hooks { get; set; }
    private static CrossUpLoc Loc { get; set; }
    private static IPC IPC { get; set; }

    public CrossUp([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
                   [RequiredVersion("1.0")] CommandManager commandManager)
    {
        PluginInterface = pluginInterface;
        PluginInterface.Create<Service>();

        CommandManager = commandManager;
        CommandManager.AddHandler(MainCommand, new(ChatCommands.OnMainCommand) { HelpMessage = Strings.HelpMsg });

        Config = PluginInterface.GetPluginConfig() as CrossUpConfig ?? new CrossUpConfig();
        Config.Initialize(PluginInterface);

        UI = new();
        Hooks = new();
        Loc = new();
        IPC = new();
    }

    /// <summary>Indicates that hotbar addons exist, the player is logged in, and the plugin's features can properly run.</summary>
    internal static bool IsSetUp;
    /// <summary>Sets up the plugin's main features and applies user configs. Runs as soon as <see cref="EventHooks.FrameworkUpdate"/> detects all the hotbar addons. Runs again if the addons are destroyed then restored.</summary>
    internal static void Setup()
    {
        try
        {
            IsSetUp = Bars.GetBases();

            SeparateEx.EnableIfReady();
            Layout.Update(true);
            Layout.ScheduleNudges(10,750);

            Color.SetAll();
        }
        catch (Exception ex)
        {
            PluginLog.LogError($"Exception: Setup Failed!\n{ex}");
            IsSetUp = false;
        }
    }

    /// <summary>Put all modified nodes back in place and remove hooks</summary>
    public void Dispose()
    {
        try { Hooks.Dispose();       } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Remove hooks!\n{ex}"); }
        try { Layout.TidyUp();       } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset Cross Hotbar layout!\n{ex}"); }
        try { Layout.Reset();        } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset Action Bars!\n{ex}"); }
        try { Color.SetAll(true);    } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset colors!\n{ex}"); }
        try { ChatCommands.Remove(); } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't remove main command!\n{ex}"); }
        try { IPC.Dispose();         } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Unregister IPC funcs!\n{ex}"); }
        try { Loc.Dispose();         } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Dispose Localization!\n{ex}"); }
        try { UI.Dispose();          } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Dispose UI!\n{ex}"); }
        IsSetUp = false;
    }
}