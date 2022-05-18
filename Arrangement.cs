using System;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{

        // arrange all the hotbars and populate them with the correct contents
    private void ArrangeAndFill(int select, int prevSelect = 0, bool forceArrange = false,bool hudFixCheck = true) { // the centrepiece of it all
        if (GetCharConfig(ConfigID.CrossEnabled) == 0) //don't do anything if the cross hotbar isn't actually turned on
        {
            ResetHud();
            return;
        }

        var baseXHB = UnitBases.Cross;
        if (baseXHB == null) return;

        var rootNode = baseXHB->UldManager.NodeList[0];
        var scale = rootNode->ScaleX;
        var mixBar = GetCharConfig(ConfigID.MixBar) == 1;

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

            // arrange the EXHB if that feature is turned on and two borrowed bars are selected
        var arrangeEx = Config.SepExBar && Config.borrowBarL > 0 && Config.borrowBarR > 0;
        if (arrangeEx) ArrangeExBars(select, prevSelect, scale, anchorX, anchorY, forceArrange);

        var lX = arrangeEx ? Config.lX : 0;
        var lY = arrangeEx ? Config.lY : 0;
        var rX = arrangeEx ? Config.rX : 0;
        var rY = arrangeEx ? Config.rY : 0;

            // vertical bar looks odd if certain CrossUp features are turned on
        var hideDivider = Config.Split > 0 || Config.SepExBar;

        NodeEdit.ByLookup.AbsoluteSize(barNodes.Cross.VertLine, hideDivider ? 0 : null, hideDivider ? 0 : null);
        NodeEdit.ByLookup.RelativePos(barNodes.Cross.padlock, Config.PadlockOffset.X + Config.Split, Config.PadlockOffset.Y);
        NodeEdit.ByLookup.RelativePos(barNodes.Cross.changeSet, Config.ChangeSetOffset.X + Config.Split, Config.ChangeSetOffset.Y);
        NodeEdit.ByLookup.RelativePos(barNodes.Cross.setText, Config.SetTextOffset.X + Config.Split, Config.SetTextOffset.Y);

        if (!forceArrange && select == prevSelect) return;
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

        // arranged the borrowed hotbars representing the EXHB
    private void ArrangeExBars(int select, int prevSelect, float scale, int anchorX, int anchorY,bool forceArrange = false)
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

        var mixBar = GetCharConfig(ConfigID.MixBar) == 1;

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

        var inactiveTransp = GetCharConfig(ConfigID.Transparency.Inactive);
        var standardTransp = GetCharConfig(ConfigID.Transparency.Active);

        var inactiveAlpha = (byte)(-0.0205 * Math.Pow(inactiveTransp, 2) - 0.5 * inactiveTransp + 255);
        var standardAlpha = (byte)(-0.0205 * Math.Pow(standardTransp, 2) - 0.5 * standardTransp + 255);
        SetExAlpha(nodesExL, nodesExR, select == 0 ? standardAlpha : inactiveAlpha);

        switch (select)
        {
            case 0: // NONE
            case 5: // LEFT WXHB
            case 6: // RIGHT WXHB
            {
                CopyButtons(BarContents.LR, 0, lId, 0, 8);
                CopyButtons(BarContents.RL, 0, rId, 0, 8);

                if (forceArrange || select != prevSelect)
                {
                    for (var i = 0; i < 8; i++)
                    {
                        // 8 buttons each from borrowed bars become EXHB
                        PlaceExButton(nodesExL[20 - i], Pos.leftEX[i], 0, 0, select); //left EXHB
                        PlaceExButton(nodesExR[20 - i], Pos.rightEX[i], 0, 0, select); // right EXHB
                        if (i >= 4) continue;

                        NodeEdit.SetVis(nodesExL[12 - i], false); // hide unneeded borrowed buttons
                        NodeEdit.SetVis(nodesExR[12 - i], false);

                        metaSlots[Pos.XHB[0, i]].Visible = false;  // hide metaSlots for left bar
                        metaSlots[Pos.XHB[1, i]].Visible = false;
                        metaSlots[Pos.XHB[2, i]].Visible = false;  // hide metaSlots for right bar
                        metaSlots[Pos.XHB[3, i]].Visible = false;  
                    }
                }

                break;
            }
            case 1: // LEFT BAR
            {
                CopyButtons(BarContents.LR, 0, lId, 0, 8);
                CopyButtons(BarContents.RL, 0, rId, 0, 8);

                CopyButtons(BarContents.XHB, 8, lId, 8, 4);
                CopyButtons(BarContents.XHB, 12, rId, 8, 4);

                if (forceArrange || select != prevSelect)
                {
                    for (var i = 0; i < 8; i++)
                    {
                        // 8 buttons each from borrowed bars become EXHB
                        PlaceExButton(nodesExL[20 - i], Pos.leftEX[i], 0, 0, select); //left EXHB
                        PlaceExButton(nodesExR[20 - i], Pos.rightEX[i], 0, 0, select); // right EXHB
                        if (i >= 4) continue;

                        metaSlots[Pos.XHB[0, i]].Scale = 1.1F;  // reveal and scale up left bar metaSlots
                        metaSlots[Pos.XHB[0, i]].Visible = true;
                        metaSlots[Pos.XHB[1, i]].Scale = 1.1F;
                        metaSlots[Pos.XHB[1, i]].Visible = true;
                        metaSlots[Pos.XHB[2, i]].Visible = prevSelect == 3 && !mixBar;  // conditionally hide right bar metaSlots
                        metaSlots[Pos.XHB[3, i]].Visible = prevSelect == 3 && !mixBar;

                        // 4 buttons each from borrowed bars become right XHB (not necessarily shown, depends on previous selection)
                        PlaceExButton(nodesExL[12 - i], Pos.XHB[2, i], -lX + Config.Split, -lY, select); //right bar (left buttons)
                        PlaceExButton(nodesExR[12 - i], Pos.XHB[3, i], -rX + Config.Split, -rY, select); //right bar (right buttons)

                    }
                }

                break;
            }
            case 2: // RIGHT BAR
            {
                CopyButtons(BarContents.LR, 0, lId, 0, 8);
                CopyButtons(BarContents.RL, 0, rId, 0, 8);

                var xContents = BarContents.XHB;
                CopyButtons(xContents, 0, lId, 8, 4);
                CopyButtons(xContents, 4, rId, 8, 4);

                if (forceArrange || select != prevSelect)
                {
                    for (var i = 0; i < 8; i++)
                    {

                        // 8 buttons each from borrowed bars become EXHB
                        PlaceExButton(nodesExL[20 - i], Pos.leftEX[i], 0, 0, select); // left EXHB
                        PlaceExButton(nodesExR[20 - i], Pos.rightEX[i], 0, 0, select); // right EXHB

                        if (i >= 4) continue;

                        metaSlots[Pos.XHB[0, i]].Visible = prevSelect == 4 && !mixBar;
                        metaSlots[Pos.XHB[1, i]].Visible = prevSelect == 4 && !mixBar;
                        metaSlots[Pos.XHB[2, i]].Scale = 1.1F;
                        metaSlots[Pos.XHB[2, i]].Visible = true;
                        metaSlots[Pos.XHB[3, i]].Visible = true;
                        metaSlots[Pos.XHB[3, i]].Scale = 1.1F;

                        // 4 buttons each from borrowed bars become left XHB (not necessarily shown, depends on previous selection)
                        PlaceExButton(nodesExL[12-i], Pos.XHB[0, i], -lX - Config.Split, -lY, select); // left bar (left buttons)
                        PlaceExButton(nodesExR[12-i], Pos.XHB[1, i], -rX - Config.Split, -rY, select); //left bar (right buttons)

                        NodeEdit.SetVis(nodesExL[12-i], prevSelect == 4 && !mixBar);
                        NodeEdit.SetVis(nodesExR[12-i], prevSelect == 4 && !mixBar);
                    }
                }

                break;
            }
            case 3: // L->R BAR
                {
                var xContents = BarContents.XHB;
                CopyButtons(xContents, 0, lId, 0, 12);
                CopyButtons(xContents, 12, rId, 8, 4);

                if (forceArrange || select != prevSelect)
                {

                    for (var i = 0; i < 8; i++)
                    {
                        metaSlots[Pos.leftEX[i]].Scale = 1.1F;
                        PlaceExButton(nodesExR[20 - i], Pos.rightEX[i], 0, 0, select); // right EXHB
                        if (i >= 4) continue;

                        metaSlots[Pos.XHB[0,i]].Visible = true;
                        metaSlots[Pos.XHB[1,i]].Visible = true;
                        metaSlots[Pos.XHB[2,i]].Visible = true;
                        metaSlots[Pos.XHB[3,i]].Visible = true;

                        PlaceExButton(nodesExL[20-i],                  Pos.XHB[0, i], -lX - Config.Split, -lY, select); //inactive main XHB
                        PlaceExButton(nodesExL[(!mixBar ? 16 : 12)-i], Pos.XHB[1, i], -lX - Config.Split, -lY, select);
                        PlaceExButton(nodesExL[(!mixBar ? 12 : 16)-i], Pos.XHB[2, i], -lX + Config.Split, -lY, select);
                        PlaceExButton(nodesExR[12-i],                  Pos.XHB[3, i], -rX + Config.Split, -rY, select); 

                    }
                }

                break;
            }
            case 4: // R->L BAR
                {
                var xContents = BarContents.XHB;
                CopyButtons(xContents, 0, lId, 8, 4);
                CopyButtons(xContents, 4, rId, 8, 4);
                CopyButtons(xContents, 8, rId, 0, 8);

                if (forceArrange || select != prevSelect)
                {
                    for (var i = 0; i < 8; i++)
                    {
                        metaSlots[Pos.rightEX[i]].Scale = 1.1F;
                        PlaceExButton(nodesExL[20 - i], Pos.leftEX[i], 0, 0, select); //left EXHB
                        if (i >= 4) continue;

                        metaSlots[Pos.XHB[0, i]].Visible = true;
                        metaSlots[Pos.XHB[1, i]].Visible = true;
                        metaSlots[Pos.XHB[2, i]].Visible = true;
                        metaSlots[Pos.XHB[3, i]].Visible = true;
                        PlaceExButton(nodesExL[12-i],                  Pos.XHB[0, i], -lX - Config.Split, -lY, select); //main XHB (1st section)
                        PlaceExButton(nodesExR[(!mixBar ? 12 : 20)-i], Pos.XHB[1, i], -rX - Config.Split, -rY, select); //main XHB (2nd section)
                        PlaceExButton(nodesExR[(!mixBar ? 20 : 12)-i], Pos.XHB[2, i], -rX + Config.Split, -rY, select); //main XHB (3rd section)
                        PlaceExButton(nodesExR[16-i],                  Pos.XHB[3, i], -rX + Config.Split, -rY, select); //main XHB (4th  section)
                    }
                }

                break;
            }
        }
    }



        //move a borrowed button into position and set its scale to animate if needed
    private static void PlaceExButton(AtkResNode* node, int msID, float xMod = 0, float yMod = 0, int select = 0,bool tween = true) 
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
            var shared = GetCharConfig(ConfigID.Hotbar.Shared[i]) == 1;
            var jobID = Service.ClientState?.LocalPlayer?.ClassJob.Id;
            if (jobID != null) CopyButtons(GetSavedBar(shared ? 0 : (int)jobID, i), 0, i, 0, 12);
        }
    }

    private void ResetBarPos(int barID) //put a borrowed hotbar back the way we found it based on HUD layout settings
    {
        var baseHotbar = UnitBases.ActionBar[barID];
        if (baseHotbar == null) return;

        var nodes = baseHotbar->UldManager.NodeList;
        var gridType = GetCharConfig(ConfigID.Hotbar.GridType[barID]);

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

        if (Status.WasHidden[barID] && (Config.borrowBarL < 1 || Config.borrowBarR < 1 || (barID != Config.borrowBarL && barID != Config.borrowBarR)) && charConfigs->GetIntValue((uint)(barID + 485)) == 1)
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
    private static bool AdjustHudEditorNode() //fix for misaligned frame around XHB when using the HUD Layout Interface
    {
        var hudScreen = (AtkUnitBase*)Service.GameGui.GetAddonByName("_HudLayoutScreen", 1);
        var rootNode = UnitBases.Cross->UldManager.NodeList[0];
        if (hudScreen == null || rootNode == null) return false;

        var scale = rootNode->ScaleX;
        var hudNodes = hudScreen->UldManager.NodeList;

        for (var i = 0; i < hudScreen->UldManager.NodeListCount; i++)
        {
            if (!hudNodes[i]->IsVisible || Math.Abs(hudNodes[i]->Y - rootNode->Y) > 1 ||
                Math.Abs(hudNodes[i]->Width - rootNode->Width * scale) > 1 ||
                Math.Abs(hudNodes[i]->Height - rootNode->Height * scale) > 1 ||
                Math.Abs(hudNodes[i]->X - rootNode->X) < 1) continue;
            hudNodes[i]->X = rootNode->X;
            hudNodes[i]->Flags_2 |= 0xD;
            return true;
        }
        return false;
    }
}

