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

namespace CrossUp
{
    public sealed unsafe partial class CrossUp : IDalamudPlugin
    {
        public string Name => "CrossUp";
        private const string MainCommand = "/xup";
        private readonly ActionManager* ActionManager;
        private DalamudPluginInterface PluginInterface { get; }
        private CommandManager CommandManager { get; }
        private Configuration Config { get; }
        private CrossUpUI CrossUpUI { get; }

        private delegate byte ActionBarBaseUpdate(AddonActionBarBase* addonActionBarBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData);
        private readonly HookWrapper<ActionBarBaseUpdate>? ActionBarBaseUpdateHook;

        private readonly ConfigModule* charConfigs = ConfigModule.Instance();
        private readonly RaptureHotbarModule* raptureModule = ClientStructsFramework.Instance()->GetUiModule()->GetRaptureHotbarModule();
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

        private void OnMainCommand(string command, string args)
        {
            CrossUpUI.SettingsVisible = true;
        }
        private static class Status
        {
            public static bool TweensExist;
            public static bool Initialized;
            public static int CrossBarSelection;
            public static int CrossBarActive = 1;
            public static int CrossBarSet;
            public static bool HudEditNodeChecked;
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
        public void Dispose()   // put all the bars back in their normal places and take out our hooks
        {
            ActionBarBaseUpdateHook?.Disable();
            Service.Framework.Update -= FrameworkUpdate;
            CommandManager.RemoveHandler(MainCommand);
            Config.Split = 0;
            Config.SepExBar = false;
            DisableEx();
            SetSelectColor(true);
            Status.Initialized = false;
            CrossUpUI.Dispose();
        }


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
                AdjustHudEditorNode();
            }
        }

