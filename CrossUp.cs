using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Numerics;
using XivCommon;
using ClientStructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using DalamudFramework = Dalamud.Game.Framework;

namespace CrossUp
{
    public unsafe sealed class CrossUp : IDalamudPlugin
    {
        public string Name => "CrossUp";
        private const string mainCommand = "/pcrossup";

        private readonly ActionManager* actionManager;
        public Configuration PluginConfig { get; private set; }
        public static CrossUp Plugin { get; private set; }
        public XivCommonBase XivCommon { get; private set; }
        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private CrossUpUI CrossUpUI { get; init; }

        private delegate byte ActionBarBaseUpdate(AddonActionBarBase* addonActionBarBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData);
        private readonly HookWrapper<ActionBarBaseUpdate> actionBarBaseUpdateHook;

        ConfigModule* charConfigs = ConfigModule.Instance();
        RaptureHotbarModule* raptureModule = ClientStructsFramework.Instance()->GetUiModule()->GetRaptureHotbarModule();

        public CrossUp(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.CommandManager = commandManager;
            this.CommandManager.AddHandler(mainCommand, new CommandInfo(OnMainCommand)
            {
                HelpMessage = "Open CrossUp Configuration"
            });

            this.PluginInterface = pluginInterface;
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            pluginInterface.Create<Service>();

            this.CrossUpUI = new CrossUpUI(this.Configuration, this);
            this.XivCommon = new XivCommonBase();

            actionBarBaseUpdateHook ??= Common.Hook<ActionBarBaseUpdate>("E8 ?? ?? ?? ?? 83 BB ?? ?? ?? ?? ?? 75 09", ActionBarBaseUpdateDetour);

            Service.Framework.Update += FrameworkUpdate;
            actionBarBaseUpdateHook?.Enable();

            SetSelectColor();
            ArrangeAndFill(0, 0, true, false);
        }

        private void DrawUI()
        {
            this.CrossUpUI.Draw();
        }
        private void DrawConfigUI()
        {
            this.CrossUpUI.SettingsVisible = true;
        }

        // Separate EXHB feature toggled on or off
        public void EnableEx() 
        {
            UpdateBarState(true, false);
            return;
        }
        public void DisableEx()
        {
            ArrangeAndFill(0, 0, true, false);
            ResetHud();
            return;
        }
        public void Dispose()   // put all the bars back in their normal places and take out our hooks
        {
            Configuration.Split = 0;
            DisableEx();
            SetSelectColor(true);
            CrossUpUI.Dispose();
            XivCommon.Dispose();
            actionBarBaseUpdateHook?.Disable();
            Service.Framework.Update -= FrameworkUpdate;
            CommandManager.RemoveHandler(mainCommand);
        }


        // HOOKS AND EVENTS
        private void OnMainCommand(string command, string args)
        {
            var barBaseXHB = (AddonActionBarBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            var xBar = (AddonActionCross*)barBaseXHB;

            this.CrossUpUI.SettingsVisible = true;
        }
        private void FrameworkUpdate(DalamudFramework framework)
        {
            if (tweensExist) TweenAllButtons();
            return;
        }
        private byte ActionBarBaseUpdateDetour(AddonActionBarBase* addonActionBarBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData)
        {
            if (addonActionBarBase->HotbarID == 0) // all the bars fire at once every time anything happens, so we'll just take the first bar
            {
                var activeNow = GetCharConfig(586);
                UpdateBarState(activeNow != crossBarActive, true);
                crossBarActive = activeNow;
            }

            var ret = actionBarBaseUpdateHook.Original(addonActionBarBase, numberArrayData, stringArrayData);

            return ret;
        }
        private int crossBarState = 0;
        private int crossBarActive = 1;
        public void UpdateBarState(bool forceArrange = false, bool HudfixCheck = false)  // kind of an intermediary function (cut out this middleman maybe?)
        {
            var newCrossBarState = GetCrossBarState();
            ArrangeAndFill(newCrossBarState, crossBarState, forceArrange, HudfixCheck);
            crossBarState = newCrossBarState;

            return;
        }
        private int GetCrossBarState() // Return value from 0-6 to indicate which cross bar is selected (if any)
        {
            var barBaseXHB = (AddonActionBarBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            var xBar = (AddonActionCross*)barBaseXHB;

            var baseLL = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);
            var baseRR = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);

            int newCrossBarState = 
            xBar->LeftBar                             ? 1 : // LEFT BAR
            xBar->RightBar                            ? 2 : // RIGHT BAR
            xBar->ExpandedHoldControlsLTRT > 0        ? 3 : // L->R EX BAR
            xBar->ExpandedHoldControlsRTLT > 0        ? 4 : // R->L EX BAR

            // there's probably a better way to find these two, watching the UI node means we'll always be one frame late
            baseLL->UldManager.NodeList[3]->IsVisible ? 5 : // WXHB L
            baseRR->UldManager.NodeList[3]->IsVisible ? 6 : // WXHB R
                                                        0;
            return newCrossBarState;
        }

        public void ExBarActivate(int barID)    // runs when a bar is selected to be one of the borrowed bars in CrossUp configs
        {
            XivCommon.Functions.Chat.SendMessage($"/hotbar display " + (barID + 1) + " on"); // show the borrowed bar if it was hidden
            ArrangeAndFill(0, 0, true, true);
            return;
        }

