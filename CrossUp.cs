using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Threading.Tasks;
using ClientStructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using DalamudFramework = Dalamud.Game.Framework;

namespace CrossUp;

public sealed unsafe partial class CrossUp : IDalamudPlugin
{
    public string Name => "CrossUp";
    private const string MainCommand = "/xup";
    private DalamudPluginInterface PluginInterface { get; }
    private CommandManager CommandManager { get; }
    private Configuration Config { get; }
    private CrossUpUI CrossUpUI { get; }

    private delegate byte ActionBarBaseUpdate(AddonActionBarBase* addonActionBarBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData);
    private readonly HookWrapper<ActionBarBaseUpdate>? ActionBarBaseUpdateHook;

    private static readonly RaptureHotbarModule* RaptureModule = (RaptureHotbarModule*)ClientStructsFramework.Instance()->GetUiModule()->GetRaptureHotbarModule();
    private readonly AgentHudLayout* hudLayout = ClientStructsFramework.Instance()->GetUiModule()->GetAgentModule()->GetAgentHudLayout();
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

        Service.Framework.Update += FrameworkUpdate;
        Status.Initialized = false;
    }

    // HOUSEKEEPING

    //stuff various functions need to keep track of
    private static class Status
    {
        public static bool TweensExist;
        public static bool Initialized;
        public static bool DoneHudCheck;
    }

    // run once logged in and bar nodes confirmed to exist
    private void Initialize()
    {
        Status.Initialized = true;
        Bars.Init();

        if (Config.SepExBar && Config.borrowBarR > 0 && Config.borrowBarL > 0) EnableEx();
        SetSelectColor();
        SetPulseColor();

        UpdateBarState(true, true);

        for (var i = 1; i <= 10; i++)
        {
            // run the update function a few times upon login (ensures bars look right by the time user sees them)
            var n = i;
            Task.Delay(500 * i).ContinueWith(delegate
            {
                if (!Status.Initialized) return;
                PluginLog.LogDebug($"Nudging Nodes Post-Login {n}/10");
                UpdateBarState(true);
            });
        }
    }

    // put all modified nodes back in place and remove hooks
    public void Dispose()
    {
        ActionBarBaseUpdateHook?.Disable();
        Service.Framework.Update -= FrameworkUpdate;
        CommandManager.RemoveHandler(MainCommand);

        ArrangeCrossBar(0, 0, true, true, true);
        ResetHud();
        ResetColors();
        Status.Initialized = false;

        CrossUpUI.Dispose();
    }

    // HOOKS AND EVENTS

    // on every new frame
    private void FrameworkUpdate(DalamudFramework framework)
    {
        if (Status.Initialized)
        {
            if (Bars.Cross.Exist)
            {
                // animate button sizes if needed
                if (Status.TweensExist) TweenAllMetaSlots();

                // if HUD layout editor is open, perform this fix once:
                Status.DoneHudCheck = hudLayout->AgentInterface.IsAgentActive() &&
                                      (Status.DoneHudCheck || AdjustHudEditorNode());
            }
            else
            {
                PluginLog.LogDebug("Cross Hotbar nodes not found; disabling plugin features");
                Status.Initialized = false;
            }

        }
        else if (Bars.Cross.Exist)
        {
            PluginLog.LogDebug("Cross Hotbar nodes found; setting up plugin features");
            try { Initialize(); }
            catch (Exception ex)
            {
                PluginLog.Log($"Exception: Initialization Failed!\n{ex}");
            }
        }
    }

    // when any hotbar updates
    private byte ActionBarBaseUpdateDetour(AddonActionBarBase* barBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData)
    {
        var ret = ActionBarBaseUpdateHook!.Original(barBase, numberArrayData, stringArrayData);
        if (!Status.Initialized) return ret;

        try
        {
            var barID = barBase->HotbarID;

            // almost all the bars fire at once every time anything happens,
            // so we'll just take barID 1 because it's always included
            if (barBase->HotbarID == 1)
            {
                UpdateBarState(Bars.Cross.Enabled != Bars.Cross.LastEnabledState, true);
                Bars.Cross.LastEnabledState = Bars.Cross.Enabled;
            }
            else if (barBase->HotbarSlotCount == 16 && barID != Bars.Cross.LastKnownSetID) // Cross Hotbar set has changed
            {
                PluginLog.LogDebug($"Switched to Cross Hotbar Set {barID - 9}");
                if (Config.RemapEx || Config.RemapW) OverrideMappings(barID);
                Bars.Cross.LastKnownSetID = barID;
            }

            // detecting drag/drop changes on the first 8 slots of borrowed bars
            if (Config.SepExBar && (barID == Config.borrowBarL || barID == Config.borrowBarR))
            {
                var barSlots = barBase->ActionBarSlotsAction;
                for (var i = 0; i < 8; i++) if (barSlots[i].Icon->AtkResNode.Flags_2 % 2 == 1)
                {
                    OnDragDropEx(barID == Config.borrowBarL);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Log($"Exception: ActionBarBaseUpdateDetour Failed!\n{ex}");
        }

        return ret;
    }

    public void UpdateBarState(bool forceArrange = false, bool hudFixCheck = false)
    {
        ArrangeCrossBar(Bars.Cross.Selection, Bars.Cross.LastKnownSelection, forceArrange, hudFixCheck);
        Bars.Cross.LastKnownSelection = Bars.Cross.Selection;
    }

    // PLUGIN INTERFACE
    private void OnMainCommand(string command, string args) => CrossUpUI.SettingsVisible = true;
    private void DrawUI() => CrossUpUI?.Draw();
    private void DrawConfigUI() => CrossUpUI.SettingsVisible = true;
}