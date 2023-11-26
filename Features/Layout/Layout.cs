using System.Threading.Tasks;
using CrossUp.Game;
using CrossUp.Game.Hotbar;
using static CrossUp.CrossUp;

namespace CrossUp.Features.Layout;

/// <summary>Methods regarding overall layout manipulation</summary>
internal class Layout
{
    /// <summary>Checks/updates the Cross Hotbar selection and calls the main arrangement functions</summary>
    internal static unsafe void Update(bool forceArrange = false, bool resetAll = false)
    {
        if (!Bars.Cross.Exists) return;

        Bars.Cross.Selection.Check();
        var select = Bars.Cross.Selection.Current;
        var previous = Bars.Cross.Selection.Previous;

        if (Bars.Cross.Enabled)
        {
            var scale = Bars.Cross.Root.Node->ScaleX;
            var splitDist = Profile.SplitOn && !resetAll ? Profile.SplitDist : 0;

            var mixBar = (bool)GameConfig.Cross.MixBar;
            var arrangeEx = !resetAll && SeparateEx.Ready && Bars.RL.Exists && Bars.LR.Exists;

            var lrX = arrangeEx ? Profile.LRpos.X : 0;
            var lrY = arrangeEx ? Profile.LRpos.Y : 0;
            var rlX = arrangeEx ? Profile.OnlyOneEx ? Profile.LRpos.X : Profile.RLpos.X : 0;
            var rlY = arrangeEx ? Profile.OnlyOneEx ? Profile.LRpos.Y : Profile.RLpos.Y : 0;

            var coords = ((int)lrX, (int)lrY, (int)rlX, (int)rlY);

            CrossLayout.Arrange(select, previous, scale, splitDist, mixBar, arrangeEx, coords, forceArrange, resetAll);

            if (arrangeEx) SeparateEx.Arrange(select, previous, scale, splitDist, mixBar, coords, forceArrange);
        }
        else
        {
            Reset();
        }

        Bars.Cross.Selection.Previous = select;
    }

    /// <summary>Calls the update function with arguments to reset everything</summary>
    internal static void TidyUp() => Update(true, true);

    /// <summary>Re-run the update function a few times on first login/load in case there's any straggler nodes caught out of position</summary>
    internal static void ScheduleNudges(int c = 5, int span = 500)
    {
        for (var i = 1; i <= c; i++) Task.Delay(span * i).ContinueWith(static delegate { Nudge(); });
    }

    /// <summary>Re-run the update function</summary>
    public static void Nudge()
    {
        if (IsSetUp && GameConfig.Cross.Enabled) Update(true);
    }

    /// <summary>Reset all the node properties we've messed with and reset any borrowed bars</summary>
    internal static void Reset()
    {
        if (!Bars.Cross.Exists) return;

        CrossLayout.Reset();
        SeparateEx.Reset();
    }
}