        // BAR ARRANGEMENT
        private void ArrangeAndFill(int state, int prevState = 0, bool forceArrange = false, bool HUDfixCheck = true) // the centrepiece of it all
        {
            if (GetCharConfig(586) == 0)    //don't do anything if the cross hotbar isn't actually turned on
            {
                ResetHud();
                return;
            }

            Configuration cfg = Configuration;

            var baseXHB = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            var nodesXHB = baseXHB->UldManager.NodeList;
            var scale = nodesXHB[0]->ScaleX;
            bool mixBar = GetCharConfig(535)==1;

            // fix for misalignment after entering HUD Layout interface (unsure if this is sufficient)
            if (HUDfixCheck && baseXHB->X - nodesXHB[0]->X - Math.Round(cfg.Split * scale) < 0) { baseXHB->X += (short)(cfg.Split * scale); }

            NodeEdit.SetVarious(nodesXHB[0],new NodeEdit.NodeProps { //reposition and resize the main bar node based on Split setting
                X = baseXHB->X - cfg.Split*scale,
                Y = baseXHB->Y,
                Width = (ushort)(588 + cfg.Split*2),
                Height = 210
            });

            int anchorX = (int)(nodesXHB[0]->X + (146 * scale));
            int anchorY = (int)(nodesXHB[0]->Y + (70 * scale));

            if (Configuration.SepExBar) ArrangeExBars(state, prevState, scale, anchorX, anchorY, forceArrange);

            int lX = cfg.SepExBar ? cfg.lX : 0;
            int lY = cfg.SepExBar ? cfg.lY : 0;
            int rX = cfg.SepExBar ? cfg.rX : 0;
            int rY = cfg.SepExBar ? cfg.rY : 0;

            bool hideDivider = cfg.Split > 0 || cfg.SepExBar == true;

            // NOTE: hardcoding a lot of default coordinate values for nodes in here. should probably consolidate those into a handy index and then just reference

            NodeEdit.SetSize(nodesXHB[7],(ushort)(hideDivider?0:9),(ushort)(hideDivider ? 0 : 76));
            NodeEdit.SetPos(nodesXHB[26], 284f + cfg.PadlockOffset.X + (cfg.Split), 152f + cfg.PadlockOffset.Y);
            NodeEdit.SetPos(nodesXHB[27], 146F + cfg.Split + cfg.ChangeSetOffset.X, cfg.ChangeSetOffset.Y);
            NodeEdit.SetPos(nodesXHB[21], 230F + cfg.Split + cfg.SetTextOffset.X, 170F + cfg.SetTextOffset.Y);

            if (state != prevState || forceArrange) // generally only want to rearrange bars if the cross hotbar state has actually changed
            {
                switch (state) 
                {
                    case 0: // NONE
                    case 5: // LEFT WXHB
                    case 6: // RIGHT WXHB
                        NodeEdit.SetPos(nodesXHB[1], 18, 79);
                        NodeEdit.SetPos(nodesXHB[7], 271, 21);
                        NodeEdit.SetPos(nodesXHB[20], 83, 11);
                        NodeEdit.SetPos(nodesXHB[19], 367 + cfg.Split * 2, 11);
                        NodeEdit.SetPos(nodesXHB[8], 422F + cfg.Split * 2, 0F);
                        NodeEdit.SetPos(nodesXHB[9], 284F + cfg.Split * 2, 0F);
                        NodeEdit.SetPos(nodesXHB[10], 138F, 0F);
                        NodeEdit.SetPos(nodesXHB[11], 0F, 0F);
                        break;
                    case 1: //LEFT BAR
                        NodeEdit.SetPos(nodesXHB[1], 18, 79);
                        NodeEdit.SetPos(nodesXHB[7], 271 + cfg.Split * 2, 21);
                        NodeEdit.SetPos(nodesXHB[20], 83, 11);
                        NodeEdit.SetPos(nodesXHB[19], 367 + cfg.Split * 2, 11);

                        NodeEdit.SetSize(nodesXHB[5], (ushort)(cfg.selectHide || (mixBar && cfg.Split > 0) ? 0 : 166), 140);
                        NodeEdit.SetSize(nodesXHB[6], (ushort)(cfg.selectHide || (mixBar && cfg.Split > 0) ? 0 : 166), 140);

                        if (prevState == 3 && cfg.SepExBar && !mixBar)
                        {
                            NodeEdit.SetPos(nodesXHB[8], 9999F, 9999F); //extremely lazy way to hide these :p
                            NodeEdit.SetPos(nodesXHB[9], 9999F, 9999F);
                        }
                        else
                        {
                            NodeEdit.SetPos(nodesXHB[8], 422F + cfg.Split * 2, 0F);
                            NodeEdit.SetPos(nodesXHB[9], 284F + cfg.Split * 2, 0F);
                        }
                        NodeEdit.SetPos(nodesXHB[10], 138F, 0F);
                        NodeEdit.SetPos(nodesXHB[11], 0F, 0F);

                        break;
                    case 2: // RIGHT BAR
                        NodeEdit.SetPos(nodesXHB[1], 18 + cfg.Split * 2, 79);
                        NodeEdit.SetPos(nodesXHB[7], 271, 21);
                        NodeEdit.SetPos(nodesXHB[20], 83 - cfg.Split * 2, 11);
                        NodeEdit.SetPos(nodesXHB[19], 367, 11);

                        NodeEdit.SetSize(nodesXHB[5], (ushort)(cfg.selectHide || (mixBar && cfg.Split > 0) ? 0 : 166), 140);
                        NodeEdit.SetSize(nodesXHB[6], (ushort)(cfg.selectHide || (mixBar && cfg.Split > 0) ? 0 : 166), 140);

                        if (prevState == 4 && cfg.SepExBar && !mixBar)
                        {
                            NodeEdit.SetPos(nodesXHB[10], 9999F, 9999F);
                            NodeEdit.SetPos(nodesXHB[11], 9999F, 9999F);
                        }
                        else
                        {
                            NodeEdit.SetPos(nodesXHB[10], 138F - cfg.Split * 2, 0F);
                            NodeEdit.SetPos(nodesXHB[11], 0F - cfg.Split * 2, 0F);
                        }

                        NodeEdit.SetPos(nodesXHB[8], 422F, 0F);
                        NodeEdit.SetPos(nodesXHB[9], 284F, 0F);

                        break;
                    case 3: // L->R BAR

                        NodeEdit.SetPos(nodesXHB[1], 18 + lX + cfg.Split, 79 + lY);
                        NodeEdit.SetPos(nodesXHB[7], 271 - lX + cfg.Split, 21 - lY);
                        NodeEdit.SetPos(nodesXHB[20], 83 - lX - cfg.Split, 11 - lY);
                        NodeEdit.SetPos(nodesXHB[19], 367 - lX + cfg.Split, 11 - lY);

                        NodeEdit.SetPos(nodesXHB[8], 9999F, 9999F);
                        NodeEdit.SetPos(nodesXHB[9], 284F, 0F);
                        NodeEdit.SetPos(nodesXHB[10], 138F, 0F);
                        NodeEdit.SetPos(nodesXHB[11], 9999F, 9999F);

                        break;
                    case 4: // R->L BAR
                        NodeEdit.SetPos(nodesXHB[1], 18 + rX + cfg.Split, 79 + rY);
                        NodeEdit.SetPos(nodesXHB[7], 271 - rX + cfg.Split, 21 - rY);
                        NodeEdit.SetPos(nodesXHB[20], 83 - rX - cfg.Split, 11 - rY);
                        NodeEdit.SetPos(nodesXHB[19], 367 - rX + cfg.Split, 11 - rY);

                        NodeEdit.SetPos(nodesXHB[8], 9999F, 9999F);
                        NodeEdit.SetPos(nodesXHB[9], 284F, 0F);
                        NodeEdit.SetPos(nodesXHB[10], 138F, 0F);
                        NodeEdit.SetPos(nodesXHB[11], 9999F, 9999F);
                        break;
                }
            }
        }
        private void ArrangeExBars(int state, int prevState, float scale, int anchorX, int anchorY, bool forceArrange = false)  // arrange our borrowed bars for EXHB if that feature is on
        {
            int lId = this.Configuration.borrowBarL;
            int rId = this.Configuration.borrowBarR;

            var baseExL = (AtkUnitBase*)Service.GameGui.GetAddonByName(barNames[lId], 1);
            var baseExR = (AtkUnitBase*)Service.GameGui.GetAddonByName(barNames[rId], 1);
            var nodesExL = baseExL->UldManager.NodeList;
            var nodesExR = baseExR->UldManager.NodeList;

            bool mixBar = GetCharConfig(535) == 1;

            Configuration cfg = this.Configuration;
            int lX = cfg.lX;
            int lY = cfg.lY;
            int rX = cfg.rX;
            int rY = cfg.rY;

            NodeEdit.SetVarious(nodesExL[0], new NodeEdit.NodeProps
            {
                Scale = scale,
                X = (float)(anchorX + lX * scale),
                Y = (float)(anchorY + lY * scale),
                OriginX = 0,
                OriginY = 0,
                Visible = true,
                Width = 295,
                Height = 120
            });

            NodeEdit.SetVarious(nodesExR[0], new NodeEdit.NodeProps
            {
                Scale = scale,
                X = (float)(anchorX + rX * scale),
                Y = (float)(anchorY + rY * scale),
                OriginX = 0,
                OriginY = 0,
                Visible = true,
                Width = 295,
                Height = 120
            });

            NodeEdit.SetScale(nodesExL[24], 0F);
            NodeEdit.SetScale(nodesExR[24], 0F);

            var contentsExL = GetExBarContents(true);
            var contentsExR = GetExBarContents(false);
            var contentsXHB = GetCrossbarContents();

            var inactiveConf = GetCharConfig(602);
            var standardConf = GetCharConfig(600);

            byte inactiveAlpha = (byte)((-0.0205 * Math.Pow(inactiveConf, 2)) - (0.5 * inactiveConf) + 255);
            byte standardAlpha = (byte)((-0.0205 * Math.Pow(standardConf, 2)) - (0.5 * standardConf) + 255);
            SetExAlpha(nodesExL, nodesExR, state == 0 ? standardAlpha : inactiveAlpha);

            // a lot of repetition here, could be condensed a great deal, but seeing exactly how each case lays out every button is helpful r/n
            switch (state) // WXHB OR NONE
            {
                case 0:
                case 5:
                case 6:
                    CopyButtons(contentsExL, 0, lId, 0, 8);
                    CopyButtons(contentsExR, 0, rId, 0, 8);

                    if (state != prevState || forceArrange)
                    {
                        SlotRangeVis(16, 31, false);
                        SetLastEightVis(nodesExL, nodesExR, false);

                        PlaceExButton(nodesExL[20], 0, cfg.Split, 0, state, true); //left EXHB
                        PlaceExButton(nodesExL[19], 1, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[18], 2, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[17], 3, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[16], 4, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[15], 5, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[14], 6, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[13], 7, cfg.Split, 0, state, true);

                        PlaceExButton(nodesExR[20], 8, cfg.Split, 0, state, true); // right EXHB
                        PlaceExButton(nodesExR[19], 9, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[18], 10, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[17], 11, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[16], 12, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[15], 13, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[14], 14, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[13], 15, cfg.Split, 0, state, true);
                    }
                    break;
                case 1:
                    CopyButtons(contentsExL, 0, lId, 0, 8);
                    CopyButtons(contentsExR, 0, rId, 0, 8);
                    CopyButtons(contentsXHB, 8, lId, 8, 4);
                    CopyButtons(contentsXHB, 12, rId, 8, 4);

                    if (state != prevState || forceArrange)
                    {
                        SlotRangeVis(16, 23, true);
                        SlotRangeVis(24, 31, prevState == 3 && !mixBar);
                        SlotRangeScale(16, 23, 1.1F);
                        SetLastEightVis(nodesExL, nodesExR, prevState == 3 && !mixBar);

                        PlaceExButton(nodesExL[20], 0, cfg.Split, 0, state, true);   //left EXHB
                        PlaceExButton(nodesExL[19], 1, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[18], 2, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[17], 3, cfg.Split, 0, state, true);

                        PlaceExButton(nodesExL[16], 4, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[15], 5, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[14], 6, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[13], 7, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[20], 8, cfg.Split, 0, state, true);   //right EXHB
                        PlaceExButton(nodesExR[19], 9, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[18], 10, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[17], 11, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[16], 12, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[15], 13, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[14], 14, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[13], 15, cfg.Split, 0, state, true);

                        PlaceExButton(nodesExL[12], 24, -lX + cfg.Split * 2, -lY, state, true); //right bar (left buttons)
                        PlaceExButton(nodesExL[11], 25, -lX + cfg.Split * 2, -lY, state, true);
                        PlaceExButton(nodesExL[10], 26, -lX + cfg.Split * 2, -lY, state, true);
                        PlaceExButton(nodesExL[9], 27, -lX + cfg.Split * 2, -lY, state, true);


                        PlaceExButton(nodesExR[12], 28, -rX + cfg.Split * 2, -rY, state, true);  //right bar (right buttons)
                        PlaceExButton(nodesExR[11], 29, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[10], 30, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[9], 31, -rX + cfg.Split * 2, -rY, state, true);
                    }

                    break;
                case 2:
                    CopyButtons(contentsExL, 0, lId, 0, 8);
                    CopyButtons(contentsExR, 0, rId, 0, 8);
                    CopyButtons(contentsXHB, 0, lId, 8, 4);
                    CopyButtons(contentsXHB, 4, rId, 8, 4);

                    if (state != prevState || forceArrange)
                    {
                        SlotRangeVis(24, 31, true);
                        SlotRangeVis(16, 23, prevState == 4 && !mixBar);
                        SlotRangeScale(24, 31, 1.1F);
                        SetLastEightVis(nodesExL, nodesExR, prevState == 4 && !mixBar);

                        PlaceExButton(nodesExL[20], 0, cfg.Split, 0, state, true); //left EXHB
                        PlaceExButton(nodesExL[19], 1, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[18], 2, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[17], 3, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[16], 4, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[15], 5, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[14], 6, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[13], 7, cfg.Split, 0, state, true);

                        PlaceExButton(nodesExR[20], 8, cfg.Split, 0, state, true);   //right EXHB
                        PlaceExButton(nodesExR[19], 9, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[18], 10, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[17], 11, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[16], 12, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[15], 13, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[14], 14, cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[13], 15, cfg.Split, 0, state, true);

                        PlaceExButton(nodesExL[12], 16, -lX, -lY, state, true); // left bar (left buttons)
                        PlaceExButton(nodesExL[11], 17, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[10], 18, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[9], 19, -lX, -lY, state, true);

                        PlaceExButton(nodesExR[12], 20, -rX, -rY, state, true); //left bar (right buttons)
                        PlaceExButton(nodesExR[11], 21, -rX, -rY, state, true);
                        PlaceExButton(nodesExR[10], 22, -rX, -rY, state, true);
                        PlaceExButton(nodesExR[9], 23, -rX, -rY, state, true);
                    }

                    break;
                case 3:
                    CopyButtons(contentsXHB, 0, lId, 0, 12);
                    CopyButtons(contentsXHB, 12, rId, 8, 4);

                    if (state != prevState || forceArrange)
                    {
                        SlotRangeVis(16, 31, true);
                        SlotRangeScale(0, 7, 1.1F);
                        if (mixBar) { SlotRangeScale(20, 23, 0.85F); }

                        PlaceExButton(nodesExL[20], 16, -lX, -lY, state, true); //inactive main XHB (first 3 sections)
                        PlaceExButton(nodesExL[19], 17, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[18], 18, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[17], 19, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[!mixBar ? 16 : 12], 20, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[!mixBar ? 15 : 11], 21, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[!mixBar ? 14 : 10], 22, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[!mixBar ? 13 : 9], 23, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[!mixBar ? 12 : 16], 24, -lX + cfg.Split * 2, -lY, state, true);
                        PlaceExButton(nodesExL[!mixBar ? 11 : 15], 25, -lX + cfg.Split * 2, -lY, state, true);
                        PlaceExButton(nodesExL[!mixBar ? 10 : 14], 26, -lX + cfg.Split * 2, -lY, state, true);
                        PlaceExButton(nodesExL[!mixBar ? 9 : 13], 27, -lX + cfg.Split * 2, -lY, state, true);

                        PlaceExButton(nodesExR[20], 8, +cfg.Split, 0, state, true); // right EXHB
                        PlaceExButton(nodesExR[19], 9, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[18], 10, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[17], 11, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[16], 12, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[15], 13, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[14], 14, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExR[13], 15, +cfg.Split, 0, state, true);

                        PlaceExButton(nodesExR[12], 28, -rX + cfg.Split * 2, -rY, state, true); // inactive main XHB (4th section)
                        PlaceExButton(nodesExR[11], 29, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[10], 30, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[9], 31, -rX + cfg.Split * 2, -rY, state, true);
                    }

                    break;
                case 4:
                    CopyButtons(contentsXHB, 0, lId, 8, 4);
                    CopyButtons(contentsXHB, 4, rId, 8, 4);
                    CopyButtons(contentsXHB, 8, rId, 0, 8);

                    if (state != prevState || forceArrange)
                    {
                        SlotRangeVis(16, 31, true);
                        SlotRangeScale(8, 15, 1.1F);
                        if (mixBar) SlotRangeScale(24, 27, 0.85F);

                        PlaceExButton(nodesExL[20], 0, +cfg.Split, 0, state, true); // left EXHB
                        PlaceExButton(nodesExL[19], 1, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[18], 2, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[17], 3, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[16], 4, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[15], 5, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[14], 6, +cfg.Split, 0, state, true);
                        PlaceExButton(nodesExL[13], 7, +cfg.Split, 0, state, true);

                        PlaceExButton(nodesExL[12], 16, -lX, -lY, state, true); //main XHB (first section)
                        PlaceExButton(nodesExL[11], 17, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[10], 18, -lX, -lY, state, true);
                        PlaceExButton(nodesExL[9], 19, -lX, -lY, state, true);

                        PlaceExButton(nodesExR[!mixBar ? 12 : 20], 20, -rX, -rY, state, true); //main XHB (second section)
                        PlaceExButton(nodesExR[!mixBar ? 11 : 19], 21, -rX, -rY, state, true);
                        PlaceExButton(nodesExR[!mixBar ? 10 : 18], 22, -rX, -rY, state, true);
                        PlaceExButton(nodesExR[!mixBar ? 9 : 17], 23, -rX, -rY, state, true);
                        PlaceExButton(nodesExR[!mixBar ? 20 : 12], 24, -rX + cfg.Split * 2, -rY, state, true); //main XHB (right side)
                        PlaceExButton(nodesExR[!mixBar ? 19 : 11], 25, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[!mixBar ? 18 : 10], 26, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[!mixBar ? 17 : 9], 27, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[16], 28, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[15], 29, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[14], 30, -rX + cfg.Split * 2, -rY, state, true);
                        PlaceExButton(nodesExR[13], 31, -rX + cfg.Split * 2, -rY, state, true);
                    }
                    break;
            }
        }
        private void PlaceExButton(AtkResNode* node, int posID, float xMod = 0, float yMod = 0, int exBarState = 0, bool Tween = false) //move a borrowed button into position and set its scale to animate if needed
        {
            var to = scaleMap[exBarState, slotPositions[posID].ScaleIndex];
            var pos = slotPositions[posID];
            pos.xMod = xMod;
            pos.yMod = yMod;
            pos.Node = node;

            if (Tween && to != pos.Scale) //only make a new tween if the button isn't already at the target scale, otherwise just set
            {
                if (pos.Tween == null || pos.Tween.ToScale != to) //only make a new tween if the button doesn't already have one with the same target scale
                {
                    pos.Tween = new ScaleTween
                    {
                        FromScale = pos.Scale,
                        ToScale = to,
                        Start = DateTime.Now,
                        Duration = new TimeSpan(0, 0, 0, 0, 40)
                    };
                    tweensExist = true;
                    return;
                }
            }
            else
            {
                pos.Tween = null;
                pos.Scale = to;
            }

            NodeEdit.SetVarious(node, new NodeEdit.NodeProps
            {
                Scale = pos.Scale,
                OriginX = pos.OrigX,
                OriginY = pos.OrigY,
                X = pos.X + xMod,
                Y = pos.Y + yMod,
                Visible = pos.Visible
            });

            return;
        }
        private void SetLastEightVis(AtkResNode** nodesL, AtkResNode** nodesR, bool show) //sometimes we need to display all 24 borrowed buttons, and sometimes we only need 16
        {
            for (var i = 9; i <= 12; i++)
            {
                NodeEdit.SetVis(nodesL[i], show);
                NodeEdit.SetVis(nodesR[i], show);
            }
        }
        private void SlotRangeVis(int start, int end, bool show)
        {
            for (var i = start; i <= end; i++) slotPositions[i].Visible = show;
        }//set visibility for a range of slots at once
        private void SlotRangeScale(int start, int end, float scale)
        {
            for (var i = start; i <= end; i++) slotPositions[i].Scale = scale;
        }//set scale for a range of slots at once
        private void SetExAlpha(AtkResNode** nodesL, AtkResNode** nodesR, byte alpha)
        {
            for (var i = 0; i < 12; i++)
            {
                nodesL[i]->Color.A = alpha;
                nodesR[i]->Color.A = alpha;
            }
        }//set alpha of our Ex buttons
        public void SetSelectColor(bool revert = false) //apply highlight colour chosen in CrossUp settings
        {
            var selectColor = revert ? new(1F, 1F, 1F) : this.Configuration.selectColor;
           
            var baseXHB = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            var baseRR = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossR", 1);
            var baseLL = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionDoubleCrossL", 1);

            NodeEdit.SetColor(baseXHB->UldManager.NodeList[4], selectColor);
            NodeEdit.SetColor(baseXHB->UldManager.NodeList[5], selectColor);
            NodeEdit.SetColor(baseXHB->UldManager.NodeList[6], selectColor);

            NodeEdit.SetColor(baseLL->UldManager.NodeList[3], selectColor);
            NodeEdit.SetColor(baseLL->UldManager.NodeList[4], selectColor);

            NodeEdit.SetColor(baseRR->UldManager.NodeList[3], selectColor);
            NodeEdit.SetColor(baseRR->UldManager.NodeList[4], selectColor);

            bool hide = !revert && this.Configuration.selectHide;   // we can't hide it by toggling visibility, so instead we do it by setting width to 0

            NodeEdit.SetSize(baseXHB->UldManager.NodeList[4],(ushort)(hide ? 0 : 304), 140);
            NodeEdit.SetSize(baseXHB->UldManager.NodeList[5],(ushort)(hide ? 0 : 166), 140);
            NodeEdit.SetSize(baseXHB->UldManager.NodeList[6],(ushort)(hide ? 0 : 166), 140);

            NodeEdit.SetSize(baseLL->UldManager.NodeList[3], (ushort)(hide ? 0 : 304), 140);
            NodeEdit.SetSize(baseLL->UldManager.NodeList[4], (ushort)(hide ? 0 : 166), 140);

            NodeEdit.SetSize(baseRR->UldManager.NodeList[3], (ushort)(hide ? 0 : 304), 140);
            NodeEdit.SetSize(baseRR->UldManager.NodeList[4], (ushort)(hide ? 0 : 166), 140);

        }
        public void ResetHud()  //cleanup: reset all the node properties we've messed with and restore actions to borrowed bars
        {
            var baseXHB = (AtkUnitBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            var nodesXHB = baseXHB->UldManager.NodeList;
            NodeEdit.SetSize(nodesXHB[7], 9, 76);
            NodeEdit.SetPos(nodesXHB[26], 284F, 152F);
            NodeEdit.SetPos(nodesXHB[27], 146F, 0F);
            NodeEdit.SetSize(nodesXHB[5], 166, 140);
            NodeEdit.SetSize(nodesXHB[6], 166, 140);

            for (var i = 1; i <= 9; i++)
            {
                ResetBarPos(i);
                bool shared = GetCharConfig((uint)(i + 515)) == 1;
                var jobID = Service.ClientState?.LocalPlayer?.ClassJob.Id;
                if (jobID != null)
                {
                    CopyButtons(GetSavedBar(shared ? 0 : (int)jobID, i, 12), 0, i, 0, 12);
                }
            }
            return;
        }
        public void ResetBarPos(int barID) //put a borrowed hotbar back the way we found it based on HUD layout settings
        {
            var baseHotbar = (AtkUnitBase*)Service.GameGui.GetAddonByName(barNames[barID], 1);
            var nodes = baseHotbar->UldManager.NodeList;

            var gridType = GetCharConfig((uint)(barID + 501));

            NodeEdit.SetVarious(nodes[0], new NodeEdit.NodeProps
            {
                Width = (ushort)barSizes[gridType].X,
                Height = (ushort)barSizes[gridType].Y,
                X = baseHotbar->X,
                Y = baseHotbar->Y,
                Scale = baseHotbar->Scale
            });

            NodeEdit.SetScale(nodes[24], 1F);

            for (var i = 0; i < 12; i++)
            {
                NodeEdit.SetVarious(nodes[20 - i], new NodeEdit.NodeProps
                {
                    X = barGrids[gridType, i].X,
                    Y = barGrids[gridType, i].Y,
                    Visible = true,
                    Scale = 1F
                });
            }
            return;
        }

        public bool tweensExist = false;
        private void TweenAllButtons()  //run any extant tweens to animate button scale
        {
            tweensExist = false;
            for (var i = 0; i < slotPositions.Length; i++)
            {
                var pos = slotPositions[i];
                var tween = pos.Tween;
                var node = pos.Node;
                if (tween != null && node != null)
                {
                    tweensExist = true;
                    var timePassed = DateTime.Now - tween.Start;
                    decimal progress = decimal.Divide(timePassed.Milliseconds, tween.Duration.Milliseconds);

                    if (progress >= 1)
                    {
                        pos.Tween = null;
                        pos.Scale = tween.ToScale;
                    }
                    else
                    {
                        pos.Scale = ((tween.ToScale - tween.FromScale) * (float)progress) + tween.FromScale;
                    }

                    NodeEdit.SetVarious(node, new NodeEdit.NodeProps
                    {
                        X = pos.X + pos.xMod,
                        Y = pos.Y + pos.yMod,
                        Scale = pos.Scale
                    });
                }
            }
            return;
        }


        // UTILITY
        public struct ButtonAction
        {
            public uint Id;
            public FFXIVClientStructs.FFXIV.Client.UI.Misc.HotbarSlotType CommandType;
        }
        private ButtonAction[] GetBarContentsByID(int barID, int slotCount, int fromSlot = 0) // main func for retrieving hotbar actions. most others point to this one
        {
            var contents = new ButtonAction[slotCount];
            var hotbar = raptureModule->HotBar[barID];

            for (int i = 0; i < slotCount; i++)
            {
                var slotStruct = hotbar->Slot[i + fromSlot];
                if (slotStruct != null)
                {
                    contents[i].CommandType = slotStruct->CommandType;
                    contents[i].Id = slotStruct->CommandType == FFXIVClientStructs.FFXIV.Client.UI.Misc.HotbarSlotType.Action ? actionManager->GetAdjustedActionId(slotStruct->CommandId) : slotStruct->CommandId;
                }
            }

            return contents;
        }
        private ButtonAction[] GetSavedBar(int job, int barID, int slotCount = 12)  // retrieve saved bar contents
        {
            var contents = new ButtonAction[slotCount];
            var savedBar = raptureModule->SavedClassJob[job]->Bar[barID];

            for (var i = 0; i < slotCount; i++)
            {
                contents[i].CommandType = savedBar->Slot[i]->Type;
                contents[i].Id = savedBar->Slot[i]->ID;
            }

            return contents;
        }
        private ButtonAction[] GetHotbarContents(int barID) //get a standard hotbar
        {
            return GetBarContentsByID(barID, 12);
        }
        private ButtonAction[] GetCrossbarContents()    //get whatever's on the cross hotbar
        {
            var barBaseXHB = (AddonActionBarBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
            var xBar = (AddonActionCross*)barBaseXHB;
            if (xBar->PetBar)
            {
                // FIX STILL NEEDED
                // I can identify if the pet hotbar is active, but I haven't worked out how to retrieve its contents.
                // Grabbing the same ol regular bar contents for now, even though it looks odd.
                // this does not affect gameplay, only visuals
                return GetBarContentsByID(barBaseXHB->HotbarID, 16);
            } else
            {
                return GetBarContentsByID(barBaseXHB->HotbarID, 16);
            }

        }
        private ButtonAction[] GetExBarContents(bool LR)    //get whatever's mapped to the Ex cross hotbars
        {
            var exConf = GetCharConfig((uint)(LR ? 561 : 560));
            int exBarTarget;
            bool useLeft;
            if (exConf < 16)
            {
                exBarTarget = (exConf >> 1) + 10;
                useLeft = exConf % 2 == 0;
            }
            else
            {
                var barBaseXHB = (AddonActionBarBase*)Service.GameGui.GetAddonByName("_ActionCross", 1);
                exBarTarget = (barBaseXHB->HotbarID + ((exConf < 18) ? -1 : 1) - 2) % 8 + 10;
                useLeft = exConf % 2 == 1;
            }

            var contents = GetBarContentsByID(exBarTarget, 8, useLeft ? 0 : 8);
            return contents;
        }
        private void CopyButtons(ButtonAction[] sourceButtons, int sourceSlot, int targetBarID, int targetSlot, int count) // copy a set of actions onto part/all of a bar
        {
            var targetBar = raptureModule->HotBar[targetBarID];
            for (var i = 0; i < count; i++)
            {
                var tButton = targetBar->Slot[i + targetSlot];
                var sButton = sourceButtons[i + sourceSlot];
                if (tButton->CommandId != sButton.Id || tButton->CommandType != sButton.CommandType)
                {
                    tButton->Set(sButton.CommandType, sButton.Id);
                }
            }
        }
        private int GetCharConfig(uint configID)    //get a char Config setting
        {
            //  501-508: int from 0-5, represents selected grid layout for hotbars 1-8
            //  515 - 523: toggle, share setting for hotbars 1 - 8
            //  535: main cross hotbar layout   0 = dpad / button / dpad / button
            //                                  1 = dpad / dpad / button / button
            //  560: mapping for RL expanded cross hotbar
            //  561: mapping for LR expanded cross hotbar
            //  586: toggle, cross hotbar active or not
            //  600: transparents of standard cross hotbar
            //  601: transparency of active cross hotbar
            //  602: transparency of inactive cross hotbar
            return charConfigs->GetIntValue(configID);
        }
        public void LogCharConfigs(uint start, uint end=0)  // for debugging
        {
            if (end < start) { end = start; }
            for (uint i=start;i<=end;i++) PluginLog.Log(i+" "+ GetCharConfig(i));
        }


        // POSITIONS AND REFERENCE
        public class ScaleTween
        {
            public DateTime Start { get; set; }
            public TimeSpan Duration { get; set; }
            public float FromScale { get; set; }
            public float ToScale { get; set; }
        }
        public class Position
        {
            public float Scale { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int OrigX { get; set; }
            public int OrigY { get; set; }
            public bool Visible { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public ScaleTween? Tween { get; set; }
            public int ScaleIndex { get; set; }
            public AtkResNode* Node { get; set; }
            public float xMod { get; set; }
            public float yMod { get; set; }
        }
        private static Position[] slotPositions = { // the positions for all the borrowed buttons

            //EXHB LEFT 0-7
            new Position{ Scale=1.0F,X=0,Y=24,OrigX=94,OrigY=39,Visible=true,ScaleIndex=2},
            new Position{ Scale=1.0F,X=42,Y=0,OrigX=52,OrigY=63,Visible=true,ScaleIndex=2},
            new Position{ Scale=1.0F,X=84,Y=24,OrigX=10,OrigY=39,Visible=true,ScaleIndex=2},
            new Position{ Scale=1.0F,X=42,Y=48,OrigX=52,OrigY=15,Visible=true,ScaleIndex=2},

            new Position{ Scale=1.0F,X=138,Y=24,OrigX=94,OrigY=39,Visible=true,ScaleIndex=2},
            new Position{ Scale=1.0F,X=180,Y=0,OrigX=52,OrigY=63,Visible=true,ScaleIndex=2},
            new Position{ Scale=1.0F,X=222,Y=24,OrigX=10,OrigY=39,Visible=true,ScaleIndex=2},
            new Position{ Scale=1.0F,X=180,Y=48,OrigX=52,OrigY=15,Visible=true,ScaleIndex=2},
            
            //EXHB RIGHT 8-15
            new Position{ Scale=1.0F,X=0,Y=24,OrigX=94,OrigY=39,Visible=true,ScaleIndex=3},
            new Position{ Scale=1.0F,X=42,Y=0,OrigX=52,OrigY=63,Visible=true,ScaleIndex=3},
            new Position{ Scale=1.0F,X=84,Y=24,OrigX=10,OrigY=39,Visible=true,ScaleIndex=3},
            new Position{ Scale=1.0F,X=42,Y=48,OrigX=52,OrigY=15,Visible=true,ScaleIndex=3},

            new Position{ Scale=1.0F,X=138,Y=24,OrigX=94,OrigY=39,Visible=true,ScaleIndex=3},
            new Position{ Scale=1.0F,X=180,Y=0,OrigX=52,OrigY=63,Visible=true,ScaleIndex=3},
            new Position{ Scale=1.0F,X=222,Y=24,OrigX=10,OrigY=39,Visible=true,ScaleIndex=3},
            new Position{ Scale=1.0F,X=180,Y=48,OrigX=52,OrigY=15,Visible=true,ScaleIndex=3},
            
            //MAIN BAR LEFT 16-23
            new Position{ Scale=1.0F,X=-142,Y=24,OrigX=94,OrigY=39,Visible=true,ScaleIndex=0},
            new Position{ Scale=1.0F,X=-100,Y=0,OrigX=52,OrigY=63,Visible=true,ScaleIndex=0},
            new Position{ Scale=1.0F,X=-58,Y=24,OrigX=10,OrigY=39,Visible=true,ScaleIndex=0},
            new Position{ Scale=1.0F,X=-100,Y=48,OrigX=52,OrigY=15,Visible=true,ScaleIndex=0},

            new Position{ Scale=1.0F,X=-9,Y=24,OrigX=95,OrigY=39,Visible=true,ScaleIndex=0},
            new Position{ Scale=1.0F,X=33,Y=0,OrigX=53,OrigY=63,Visible=true,ScaleIndex=0},
            new Position{ Scale=1.0F,X=75,Y=24,OrigX=11,OrigY=39,Visible=true,ScaleIndex=0},
            new Position{ Scale=1.0F,X=33,Y=48,OrigX=53,OrigY=15,Visible=true,ScaleIndex=0},
            
            //MAIN BAR RIGHT 24-31
            new Position{ Scale=1.0F,X=142,Y=24,OrigX=94,OrigY=39,Visible=true,ScaleIndex=1},
            new Position{ Scale=1.0F,X=184,Y=0,OrigX=52,OrigY=63,Visible=true,ScaleIndex=1},
            new Position{ Scale=1.0F,X=226,Y=24,OrigX=10,OrigY=39,Visible=true,ScaleIndex=1},
            new Position{ Scale=1.0F,X=184,Y=48,OrigX=52,OrigY=15,Visible=true,ScaleIndex=1},

            new Position{ Scale=1.0F,X=275,Y=24,OrigX=95,OrigY=39,Visible=true,ScaleIndex=1},
            new Position{ Scale=1.0F,X=317,Y=0,OrigX=53,OrigY=63,Visible=true,ScaleIndex=1},
            new Position{ Scale=1.0F,X=359,Y=24,OrigX=11,OrigY=39,Visible=true,ScaleIndex=1},
            new Position{ Scale=1.0F,X=317,Y=48,OrigX=53,OrigY=15,Visible=true,ScaleIndex=1},
        };
        public static readonly float[,] scaleMap = new float[7, 4] { // the scale each section of buttons should be at in each state
            {1F,1F,1F,1F},             //0: none selected
            {1.1F,0.85F,0.85F,0.85F},  //1: left selected
            {0.85F,1.1F,0.85F,0.85F},  //2: right selected
            {0.85F,0.85F,1.1F,0.85F},  //3: LR selected
            {0.85F,0.85F,0.85F,1.1F},  //4: RL selected
            {0.85F,0.85F,0.85F,0.85F}, //5: WXHB L selected
            {0.85F,0.85F,0.85F,0.85F}, //6: WXHB R selected
        };
        public readonly Vector2[,] barGrids = {
            {
                new Vector2{ X=34,Y=0},
                new Vector2{ X=79,Y=0},
                new Vector2{ X=124,Y=0},
                new Vector2{ X=169,Y=0},
                new Vector2{ X=214,Y=0},
                new Vector2{ X=259,Y=0},
                new Vector2{ X=304,Y=0},
                new Vector2{ X=349,Y=0},
                new Vector2{ X=394,Y=0},
                new Vector2{ X=439,Y=0},
                new Vector2{ X=484,Y=0},
                new Vector2{ X=529,Y=0},
            },
            {
                new Vector2{ X=34,Y=0},
                new Vector2{ X=79,Y=0},
                new Vector2{ X=124,Y=0},
                new Vector2{ X=169,Y=0},
                new Vector2{ X=214,Y=0},
                new Vector2{ X=259,Y=0},
                new Vector2{ X=34,Y=49},
                new Vector2{ X=79,Y=49},
                new Vector2{ X=124,Y=49},
                new Vector2{ X=169,Y=49},
                new Vector2{ X=214,Y=49},
                new Vector2{ X=259,Y=49},
            },
            {
                new Vector2{ X=34,Y=0},
                new Vector2{ X=79,Y=0},
                new Vector2{ X=124,Y=0},
                new Vector2{ X=169,Y=0},
                new Vector2{ X=34,Y=49},
                new Vector2{ X=79,Y=49},
                new Vector2{ X=124,Y=49},
                new Vector2{ X=169,Y=49},
                new Vector2{ X=34,Y=98},
                new Vector2{ X=79,Y=98},
                new Vector2{ X=124,Y=98},
                new Vector2{ X=169,Y=98},
            },
            {
                new Vector2{ X=0,Y=0},
                new Vector2{ X=45,Y=0},
                new Vector2{ X=90,Y=0},
                new Vector2{ X=0,Y=49},
                new Vector2{ X=45,Y=49},
                new Vector2{ X=90,Y=49},
                new Vector2{ X=0,Y=98},
                new Vector2{ X=45,Y=98},
                new Vector2{ X=90,Y=98},
                new Vector2{ X=0,Y=147},
                new Vector2{ X=45,Y=147},
                new Vector2{ X=90,Y=147},
            },
            {
                new Vector2{ X=0,Y=0},
                new Vector2{ X=45,Y=0},
                new Vector2{ X=0,Y=49},
                new Vector2{ X=45,Y=49},
                new Vector2{ X=0,Y=98},
                new Vector2{ X=45,Y=98},
                new Vector2{ X=0,Y=147},
                new Vector2{ X=45,Y=147},
                new Vector2{ X=0,Y=196},
                new Vector2{ X=45,Y=196},
                new Vector2{ X=0,Y=245},
                new Vector2{ X=45,Y=245},
            },
            {
                new Vector2{ X=0,Y=14},
                new Vector2{ X=0,Y=59},
                new Vector2{ X=0,Y=104},
                new Vector2{ X=0,Y=149},
                new Vector2{ X=0,Y=194},
                new Vector2{ X=0,Y=239},
                new Vector2{ X=0,Y=284},
                new Vector2{ X=0,Y=329},
                new Vector2{ X=0,Y=374},
                new Vector2{ X=0,Y=419},
                new Vector2{ X=0,Y=464},
                new Vector2{ X=0,Y=509},
            },
        }; // default grid layouts for when restoring borrowed hotbars to their normal state
        public readonly Vector2[] barSizes =    // default bar sizes for same purpose
        {
            new Vector2{ X=624,Y=72 },
            new Vector2{ X=331,Y=121 },
            new Vector2{ X=241,Y=170 },
            new Vector2{ X=162,Y=260 },
            new Vector2{ X=117,Y=358 },
            new Vector2{ X=72,Y=618 },
        };
        private static readonly string[] barNames = {
            "_ActionBar",
            "_ActionBar01",
            "_ActionBar02",
            "_ActionBar03",
            "_ActionBar04",
            "_ActionBar05",
            "_ActionBar06",
            "_ActionBar07",
            "_ActionBar08",
            "_ActionBar09",
        };


    }
}
