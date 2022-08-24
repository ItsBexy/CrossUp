using System.Threading.Tasks;
using Dalamud.Logging;
using static CrossUp.CrossUp.Bars.Cross.Selection;

namespace CrossUp;

public sealed unsafe partial class CrossUp
{
    /// <summary>Methods regarding overall layout manipulation</summary>
    public static partial class Layout
    {
        /// <summary>Checks/updates the Cross Hotbar selection and calls the main arrangement functions</summary>
        internal static void Update(bool forceArrange = false, bool hudFixCheck = false, bool resetAll = false)
        {
            if (!Bars.Cross.Exists) return;

            var select = Current;
            if (Bars.Cross.Enabled)
            {
                var scale = Bars.Cross.Root.Node->ScaleX;
                var split = resetAll ? 0 : Config.Split;
                var mixBar = (bool)CharConfig.MixBar;
                var arrangeEx = !resetAll && SeparateEx.Ready && Bars.RL.Exists && Bars.LR.Exists;
                var lockCenter = Config.LockCenter;

                var lrX = arrangeEx ? Config.LRpos.X : 0;
                var lrY = arrangeEx ? Config.LRpos.Y : 0;
                var rlX = arrangeEx ? Config.OnlyOneEx ? Config.LRpos.X : Config.RLpos.X : 0;
                var rlY = arrangeEx ? Config.OnlyOneEx ? Config.LRpos.Y : Config.RLpos.Y : 0;

                var coords = ((int)lrX, (int)lrY, (int)rlX, (int)rlY);

                if (lockCenter)       Cross.Recenter(scale);
                else if (hudFixCheck) Cross.HudOffsetFix(split, scale);

                Cross.Arrange(select, Previous, scale, split, mixBar, arrangeEx, coords, forceArrange, resetAll);

                if (arrangeEx) SeparateEx.Arrange(select, Previous, scale, split, mixBar, coords, forceArrange);
            }
            else
            {
                ResetBars();
            }

            Previous = select;
        }

        /// <summary>Calls the update function with arguments to reset everything</summary>
        internal static void TidyUp() => Update(true, true, true);

        /// <summary>Re-run the update function a few times on first login/load in case there's any straggler nodes caught out of position</summary>
        internal static void ScheduleNudges(int c=5,int span = 500,bool log = true)
        {
            for (var i = 1; i <= c; i++)
            {
                var n = i;
                Task.Delay(span * i).ContinueWith(delegate { Nudge(c, n, log); });
            }
        }

        /// <summary>Re-run the update function</summary>
        private static void Nudge(int c, int n, bool log=true)
        {
            try
            {
                if (!IsSetUp) return;
                if (log) {PluginLog.LogDebug($"Nudging Nodes {n}/{c}");}

                if (Config.DisposeBaseX != null && Config.DisposeRootX != null) Cross.RestoreXPos();
                Update(true);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>Reset all the node properties we've messed with and reset any borrowed bars</summary>
        internal static void ResetBars()
        {
            if (!Bars.Cross.Exists) return;

            Bars.Cross.VertLine.SetSize();
            Bars.Cross.Padlock.SetRelativePos();
            Bars.Cross.Padlock[2u].SetVis(true);
            Bars.Cross.SetDisplay.SetVis(true).SetRelativePos();
            Bars.Cross.ChangeSetDisplay.Container.SetRelativePos();
            Bars.Cross.LTtext.SetScale();
            Bars.Cross.RTtext.SetScale();
            Bars.Cross.MiniSelectL.SetSize();
            Bars.Cross.MiniSelectR.SetSize();

            var job = Job.Current;
            for (var barID = 1; barID <= 9; barID++) ResetBar(barID, job);
        }

        /// <summary>Put a borrowed hotbar back the way we found it based on HUD layout settings and saved actions</summary>
        private static void ResetBar(int barID, int job)
        {
            if (!Bars.ActionBars[barID].Exists) return;

            Actions.Copy(Actions.GetSaved(CharConfig.Hotbar.Shared[barID] ? 0 : job, barID), 0, barID, 0, 12);

            Bars.ActionBars[barID].Root.SetPos(Bars.ActionBars[barID].Base.X, Bars.ActionBars[barID].Base.Y)
                                       .SetSize()
                                       .SetScale(Bars.ActionBars[barID].Base.Scale);

            Bars.ActionBars[barID].BarNumText.SetScale();

            for (var i = 0; i < 12; i++)
            {
                var buttonNode = Bars.ActionBars[barID].Buttons[i,getDef:true];

                buttonNode.SetRelativePos()
                          .SetVis(true)
                          .SetScale();

                buttonNode[2u].SetVis(true);
            }

            if (CharConfig.Hotbar.Visible[barID] && Bars.WasHidden[barID] && ((barID != Bars.LR.ID && barID != Bars.RL.ID) || !SeparateEx.Ready))
            {
                CharConfig.Hotbar.Visible[barID].Set(0);
            }
        }
    }
}

