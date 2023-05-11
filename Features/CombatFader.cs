using System;
using CrossUp.Game;
using static CrossUp.CrossUp;

namespace CrossUp.Features;

internal class CombatFader
{
    internal struct FadeTween
    {
        private static DateTime Start { get; set; }
        private static TimeSpan Duration { get; set; }
        private static int From { get; set; }
        private static int? To { get; set; }
        internal static bool Active { get; private set; }

        internal static void Run()
        {
            var progress = (float)decimal.Divide((DateTime.Now - Start).Milliseconds, Duration.Milliseconds);

            if (!(progress >= 1) && GameConfig.Transparency.Standard != To!)
            {
                GameConfig.Transparency.Standard.Set((int)(progress < 1 ? (To! - From) * progress + From : To!));
            }
            else
            {
                Active = false;
                GameConfig.Transparency.Standard.Set((int)To!);
                To = null;
            }
        }

        internal static void Begin(bool inCombat)
        {
            var to = inCombat ? Profile.TranspInCombat : Profile.TranspOutOfCombat;
            var dur = inCombat ? 150 : 350;

            if ((Active && To == to) || GameConfig.Transparency.Standard == to) return;

            Start = DateTime.Now;
            Duration = new(0, 0, 0, 0, dur);
            From = GameConfig.Transparency.Standard;
            To = to;
            Active = true;
        }
    }
}