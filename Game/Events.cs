using System;
using System.Threading.Tasks;
using CrossUp.Features;
using CrossUp.Game.Hotbar;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.ClientState.Conditions;
using static CrossUp.CrossUp;
using static CrossUp.Utility.Service;

namespace CrossUp.Game
{
    internal sealed class Events : IDisposable
    {
        private readonly string[] AllHotbars = { "_ActionCross", "_ActionBar", "_ActionBar01", "_ActionBar02", "_ActionBar03", "_ActionBar04", "_ActionBar05", "_ActionBar06", "_ActionBar07", "_ActionBar08", "_ActionBar09" };

        public Events()
        {
            AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_MainCross", Bars.MainMenu.OnDraw);
            AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_ActionCross", Bars.Cross.OnDraw);
            AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_ActionCross", Bars.Cross.OnUpdate);
            AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "_ActionCross", Bars.Cross.OnFinalize);
            AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, AllHotbars, Bars.OnReceiveEvent);

            Condition.ConditionChange += OnConditionChange;
        }

        public void Dispose()
        {
            AddonLifecycle.UnregisterListener(AddonEvent.PreDraw);
            AddonLifecycle.UnregisterListener(AddonEvent.PostRequestedUpdate);
            AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize);
            AddonLifecycle.UnregisterListener(AddonEvent.PostReceiveEvent);

            Condition.ConditionChange -= OnConditionChange;
        }

        /// <summary>Runs the fader feature when the player's condition changes</summary>
        public static void OnConditionChange(ConditionFlag flag = 0, bool value = true)
        {
            if (!Profile.CombatFadeInOut) return;

            var show = Condition.Any(ConditionFlag.InCombat, ConditionFlag.Crafting, ConditionFlag.PreparingToCraft, ConditionFlag.Crafting40, ConditionFlag.Fishing, ConditionFlag.Gathering, ConditionFlag.Gathering42, ConditionFlag.PvPDisplayActive);
            CombatFader.FadeTween.Begin(show);
        }

        private static bool LogFrameCatch = true;

        /// <summary>Logs a maximum of once every five seconds, so failed draw events don't instantly flood the log.</summary>
        public static void LogPolitely(string msg)
        {
            if (!LogFrameCatch) return;
            LogFrameCatch = false;
            Log.Error(msg);
            Task.Delay(5000).ContinueWith(static delegate { LogFrameCatch = true; });
        }
    }
}