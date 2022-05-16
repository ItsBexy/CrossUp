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
    public unsafe sealed class CrossUp : IDalamudPlugin
    {
        public string Name => "CrossUp";
        private const string mainCommand = "/pcrossup";
        private readonly ActionManager* actionManager;
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

            actionBarBaseUpdateHook ??= Common.Hook<ActionBarBaseUpdate>("E8 ?? ?? ?? ?? 83 BB ?? ?? ?? ?? ?? 75 09", ActionBarBaseUpdateDetour);
            actionBarBaseUpdateHook?.Enable();

            Service.Framework.Update += FrameworkUpdate;
            Status.initialized = false;
        }
        private static class Status
        {
            public static bool tweensExist = false;
            public static bool initialized = false;
            public static int crossBarSelection = 0;
            public static int crossBarActive = 1;
        }
        private void DrawUI()
        {
            this.CrossUpUI.Draw();
        }
        private void DrawConfigUI()
        {
            this.CrossUpUI.SettingsVisible = true;
        }
        public void Dispose()   // put all the bars back in their normal places and take out our hooks
        {
            actionBarBaseUpdateHook?.Disable();
            Service.Framework.Update -= FrameworkUpdate;
            CommandManager.RemoveHandler(mainCommand);
            this.Configuration.Split = 0;
            this.Configuration.SepExBar = false;
            DisableEx();
            SetSelectColor(true);
            Status.initialized = false;
            CrossUpUI.Dispose();
        }

        // HOOKS AND EVENTS
        private void OnMainCommand(string command, string args)
        {
            this.CrossUpUI.SettingsVisible = true;
        }
        private void FrameworkUpdate(DalamudFramework framework)
        {
            if (!Service.ClientState.IsLoggedIn && Ref.UnitBases.Cross == null)
                return;

            if (!Status.initialized)
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
            else if (Status.initialized && Status.tweensExist)
            {
                TweenAllButtons();
            }
            else
            {
                UpdateBarState();
            }

            return;
        }
        private void Initialize()
        {
            Status.initialized = true;
            SetSelectColor();
            UpdateBarState(true, false);

            for (var i = 1; i <= 10; i++)
            {
                Task.Delay(500 * i).ContinueWith(antecedent => { if (Status.initialized) { UpdateBarState(true, false); } });
            }

            return;
        }
        private byte ActionBarBaseUpdateDetour(AddonActionBarBase* addonActionBarBase, NumberArrayData** numberArrayData, StringArrayData** stringArrayData)
        {
            var ret = actionBarBaseUpdateHook.Original(addonActionBarBase, numberArrayData, stringArrayData);

            try
            {
                if (Status.initialized && addonActionBarBase->HotbarID == 0) // all the bars fire at once every time anything happens, so we'll just take the first bar
                {
                    var activeNow = GetCharConfig(586);
                    UpdateBarState(activeNow != Status.crossBarActive, true);
                    Status.crossBarActive = activeNow;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Log(ex + "");
            }

            return ret;
        }
        public void UpdateBarState(bool forceArrange = false, bool HudfixCheck = false)
        {
            var newSelection = GetCrossBarSelection();
            ArrangeAndFill(newSelection, Status.crossBarSelection, forceArrange, HudfixCheck);
            Status.crossBarSelection = newSelection;

            return;
        }
        private static int GetCrossBarSelection() // Return value from 0-6 to indicate which cross bar is selected (if any)
        {
            var barBaseXHB = (AddonActionBarBase*)Ref.UnitBases.Cross;
            var xBar = (AddonActionCross*)Ref.UnitBases.Cross;

            var baseLL = Ref.UnitBases.LL;
            var baseRR = Ref.UnitBases.RR;

            if (barBaseXHB == null || xBar == null || baseLL == null || baseRR == null)
            {
                return Status.crossBarSelection;
            }

            int newCrossBarState =
            xBar->LeftBar                      ? 1 : // LEFT BAR
            xBar->RightBar                     ? 2 : // RIGHT BAR
            xBar->ExpandedHoldControlsLTRT > 0 ? 3 : // L->R EX BAR
            xBar->ExpandedHoldControlsRTLT > 0 ? 4 : // R->L EX BAR

            // need a better way to find these two, watching the UI node means we'll always be one frame late
            baseLL->UldManager.NodeList[3]->IsVisible ? 5 : // WXHB L
            baseRR->UldManager.NodeList[3]->IsVisible ? 6 : // WXHB R
                                                        0;
            return newCrossBarState;
        }

        public void EnableEx()
        {
            var lID = this.Configuration.borrowBarL;
            var rID = this.Configuration.borrowBarR;

            if (lID < 0 || rID < 0) { return; }

            EnableBorrowedBar(lID);
            EnableBorrowedBar(rID);

            UpdateBarState(true, false);
            Task.Delay(20).ContinueWith(antecedent => { UpdateBarState(true, false); });

            return;
        }

        private static bool[] wasHidden = { false, false, false, false, false, false, false, false, false, false };
        public void EnableBorrowedBar(int id)
        {
            var unitBase = Ref.UnitBases.ActionBar[id];
            if (unitBase == null) { return; }

            var visID = (uint)(id + 485);
            if (charConfigs->GetIntValue(visID) == 0)
            {
                wasHidden[id] = true;
                charConfigs->SetOption(visID, 1);
            }

            for (var i = 9; i <= 20; i++) unitBase->UldManager.NodeList[i]->Flags_2 |= 0xD;
        }
        public void DisableEx()
        {
            ArrangeAndFill(0, 0, true, false);
            ResetHud();
            return;
        }

        // BAR ARRANGEMENT
        public class ScaleTween
        {
            public DateTime Start { get; set; }
            public TimeSpan Duration { get; set; }
            public float FromScale { get; set; }
            public float ToScale { get; set; }
        }

        public class MetaSlot
        {
            public bool Visible { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public float Scale { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public int OrigX { get; set; }
            public int OrigY { get; set; }
            public ScaleTween? Tween { get; set; }
            public int ScaleIndex { get; set; }
            public AtkResNode* Node { get; set; }
            public float Xmod { get; set; }
            public float Ymod { get; set; }
            public static implicit operator NodeEdit.PropertySet(MetaSlot p) => new() {X=p.X+p.Xmod,Y=p.Y+p.Ymod,Scale=p.Scale,Width=p.Width,Height=p.Height,Visible=p.Visible,OrigX=p.OrigX,OrigY=p.OrigY};
        }

        private void ArrangeAndFill(int select, int prevSelect = 0, bool forceArrange = false, bool HUDfixCheck = true) // the centrepiece of it all
        {
            if (GetCharConfig(586) == 0)    //don't do anything if the cross hotbar isn't actually turned on
            {
                ResetHud();
                return;
            }

            Configuration cfg = Configuration;

            var baseXHB = Ref.UnitBases.Cross;
            if (baseXHB == null) { return; }
            var rootNode = baseXHB->UldManager.NodeList[0];

            var scale = rootNode->ScaleX;
            var mixBar = GetCharConfig(535) == 1;

            // fix for misalignment after entering HUD Layout interface (unsure if this is sufficient)
            if (HUDfixCheck && baseXHB->X - rootNode->X - Math.Round(cfg.Split * scale) < 0) { baseXHB->X += (short)(cfg.Split * scale); }

            NodeEdit.SetVarious(rootNode, new NodeEdit.PropertySet
            {
                X = baseXHB->X - cfg.Split * scale,
                Y = baseXHB->Y,
                Width = (ushort)(588 + cfg.Split * 2),
                Height = 210
            });


            int anchorX = (int)(rootNode->X + (146 * scale));
            int anchorY = (int)(rootNode->Y + (70 * scale));

            bool arrangeEX = cfg.SepExBar && cfg.borrowBarL > 0 && cfg.borrowBarR > 0;
            if (arrangeEX)
            {
                ArrangeExBars(select, prevSelect, scale, anchorX, anchorY, forceArrange);
            }

            int lX = arrangeEX ? cfg.lX : 0;
            int lY = arrangeEX ? cfg.lY : 0;
            int rX = arrangeEX ? cfg.rX : 0;
            int rY = arrangeEX ? cfg.rY : 0;

            bool hideDivider = cfg.Split > 0 || cfg.SepExBar;

            NodeEdit.ByLookup.AbsoluteSize(Ref.barNodes.Cross.VertLine, hideDivider ? 0 : null, hideDivider ? 0 : null);
            NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.padlock, cfg.PadlockOffset.X + cfg.Split, cfg.PadlockOffset.Y);
            NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.changeSet, cfg.ChangeSetOffset.X + cfg.Split, cfg.ChangeSetOffset.Y);
            NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.setText, cfg.SetTextOffset.X + cfg.Split, cfg.SetTextOffset.Y);

            if (select != prevSelect || forceArrange) // generally only want to rearrange bars if the cross hotbar state has actually changed
            {
                switch (select)
                {
                    case 0: // NONE
                    case 5: // LEFT WXHB
                    case 6: // RIGHT WXHB
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Component, 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.VertLine, 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.LT, 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.RT, cfg.Split * 2, 0F);

                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[0], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[1], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[2], cfg.Split * 2, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[3], cfg.Split * 2, 0F);
                        break;
                    case 1: //LEFT BAR

                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Component, 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.VertLine, cfg.Split * 2, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.LT, 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.RT, cfg.Split * 2, 0F);

                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[0], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[1], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[2], cfg.Split * 2, prevSelect == 3 && arrangeEX && !mixBar ? 9999F : 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[3], cfg.Split * 2, prevSelect == 3 && arrangeEX && !mixBar ? 9999F : 0F);

                        NodeEdit.ByLookup.AbsoluteSize(Ref.barNodes.Cross.miniSelectL, (ushort)(cfg.selectHide || (mixBar && cfg.Split > 0) ? 0 : 166), 140);
                        NodeEdit.ByLookup.AbsoluteSize(Ref.barNodes.Cross.miniSelectR, (ushort)(cfg.selectHide || (mixBar && cfg.Split > 0) ? 0 : 166), 140);

                        break;
                    case 2: // RIGHT BAR

                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Component, cfg.Split * 2, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.VertLine, 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.LT, -cfg.Split * 2, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.RT, 0F, 0F);

                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[0], -cfg.Split * 2, prevSelect == 4 && arrangeEX && !mixBar ? 9999F : 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[1], -cfg.Split * 2, prevSelect == 4 && arrangeEX && !mixBar ? 9999F : 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[2], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[3], 0F, 0F);

                        NodeEdit.ByLookup.AbsoluteSize(Ref.barNodes.Cross.miniSelectL, (ushort)(cfg.selectHide || (mixBar && cfg.Split > 0) ? 0 : 166), 140);
                        NodeEdit.ByLookup.AbsoluteSize(Ref.barNodes.Cross.miniSelectR, (ushort)(cfg.selectHide || (mixBar && cfg.Split > 0) ? 0 : 166), 140);

                        break;
                    case 3: // L->R BAR

                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Component, lX + cfg.Split, lY);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.VertLine, -lX + cfg.Split, -lY);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.LT, -lX - cfg.Split, -lY);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.RT, -lX + cfg.Split, -lY);

                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[0], 9999F, 9999F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[1], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[2], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[3], 9999F, 9999F);

                        break;
                    case 4: // R->L BAR
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Component, rX + cfg.Split, rY);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.VertLine, -rX + cfg.Split, -rY);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.LT, -rX - cfg.Split, -rY);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.RT, -rX + cfg.Split, -rY);

                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[0], 9999F, 9999F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[1], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[2], 0F, 0F);
                        NodeEdit.ByLookup.RelativePos(Ref.barNodes.Cross.Sets[3], 9999F, 9999F);
                        break;
                }
            }
        }
        private void ArrangeExBars(int select, int prevSelect, float scale, int anchorX, int anchorY, bool forceArrange = false)  // arrange our borrowed bars for EXHB if that feature is on
        {
            int lId = this.Configuration.borrowBarL;
            int rId = this.Configuration.borrowBarR;

            var baseExL = Ref.UnitBases.ActionBar[lId];
            var baseExR = Ref.UnitBases.ActionBar[rId];
            if (baseExL == null || baseExR == null) { return; }

            SetDragDropNodeVis(baseExL, true);
            SetDragDropNodeVis(baseExR, true);
            SetKeybindVis(baseExL, false);
            SetKeybindVis(baseExR, false);

            var nodesExL = baseExL->UldManager.NodeList;
            var nodesExR = baseExR->UldManager.NodeList;

            bool mixBar = GetCharConfig(535) == 1;

            Configuration cfg = this.Configuration;
            int lX = cfg.lX;
            int lY = cfg.lY;
            int rX = cfg.rX;
            int rY = cfg.rY;

            NodeEdit.SetVarious(nodesExL[0], new NodeEdit.PropertySet
            {
                Scale = scale,
                X = (float)(anchorX + lX * scale + cfg.Split),
                Y = (float)(anchorY + lY * scale),
                Visible = true,
                Width = 295,
                Height = 120
            });

            NodeEdit.SetVarious(nodesExR[0], new NodeEdit.PropertySet
            {
                Scale = scale,
                X = (float)(anchorX + rX * scale + cfg.Split),
                Y = (float)(anchorY + rY * scale),
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
            SetExAlpha(nodesExL, nodesExR, select == 0 ? standardAlpha : inactiveAlpha);

            // a lot of repetition here, could be condensed a great deal, but seeing exactly how each case lays out every button is helpful r/n
            switch (select) // WXHB OR NONE
            {
                case 0:
                case 5:
                case 6:
                    CopyButtons(contentsExL, 0, lId, 0, 8);
                    CopyButtons(contentsExR, 0, rId, 0, 8);

                    if (select != prevSelect || forceArrange)
                    {
                        SlotRangeVis(16, 31, false);
                        SetLastEightVis(nodesExL, nodesExR, false);

                        PlaceExButton(nodesExL[20], Ref.Pos.leftEX[0], 0, 0, select, true); //left EXHB
                        PlaceExButton(nodesExL[19], Ref.Pos.leftEX[1], 0, 0, select, true);
                        PlaceExButton(nodesExL[18], Ref.Pos.leftEX[2], 0, 0, select, true);
                        PlaceExButton(nodesExL[17], Ref.Pos.leftEX[3], 0, 0, select, true);
                        PlaceExButton(nodesExL[16], Ref.Pos.leftEX[4], 0, 0, select, true);
                        PlaceExButton(nodesExL[15], Ref.Pos.leftEX[5], 0, 0, select, true);
                        PlaceExButton(nodesExL[14], Ref.Pos.leftEX[6], 0, 0, select, true);
                        PlaceExButton(nodesExL[13], Ref.Pos.leftEX[7], 0, 0, select, true);

                        PlaceExButton(nodesExR[20], Ref.Pos.rightEX[0], 0, 0, select, true); // right EXHB
                        PlaceExButton(nodesExR[19], Ref.Pos.rightEX[1], 0, 0, select, true);
                        PlaceExButton(nodesExR[18], Ref.Pos.rightEX[2], 0, 0, select, true);
                        PlaceExButton(nodesExR[17], Ref.Pos.rightEX[3], 0, 0, select, true);
                        PlaceExButton(nodesExR[16], Ref.Pos.rightEX[4], 0, 0, select, true);
                        PlaceExButton(nodesExR[15], Ref.Pos.rightEX[5], 0, 0, select, true);
                        PlaceExButton(nodesExR[14], Ref.Pos.rightEX[6], 0, 0, select, true);
                        PlaceExButton(nodesExR[13], Ref.Pos.rightEX[7], 0, 0, select, true);
                    }
                    break;
                case 1:
                    CopyButtons(contentsExL, 0, lId, 0, 8);
                    CopyButtons(contentsExR, 0, rId, 0, 8);
                    CopyButtons(contentsXHB, 8, lId, 8, 4);
                    CopyButtons(contentsXHB, 12, rId, 8, 4);

                    if (select != prevSelect || forceArrange)
                    {
                        SlotRangeVis(16, 23, true);
                        SlotRangeVis(24, 31, prevSelect == 3 && !mixBar);
                        SlotRangeScale(16, 23, 1.1F);
                        SetLastEightVis(nodesExL, nodesExR, prevSelect == 3 && !mixBar);

                        PlaceExButton(nodesExL[20], Ref.Pos.leftEX[0], 0, 0, select, true); //left EXHB
                        PlaceExButton(nodesExL[19], Ref.Pos.leftEX[1], 0, 0, select, true);
                        PlaceExButton(nodesExL[18], Ref.Pos.leftEX[2], 0, 0, select, true);
                        PlaceExButton(nodesExL[17], Ref.Pos.leftEX[3], 0, 0, select, true);
                        PlaceExButton(nodesExL[16], Ref.Pos.leftEX[4], 0, 0, select, true);
                        PlaceExButton(nodesExL[15], Ref.Pos.leftEX[5], 0, 0, select, true);
                        PlaceExButton(nodesExL[14], Ref.Pos.leftEX[6], 0, 0, select, true);
                        PlaceExButton(nodesExL[13], Ref.Pos.leftEX[7], 0, 0, select, true);

                        PlaceExButton(nodesExR[20], Ref.Pos.rightEX[0], 0, 0, select, true); // right EXHB
                        PlaceExButton(nodesExR[19], Ref.Pos.rightEX[1], 0, 0, select, true);
                        PlaceExButton(nodesExR[18], Ref.Pos.rightEX[2], 0, 0, select, true);
                        PlaceExButton(nodesExR[17], Ref.Pos.rightEX[3], 0, 0, select, true);
                        PlaceExButton(nodesExR[16], Ref.Pos.rightEX[4], 0, 0, select, true);
                        PlaceExButton(nodesExR[15], Ref.Pos.rightEX[5], 0, 0, select, true);
                        PlaceExButton(nodesExR[14], Ref.Pos.rightEX[6], 0, 0, select, true);
                        PlaceExButton(nodesExR[13], Ref.Pos.rightEX[7], 0, 0, select, true);

                        PlaceExButton(nodesExL[12], Ref.Pos.XHB[2, 0], -lX + cfg.Split, -lY, select, true); //right bar (left buttons)
                        PlaceExButton(nodesExL[11], Ref.Pos.XHB[2, 1], -lX + cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[10], Ref.Pos.XHB[2, 2], -lX + cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[9], Ref.Pos.XHB[2, 3], -lX + cfg.Split, -lY, select, true);

                        PlaceExButton(nodesExR[12], Ref.Pos.XHB[3, 0], -rX + cfg.Split, -rY, select, true);  //right bar (right buttons)
                        PlaceExButton(nodesExR[11], Ref.Pos.XHB[3, 1], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[10], Ref.Pos.XHB[3, 2], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[9], Ref.Pos.XHB[3, 3], -rX + cfg.Split, -rY, select, true);
                    }

                    break;
                case 2:
                    CopyButtons(contentsExL, 0, lId, 0, 8);
                    CopyButtons(contentsExR, 0, rId, 0, 8);
                    CopyButtons(contentsXHB, 0, lId, 8, 4);
                    CopyButtons(contentsXHB, 4, rId, 8, 4);

                    if (select != prevSelect || forceArrange)
                    {
                        SlotRangeVis(24, 31, true);
                        SlotRangeVis(16, 23, prevSelect == 4 && !mixBar);
                        SlotRangeScale(24, 31, 1.1F);
                        SetLastEightVis(nodesExL, nodesExR, prevSelect == 4 && !mixBar);

                        PlaceExButton(nodesExL[20], Ref.Pos.leftEX[0], 0, 0, select, true); //left EXHB
                        PlaceExButton(nodesExL[19], Ref.Pos.leftEX[1], 0, 0, select, true);
                        PlaceExButton(nodesExL[18], Ref.Pos.leftEX[2], 0, 0, select, true);
                        PlaceExButton(nodesExL[17], Ref.Pos.leftEX[3], 0, 0, select, true);
                        PlaceExButton(nodesExL[16], Ref.Pos.leftEX[4], 0, 0, select, true);
                        PlaceExButton(nodesExL[15], Ref.Pos.leftEX[5], 0, 0, select, true);
                        PlaceExButton(nodesExL[14], Ref.Pos.leftEX[6], 0, 0, select, true);
                        PlaceExButton(nodesExL[13], Ref.Pos.leftEX[7], 0, 0, select, true);

                        PlaceExButton(nodesExR[20], Ref.Pos.rightEX[0], 0, 0, select, true); // right EXHB
                        PlaceExButton(nodesExR[19], Ref.Pos.rightEX[1], 0, 0, select, true);
                        PlaceExButton(nodesExR[18], Ref.Pos.rightEX[2], 0, 0, select, true);
                        PlaceExButton(nodesExR[17], Ref.Pos.rightEX[3], 0, 0, select, true);
                        PlaceExButton(nodesExR[16], Ref.Pos.rightEX[4], 0, 0, select, true);
                        PlaceExButton(nodesExR[15], Ref.Pos.rightEX[5], 0, 0, select, true);
                        PlaceExButton(nodesExR[14], Ref.Pos.rightEX[6], 0, 0, select, true);
                        PlaceExButton(nodesExR[13], Ref.Pos.rightEX[7], 0, 0, select, true);

                        PlaceExButton(nodesExL[12], Ref.Pos.XHB[0, 0], -lX - cfg.Split, -lY, select, true); // left bar (left buttons)
                        PlaceExButton(nodesExL[11], Ref.Pos.XHB[0, 1], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[10], Ref.Pos.XHB[0, 2], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[9], Ref.Pos.XHB[0, 3], -lX - cfg.Split, -lY, select, true);

                        PlaceExButton(nodesExR[12], Ref.Pos.XHB[1, 0], -rX - cfg.Split, -rY, select, true); //left bar (right buttons)
                        PlaceExButton(nodesExR[11], Ref.Pos.XHB[1, 1], -rX - cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[10], Ref.Pos.XHB[1, 2], -rX - cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[9], Ref.Pos.XHB[1, 3], -rX - cfg.Split, -rY, select, true);
                    }

                    break;
                case 3:
                    CopyButtons(contentsXHB, 0, lId, 0, 12);
                    CopyButtons(contentsXHB, 12, rId, 8, 4);

                    if (select != prevSelect || forceArrange)
                    {
                        SlotRangeVis(16, 31, true);
                        SlotRangeScale(0, 7, 1.1F);
                        if (mixBar) { SlotRangeScale(20, 23, 0.85F); }

                        PlaceExButton(nodesExL[20], Ref.Pos.XHB[0, 0], -lX - cfg.Split, -lY, select, true); //inactive main XHB (first 3 sections)
                        PlaceExButton(nodesExL[19], Ref.Pos.XHB[0, 1], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[18], Ref.Pos.XHB[0, 2], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[17], Ref.Pos.XHB[0, 3], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[!mixBar ? 16 : 12], Ref.Pos.XHB[1, 0], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[!mixBar ? 15 : 11], Ref.Pos.XHB[1, 1], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[!mixBar ? 14 : 10], Ref.Pos.XHB[1, 2], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[!mixBar ? 13 : 9], Ref.Pos.XHB[1, 3], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[!mixBar ? 12 : 16], Ref.Pos.XHB[2, 0], -lX + cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[!mixBar ? 11 : 15], Ref.Pos.XHB[2, 1], -lX + cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[!mixBar ? 10 : 14], Ref.Pos.XHB[2, 2], -lX + cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[!mixBar ? 9 : 13], Ref.Pos.XHB[2, 3], -lX + cfg.Split, -lY, select, true);

                        PlaceExButton(nodesExR[20], Ref.Pos.rightEX[0], 0, 0, select, true); // right EXHB
                        PlaceExButton(nodesExR[19], Ref.Pos.rightEX[1], 0, 0, select, true);
                        PlaceExButton(nodesExR[18], Ref.Pos.rightEX[2], 0, 0, select, true);
                        PlaceExButton(nodesExR[17], Ref.Pos.rightEX[3], 0, 0, select, true);
                        PlaceExButton(nodesExR[16], Ref.Pos.rightEX[4], 0, 0, select, true);
                        PlaceExButton(nodesExR[15], Ref.Pos.rightEX[5], 0, 0, select, true);
                        PlaceExButton(nodesExR[14], Ref.Pos.rightEX[6], 0, 0, select, true);
                        PlaceExButton(nodesExR[13], Ref.Pos.rightEX[7], 0, 0, select, true);

                        PlaceExButton(nodesExR[12], Ref.Pos.XHB[3, 0], -rX + cfg.Split, -rY, select, true); // inactive main XHB (4th section)
                        PlaceExButton(nodesExR[11], Ref.Pos.XHB[3, 1], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[10], Ref.Pos.XHB[3, 2], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[9], Ref.Pos.XHB[3, 3], -rX + cfg.Split, -rY, select, true);
                    }

                    break;
                case 4:
                    CopyButtons(contentsXHB, 0, lId, 8, 4);
                    CopyButtons(contentsXHB, 4, rId, 8, 4);
                    CopyButtons(contentsXHB, 8, rId, 0, 8);

                    if (select != prevSelect || forceArrange)
                    {
                        SlotRangeVis(16, 31, true);
                        SlotRangeScale(8, 15, 1.1F);
                        if (mixBar)
                        {
                            SlotRangeScale(24, 27, 0.85F);
                        }

                        PlaceExButton(nodesExL[20], Ref.Pos.leftEX[0], 0, 0, select, true); //left EXHB
                        PlaceExButton(nodesExL[19], Ref.Pos.leftEX[1], 0, 0, select, true);
                        PlaceExButton(nodesExL[18], Ref.Pos.leftEX[2], 0, 0, select, true);
                        PlaceExButton(nodesExL[17], Ref.Pos.leftEX[3], 0, 0, select, true);
                        PlaceExButton(nodesExL[16], Ref.Pos.leftEX[4], 0, 0, select, true);
                        PlaceExButton(nodesExL[15], Ref.Pos.leftEX[5], 0, 0, select, true);
                        PlaceExButton(nodesExL[14], Ref.Pos.leftEX[6], 0, 0, select, true);
                        PlaceExButton(nodesExL[13], Ref.Pos.leftEX[7], 0, 0, select, true);

                        PlaceExButton(nodesExL[12], Ref.Pos.XHB[0, 0], -lX - cfg.Split, -lY, select, true); //main XHB (1st section)
                        PlaceExButton(nodesExL[11], Ref.Pos.XHB[0, 1], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[10], Ref.Pos.XHB[0, 2], -lX - cfg.Split, -lY, select, true);
                        PlaceExButton(nodesExL[9], Ref.Pos.XHB[0, 3], -lX - cfg.Split, -lY, select, true);

                        PlaceExButton(nodesExR[!mixBar ? 12 : 20], Ref.Pos.XHB[1, 0], -rX - cfg.Split, -rY, select, true); //main XHB (last 3 sections)
                        PlaceExButton(nodesExR[!mixBar ? 11 : 19], Ref.Pos.XHB[1, 1], -rX - cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[!mixBar ? 10 : 18], Ref.Pos.XHB[1, 2], -rX - cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[!mixBar ? 9 : 17], Ref.Pos.XHB[1, 3], -rX - cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[!mixBar ? 20 : 12], Ref.Pos.XHB[2, 0], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[!mixBar ? 19 : 11], Ref.Pos.XHB[2, 1], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[!mixBar ? 18 : 10], Ref.Pos.XHB[2, 2], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[!mixBar ? 17 : 9], Ref.Pos.XHB[2, 3], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[16], Ref.Pos.XHB[3, 0], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[15], Ref.Pos.XHB[3, 1], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[14], Ref.Pos.XHB[3, 2], -rX + cfg.Split, -rY, select, true);
                        PlaceExButton(nodesExR[13], Ref.Pos.XHB[3, 3], -rX + cfg.Split, -rY, select, true);
                    }
                    break;
            }
        }
        private static void PlaceExButton(AtkResNode* node, int msID, float xMod = 0, float yMod = 0, int select = 0, bool Tween = false) //move a borrowed button into position and set its scale to animate if needed
        {
            var to = Ref.scaleMap[select, Ref.metaSlots[msID].ScaleIndex];
            var pos = Ref.metaSlots[msID];
            pos.Xmod = xMod;
            pos.Ymod = yMod;
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
                    Status.tweensExist = true;
                    return;
                }
            }
            else
            {
                pos.Tween = null;
                pos.Scale = to;
            }

            NodeEdit.SetVarious(node,pos);

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
        private static void SlotRangeVis(int start, int end, bool show)
        {
            for (var i = start; i <= end; i++)
            {
                Ref.metaSlots[i].Visible = show;
            }
        }//set visibility for a range of slots at once
        private static void SlotRangeScale(int start, int end, float scale)
        {
            for (var i = start; i <= end; i++)
            {
                Ref.metaSlots[i].Scale = scale;
            }
        }//set scale for a range of slots at once
        private void SetExAlpha(AtkResNode** nodesL, AtkResNode** nodesR, byte alpha)
        {
            for (var i = 0; i < 12; i++)
            {
                nodesL[i]->Color.A = alpha;
                nodesR[i]->Color.A = alpha;
            }
        }//set alpha of our Ex buttons
        private void SetKeybindVis(AtkUnitBase* baseHotbar, bool show)
        {
            if (baseHotbar == null) { return; }

            var nodes = baseHotbar->UldManager.NodeList;
            for (var i = 9; i <= 20; i++)
            {
                var keyTextNode = nodes[i]->GetComponent()->UldManager.NodeList[1];
                NodeEdit.SetVis(keyTextNode, show);
            }

            return;
        }

        private void SetDragDropNodeVis(AtkUnitBase* baseHotbar, bool show)
        {
            if (baseHotbar == null) { return; }

            var nodes = baseHotbar->UldManager.NodeList;
            for (var i = 9; i <= 20; i++)
            {
                var dragDropNode = nodes[i]->GetComponent()->UldManager.NodeList[0];
                NodeEdit.SetVis(dragDropNode, show);
            }
            return;
        }

        public void SetSelectColor(bool revert = false) //apply highlight colour chosen in CrossUp settings
        {
            var selectColor = revert ? new(1F, 1F, 1F) : this.Configuration.selectColor;

            var baseXHB = Ref.UnitBases.Cross;
            var baseRR = Ref.UnitBases.RR;
            var baseLL = Ref.UnitBases.LL;
            if (baseXHB == null || baseRR == null || baseLL == null) { return; }

            NodeEdit.SetColor(baseXHB->UldManager.NodeList[4], selectColor);
            NodeEdit.SetColor(baseXHB->UldManager.NodeList[5], selectColor);
            NodeEdit.SetColor(baseXHB->UldManager.NodeList[6], selectColor);

            NodeEdit.SetColor(baseLL->UldManager.NodeList[3], selectColor);
            NodeEdit.SetColor(baseLL->UldManager.NodeList[4], selectColor);

            NodeEdit.SetColor(baseRR->UldManager.NodeList[3], selectColor);
            NodeEdit.SetColor(baseRR->UldManager.NodeList[4], selectColor);

            bool hide = !revert && this.Configuration.selectHide;   // we can't hide it by toggling visibility, so instead we do it by setting width to 0

            NodeEdit.SetSize(baseXHB->UldManager.NodeList[4], (ushort)(hide ? 0 : 304), 140);
            NodeEdit.SetSize(baseXHB->UldManager.NodeList[5], (ushort)(hide ? 0 : 166), 140);
            NodeEdit.SetSize(baseXHB->UldManager.NodeList[6], (ushort)(hide ? 0 : 166), 140);

            NodeEdit.SetSize(baseLL->UldManager.NodeList[3], (ushort)(hide ? 0 : 304), 140);
            NodeEdit.SetSize(baseLL->UldManager.NodeList[4], (ushort)(hide ? 0 : 166), 140);

            NodeEdit.SetSize(baseRR->UldManager.NodeList[3], (ushort)(hide ? 0 : 304), 140);
            NodeEdit.SetSize(baseRR->UldManager.NodeList[4], (ushort)(hide ? 0 : 166), 140);

        }
        public void ResetHud()  //cleanup: reset all the node properties we've messed with and restore actions to borrowed bars
        {
            var baseXHB = Ref.UnitBases.Cross;
            if (baseXHB == null) { return; }

            var nodesXHB = baseXHB->UldManager.NodeList;
            NodeEdit.ByLookup.AbsoluteSize(Ref.barNodes.Cross.VertLine);
            NodeEdit.ByLookup.AbsoluteSize(Ref.barNodes.Cross.miniSelectL);
            NodeEdit.ByLookup.AbsoluteSize(Ref.barNodes.Cross.miniSelectR);
            NodeEdit.ByLookup.AbsolutePos(Ref.barNodes.Cross.padlock);
            NodeEdit.ByLookup.AbsolutePos(Ref.barNodes.Cross.changeSet);

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
            var baseHotbar = Ref.UnitBases.ActionBar[barID];
            if (baseHotbar == null) { return; }

            var nodes = baseHotbar->UldManager.NodeList;

            var gridType = GetCharConfig((uint)(barID + 501));

            NodeEdit.SetVarious(nodes[0], new NodeEdit.PropertySet
            {
                Width = (ushort)Ref.barSizes[gridType].X,
                Height = (ushort)Ref.barSizes[gridType].Y,
                X = baseHotbar->X,
                Y = baseHotbar->Y,
                Scale = baseHotbar->Scale
            });

            NodeEdit.SetScale(nodes[24], 1F);

            for (var i = 0; i < 12; i++)
            {
                NodeEdit.SetVarious(nodes[20 - i], new NodeEdit.PropertySet
                {
                    X = Ref.barGrids[gridType, i].X,
                    Y = Ref.barGrids[gridType, i].Y,
                    Visible = true,
                    Scale = 1F
                });
            }

            if (wasHidden[barID] && (this.Configuration.borrowBarL < 1 || this.Configuration.borrowBarR < 1 || (barID != this.Configuration.borrowBarL && barID != this.Configuration.borrowBarR)) && charConfigs->GetIntValue((uint)(barID + 485)) == 1)
            {
                charConfigs->SetOption((uint)(barID + 485), 0);
            }

            SetKeybindVis(baseHotbar, true);
            return;
        }

        private static void TweenAllButtons()  //run any extant tweens to animate button scale
        {
            Status.tweensExist = false;
            for (var i = 0; i < Ref.metaSlots.Length; i++)
            {
                var metaSlot = Ref.metaSlots[i];
                var tween = metaSlot.Tween;
                var node = metaSlot.Node;
                if (tween != null && node != null)
                {
                    Status.tweensExist = true;
                    var timePassed = DateTime.Now - tween.Start;
                    decimal progress = decimal.Divide(timePassed.Milliseconds, tween.Duration.Milliseconds);

                    if (progress >= 1)
                    {
                        metaSlot.Tween = null;
                        metaSlot.Scale = tween.ToScale;
                    }
                    else
                    {
                        metaSlot.Scale = ((tween.ToScale - tween.FromScale) * (float)progress) + tween.FromScale;
                    }

                    NodeEdit.SetVarious(node, metaSlot);
                }
            }
            return;
        }

        // UTILITY
        public struct ButtonAction
        {
            public uint Id;
            public HotbarSlotType CommandType;
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
                    contents[i].Id = slotStruct->CommandType == HotbarSlotType.Action ? actionManager->GetAdjustedActionId(slotStruct->CommandId) : slotStruct->CommandId;
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
        private ButtonAction[] GetCrossbarContents()    //get whatever's on the cross hotbar
        {
            var barBaseXHB = (AddonActionBarBase*)Ref.UnitBases.Cross;
            var xBar = (AddonActionCross*)barBaseXHB;
            if (xBar->PetBar)
            {
                // FIX STILL NEEDED
                // I can identify if the pet hotbar is active, but I haven't worked out how to retrieve its contents.
                // Grabbing the same ol regular bar contents for now, even though it looks odd.
                // this does not affect gameplay, only visuals
                return GetBarContentsByID(barBaseXHB->HotbarID, 16);
            }
            else
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
                var barBaseXHB = (AddonActionBarBase*)Ref.UnitBases.Cross;
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
            //  485-494: toggle, hotbar 1-10 (0-9 internally) visible or not
            //  501-510: int from 0-5, represents selected grid layout for hotbars 1-10 (0-9 internally)
            //  515-523: toggle, share setting for cross hotbars 1 - 8
            //  535: main cross hotbar layout   0 = dpad / button / dpad / button
            //                                  1 = dpad / dpad / button / button
            //  560: mapping for RL expanded cross hotbar
            //  561: mapping for LR expanded cross hotbar
            //  586: toggle, cross hotbar active or not
            //  600: transparency of standard cross hotbar
            //  601: transparency of active cross hotbar
            //  602: transparency of inactive cross hotbar
            return charConfigs->GetIntValue(configID);
        }
        public void LogCharConfigs(uint start, uint end = 0)  // for debugging
        {
            if (end < start) { end = start; }
            for (uint i = start; i <= end; i++)
            {
                PluginLog.Log(i + " " + GetCharConfig(i));
            }
        }

    }
}
