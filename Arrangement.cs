using System;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    private static void AdjustHudEditorNode() //fix for misaligned frame around XHB when using the HUD Layout Interface
    {
        var hudScreen = (AtkUnitBase*)Service.GameGui.GetAddonByName("_HudLayoutScreen", 1); //would like to find a more efficient bool to check
        if (hudScreen == null) { Status.HudEditNodeChecked = false; return; }
        if (Status.HudEditNodeChecked) return;

        var rootNode = UnitBases.Cross->UldManager.NodeList[0];
        var scale = rootNode->ScaleX;

        var hudNodes = hudScreen->UldManager.NodeList;
        for (var i = 0; i < hudScreen->UldManager.NodeListCount; i++)
        {
            if (hudNodes[i]->IsVisible && !(Math.Abs(hudNodes[i]->Y - rootNode->Y) > 1) &&
                !(Math.Abs(hudNodes[i]->Width - rootNode->Width * scale) > 1) &&
                !(Math.Abs(hudNodes[i]->Height - rootNode->Height * scale) > 1) &&
                !(Math.Abs(hudNodes[i]->X - rootNode->X) < 1))
            {
                hudNodes[i]->X = rootNode->X;
                hudNodes[i]->Flags_2 |= 0xD;
                Status.HudEditNodeChecked = true;
                break;
            }
        }
    }

    public class ScaleTween
    {
        public DateTime Start { get; init; }
        public TimeSpan Duration { get; init; }
        public float FromScale { get; init; }
        public float ToScale { get; init; }
    }

    private void ArrangeAndFill(int select, int prevSelect = 0, bool forceArrange = false,bool hudFixCheck = true) { // the centrepiece of it all
        if (GetCharConfig(586) == 0) //don't do anything if the cross hotbar isn't actually turned on
        {
            ResetHud();
            return;
        }

        var baseXHB = UnitBases.Cross;
        if (baseXHB == null) return;

        var rootNode = baseXHB->UldManager.NodeList[0];

        var scale = rootNode->ScaleX;
        var mixBar = GetCharConfig(535) == 1;

        // fix for misalignment after entering/exiting HUD Layout interface
        if (hudFixCheck && baseXHB->X - rootNode->X - Math.Round(Config.Split * scale) < 0) baseXHB->X += (short)(Config.Split * scale);

        NodeEdit.SetVarious(rootNode, new()
        {
            X = baseXHB->X - Config.Split * scale,
            Y = baseXHB->Y,
            Width = (ushort)(588 + Config.Split * 2),
            Height = 210
        });

        var anchorX = (int)(rootNode->X + 146 * scale);
        var anchorY = (int)(rootNode->Y + 70 * scale);

        var arrangeEx = Config.SepExBar && Config.borrowBarL > 0 && Config.borrowBarR > 0;
        if (arrangeEx) ArrangeExBars(select, prevSelect, scale, anchorX, anchorY, forceArrange);

        var lX = arrangeEx ? Config.lX : 0;
        var lY = arrangeEx ? Config.lY : 0;
        var rX = arrangeEx ? Config.rX : 0;
        var rY = arrangeEx ? Config.rY : 0;

        var hideDivider = Config.Split > 0 || Config.SepExBar;

        NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.VertLine, hideDivider ? 0 : null, hideDivider ? 0 : null);
        NodeEdit.ByLookup.RelativePos(barNodes.Cross.padlock, Config.PadlockOffset.X + Config.Split, Config.PadlockOffset.Y);
        NodeEdit.ByLookup.RelativePos(barNodes.Cross.changeSet, Config.ChangeSetOffset.X + Config.Split, Config.ChangeSetOffset.Y);
        NodeEdit.ByLookup.RelativePos(barNodes.Cross.setText, Config.SetTextOffset.X + Config.Split, Config.SetTextOffset.Y);

        if (select == prevSelect && !forceArrange) return;
        switch (select)
        {
            case 0: // NONE
            case 5: // LEFT WXHB
            case 6: // RIGHT WXHB
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Component, 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.LT, 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.RT, Config.Split * 2, 0F);

                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[0], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[1], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[2], Config.Split * 2, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[3], Config.Split * 2, 0F);
                break;
            case 1: //LEFT BAR

                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Component, 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.LT, 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.RT, Config.Split * 2, 0F);

                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[0], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[1], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[2], Config.Split * 2,prevSelect == 3 && arrangeEx && !mixBar ? 9999F : 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[3], Config.Split * 2,prevSelect == 3 && arrangeEx && !mixBar ? 9999F : 0F);

                NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.miniSelectL,(ushort)(Config.selectHide || (mixBar && Config.Split > 0) ? 0 : 166), 140);
                NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.miniSelectR,(ushort)(Config.selectHide || (mixBar && Config.Split > 0) ? 0 : 166), 140);

                break;
            case 2: // RIGHT BAR

                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Component, Config.Split * 2, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.LT, -Config.Split * 2, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.RT, 0F, 0F);

                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[0], -Config.Split * 2, prevSelect == 4 && arrangeEx && !mixBar ? 9999F : 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[1], -Config.Split * 2, prevSelect == 4 && arrangeEx && !mixBar ? 9999F : 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[2], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[3], 0F, 0F);

                NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.miniSelectL, (ushort)(Config.selectHide || (mixBar && Config.Split > 0) ? 0 : 166), 140);
                NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.miniSelectR, (ushort)(Config.selectHide || (mixBar && Config.Split > 0) ? 0 : 166), 140);

                break;
            case 3: // L->R BAR

                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Component, lX + Config.Split, lY);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.LT, -lX - Config.Split, -lY);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.RT, -lX + Config.Split, -lY);

                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[0], 9999F, 9999F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[1], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[2], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[3], 9999F, 9999F);

                break;
            case 4: // R->L BAR
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Component, rX + Config.Split, rY);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.LT, -rX - Config.Split, -rY);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.RT, -rX + Config.Split, -rY);

                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[0], 9999F, 9999F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[1], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[2], 0F, 0F);
                NodeEdit.ByLookup.RelativePos(barNodes.Cross.Sets[3], 9999F, 9999F);
                break;
        }
    }

    private void ArrangeExBars(int select, int prevSelect, float scale, int anchorX, int anchorY,
        bool forceArrange = false) // arrange our borrowed bars for EXHB if that feature is on
    {
        var lId = Config.borrowBarL;
        var rId = Config.borrowBarR;

        var baseExL = UnitBases.ActionBar[lId];
        var baseExR = UnitBases.ActionBar[rId];
        if (baseExL == null || baseExR == null) return;

        SetDragDropNodeVis(baseExL, true);
        SetDragDropNodeVis(baseExR, true);
        SetKeybindVis(baseExL, false);
        SetKeybindVis(baseExR, false);

        var nodesExL = baseExL->UldManager.NodeList;
        var nodesExR = baseExR->UldManager.NodeList;

        var mixBar = GetCharConfig(535) == 1;

        var lX = Config.lX;
        var lY = Config.lY;
        var rX = Config.rX;
        var rY = Config.rY;

        NodeEdit.SetVarious(nodesExL[0], new()
        {
            Scale = scale,
            X = anchorX + (lX + Config.Split) * scale,
            Y = anchorY + lY * scale,
            Visible = true,
            Width = 295,
            Height = 120
        });

        NodeEdit.SetVarious(nodesExR[0], new()
        {
            Scale = scale,
            X = anchorX + (rX + Config.Split) * scale,
            Y = anchorY + rY * scale,
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

        switch (select)
        {
            // a lot of repetition here, could be condensed a great deal, but seeing exactly how each case lays out every button is helpful r/n
            case 0 or 5 or 6:
            {
                CopyButtons(contentsExL, 0, lId, 0, 8);
                CopyButtons(contentsExR, 0, rId, 0, 8);

                if (select != prevSelect || forceArrange)
                {
                    SlotRangeVis(16, 31, false);
                    SetLastEightVis(nodesExL, nodesExR, false);

                    PlaceExButton(nodesExL[20], Pos.leftEX[0], 0, 0, select); //left EXHB
                    PlaceExButton(nodesExL[19], Pos.leftEX[1], 0, 0, select);
                    PlaceExButton(nodesExL[18], Pos.leftEX[2], 0, 0, select);
                    PlaceExButton(nodesExL[17], Pos.leftEX[3], 0, 0, select);
                    PlaceExButton(nodesExL[16], Pos.leftEX[4], 0, 0, select);
                    PlaceExButton(nodesExL[15], Pos.leftEX[5], 0, 0, select);
                    PlaceExButton(nodesExL[14], Pos.leftEX[6], 0, 0, select);
                    PlaceExButton(nodesExL[13], Pos.leftEX[7], 0, 0, select);

                    PlaceExButton(nodesExR[20], Pos.rightEX[0], 0, 0, select); // right EXHB
                    PlaceExButton(nodesExR[19], Pos.rightEX[1], 0, 0, select);
                    PlaceExButton(nodesExR[18], Pos.rightEX[2], 0, 0, select);
                    PlaceExButton(nodesExR[17], Pos.rightEX[3], 0, 0, select);
                    PlaceExButton(nodesExR[16], Pos.rightEX[4], 0, 0, select);
                    PlaceExButton(nodesExR[15], Pos.rightEX[5], 0, 0, select);
                    PlaceExButton(nodesExR[14], Pos.rightEX[6], 0, 0, select);
                    PlaceExButton(nodesExR[13], Pos.rightEX[7], 0, 0, select);
                }

                break;
            }
            case 1:
            {
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

                    PlaceExButton(nodesExL[20], Pos.leftEX[0], 0, 0, select); //left EXHB
                    PlaceExButton(nodesExL[19], Pos.leftEX[1], 0, 0, select);
                    PlaceExButton(nodesExL[18], Pos.leftEX[2], 0, 0, select);
                    PlaceExButton(nodesExL[17], Pos.leftEX[3], 0, 0, select);
                    PlaceExButton(nodesExL[16], Pos.leftEX[4], 0, 0, select);
                    PlaceExButton(nodesExL[15], Pos.leftEX[5], 0, 0, select);
                    PlaceExButton(nodesExL[14], Pos.leftEX[6], 0, 0, select);
                    PlaceExButton(nodesExL[13], Pos.leftEX[7], 0, 0, select);

                    PlaceExButton(nodesExR[20], Pos.rightEX[0], 0, 0, select); // right EXHB
                    PlaceExButton(nodesExR[19], Pos.rightEX[1], 0, 0, select);
                    PlaceExButton(nodesExR[18], Pos.rightEX[2], 0, 0, select);
                    PlaceExButton(nodesExR[17], Pos.rightEX[3], 0, 0, select);
                    PlaceExButton(nodesExR[16], Pos.rightEX[4], 0, 0, select);
                    PlaceExButton(nodesExR[15], Pos.rightEX[5], 0, 0, select);
                    PlaceExButton(nodesExR[14], Pos.rightEX[6], 0, 0, select);
                    PlaceExButton(nodesExR[13], Pos.rightEX[7], 0, 0, select);

                    PlaceExButton(nodesExL[12], Pos.XHB[2, 0], -lX + Config.Split, -lY, select); //right bar (left buttons)
                    PlaceExButton(nodesExL[11], Pos.XHB[2, 1], -lX + Config.Split, -lY, select);
                    PlaceExButton(nodesExL[10], Pos.XHB[2, 2], -lX + Config.Split, -lY, select);
                    PlaceExButton(nodesExL[9], Pos.XHB[2, 3], -lX + Config.Split, -lY, select);

                    PlaceExButton(nodesExR[12], Pos.XHB[3, 0], -rX + Config.Split, -rY, select); //right bar (right buttons)
                    PlaceExButton(nodesExR[11], Pos.XHB[3, 1], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[10], Pos.XHB[3, 2], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[9], Pos.XHB[3, 3], -rX + Config.Split, -rY, select);
                }

                break;
            }
            case 2:
            {
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

                    PlaceExButton(nodesExL[20], Pos.leftEX[0], 0, 0, select); //left EXHB
                    PlaceExButton(nodesExL[19], Pos.leftEX[1], 0, 0, select);
                    PlaceExButton(nodesExL[18], Pos.leftEX[2], 0, 0, select);
                    PlaceExButton(nodesExL[17], Pos.leftEX[3], 0, 0, select);
                    PlaceExButton(nodesExL[16], Pos.leftEX[4], 0, 0, select);
                    PlaceExButton(nodesExL[15], Pos.leftEX[5], 0, 0, select);
                    PlaceExButton(nodesExL[14], Pos.leftEX[6], 0, 0, select);
                    PlaceExButton(nodesExL[13], Pos.leftEX[7], 0, 0, select);

                    PlaceExButton(nodesExR[20], Pos.rightEX[0], 0, 0, select); // right EXHB
                    PlaceExButton(nodesExR[19], Pos.rightEX[1], 0, 0, select);
                    PlaceExButton(nodesExR[18], Pos.rightEX[2], 0, 0, select);
                    PlaceExButton(nodesExR[17], Pos.rightEX[3], 0, 0, select);
                    PlaceExButton(nodesExR[16], Pos.rightEX[4], 0, 0, select);
                    PlaceExButton(nodesExR[15], Pos.rightEX[5], 0, 0, select);
                    PlaceExButton(nodesExR[14], Pos.rightEX[6], 0, 0, select);
                    PlaceExButton(nodesExR[13], Pos.rightEX[7], 0, 0, select);

                    PlaceExButton(nodesExL[12], Pos.XHB[0, 0], -lX - Config.Split, -lY, select); // left bar (left buttons)
                    PlaceExButton(nodesExL[11], Pos.XHB[0, 1], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[10], Pos.XHB[0, 2], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[9], Pos.XHB[0, 3], -lX - Config.Split, -lY, select);

                    PlaceExButton(nodesExR[12], Pos.XHB[1, 0], -rX - Config.Split, -rY, select); //left bar (right buttons)
                    PlaceExButton(nodesExR[11], Pos.XHB[1, 1], -rX - Config.Split, -rY, select);
                    PlaceExButton(nodesExR[10], Pos.XHB[1, 2], -rX - Config.Split, -rY, select);
                    PlaceExButton(nodesExR[9], Pos.XHB[1, 3], -rX - Config.Split, -rY, select);
                }

                break;
            }
            case 3:
            {
                CopyButtons(contentsXHB, 0, lId, 0, 12);
                CopyButtons(contentsXHB, 12, rId, 8, 4);

                if (select != prevSelect || forceArrange)
                {
                    SlotRangeVis(16, 31, true);
                    SlotRangeScale(0, 7, 1.1F);
                    if (mixBar)
                    {
                        SlotRangeScale(20, 23, 0.85F);
                    }

                    PlaceExButton(nodesExL[20], Pos.XHB[0, 0], -lX - Config.Split, -lY, select); //inactive main XHB (first 3 sections)
                    PlaceExButton(nodesExL[19], Pos.XHB[0, 1], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[18], Pos.XHB[0, 2], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[17], Pos.XHB[0, 3], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[!mixBar ? 16 : 12], Pos.XHB[1, 0], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[!mixBar ? 15 : 11], Pos.XHB[1, 1], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[!mixBar ? 14 : 10], Pos.XHB[1, 2], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[!mixBar ? 13 : 9], Pos.XHB[1, 3], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[!mixBar ? 12 : 16], Pos.XHB[2, 0], -lX + Config.Split, -lY, select);
                    PlaceExButton(nodesExL[!mixBar ? 11 : 15], Pos.XHB[2, 1], -lX + Config.Split, -lY, select);
                    PlaceExButton(nodesExL[!mixBar ? 10 : 14], Pos.XHB[2, 2], -lX + Config.Split, -lY, select);
                    PlaceExButton(nodesExL[!mixBar ? 9 : 13], Pos.XHB[2, 3], -lX + Config.Split, -lY, select);

                    PlaceExButton(nodesExR[20], Pos.rightEX[0], 0, 0, select); // right EXHB
                    PlaceExButton(nodesExR[19], Pos.rightEX[1], 0, 0, select);
                    PlaceExButton(nodesExR[18], Pos.rightEX[2], 0, 0, select);
                    PlaceExButton(nodesExR[17], Pos.rightEX[3], 0, 0, select);
                    PlaceExButton(nodesExR[16], Pos.rightEX[4], 0, 0, select);
                    PlaceExButton(nodesExR[15], Pos.rightEX[5], 0, 0, select);
                    PlaceExButton(nodesExR[14], Pos.rightEX[6], 0, 0, select);
                    PlaceExButton(nodesExR[13], Pos.rightEX[7], 0, 0, select);

                    PlaceExButton(nodesExR[12], Pos.XHB[3, 0], -rX + Config.Split, -rY, select); // inactive main XHB (4th section)
                    PlaceExButton(nodesExR[11], Pos.XHB[3, 1], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[10], Pos.XHB[3, 2], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[9], Pos.XHB[3, 3], -rX + Config.Split, -rY, select);
                }

                break;
            }
            case 4:
            {
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

                    PlaceExButton(nodesExL[20], Pos.leftEX[0], 0, 0, select); //left EXHB
                    PlaceExButton(nodesExL[19], Pos.leftEX[1], 0, 0, select);
                    PlaceExButton(nodesExL[18], Pos.leftEX[2], 0, 0, select);
                    PlaceExButton(nodesExL[17], Pos.leftEX[3], 0, 0, select);
                    PlaceExButton(nodesExL[16], Pos.leftEX[4], 0, 0, select);
                    PlaceExButton(nodesExL[15], Pos.leftEX[5], 0, 0, select);
                    PlaceExButton(nodesExL[14], Pos.leftEX[6], 0, 0, select);
                    PlaceExButton(nodesExL[13], Pos.leftEX[7], 0, 0, select);

                    PlaceExButton(nodesExL[12], Pos.XHB[0, 0], -lX - Config.Split, -lY, select); //main XHB (1st section)
                    PlaceExButton(nodesExL[11], Pos.XHB[0, 1], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[10], Pos.XHB[0, 2], -lX - Config.Split, -lY, select);
                    PlaceExButton(nodesExL[9], Pos.XHB[0, 3], -lX - Config.Split, -lY, select);

                    PlaceExButton(nodesExR[!mixBar ? 12 : 20], Pos.XHB[1, 0], -rX - Config.Split, -rY, select); //main XHB (last 3 sections)
                    PlaceExButton(nodesExR[!mixBar ? 11 : 19], Pos.XHB[1, 1], -rX - Config.Split, -rY, select);
                    PlaceExButton(nodesExR[!mixBar ? 10 : 18], Pos.XHB[1, 2], -rX - Config.Split, -rY, select);
                    PlaceExButton(nodesExR[!mixBar ? 9 : 17], Pos.XHB[1, 3], -rX - Config.Split, -rY, select);
                    PlaceExButton(nodesExR[!mixBar ? 20 : 12], Pos.XHB[2, 0], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[!mixBar ? 19 : 11], Pos.XHB[2, 1], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[!mixBar ? 18 : 10], Pos.XHB[2, 2], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[!mixBar ? 17 : 9], Pos.XHB[2, 3], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[16], Pos.XHB[3, 0], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[15], Pos.XHB[3, 1], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[14], Pos.XHB[3, 2], -rX + Config.Split, -rY, select);
                    PlaceExButton(nodesExR[13], Pos.XHB[3, 3], -rX + Config.Split, -rY, select);
                }

                break;
            }
        }
    }

    private static void PlaceExButton(AtkResNode* node, int msID, float xMod = 0, float yMod = 0, int select = 0,
        bool tween = true) //move a borrowed button into position and set its scale to animate if needed
    {
        var to = scaleMap[select, metaSlots[msID].ScaleIndex];
        var pos = metaSlots[msID];
        pos.Xmod = xMod;
        pos.Ymod = yMod;
        pos.Node = node;

        if (tween && Math.Abs(to - pos.Scale) > 0.01f) //only make a new tween if the button isn't already at the target scale, otherwise just set
        {
            if (pos.Tween == null ||
                Math.Abs(pos.Tween.ToScale - to) > 0.01f) //only make a new tween if the button doesn't already have one with the same target scale
            {
                pos.Tween = new()
                {
                    FromScale = pos.Scale,
                    ToScale = to,
                    Start = DateTime.Now,
                    Duration = new(0, 0, 0, 0, 40)
                };
                Status.TweensExist = true;
                return;
            }
        }
        else
        {
            pos.Tween = null;
            pos.Scale = to;
        }

        NodeEdit.SetVarious(node, pos);
    }

    private static void
        SetLastEightVis(AtkResNode** nodesL, AtkResNode** nodesR, bool show) //sometimes we need to display all 24 borrowed buttons, and sometimes we only need 16
    {
        for (var i = 9; i <= 12; i++)
        {
            NodeEdit.SetVis(nodesL[i], show);
            NodeEdit.SetVis(nodesR[i], show);
        }
    }

    private static void SlotRangeVis(int start, int end, bool show)
    {
        for (var i = start; i <= end; i++) metaSlots[i].Visible = show;
    }

    private static void SlotRangeScale(int start, int end, float scale)
    {
        for (var i = start; i <= end; i++) metaSlots[i].Scale = scale;
    }

    private static void SetExAlpha(AtkResNode** nodesL, AtkResNode** nodesR, byte alpha)
    {
        for (var i = 0; i < 12; i++)
        {
            nodesL[i]->Color.A = alpha;
            nodesR[i]->Color.A = alpha;
        }
    }

    private static void SetKeybindVis(AtkUnitBase* baseHotbar, bool show)
    {
        var nodes = baseHotbar->UldManager.NodeList;
        for (var i = 9; i <= 20; i++)
        {
            var keyTextNode = nodes[i]->GetComponent()->UldManager.NodeList[1];
            NodeEdit.SetVis(keyTextNode, show);
        }
    }

    private static void SetDragDropNodeVis(AtkUnitBase* baseHotbar, bool show)
    {
        if (baseHotbar == null) return;

        var nodes = baseHotbar->UldManager.NodeList;
        for (var i = 9; i <= 20; i++)
        {
            var dragDropNode = nodes[i]->GetComponent()->UldManager.NodeList[0];
            NodeEdit.SetVis(dragDropNode, show);
        }
    }

    public void SetSelectColor(bool revert = false) //apply highlight colour chosen in CrossUp settings
    {
        var selectColor = revert ? new(1F, 1F, 1F) : Config.selectColor;

        var baseXHB = UnitBases.Cross;
        var baseRR = UnitBases.RR;
        var baseLL = UnitBases.LL;
        if (baseXHB == null || baseRR == null || baseLL == null) return;

        NodeEdit.SetColor(baseXHB->UldManager.NodeList[4], selectColor);
        NodeEdit.SetColor(baseXHB->UldManager.NodeList[5], selectColor);
        NodeEdit.SetColor(baseXHB->UldManager.NodeList[6], selectColor);

        NodeEdit.SetColor(baseLL->UldManager.NodeList[3], selectColor);
        NodeEdit.SetColor(baseLL->UldManager.NodeList[4], selectColor);

        NodeEdit.SetColor(baseRR->UldManager.NodeList[3], selectColor);
        NodeEdit.SetColor(baseRR->UldManager.NodeList[4], selectColor);

        var hide = !revert && Config.selectHide; // we can't hide it by toggling visibility, so instead we do it by setting width to 0

        NodeEdit.SetSize(baseXHB->UldManager.NodeList[4], (ushort)(hide ? 0 : 304), 140);
        NodeEdit.SetSize(baseXHB->UldManager.NodeList[5], (ushort)(hide ? 0 : 166), 140);
        NodeEdit.SetSize(baseXHB->UldManager.NodeList[6], (ushort)(hide ? 0 : 166), 140);

        NodeEdit.SetSize(baseLL->UldManager.NodeList[3], (ushort)(hide ? 0 : 304), 140);
        NodeEdit.SetSize(baseLL->UldManager.NodeList[4], (ushort)(hide ? 0 : 166), 140);

        NodeEdit.SetSize(baseRR->UldManager.NodeList[3], (ushort)(hide ? 0 : 304), 140);
        NodeEdit.SetSize(baseRR->UldManager.NodeList[4], (ushort)(hide ? 0 : 166), 140);
    }

    public void
        ResetHud() //cleanup: reset all the node properties we've messed with and restore actions to borrowed bars
    {
        var baseXHB = UnitBases.Cross;
        if (baseXHB == null) return;

        NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.VertLine);
        NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.miniSelectL);
        NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.miniSelectR);
        NodeEdit.ByLookup.AbsolutePos(barNodes.Cross.padlock);
        NodeEdit.ByLookup.AbsolutePos(barNodes.Cross.setText);
        NodeEdit.ByLookup.AbsolutePos(barNodes.Cross.changeSet);

        for (var i = 1; i <= 9; i++)
        {
            ResetBarPos(i);
            bool shared = GetCharConfig((uint)(i + 515)) == 1;
            var jobID = Service.ClientState?.LocalPlayer?.ClassJob.Id;
            if (jobID != null) CopyButtons(GetSavedBar(shared ? 0 : (int)jobID, i), 0, i, 0, 12);
        }
    }

    private void ResetBarPos(int barID) //put a borrowed hotbar back the way we found it based on HUD layout settings
    {
        var baseHotbar = UnitBases.ActionBar[barID];
        if (baseHotbar == null) return;

        var nodes = baseHotbar->UldManager.NodeList;
        var gridType = GetCharConfig((uint)(barID + 501));

        NodeEdit.SetVarious(nodes[0], new()
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
            NodeEdit.SetVarious(nodes[20 - i], new()
            {
                X = barGrids[gridType, i].X,
                Y = barGrids[gridType, i].Y,
                Visible = true,
                Scale = 1F
            });
        }

        if (WasHidden[barID] && (Config.borrowBarL < 1 || Config.borrowBarR < 1 || (barID != Config.borrowBarL && barID != Config.borrowBarR)) && charConfigs->GetIntValue((uint)(barID + 485)) == 1)
        {
            charConfigs->SetOption((uint)(barID + 485), 0);
        }

        SetKeybindVis(baseHotbar, true);
    }

    private static void TweenAllButtons() //run any extant tweens to animate button scale
    {
        Status.TweensExist = false;
        foreach (var metaSlot in metaSlots)
        {
            var tween = metaSlot.Tween;
            var node = metaSlot.Node;
            if (tween == null || node == null) continue;
            Status.TweensExist = true;
            var timePassed = DateTime.Now - tween.Start;
            var progress = decimal.Divide(timePassed.Milliseconds, tween.Duration.Milliseconds);

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
}