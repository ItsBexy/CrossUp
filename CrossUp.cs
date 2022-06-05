using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

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

        CrossUpUI = new CrossUpUI(Config, this);

        ActionBarBaseUpdateHook ??= Common.Hook<ActionBarBaseUpdate>("E8 ?? ?? ?? ?? 83 BB ?? ?? ?? ?? ?? 75 09", ActionBarBaseUpdateDetour);
        ActionBarBaseUpdateHook?.Enable();

        ActionBarReceiveEventHook ??= Common.Hook<ActionBarReceiveEvent>("E8 ?? ?? ?? FF 66 83 FB ?? ?? ?? ?? BF 3F", ActionBarReceiveEventDetour);
        ActionBarReceiveEventHook?.Enable();

        Service.Framework.Update += FrameworkUpdate;
        Initialized = false;
    }

    /// <summary>Whether the plugin has set itself up</summary>
    private static bool Initialized;

    /// <summary>Sets up the plugin's main function and applies user configs</summary>
    private static void Initialize()
    {
        try
        {
            Initialized = true;
            Bars.Init();

            if (SeparateEx.Ready) SeparateEx.Enable();
            Color.SetSelectBG();
            Color.SetPulse();

            Layout.Update(true, true);
            Layout.ScheduleNudges(10);
        }
        catch (Exception ex)
        {
            PluginLog.LogError("Exception: Initialization Failed!\n"+ex);
            Initialized = false;
        }
    }

    /// <summary>Restores the last known X coordinates of the Cross Hotbar's AtkUnitBase and root node</summary>
    private static void RestoreDisposalXPos()
    {
        try
        {
            if (!Bars.Cross.Exist) return;
            if (Bars.Cross.Base->X != (short)Config.DisposeBaseX! || Math.Abs(Bars.Cross.Root.Node->X - (float)Config.DisposeRootX!) > 0.5F) PluginLog.LogDebug("Correcting Cross Hotbar X Position");

            Bars.Cross.Base->X = (short)Config.DisposeBaseX!;
            Bars.Cross.Root.Node->X = (float)Config.DisposeRootX!;
        } catch (Exception ex) { PluginLog.LogWarning("Exception: Couldn't restore Cross Hotbar X Position!\n" + ex); }
    }

    /// <summary>Records the X coordinates of the Cross Hotbar's AtkUnitBase and root node on disable/dispose</summary>
    private static void StoreDisposalXPos()
    {
        if (!Bars.Cross.Exist) { return; }
        PluginLog.LogDebug($"Storing Cross Hotbar X Position; UnitBase: {Bars.Cross.Base->X}, Root Node: {Bars.Cross.Root.Node->X}");
        Config.DisposeBaseX = Bars.Cross.Base->X;
        Config.DisposeRootX = Bars.Cross.Root.Node->X;
        Config.Save();
    }

    /// <summary>Put all modified nodes back in place and remove hooks</summary>
    public void Dispose()
    {
        try
        {
            ActionBarBaseUpdateHook?.Disable();
            ActionBarReceiveEventHook?.Disable();
            Service.Framework.Update -= FrameworkUpdate;
        }   catch (Exception ex) { PluginLog.LogError("Exception on Dispose: Couldn't Remove hooks!\n" + ex); }

        try { StoreDisposalXPos(); }
            catch (Exception ex) { PluginLog.LogWarning("Exception on Dispose: Couldn't store Cross Hotbar X Position!\n" + ex); }

        try { Layout.Arrange(0, 0, true, true, true); }
            catch (Exception ex) { PluginLog.LogWarning("Exception on Dispose: Couldn't reset Cross Hotbar layout!\n" + ex); }

        try { Layout.Reset(); }
            catch (Exception ex) { PluginLog.LogWarning("Exception on Dispose: Couldn't reset Action Bars!\n" + ex); }

        try { Color.Reset(); }
            catch (Exception ex) { PluginLog.LogWarning("Exception on Dispose: Couldn't reset colors!\n" + ex); }

        Initialized = false;

        try
        {
            CommandManager.RemoveHandler(MainCommand); 
            CrossUpUI.Dispose();
        }   catch (Exception ex) { PluginLog.LogError("Exception on Dispose: Couldn't Dispose Plugin Interface!\n" + ex); }
    }

    /// <summary>"/xup" Command</summary>
    private void OnMainCommand(string command, string args) => CrossUpUI.SettingsVisible = true;
    private void DrawUI() => CrossUpUI?.Draw();
    private void DrawConfigUI() => CrossUpUI.SettingsVisible = true;
}