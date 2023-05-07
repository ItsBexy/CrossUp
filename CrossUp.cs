using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace CrossUp;

public sealed partial class CrossUp : IDalamudPlugin
{
    public string Name => "CrossUp";
    private const string MainCommand = "/xup";
    private DalamudPluginInterface PluginInterface { get; }
    private CommandManager CommandManager { get; }
    private CrossUpUI CrossUpUI { get; }
    private static Configuration Config { get; set; } = null!;
    private static ResourceManager ResourceManager = null!;

    public CrossUp([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
                   [RequiredVersion("1.0")] CommandManager commandManager)       
    {
        CommandManager = commandManager;
        CommandManager.AddHandler(MainCommand, new CommandInfo(OnMainCommand) { HelpMessage = CrossUpUI.Strings.HelpMsg });
        IpcManager.Register(pluginInterface);

        PluginInterface = pluginInterface;
        Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Initialize(PluginInterface);
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        PluginInterface.Create<Service>();
        ResourceManager = new ResourceManager();

        CrossUpUI = new CrossUpUI(Config, this);

        EnableHooks();
    }

    /// <summary>Indicates that hotbar addons exist, the player is logged in, and the plugin's features can properly run.</summary>
    internal static bool IsSetUp;
    /// <summary>Sets up the plugin's main features and applies user configs. Runs as soon as <see cref="FrameworkUpdate"/> detects all the hotbar addons. Runs again if the addons are destroyed then restored.</summary>
    private static void Setup()
    {
        try
        {

            Bars.GetBases();
            IsSetUp = true;

            Layout.SeparateEx.EnableIfReady();

            Layout.Update(true);
            Layout.ScheduleNudges(10,750);

            Color.SetSelectBG();
            Color.SetPulse();
            Color.SetText();
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
        try { DisableHooks();           } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Remove hooks!\n{ex}"); }
        try { Layout.TidyUp();          } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset Cross Hotbar layout!\n{ex}"); }
        try { Layout.ResetBars();       } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset Action Bars!\n{ex}"); }
        try { Color.SetAll(true);       } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset colors!\n{ex}"); }
        try { DisposeUI();              } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Dispose Plugin Interface!\n{ex}"); }
        try { IpcManager.Unregister();  } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Unregister IPC funcs!\n{ex}"); }
        try { ResourceManager.Dispose();} catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Dispose ResourceManager!\n{ex}"); }
        IsSetUp = false;
    }

    private void DrawUI() => CrossUpUI?.Draw();
    private void DrawConfigUI() => CrossUpUI.SettingsVisible = true;
    private void DisposeUI()
    {
        CommandManager.RemoveHandler(MainCommand);
        CrossUpUI.Dispose();
    }
}