        private void Initialize()
        {
            Status.Initialized = true;
            SetSelectColor();
            UpdateBarState(true);

            for (var i = 1; i <= 10; i++)
            {
                Task.Delay(500 * i).ContinueWith(delegate{if (Status.Initialized){UpdateBarState(true);}});
            }
        }
        private byte ActionBarBaseUpdateDetour(AddonActionBarBase* addonActionBarBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData)
        {
            var ret = ActionBarBaseUpdateHook!.Original(addonActionBarBase, numberArrayData, stringArrayData);

            if (!Status.Initialized) return ret;

            try
            {
                if (addonActionBarBase->HotbarID == 1) // all the bars fire at once every time anything happens, so we'll just take this one (using 0 goes weird with pet bar)
                {
                    var activeNow = GetCharConfig(586);
                    UpdateBarState(activeNow != Status.CrossBarActive, true);
                    Status.CrossBarActive = activeNow;
                }
                else if (addonActionBarBase->HotbarSlotCount == 16)
                {
                    var barID = addonActionBarBase->HotbarID;
                    if (barID != Status.CrossBarSet)
                    {
                        if (Config.RemapEx || Config.RemapW)
                        {
                            OverrideMappings(barID);
                        }
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

        private void OverrideMappings(int barID) // Change config setting for EXHB/WXHB base
        {
            var pvp = GetCharConfig(563) == 1 && Service.ClientState.IsPvP;
            var index = barID - 10;
            if (Config.RemapEx)
            {
                var overrideLR = Config.MappingsEx[0, index];
                var overrideRL = Config.MappingsEx[1, index];
                var configLR = (uint)(pvp ? 584 : 561);
                var configRL = (uint)(pvp ? 583 : 560);
                if (GetCharConfig(configLR) != overrideLR) charConfigs->SetOption(configLR, overrideLR, 1);
                if (GetCharConfig(configRL) != overrideRL) charConfigs->SetOption(configRL, overrideRL, 1);
            }

            if (Config.RemapW)
            {
                var overrideLL = Config.MappingsW[0, index];
                var overrideRR = Config.MappingsW[1, index];
                var configLL = (uint)(pvp ? 591 : 588);
                var configRR = (uint)(pvp ? 592 : 589);
                if (GetCharConfig(configLL) != overrideLL) charConfigs->SetOption(configLL, overrideLL, 1);
                if (GetCharConfig(configRR) != overrideRR) charConfigs->SetOption(configRR, overrideRR, 1);
            }
        }
        public void UpdateBarState(bool forceArrange = false, bool hudFixCheck = false)
        {
            var newSelection = GetCrossBarSelection();
            ArrangeAndFill(newSelection, Status.CrossBarSelection, forceArrange, hudFixCheck);
            Status.CrossBarSelection = newSelection;
        }
        private static int GetCrossBarSelection() // Return value from 0-6 to indicate which cross bar is selected (if any)
        {
            var barBaseXHB = (AddonActionBarBase*)UnitBases.Cross;
            var xBar = (AddonActionCross*)UnitBases.Cross;

            var LLBar = (AddonActionDoubleCrossBase*)UnitBases.LL;
            var RRbar = (AddonActionDoubleCrossBase*)UnitBases.RR;

            if (barBaseXHB == null || xBar == null || LLBar == null || RRbar == null)
            {
                return Status.CrossBarSelection;
            }

            var newCrossBarState =
            xBar->LeftBar                      ? 1 : // LEFT BAR
            xBar->RightBar                     ? 2 : // RIGHT BAR
            xBar->ExpandedHoldControlsLTRT > 0 ? 3 : // L->R EX BAR
            xBar->ExpandedHoldControlsRTLT > 0 ? 4 : // R->L EX BAR
            LLBar->Selected                    ? 5 : // WXHB L
            RRbar->Selected                    ? 6 : // WXHB R
                                                 0 ;
            return newCrossBarState;
        }
        public void EnableEx() // Turn on Separate Ex bar function
        {
            var lID = Config.borrowBarL;
            var rID = Config.borrowBarR;

            if (lID < 0 || rID < 0) { return; }

            EnableBorrowedBar(lID);
            EnableBorrowedBar(rID);

            UpdateBarState(true);
            Task.Delay(20).ContinueWith(delegate { if (Status.Initialized) { UpdateBarState(true); } });
        }

        private static readonly bool[] WasHidden = { false, false, false, false, false, false, false, false, false, false };

        private void EnableBorrowedBar(int id)
        {
            var unitBase = UnitBases.ActionBar[id];
            if (unitBase == null) { return; }

            var visID = (uint)(id + 485);
            if (charConfigs->GetIntValue(visID) == 0)
            {
                WasHidden[id] = true;
                charConfigs->SetOption(visID, 1);
            }

            for (var i = 9; i <= 20; i++) unitBase->UldManager.NodeList[i]->Flags_2 |= 0xD;
        }
        public void DisableEx()
        {
            ArrangeAndFill(0, 0, true, false);
            ResetHud();
        }
        public int GetCharConfig(uint configID)    //get a char Config setting
        {
            //  485-494: toggle, hotbar 1-10 (0-9 internally) visible or not
            //  501-510: int from 0-5, represents selected grid layout for hotbars 1-10 (0-9 internally)
            //  515-523: toggle, share setting for cross hotbars 1 - 8
            //  535: main cross hotbar layout   0 = dpad / button / dpad / button
            //                                  1 = dpad / dpad / button / button
            //  560: Expanded Hold Controls R (PvE)
            //  561: Expanded Hold Controls L (PvE)
            //  563: Toggle Separate PvP Configs
            //  583: Expanded Hold Controls R (PvP)
            //  584: Expanded Hold Controls L (PvP)
            //  586: toggle, cross hotbar active or not
            //  588: WXHB L (PvE)
            //  589: WXHB R (PvE)
            //  591: WXHB L (PvP)
            //  592: WXHB R (PvP)
            //  600: transparency of standard cross hotbar
            //  601: transparency of active cross hotbar
            //  602: transparency of inactive cross hotbar
            return charConfigs->GetIntValue(configID);
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
