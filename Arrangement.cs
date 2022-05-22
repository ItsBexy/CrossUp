using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    public static AtkResNode* GetChild(AtkResNode* node, int i) => node->GetAsAtkComponentNode()->Component->UldManager.NodeList[i];
    public static AtkResNode* GetChild(AtkUnitBase* unitBase, int i) => unitBase->UldManager.NodeList[i];
    public static AtkResNode* GetChild(AtkResNode* node, params int[] path)
    {
        foreach (var i in path) node = GetChild(node, i);
        return node;
    }
    public static AtkResNode* GetChild(AtkUnitBase* unitBase, params int[] path)
    {
        AtkResNode* node = null;
        for (var i = 0; i < path.Length; i++) node = i == 0 ? GetChild(unitBase, path[i]) : GetChild(node, path[i]);
        return node;
    }

    public static AtkResNode* GetChild(NodeRef nodeRef, int i) => GetChild(nodeRef.Node,i);
    public static AtkResNode* GetChild(NodeRef nodeRef, params int[] path) => GetChild(nodeRef.Node, path);

    // arrange all the hotbars and populate them with the correct contents
    private void ArrangeAndFill(int select, int prevSelect = 0, bool forceArrange = false,bool hudFixCheck = true) {
           //don't do anything if the cross hotbar isn't actually turned on
        if (GetCharConfig(ConfigID.CrossEnabled) == 0) 
        {
            ResetHud();
            return;
        }

        var baseXHB = UnitBases.Cross;
        if (baseXHB == null || baseXHB->UldManager.NodeListSize == 0) return;

        var rootNode = GetChild(baseXHB,0);
        var scale = rootNode->ScaleX;
        var mixBar = GetCharConfig(ConfigID.MixBar) == 1;

        for (var i = 0; i < 4; i++)
        {
            MetaSlots[20 + i].ScaleIndex = mixBar ? 1 : 0;
            MetaSlots[24 + i].ScaleIndex = mixBar ? 0 : 1;
        }

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
        var rX = arrangeEx ? Config.OnlyOneEx ? Config.lX : Config.rX : 0;
        var rY = arrangeEx ? Config.OnlyOneEx ? Config.lY : Config.rY : 0;

            // vertical bar looks odd if certain CrossUp features are turned on, so hiding if necessary
        var hideDivider = Config.Split > 0 || Config.SepExBar;


        NodeEdit.SetSize(BarNodes.Cross.VertLine, hideDivider ? 0 : null, hideDivider ? 0 : null);
        NodeEdit.RelativePos(BarNodes.Cross.Padlock, Config.PadlockOffset.X + Config.Split,  Config.PadlockOffset.Y);
        NodeEdit.SetVis(GetChild(BarNodes.Cross.Padlock,1),!Config.HidePadlock);

        NodeEdit.RelativePos(BarNodes.Cross.ChangeSet, Config.ChangeSetOffset.X + Config.Split, Config.ChangeSetOffset.Y);
        NodeEdit.RelativePos(BarNodes.Cross.SetText, Config.SetTextOffset.X + Config.Split, Config.SetTextOffset.Y);
        NodeEdit.SetVis(BarNodes.Cross.SetText,!Config.HideSetText);

        if (!forceArrange && select == prevSelect) return;
        switch (select)
        {
            case 0: // NONE
            case 5: // LEFT WXHB
            case 6: // RIGHT WXHB
                NodeEdit.RelativePos(BarNodes.Cross.Component, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.LT, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.RT, Config.Split * 2, 0F);

                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left1, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left2, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right1, Config.Split * 2, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right2, Config.Split * 2, 0F);
                break;
            case 1: //LEFT BAR

                NodeEdit.RelativePos(BarNodes.Cross.Component, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.LT, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.RT, Config.Split * 2, 0F);

                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left1, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left2, 0F, prevSelect == 3 && arrangeEx && mixBar ? 9999F : 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right1, Config.Split * 2,prevSelect == 3 && arrangeEx && !mixBar ? 9999F : 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right2, Config.Split * 2,prevSelect == 3 && arrangeEx ? 9999F : 0F);

                NodeEdit.SetSize(BarNodes.Cross.MiniSelectL,(ushort)(Config.selectHide || (mixBar && Config.Split > 0) ? 0 : 166), 140);
                NodeEdit.SetSize(BarNodes.Cross.MiniSelectR,(ushort)(Config.selectHide || (mixBar && Config.Split > 0) ? 0 : 166), 140);

                break;
            case 2: // RIGHT BAR

                NodeEdit.RelativePos(BarNodes.Cross.Component, Config.Split * 2, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.LT, -Config.Split * 2, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.RT, 0F, 0F);

                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left1, -Config.Split * 2, prevSelect == 4 && arrangeEx ? 9999F : 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left2, -Config.Split * 2, prevSelect == 4 && arrangeEx && !mixBar ? 9999F : 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right1, 0F, prevSelect == 4 && arrangeEx && mixBar ? 9999F : 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right2, 0F, 0F);

                NodeEdit.SetSize(BarNodes.Cross.MiniSelectL, (ushort)(Config.selectHide || (mixBar && Config.Split > 0) ? 0 : 166), 140);
                NodeEdit.SetSize(BarNodes.Cross.MiniSelectR, (ushort)(Config.selectHide || (mixBar && Config.Split > 0) ? 0 : 166), 140);

                break;
            case 3: // L->R BAR

                NodeEdit.RelativePos(BarNodes.Cross.Component, lX + Config.Split, lY);
                NodeEdit.RelativePos(BarNodes.Cross.LT, -lX - Config.Split, -lY);
                NodeEdit.RelativePos(BarNodes.Cross.RT, -lX + Config.Split, -lY);

                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left1, 9999F, 9999F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left2, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right1, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right2, 9999F, 9999F);

                break;
            case 4: // R->L BAR
                NodeEdit.RelativePos(BarNodes.Cross.Component, rX + Config.Split, rY);
                NodeEdit.RelativePos(BarNodes.Cross.LT, -rX - Config.Split, -rY);
                NodeEdit.RelativePos(BarNodes.Cross.RT, -rX + Config.Split, -rY);

                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left1, 9999F, 9999F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Left2, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right1, 0F, 0F);
                NodeEdit.RelativePos(BarNodes.Cross.Sets.Right2, 9999F, 9999F);
                break;
        }
    }

        // arrange the borrowed hotbars representing the EXHB


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
        var rX = !Config.OnlyOneEx ? Config.rX : Config.lX;
        var rY = !Config.OnlyOneEx ? Config.rY : Config.lY;

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

        var inactiveAlpha = TransToAlpha(GetCharConfig(ConfigID.Transparency.Inactive));
        var standardAlpha = TransToAlpha(GetCharConfig(ConfigID.Transparency.Active));
        SetExAlpha(nodesExL, nodesExR, select == 0 ? standardAlpha : inactiveAlpha);

        for (var i = 0; i < 8; i++) MetaSlots[Pos.RightEx[i]].Visible = !Config.OnlyOneEx;

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
                        PlaceExButton(nodesExL[20 - i], Pos.LeftEx[i], 0, 0, select);  //left EXHB
                        PlaceExButton(nodesExR[20 - i], Pos.RightEx[i], 0, 0, select); // right EXHB
                        if (i >= 4) continue;

                        for (var j = 0; j < 4; j++) MetaSlots[Pos.XHB[j, i]].Visible = false; // hide metaSlots for XHB

                        NodeEdit.SetVis(nodesExL[12 - i], false); // hide unneeded borrowed buttons
                        NodeEdit.SetVis(nodesExR[12 - i], false);
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
                        PlaceExButton(nodesExL[20 - i], Pos.LeftEx[i], 0, 0, select);  //left EXHB
                        PlaceExButton(nodesExR[20 - i], Pos.RightEx[i], 0, 0, select); // right EXHB

                        if (i >= 4) continue;

                        // handle metaSlots for XHB depending on previous selection and display type
                        MetaSlots[Pos.XHB[0, i]].Visible = true;                        
                        MetaSlots[Pos.XHB[1, i]].Visible = prevSelect == 3 && mixBar;
                        MetaSlots[Pos.XHB[2, i]].Visible = prevSelect == 3 && !mixBar;
                        MetaSlots[Pos.XHB[3, i]].Visible = prevSelect == 3;

                        MetaSlots[Pos.XHB[0, i]].Scale = ScaleMap[1, 0];
                        MetaSlots[Pos.XHB[1, i]].Scale = ScaleMap[1, mixBar ? 1 : 0];
                        MetaSlots[Pos.XHB[2, i]].Scale = ScaleMap[1, mixBar ? 0 : 1];
                        MetaSlots[Pos.XHB[3, i]].Scale = ScaleMap[1, 1];
                            
                        PlaceExButton(nodesExL[12 - i], Pos.XHB[mixBar ? 1 : 2, i], -lX + (mixBar ? -1 : 1) * Config.Split, -lY, select); //right XHB (left buttons)
                        PlaceExButton(nodesExR[12 - i], Pos.XHB[3, i], -rX + Config.Split, -rY, select);                                  //right XHB (right buttons)
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
                        PlaceExButton(nodesExL[20 - i], Pos.LeftEx[i], 0, 0, select); // left EXHB
                        PlaceExButton(nodesExR[20 - i], Pos.RightEx[i], 0, 0, select); // right EXHB

                        if (i >= 4) continue;

                        // handle metaSlots for XHB depending on previous selection and display type
                        MetaSlots[Pos.XHB[0, i]].Visible = prevSelect == 4;
                        MetaSlots[Pos.XHB[1, i]].Visible = prevSelect == 4 && !mixBar;
                        MetaSlots[Pos.XHB[2, i]].Visible = prevSelect == 4 && mixBar;
                        MetaSlots[Pos.XHB[3, i]].Visible = true;

                        MetaSlots[Pos.XHB[0, i]].Scale = ScaleMap[2, 0];
                        MetaSlots[Pos.XHB[1, i]].Scale = ScaleMap[2, mixBar ? 1 : 0];
                        MetaSlots[Pos.XHB[2, i]].Scale = ScaleMap[2, mixBar ? 0 : 1];
                        MetaSlots[Pos.XHB[3, i]].Scale = ScaleMap[2, 1];

                        PlaceExButton(nodesExL[12-i], Pos.XHB[0, i], -lX - Config.Split, -lY, select);                              // left XHB (left buttons)
                        PlaceExButton(nodesExR[12-i], Pos.XHB[mixBar?2:1, i], -rX - (mixBar ? -1 : 1) * Config.Split, -rY, select); // left XHB (right buttons)
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
                        MetaSlots[Pos.LeftEx[i]].Scale = 1.1F;

                        PlaceExButton(nodesExR[20 - i], Pos.RightEx[i], 0, 0, select); // right EXHB

                        if (i >= 4) continue;

                        for (var j = 0; j < 4; j++) MetaSlots[Pos.XHB[j,i]].Visible = true;

                        PlaceExButton(nodesExL[20-i],                  Pos.XHB[0, i], -lX - Config.Split, -lY, select); // inactive XHB
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
                        MetaSlots[Pos.RightEx[i]].Scale = 1.1F;
                        if (Config.OnlyOneEx) MetaSlots[Pos.LeftEx[i]].Scale = 1.1F;

                        PlaceExButton(nodesExL[20 - i], Config.OnlyOneEx ? Pos.RightEx[i] : Pos.LeftEx[i], 0, 0, select); // left EXHB

                        if (i >= 4) continue;

                        for (var j = 0; j < 4; j++) MetaSlots[Pos.XHB[j, i]].Visible = true;

                        PlaceExButton(nodesExL[12-i],                  Pos.XHB[0, i], -lX - Config.Split, -lY, select); // inactive XHB
                        PlaceExButton(nodesExR[(!mixBar ? 12 : 20)-i], Pos.XHB[1, i], -rX - Config.Split, -rY, select);
                        PlaceExButton(nodesExR[(!mixBar ? 20 : 12)-i], Pos.XHB[2, i], -rX + Config.Split, -rY, select);
                        PlaceExButton(nodesExR[16-i],                  Pos.XHB[3, i], -rX + Config.Split, -rY, select); 
                    }
                }

                break;
            }
        }
    }

        //move a borrowed button into position and set its scale to animate if needed
    private static void PlaceExButton(AtkResNode* node, int msID, float xMod = 0, float yMod = 0, int select = 0,bool tween = true) 
    {
        var to = ScaleMap[select, MetaSlots[msID].ScaleIndex];
        var pos = MetaSlots[msID];
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

    private static byte TransToAlpha(int t) => (byte)(-0.0205 * Math.Pow(t, 2) - 0.5 * t + 255);
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

        //apply highlight colour chosen in CrossUp settings
    public void SetSelectColor(bool revert = false) 
    {
        var selectColor = revert ? new(1F, 1F, 1F) : Config.selectColor;

        NodeEdit.SetColor(BarNodes.Cross.SelectBG, selectColor);
        NodeEdit.SetColor(BarNodes.Cross.MiniSelectR, selectColor);
        NodeEdit.SetColor(BarNodes.Cross.MiniSelectL, selectColor);

        NodeEdit.SetColor(BarNodes.LL.SelectBG, selectColor);
        NodeEdit.SetColor(BarNodes.LL.MiniSelect, selectColor);

        NodeEdit.SetColor(BarNodes.RR.SelectBG, selectColor);
        NodeEdit.SetColor(BarNodes.RR.MiniSelect, selectColor);

        var hide = !revert && Config.selectHide; // we can't hide it by toggling visibility, so instead we do it by setting width to 0

        NodeEdit.SetSize(BarNodes.Cross.SelectBG, new() { X = hide ? 0 : 304, Y = 140 });
        NodeEdit.SetSize(BarNodes.Cross.MiniSelectR, new() { X = hide ? 0 : 166, Y = 140 });
        NodeEdit.SetSize(BarNodes.Cross.MiniSelectL, new() { X = hide ? 0 : 166, Y = 140 });

        NodeEdit.SetSize(BarNodes.LL.SelectBG, new() { X = hide ? 0 : 304, Y = 140 });
        NodeEdit.SetSize(BarNodes.LL.MiniSelect, new Vector2 { X = hide ? 0 : 166, Y = 140 });

        NodeEdit.SetSize(BarNodes.RR.SelectBG, new() { X = hide ? 0 : 304, Y = 140 });
        NodeEdit.SetSize(BarNodes.RR.MiniSelect, new() { X = hide ? 0 : 166, Y = 140 });
    }


    public void SetPulseColor(bool revert=false)
    {
        var glowA = revert ? new(1F, 1F, 1F) : Config.GlowA;
        var glowB = revert ? new(1F, 1F, 1F) : Config.GlowB;

        for (var i = 11; i >= 8; i--)
        {
            for (var b = 0; b <= 3; b++)
            {
                NodeEdit.SetColor(GetChild(UnitBases.Cross, i, b, 2, 10 ), glowA);
                NodeEdit.SetColor(GetChild(UnitBases.Cross, i, b, 2, 14 ), glowB);
            }
        }

        if (!Config.SepExBar && !revert) return;
        {
            var baseExL = UnitBases.ActionBar[Config.borrowBarL];
            var baseExR = UnitBases.ActionBar[Config.borrowBarR];

            for (var i = 9; i <= 20; i++)
            {
                NodeEdit.SetColor(GetChild(baseExL, i, 0, 2, 10 ), glowA);
                NodeEdit.SetColor(GetChild(baseExR, i, 0, 2, 10 ), glowA);

                NodeEdit.SetColor(GetChild(baseExL, i, 0, 2, 14 ), glowB);
                NodeEdit.SetColor(GetChild(baseExR, i, 0, 2, 14 ), glowB);
            }
        }
    }

         // reset all the node properties we've messed with and restore actions to borrowed bars
    public void ResetHud() 
    {
        var baseXHB = UnitBases.Cross;
        if (baseXHB == null) return;

        NodeEdit.SetSize(BarNodes.Cross.VertLine);
        NodeEdit.SetSize(BarNodes.Cross.MiniSelectL);
        NodeEdit.SetSize(BarNodes.Cross.MiniSelectR);
        NodeEdit.SetPos(BarNodes.Cross.Padlock);
        NodeEdit.SetPos(BarNodes.Cross.SetText);
        NodeEdit.SetPos(BarNodes.Cross.ChangeSet);

        for (var i = 1; i <= 9; i++)
        {
            ResetBarPos(i);
            var shared = GetCharConfig(ConfigID.Hotbar.Shared[i]) == 1;
            var jobID = Service.ClientState?.LocalPlayer?.ClassJob.Id;
            if (jobID != null) CopyButtons(GetSavedBar(shared ? 0 : (int)jobID, i), 0, i, 0, 12);
        }
    }

        //put a borrowed hotbar back the way we found it based on HUD layout settings
    private void ResetBarPos(int barID)
    {
        var baseHotbar = UnitBases.ActionBar[barID];
        if (baseHotbar == null) return;

        var nodes = baseHotbar->UldManager.NodeList;
        var gridType = GetCharConfig(ConfigID.Hotbar.GridType[barID]);

        NodeEdit.SetVarious(nodes[0], new()
        {
            Width = (ushort)ActionBarSizes[gridType].X,
            Height = (ushort)ActionBarSizes[gridType].Y,
            X = baseHotbar->X,
            Y = baseHotbar->Y,
            Scale = baseHotbar->Scale
        });

        NodeEdit.SetScale(nodes[24], 1F);

        for (var i = 0; i < 12; i++)
        {
            NodeEdit.SetVarious(nodes[20 - i], new()
            {
                X = ActionBarGrids[gridType, i].X,
                Y = ActionBarGrids[gridType, i].Y,
                Visible = true,
                Scale = 1F
            });
        }

        if (Status.WasHidden[barID] && (Config.borrowBarL < 1 || Config.borrowBarR < 1 || (barID != Config.borrowBarL && barID != Config.borrowBarR)) && GetCharConfig(ConfigID.Hotbar.Visible[barID]) == 1)
        {
            SetCharConfig(ConfigID.Hotbar.Visible[barID], 0);
        }

        SetKeybindVis(baseHotbar, true);
    }
        
        //run any extant tweens to animate button scale
    private static void TweenAllButtons() 
    {
        Status.TweensExist = false;
        foreach (var metaSlot in MetaSlots)
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
                metaSlot.Scale = (tween.ToScale - tween.FromScale) * (float)progress + tween.FromScale;
            }

            NodeEdit.SetVarious(node, metaSlot);
        }
    }
        //fix for misaligned frame around XHB when using the HUD Layout Interface
    private static bool AdjustHudEditorNode() 
    {
        var hudScreen = (AtkUnitBase*)Service.GameGui.GetAddonByName("_HudLayoutScreen", 1);
        var rootNode = GetChild(UnitBases.Cross,0);
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

