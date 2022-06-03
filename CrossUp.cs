using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Threading.Tasks;

namespace CrossUp;

public sealed unsafe partial class CrossUp : IDalamudPlugin
{
    public string Name => "CrossUp";
    private const string MainCommand = "/xup";
    private DalamudPluginInterface PluginInterface { get; }
    private CommandManager CommandManager { get; }
    private Configuration Config { get; }
    private CrossUpUI CrossUpUI { get; }

    public CrossUp(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
        CommandManager = commandManager;
        CommandManager.AddHandler(MainCommand, new CommandInfo(OnMainCommand) { HelpMessage = "Open CrossUp Config" });

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

    /// <summary>Set up plugin once player is logged in and bar nodes are confirmed to exist</summary>
    private void Initialize()
    {
        try
        {
            Initialized = true;
            Bars.Init();

            if (ExBarsOk) EnableEx();
            SetSelectColor();
            SetPulseColor();

            UpdateBarState(true, true);
            ScheduleNudges();
        }
        catch (Exception ex)
        {
            PluginLog.LogError($"Exception: Initialization Failed!\n{ex}");
        }
    }

    /// <summary>Run the UpdateBarState() function a few times on login (hopefully ensures bars look right by the time user sees them)</summary>
    private void ScheduleNudges()
    {
        for (var i = 1; i <= 5; i++)
        {
            var n = i;
            Task.Delay(500 * i).ContinueWith(delegate {Nudge(n);});
        }
    }

    /// <summary>Run UpdateBarState() independently of any hotbar events</summary>
    private void Nudge(int n)
    {
        if (!Initialized) return;
        PluginLog.LogDebug($"Nudging Nodes Post-Login {n}/5");

        if (Config.DisposeBaseX != null && Config.DisposeRootX != null) RestoreDisposalState();
        UpdateBarState(true);
    }

    private void RestoreDisposalState()
    {
        if (Bars.Cross.Base->X != (short)Config.DisposeBaseX! || Math.Abs(Bars.Cross.Root.Node->X - (float)Config.DisposeRootX!) > 0.1f)
        {
            PluginLog.Log("IT'S MISMATCHED; HOPE THIS FIXES IT");
        }
        Bars.Cross.Base->X = (short)Config.DisposeBaseX!;
        Bars.Cross.Root.Node->X = (float)Config.DisposeRootX!;
    }

    public void StoreDisposalState()
    {
        PluginLog.Log($"Storing Cross Hotbar X position; UnitBase: {Bars.Cross.Base->X}, Root Node: {Bars.Cross.Root.Node->X}");
        Config.DisposeBaseX = Bars.Cross.Base->X;
        Config.DisposeRootX = Bars.Cross.Root.Node->X;
        Config.Save();
    }

    /// <summary>Put all modified nodes back in place and remove hooks</summary>
    public void Dispose()
    {
        StoreDisposalState();
        ActionBarBaseUpdateHook?.Disable();
        ActionBarReceiveEventHook?.Disable();
        Service.Framework.Update -= FrameworkUpdate;
        CommandManager.RemoveHandler(MainCommand);

        ArrangeCrossBar(0, 0, true, true, true);
        ResetHud();
        ResetColors();
        Initialized = false;

        CrossUpUI.Dispose();
    }

    /// <summary>"/xup" Command</summary>
    private void OnMainCommand(string command, string args) => CrossUpUI.SettingsVisible = true;
    private void DrawUI() => CrossUpUI?.Draw();
    private void DrawConfigUI() => CrossUpUI.SettingsVisible = true;
}