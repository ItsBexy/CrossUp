using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Threading.Tasks;
using ClientStructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using DalamudFramework = Dalamud.Game.Framework;
using System.Runtime;

namespace CrossUp
{
    public sealed unsafe partial class CrossUp : IDalamudPlugin
    {
        public string Name => "CrossUp";
        private const string MainCommand = "/xup";
        private static readonly ActionManager* ActionManager;
        private DalamudPluginInterface PluginInterface { get; }
        private CommandManager CommandManager { get; }
        private Configuration Config { get; }
        private CrossUpUI CrossUpUI { get; }

        private delegate byte ActionBarBaseUpdate(AddonActionBarBase* addonActionBarBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData);
        private readonly HookWrapper<ActionBarBaseUpdate>? ActionBarBaseUpdateHook;

        private static readonly ConfigModule* charConfigs = ConfigModule.Instance();
        private static readonly RaptureHotbarModule* raptureModule = ClientStructsFramework.Instance()->GetUiModule()->GetRaptureHotbarModule();
        private readonly AgentHudLayout* hudLayout = ClientStructsFramework.Instance()->GetUiModule()->GetAgentModule()->GetAgentHudLayout();
        public CrossUp(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            CommandManager = commandManager;
            CommandManager.AddHandler(MainCommand, new CommandInfo(OnMainCommand)
            {
                HelpMessage = "Open CrossUp Config"
            });

            PluginInterface = pluginInterface;
            Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(PluginInterface);
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            pluginInterface.Create<Service>();

            CrossUpUI = new(Config, this);

            ActionBarBaseUpdateHook ??= Common.Hook<ActionBarBaseUpdate>("E8 ?? ?? ?? ?? 83 BB ?? ?? ?? ?? ?? 75 09", ActionBarBaseUpdateDetour);
            ActionBarBaseUpdateHook?.Enable();

            Service.Framework.Update += FrameworkUpdate;
            Status.Initialized = false;
        }

        // HOUSEKEEPING
        private static class Status     //stuff other functions need to keep track of
        {
            public static bool TweensExist;
            public static bool Initialized;
            public static int CrossBarSelection;
            public static int CrossBarActive = 1;
            public static int CrossBarSet;
            public static bool DoneHudCheck;
            public static readonly bool[] WasHidden = new bool[10];
        }

            // run once logged in and bar nodes confirmed to exist
        private void Initialize()
        {
            Status.Initialized = true;
            SetSelectColor();
            UpdateBarState(true);

            for (var i = 1; i <= 10; i++)
            {
                // run the update function a few times upon login (ensures bars look right by the time user sees them)
                Task.Delay(500 * i).ContinueWith(delegate { if (Status.Initialized) { UpdateBarState(true); } });
            }
        }

             // put all modified nodes back in place and remove hooks
        public void Dispose()  
        {
            ActionBarBaseUpdateHook?.Disable();
            Service.Framework.Update -= FrameworkUpdate;
            CommandManager.RemoveHandler(MainCommand);

            Config.Split = 0;
            Config.SepExBar = false;
            ArrangeAndFill(0, 0, true, false);
            ResetHud();
            SetSelectColor(true);
            Status.Initialized = false;

            CrossUpUI.Dispose();
        }

        // HOOKS AND EVENTS

            // on every new frame
        private void FrameworkUpdate(DalamudFramework framework)
        {
            if (!Status.Initialized && Service.ClientState.IsLoggedIn && UnitBases.Cross != null)
            {
                try
                {
                    Initialize();
                }
                catch (Exception ex)
                {
                    PluginLog.Log(ex + "");
                }
            }
            else if (Status.Initialized)
            {
                if (Status.TweensExist) TweenAllButtons();

                // if HUD layout editor is open, perform this fix once:
                Status.DoneHudCheck = hudLayout->AgentInterface.IsAgentActive() && (Status.DoneHudCheck || AdjustHudEditorNode());
            }
        }

