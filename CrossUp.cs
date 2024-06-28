using System;
using CrossUp.Commands;
using CrossUp.Features;
using CrossUp.Features.Layout;
using CrossUp.Game;
using CrossUp.Game.Hotbar;
using CrossUp.UI;
using CrossUp.UI.Localization;
using CrossUp.Utility;
using Dalamud.Plugin;
using static CrossUp.Utility.Service;

#pragma warning disable CS8618

namespace CrossUp;

internal sealed class CrossUp : IDalamudPlugin
{
    // ReSharper disable once UnusedMember.Global
    public string Name => "CrossUp";

    internal static IDalamudPluginInterface PluginInterface { get; private set; }
    internal static CrossUpUI UI                           { get; private set; }
    internal static CrossUpConfig Config                   { get; private set; }
    internal static ConfigProfile Profile => Config.Profiles[Config.UniqueHud ? HudData.CurrentSlot : 0];

    private static Events Events   { get; set; }
    private static HudData HudData { get; set; }
    private static ChatCmd ChatCmd { get; set; }
    private static IPC IPC         { get; set; }
    private static CrossUpLoc Loc  { get; set; }

    public CrossUp(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        PluginInterface.Create<Service>();

        Config = PluginInterface.GetPluginConfig() as CrossUpConfig ?? new CrossUpConfig();
        Config.Initialize(PluginInterface);

        Events  = new();
        UI      = new();
        HudData = new();
        ChatCmd = new();
        IPC     = new();
        Loc     = new();
    }

    /// <summary>Indicates that hotbar addons exist, the player is logged in, and the plugin's features can properly run.</summary>
    internal static bool IsSetUp;

    /// <summary>Sets up the plugin's main features and applies user configs. Runs as soon as <see cref="Bars.Cross.OnDraw"/> detects the Cross hotbar. Runs again if the addons are destroyed then restored.</summary>
    internal static void Setup()
    {
        try
        {
            IsSetUp = true;

            SeparateEx.EnableIfReady();
            Layout.Update(true);
            Layout.ScheduleNudges(10,750);

            Color.SetAll();
        }
        catch (Exception ex)
        {
            Log.Error($"Exception: Setup Failed!\n{ex}");
            IsSetUp = false;
        }
    }

    /// <summary>Put all modified nodes back in place and remove hooks</summary>
    public void Dispose()
    {
        try { HudData.Dispose();    } catch (Exception ex) { Log.Error($"Exception on Dispose: Couldn't Remove HUD hooks!\n{ex}"); }
        try { Events.Dispose(); } catch (Exception ex) { Log.Error($"Exception on Dispose: Couldn't Remove event listeners!\n{ex}"); }
        try { Layout.TidyUp();    } catch (Exception ex) { Log.Warning($"Exception on Dispose: Couldn't reset Cross Hotbar layout!\n{ex}"); }
        try { Layout.Reset();     } catch (Exception ex) { Log.Warning($"Exception on Dispose: Couldn't reset Action Bars!\n{ex}"); }
        try { Color.SetAll(true); } catch (Exception ex) { Log.Warning($"Exception on Dispose: Couldn't reset colors!\n{ex}"); }
        try { ChatCmd.Dispose();  } catch (Exception ex) { Log.Error($"Exception on Dispose: Couldn't remove chat commands!\n{ex}"); }
        try { IPC.Dispose();      } catch (Exception ex) { Log.Error($"Exception on Dispose: Couldn't Unregister IPC funcs!\n{ex}"); }
        try { Loc.Dispose();      } catch (Exception ex) { Log.Error($"Exception on Dispose: Couldn't Dispose Localization!\n{ex}"); }
        try { UI.Dispose();       } catch (Exception ex) { Log.Error($"Exception on Dispose: Couldn't Dispose UI!\n{ex}"); }
        IsSetUp = false;
    }
}