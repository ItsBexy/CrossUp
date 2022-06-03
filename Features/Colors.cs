using System.Numerics;

namespace CrossUp;

public sealed partial class CrossUp
{
    /// <summary>Apply selected highlight colour to all XHB and WXHB highlight ninegrids</summary>
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

    /// <summary>Set/Reset colors of pressed buttons</summary>
    public void SetPulseColor(bool reset = false)
    {
        if (!Bars.Cross.Exist) return;

        var glowA = reset ? new(1F, 1F, 1F) : Config.GlowA;
        var glowB = reset ? new(1F, 1F, 1F) : Config.GlowB;

        for (var i = 0; i <= 3; i++)
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

    /// <summary>Reset all colours to #FFFFFF (called on dispose)</summary>
    private void ResetColors()
    {
        SetSelectColor(true);
        SetPulseColor(true);
    }
}