            // when any hotbar updates
        private byte ActionBarBaseUpdateDetour(AddonActionBarBase* addonActionBarBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData)
        {
            var ret = ActionBarBaseUpdateHook!.Original(addonActionBarBase, numberArrayData, stringArrayData);

            if (!Status.Initialized) return ret;

            try
            {
                
                // almost all the bars fire at once every time anything happens,
                // so we'll just take barID 1 because it's always included
                if (addonActionBarBase->HotbarID == 1) 
                {
                    var activeNow = GetCharConfig(ConfigID.CrossEnabled);
                    UpdateBarState(activeNow != Status.CrossBarActive, true);
                    Status.CrossBarActive = activeNow;
                }
                else if (addonActionBarBase->HotbarSlotCount == 16) // 16 slots means it's the cross bar
                { 
                    //check if the cross hotbar set has changed
                    var barID = addonActionBarBase->HotbarID;
                    if (barID != Status.CrossBarSet)
                    {
                        if (Config.RemapEx || Config.RemapW) OverrideMappings(barID);
                        Status.CrossBarSet = barID;
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Log(ex + "");
            }

            return ret;
        }

        // CROSS HOTBAR STATE

            // check which bar is selected, if any (returns value from 0-6)
        private static int GetCrossBarSelection()
        {
            var xBar  = (AddonActionCross*)UnitBases.Cross;
            var LLBar = (AddonActionDoubleCrossBase*)UnitBases.LL;
            var RRbar = (AddonActionDoubleCrossBase*)UnitBases.RR;

            if (xBar == null || LLBar == null || RRbar == null) return Status.CrossBarSelection;

            var newCrossBarSelection =
                    xBar->LeftBar   ? 1 : // LEFT BAR
                    xBar->RightBar  ? 2 : // RIGHT BAR
                    xBar->LRBar > 0 ? 3 : // L->R EX BAR
                    xBar->RLBar > 0 ? 4 : // R->L EX BAR
                    LLBar->Selected ? 5 : // WXHB LL
                    RRbar->Selected ? 6 : // WXHB RR
                                      0 ;

            return newCrossBarSelection;
        }

            // run arrangement updates based on bar change (previous -> current)
        public void UpdateBarState(bool forceArrange = false, bool hudFixCheck = false)
        {
            var newSelection = GetCrossBarSelection();
            ArrangeAndFill(newSelection, Status.CrossBarSelection, forceArrange, hudFixCheck);
            Status.CrossBarSelection = newSelection;
        }

        // PLUGIN INTERFACE

            // config button or /xup command
        private void OnMainCommand(string command, string args)
        {
            CrossUpUI.SettingsVisible = true;
        }
        private void DrawUI()
        {
            var crossUpUI = CrossUpUI;
            if (crossUpUI != null) crossUpUI.Draw();
        }
        private void DrawConfigUI()
        {
            CrossUpUI.SettingsVisible = true;
        }

        // SEPARATE EXPANDED HOLD CONTROLS

            // turn on separate Ex bar feature
        public void EnableEx()
        {
            var lID = Config.borrowBarL;
            var rID = Config.borrowBarR;

            if (lID < 0 || rID < 0) { return; }

            EnableBorrowedBar(lID);
            EnableBorrowedBar(rID);

            UpdateBarState(true);
            Task.Delay(20).ContinueWith(delegate { if (Status.Initialized) { UpdateBarState(true); } });
        }

            // turn on each specific bar
        private void EnableBorrowedBar(int id)
        {
            var unitBase = UnitBases.ActionBar[id];
            if (unitBase == null) return;
            
            var visID = ConfigID.Hotbar.Visible[id];
            if (charConfigs->GetIntValue(visID) == 0)
            {
                Status.WasHidden[id] = true;
                charConfigs->SetOption(visID, 1);
            }

            for (var i = 9; i <= 20; i++) unitBase->UldManager.NodeList[i]->Flags_2 |= 0xD;
        }

            // disable the feature
        public void DisableEx()
        {
            Config.SepExBar = false;
            ArrangeAndFill(0, 0, true, false);
            ResetHud();
        }

        // EXHB/WXHB CUSTOM MAPPING

            // set Character Configs to match user's override prefs
        private void OverrideMappings(int barID)
        {
            var usePvP = GetCharConfig(ConfigID.SepPvP) == 1 && Service.ClientState.IsPvP ? 1:0;
            var index = barID - 10;
            if (Config.RemapEx)
            {
                var overrideLR = Config.MappingsEx[0, index];
                var overrideRL = Config.MappingsEx[1, index];
                var configLR = ConfigID.LRset[usePvP];
                var configRL = ConfigID.RLset[usePvP];
                if (GetCharConfig(configLR) != overrideLR) charConfigs->SetOption(configLR, overrideLR, 1);
                if (GetCharConfig(configRL) != overrideRL) charConfigs->SetOption(configRL, overrideRL, 1);
            }

            if (Config.RemapW)
            {
                var overrideLL = Config.MappingsW[0, index];
                var overrideRR = Config.MappingsW[1, index];
                var configLL = ConfigID.LLset[usePvP];
                var configRR = ConfigID.RRset[usePvP];
                if (GetCharConfig(configLL) != overrideLL) charConfigs->SetOption(configLL, overrideLL, 1);
                if (GetCharConfig(configRR) != overrideRR) charConfigs->SetOption(configRR, overrideRR, 1);
            }
        }

        // CHARACTER CONFIGURATION
        private static int GetCharConfig(uint configID) => charConfigs->GetIntValue(configID);
        

            //relevant character configuration IDs
        public readonly struct ConfigID
        {
            public const uint CrossEnabled = 586; // checkbox -- Cross Hotbar enabled
            public const uint SepPvP = 563;       // checkbox -- using different settings for PvP
            public const uint MixBar = 535;       // checkbox -- main XHB layout
                                                  //           0 = dpad / button / dpad / button
                                                  //           1 = dpad / dpad / button / button

            // dropdowns -- selected sets for EXHB and WXHB [PvE, PvP]
            // returns 0-19 representing the left/right sides of each bar, and the four cycle options
            public static readonly uint[] LRset = { 561, 584 };
            public static readonly uint[] RLset = { 560, 583 };
            public static readonly uint[] LLset = { 588, 591 };
            public static readonly uint[] RRset = { 589, 592 };
            public struct Transparency  // slider -- transparency for cross hotbar buttons
            {
                public const uint Standard = 600;
                public const uint Active   = 601;
                public const uint Inactive = 602;
            }
            public struct Hotbar    // kb/mouse hotbars 1-10 (0-9 internally)
            {
                public static readonly uint[] Visible  = { 485, 486, 487, 488, 489, 490, 491, 492, 493, 494 }; //checkbox -- bar is visible
                public static readonly uint[] Shared   = { 515, 516, 517, 518, 519, 520, 521, 522, 523, 524 }; //checkbox -- bar is shared between jobs
                public static readonly uint[] GridType = { 501, 502, 503, 504, 505, 506, 507, 508, 509, 510 }; //radio    -- bar grid type (0-5)
            }
        }

             // ReSharper disable once UnusedMember.Global
        public void LogCharConfigs(uint start, uint end = 0)
        {
            if (end < start) { end = start; }
            for (var i = start; i <= end; i++)
            {
                PluginLog.Log(i + " " + GetCharConfig(i));
            }
        }
    }
}
