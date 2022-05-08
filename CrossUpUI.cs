using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Logging;
using Dalamud.Interface.Colors;

namespace CrossUp
{
    class CrossUpUI : IDisposable
    {
        private Configuration configuration;
        private CrossUp crossUp;

        //private bool visible = false;
       /* public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }*/

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

     
        public CrossUpUI(Configuration configuration,CrossUp crossup)
        {
            this.configuration = configuration;
            this.crossUp = crossup;


        }

        public void Dispose() { 
        
        }
        public void Draw()
        {
            DrawSettingsWindow();
        }

     
        public void DrawSettingsWindow()
        {
            if (!SettingsVisible) return;

            ImGui.SetNextWindowSize(new Vector2(400, 630), ImGuiCond.Always);
            if (ImGui.Begin("CrossUp Settings", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {

                var sepExBar = this.configuration.SepExBar;
                var lX = this.configuration.lX;
                var lY = this.configuration.lY;
                var rX = this.configuration.rX;
                var rY = this.configuration.rY;
                var borrowBarL = this.configuration.borrowBarL;
                var borrowBarR = this.configuration.borrowBarR;
                var selectColor = this.configuration.selectColor;
                var selectHide = this.configuration.selectHide;
                var split = this.configuration.Split;
                var padlockX = (int)this.configuration.PadlockOffset.X;
                var padlockY = (int)this.configuration.PadlockOffset.Y;
                var setTextX = (int)this.configuration.SetTextOffset.X;
                var setTextY = (int)this.configuration.SetTextOffset.Y;
                var changeSetX = (int)this.configuration.ChangeSetOffset.X;
                var changeSetY = (int)this.configuration.ChangeSetOffset.Y;

                bool[] borrowBars =
                {
                    false,
                    this.configuration.borrowBarL==1 || this.configuration.borrowBarR==1,
                    this.configuration.borrowBarL==2 || this.configuration.borrowBarR==2,
                    this.configuration.borrowBarL==3 || this.configuration.borrowBarR==3,
                    this.configuration.borrowBarL==4 || this.configuration.borrowBarR==4,
                    this.configuration.borrowBarL==5 || this.configuration.borrowBarR==5,
                    this.configuration.borrowBarL==6 || this.configuration.borrowBarR==6,
                    this.configuration.borrowBarL==7 || this.configuration.borrowBarR==7,
                    this.configuration.borrowBarL==8 || this.configuration.borrowBarR==8,
                    this.configuration.borrowBarL==9 || this.configuration.borrowBarR==9,
                };

                var borrowCount = 0;
                for (var i=1;i<10;i++)
                {
                    if (borrowBars[i]) borrowCount++;
                }



                ImGui.Spacing();
                ImGui.Text("Customize Bar Highlight Color");

                ImGui.SetNextItemWidth(200);
                if (ImGui.ColorEdit3("Bar Highlight Color", ref selectColor))
                {
                    this.configuration.selectColor = selectColor;
                    this.configuration.Save();
                    this.crossUp.SetSelectColor();
                }

                ImGui.SameLine();
                if (ImGui.Checkbox("Hide##HideHighlight", ref selectHide))
                {
                    this.configuration.selectHide = selectHide;
                    this.configuration.Save();
                    this.crossUp.SetSelectColor();
                }


                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Text("Separate Left/Right Cross Hotbars");

                ImGui.SetNextItemWidth(100);
                if (ImGui.InputInt("Distance", ref split))
                {
                    split = Math.Max(split, 0);
                    this.configuration.Split = split;
                    this.configuration.Save();
                    this.crossUp.ResetHud();
                    this.crossUp.UpdateBarState(true, false);
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Text("Reposition Elements");
                ImGui.BeginTable("Reposition", 3, 8192);



                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Padlock Icon");

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100);
                if (ImGui.InputInt("X##PadX", ref padlockX))
                {
                    this.configuration.PadlockOffset = new(padlockX,this.configuration.PadlockOffset.Y);
                    this.configuration.Save();
                    this.crossUp.UpdateBarState(true, true);
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100);
                if (ImGui.InputInt("Y##PadY", ref padlockY))
                {
                    this.configuration.PadlockOffset = new(this.configuration.PadlockOffset.X,padlockY);
                    this.configuration.Save();
                    this.crossUp.UpdateBarState(true, true);
                }


                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("SET # Text");

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100);
                if (ImGui.InputInt("X##SetX", ref setTextX))
                {
                    this.configuration.SetTextOffset = new(setTextX, this.configuration.SetTextOffset.Y);
                    this.configuration.Save();
                    this.crossUp.UpdateBarState(true, true);
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100);
                if (ImGui.InputInt("Y##SetY", ref setTextY))
                {
                    this.configuration.SetTextOffset = new(this.configuration.SetTextOffset.X, setTextY);
                    this.configuration.Save();
                    this.crossUp.UpdateBarState(true, true);
                }



                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("CHANGE SET Display");

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100);
                if (ImGui.InputInt("X##ChangeSetX", ref changeSetX))
                {
                    this.configuration.ChangeSetOffset = new(changeSetX, this.configuration.ChangeSetOffset.Y);
                    this.configuration.Save();
                    this.crossUp.UpdateBarState(true, true);
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100);
                if (ImGui.InputInt("Y##ChangeSetY", ref changeSetY))
                {
                    this.configuration.ChangeSetOffset = new(this.configuration.ChangeSetOffset.X, changeSetY);
                    this.configuration.Save();
                    this.crossUp.UpdateBarState(true, true);
                }



                ImGui.EndTable();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

            
                if (ImGui.Checkbox("Display Expanded Hold Controls Separately", ref sepExBar))
                {
                    this.configuration.SepExBar = sepExBar;
                    if (sepExBar && this.configuration.borrowBarR > 0 && this.configuration.borrowBarR > 0)
                    {
                        this.crossUp.EnableEx();
                    }
                    else
                    {
                        this.crossUp.DisableEx();
                    }
                    this.configuration.Save();
                }
                ImGui.Spacing();

                if (this.configuration.SepExBar)
                {
                    ImGui.BeginTable("BarBorrowDesc", 2, 24576);

                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    ImGui.TextColored(Dalamud.Interface.Colors.ImGuiColors.DalamudGrey,"NOTE: This feature functions by borrowing\nthe buttons from two of your standard\nmouse/kb hotbars. The hotbars you choose\nwill not be overwritten, but they will be\nunavailable while the feature is active.\n\n\n");

                    ImGui.Indent(70);
                    ImGui.TextWrapped("POSITION BARS");

                    ImGui.Indent(-70);

                    ImGui.BeginTable("Coords", 3, 8192);


                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    ImGui.Text("Left EXHB Position");

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("X##LX", ref lX))
                    {
                        this.configuration.lX = lX;
                        this.configuration.Save();
                        this.crossUp.UpdateBarState(true, true);
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Y##LY", ref lY))
                    {
                        this.configuration.lY = lY;
                        this.configuration.Save();
                        this.crossUp.UpdateBarState(true, true);
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();

                    ImGui.Text("Right EXHB Position");

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("X##RX", ref rX))
                    {
                        this.configuration.rX = rX;
                        this.configuration.Save();
                        this.crossUp.UpdateBarState(true, true);
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Y##RY", ref rY))
                    {
                        this.configuration.rY = rY;
                        this.configuration.Save();
                        this.crossUp.UpdateBarState(true, true);
                    }

                    ImGui.EndTable();

                    ImGui.TableNextColumn();

                    ImGui.Text("SELECT 2 BARS:");
                    ImGui.BeginTable("BarBorrow", 2, 8192);


                    for (var i = 1; i < 10; i++)
                    {
                       
                        ImGui.TableNextColumn();

                        if (borrowBars[i] || borrowCount < 2)
                        {
                            if (ImGui.Checkbox("##using" + (i + 1), ref borrowBars[i]))
                            {
                                if (borrowBars[i])
                                {
                                    if (this.configuration.borrowBarL <= 0)
                                    {
                                        this.configuration.borrowBarL = i;
                                    }
                                    else if (this.configuration.borrowBarR <= 0)
                                    {
                                        this.configuration.borrowBarR = i;
                                    }

                                    this.crossUp.ResetHud();
                                    this.configuration.Save();
                                    this.crossUp.ExBarActivate(i);
                                }
                                else
                                {
                                    if (this.configuration.borrowBarL == i)
                                    {
                                        this.configuration.borrowBarL = -1;
                                    }
                                    else if (this.configuration.borrowBarR == i)
                                    {
                                        this.configuration.borrowBarR = -1;
                                    }
                                    this.crossUp.ResetHud();
                                    this.crossUp.UpdateBarState();
                                    this.configuration.Save();
                                }
                            }
                        } else
                        {
                            bool disabled = false;
                            ImGui.Checkbox("##disabled",ref disabled);
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text("Hotbar " + (i + 1));
                       

                    }


                    ImGui.EndTable();
                    ImGui.EndTable();

                }



            }





            ImGui.End();
        }



    }
}
