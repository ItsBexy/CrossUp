using System;
using System.Numerics;
using Dalamud.Logging;

namespace CrossUp;

public sealed partial class CrossUp
{
    /// <summary>Color Customization features</summary>
    public class Color
    {
        /// <summary>Apply selected highlight colour to all XHB and WXHB highlight ninegrids</summary>
        public static void SetSelectBG(bool reset = false)
        {
            var selectColor = reset ? new(1F, 1F, 1F) : Config.selectColor;
            var hide = !reset && Config.selectHide; // we can't hide these by toggling visibility or changing alpha, so instead we do it by setting width to 0
            var miniSize = new Vector2 { X = 166, Y = 140 };
            var regSize = new Vector2 { X = 304, Y = 140 };
            var hideSize = new Vector2 { X = 0, Y = 0 };

            if (Bars.Cross.Exist)
            {
                Bars.Cross.SelectBG.SetColor(selectColor).SetSize(hide ? hideSize : regSize);
                Bars.Cross.MiniSelectR.SetColor(selectColor).SetSize(hide ? hideSize : miniSize);
                Bars.Cross.MiniSelectL.SetColor(selectColor).SetSize(hide ? hideSize : miniSize);
            }

            if (Bars.WXLL.Exist)
            {
                Bars.WXLL.SelectBG.SetColor(selectColor).SetSize(hide ? hideSize : regSize);
                Bars.WXLL.MiniSelect.SetColor(selectColor).SetSize(hide ? hideSize : miniSize);
            }

            if (Bars.WXRR.Exist)
            {
                Bars.WXRR.SelectBG.SetColor(selectColor).SetSize(hide ? hideSize : regSize);
                Bars.WXRR.MiniSelect.SetColor(selectColor).SetSize(hide ? hideSize : miniSize);
            }

            if (Bars.ActionContents.Exist)
            {
                Bars.ActionContents.BG1.SetColor(selectColor);
                Bars.ActionContents.BG2.SetColor(selectColor);
                Bars.ActionContents.BG3.SetColor(selectColor);
                Bars.ActionContents.BG4.SetColor(selectColor);
            }
        }

        /// <summary>Set/Reset colors of pressed buttons</summary>
        public static void SetPulse(bool reset = false)
        {
            var glowA = reset ? new(1F, 1F, 1F) : Config.GlowA;
            var glowB = reset ? new(1F, 1F, 1F) : Config.GlowB;

            if (Bars.Cross.Exist)
            {
                for (var i = 0; i < 4; i++)
                {
                    Bars.Cross.Left.GroupL.ChildNode(i, 2, 10).SetColor(glowA);
                    Bars.Cross.Left.GroupR.ChildNode(i, 2, 10).SetColor(glowA);
                    Bars.Cross.Right.GroupL.ChildNode(i, 2, 10).SetColor(glowA);
                    Bars.Cross.Right.GroupR.ChildNode(i, 2, 10).SetColor(glowA);

                    Bars.Cross.Left.GroupL.ChildNode(i, 2, 14).SetColor(glowB);
                    Bars.Cross.Left.GroupR.ChildNode(i, 2, 14).SetColor(glowB);
                    Bars.Cross.Right.GroupL.ChildNode(i, 2, 14).SetColor(glowB);
                    Bars.Cross.Right.GroupR.ChildNode(i, 2, 14).SetColor(glowB);
                }
            }

            if (Bars.WXLL.Exist)
            {
                for (var i = 0; i < 4; i++)
                {
                    Bars.WXLL.GroupL.ChildNode(i, 2, 10).SetColor(glowA);
                    Bars.WXLL.GroupR.ChildNode(i, 2, 10).SetColor(glowA);

                    Bars.WXLL.GroupL.ChildNode(i, 2, 14).SetColor(glowB);
                    Bars.WXLL.GroupR.ChildNode(i, 2, 14).SetColor(glowB);
                }
            }

            if (Bars.WXRR.Exist)
            {
                for (var i = 0; i < 4; i++)
                {
                    Bars.WXRR.GroupL.ChildNode(i, 2, 10).SetColor(glowA);
                    Bars.WXRR.GroupR.ChildNode(i, 2, 10).SetColor(glowA);

                    Bars.WXRR.GroupL.ChildNode(i, 2, 14).SetColor(glowB);
                    Bars.WXRR.GroupR.ChildNode(i, 2, 14).SetColor(glowB);
                }
            }

            if (SeparateEx.Ready && Bars.LR.BorrowBar.Exist && Bars.RL.BorrowBar.Exist)
            {
                foreach (var button in Bars.LR.BorrowBar.Button)
                {
                    button.ChildNode(0, 2, 10).SetColor(glowA);
                    button.ChildNode(0, 2, 14).SetColor(glowB);
                }

                foreach (var button in Bars.RL.BorrowBar.Button)
                {
                    button.ChildNode(0, 2, 10).SetColor(glowA);
                    button.ChildNode(0, 2, 14).SetColor(glowB);
                }
            }
        }

        /// <summary>Reset all colours to #FFFFFF (called on dispose)</summary>
        public static void Reset()
        {
            SetSelectBG(true);
            SetPulse(true);
        }
    }
}