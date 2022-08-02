using System;

namespace CrossUp;

public sealed partial class CrossUp
{
    public struct FadeTween
    {
        private static DateTime Start { get; set; }
        private static TimeSpan Duration { get; set; }
        private static int From { get; set; }
        private static int? To { get; set; }
        internal static bool Active { get; private set; }
        internal static void Run()
        {
            var progress = (float)decimal.Divide((DateTime.Now - Start).Milliseconds, Duration.Milliseconds);

            if (!(progress >= 1) && CharConfig.Transparency.Standard != To!)
            {
                CharConfig.Transparency.Standard.Set((int)(progress < 1 ? (To! - From) * progress + From : To!));
            }
            else
            {
                Active = false;
                CharConfig.Transparency.Standard.Set((int)To!);
                To = null;
            }
        }
        internal static void Begin(bool inCombat)
        {
            var to = inCombat ? Config.TranspInCombat : Config.TranspOutOfCombat;
            var dur = inCombat ? 150 : 350;
  
            if ((Active && To == to) || CharConfig.Transparency.Standard == to) return;

            Start = DateTime.Now;
            Duration = new(0, 0, 0, 0, dur);
            From = CharConfig.Transparency.Standard;
            To = to;
            Active = true;
        }
    }
}