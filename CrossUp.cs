using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace CrossUp;

public sealed unsafe partial class CrossUp : IDalamudPlugin
{
    public string Name => "CrossUp";
    private const string MainCommand = "/xup";
    private DalamudPluginInterface PluginInterface { get; }
    private CommandManager CommandManager { get; }
    private static Configuration Config { get; set; } = null!;
    private CrossUpUI CrossUpUI { get; }

    public CrossUp(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
        CommandManager = commandManager;
        CommandManager.AddHandler(MainCommand, new CommandInfo(OnMainCommand) { HelpMessage = CrossUpUI.Strings.HelpMsg });

        PluginInterface = pluginInterface;
        Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Initialize(PluginInterface);
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        pluginInterface.Create<Service>();

        CrossUpUI = new CrossUpUI(Config, this, PluginInterface, CommandManager);

        EnableHooks();

        IsSetUp = false;
    }

    /// <summary>Whether the plugin has set its features up</summary>
    internal static bool IsSetUp;

    /// <summary>Sets up the plugin's main features and applies user configs</summary>
    private static void Setup()
    {
        try
        {
            IsSetUp = true;
            Bars.Init();

            if (Layout.SeparateEx.Ready) Layout.SeparateEx.Enable();
            Color.SetSelectBG();
            Color.SetPulse();
            Color.SetText();
            Layout.Update(true, true);
            Layout.ScheduleNudges(10);
        }
        catch (Exception ex)
        {
            PluginLog.LogError("Exception: Setup Failed!\n"+ex);
            IsSetUp = false;
        }
    }

    /// <summary>Restores the last known X coordinates of the Cross Hotbar's AtkUnitBase and root node</summary>
    private static void RestoreCrossXPos()
    {
        try
        {
            if (!Bars.Cross.Exists || Config.LockCenter) return;
            if (Bars.Cross.Base->X != (short)Config.DisposeBaseX! || Math.Abs(Bars.Cross.Root.Node->X - (float)Config.DisposeRootX!) > 0.5F) PluginLog.LogDebug("Correcting Cross Hotbar X Position");

            Bars.Cross.Base->X = (short)Config.DisposeBaseX!;
            Bars.Cross.Root.Node->X = (float)Config.DisposeRootX!;
        } catch (Exception ex) { PluginLog.LogWarning("Exception: Couldn't restore Cross Hotbar X Position!\n" + ex); }
    }

    /// <summary>Records the X coordinates of the Cross Hotbar's AtkUnitBase and root node on disable/dispose</summary>
    internal static void StoreCrossXPos()
    {
        if (!Bars.Cross.Exists) { return; }
        PluginLog.LogDebug($"Storing Cross Hotbar X Position; UnitBase: {Bars.Cross.Base->X}, Root Node: {Bars.Cross.Root.Node->X}");
        Config.DisposeBaseX = Bars.Cross.Base->X;
        Config.DisposeRootX = Bars.Cross.Root.Node->X;
        Config.Save();
    }

    /// <summary>Put all modified nodes back in place and remove hooks</summary>
    public void Dispose()
    {
        try { DisableHooks();     } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Remove hooks!\n{ex}"); }
        try { StoreCrossXPos();   } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't store Cross Hotbar X Position!\n{ex}"); }
        try { Layout.TidyUp();    } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset Cross Hotbar layout!\n{ex}"); }
        try { Layout.ResetBars(); } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset Action Bars!\n{ex}"); }
        try { Color.Reset();      } catch (Exception ex) { PluginLog.LogWarning($"Exception on Dispose: Couldn't reset colors!\n{ex}"); }
        try { DisposeUI();        } catch (Exception ex) { PluginLog.LogError($"Exception on Dispose: Couldn't Dispose Plugin Interface!\n{ex}"); }

        IsSetUp = false;
    }

    /// <summary>"/xup" Command</summary>
    private void OnMainCommand(string command, string args) => CrossUpUI.SettingsVisible = !CrossUpUI.SettingsVisible;
    private void DrawUI() => CrossUpUI?.Draw();
    private void DrawConfigUI() => CrossUpUI.SettingsVisible = true;
    private void DisposeUI()
    {
        CommandManager.RemoveHandler(MainCommand);
        CrossUpUI.Dispose();
    }
}