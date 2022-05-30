using System.Numerics;

namespace CrossUp;

public sealed partial class CrossUp
{
    // apply selected highlight colour to all XHB and WXHB highlight ninegrids
    public void SetSelectColor(bool reset = false)
    {
        if (!Bars.Cross.Exist) return;

        var selectColor = reset ? new(1F, 1F, 1F) : Config.selectColor;
        var hide = !reset && Config.selectHide; // we can't hide it by toggling visibility, so instead we do it by setting width to 0
        var miniSize = new Vector2 { X = 166, Y = 140 };
        var regSize = new Vector2 { X = 304, Y = 140 };
        var hideSize = new Vector2 { X = 0, Y = 0 };

        Bars.Cross.SelectBG.SetColor(selectColor).SetSize(hide ? hideSize : regSize);
        Bars.WXRR.SelectBG.SetColor(selectColor).SetSize(hide ? hideSize : regSize);
        Bars.WXLL.SelectBG.SetColor(selectColor).SetSize(hide ? hideSize : regSize);

        Bars.Cross.MiniSelectR.SetColor(selectColor).SetSize(hide ? hideSize : miniSize);
        Bars.Cross.MiniSelectL.SetColor(selectColor).SetSize(hide ? hideSize : miniSize);
        Bars.WXLL.MiniSelect.SetColor(selectColor).SetSize(hide ? hideSize : miniSize);
        Bars.WXRR.MiniSelect.SetColor(selectColor).SetSize(hide ? hideSize : miniSize);
    }

    // set colors of pressed buttons
    public void SetPulseColor(bool reset = false)
    {
        if (!Bars.Cross.Exist) return;

        var glowA = reset ? new(1F, 1F, 1F) : Config.GlowA;
        var glowB = reset ? new(1F, 1F, 1F) : Config.GlowB;

        for (var i = 0; i <= 3; i++)
        {
            Bars.Cross.LeftL.ChildNode(i, 2, 10).SetColor(glowA);
            Bars.Cross.LeftR.ChildNode(i, 2, 10).SetColor(glowA);
            Bars.Cross.RightL.ChildNode(i, 2, 10).SetColor(glowA);
            Bars.Cross.RightR.ChildNode(i, 2, 10).SetColor(glowA);
            
            Bars.Cross.LeftL.ChildNode(i, 2, 14).SetColor(glowB);
            Bars.Cross.LeftR.ChildNode(i, 2, 14).SetColor(glowB);
            Bars.Cross.RightL.ChildNode(i, 2, 14).SetColor(glowB);
            Bars.Cross.RightR.ChildNode(i, 2, 14).SetColor(glowB);
        }

        if ((!Config.SepExBar && !reset) || !Bars.LR.BorrowBar.Exist || !Bars.RL.BorrowBar.Exist) return;
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

    // reset all colours to #FFFFFF (called on dispose)
    private void ResetColors()
    {
        SetSelectColor(true);
        SetPulseColor(true);
    }
}