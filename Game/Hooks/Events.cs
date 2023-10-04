using System;
using System.Threading.Tasks;
using CrossUp.Features;
using CrossUp.Features.Layout;
using CrossUp.Game.Hotbar;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using static CrossUp.CrossUp;
using static CrossUp.Game.Hooks.HudHooks;
using static CrossUp.Utility.Service;

namespace CrossUp.Game.Hooks
{
    internal sealed class Events : IDisposable
    {
        static Events()
        {
            Framework.Update          += OnFrameUpdate;
            Condition.ConditionChange += OnConditionChange;
        }

        public void Dispose()
        {
            Framework.Update          -= OnFrameUpdate;
            Condition.ConditionChange -= OnConditionChange;
        }

        private static bool LogFrameCatch = true;

        /// <summary><list type="bullet">
        /// <item>Checks if conditions are right to initialize the plugin, or (once initialized) if it needs to be disabled again.</item>
        /// <item>Calls animation function (<see cref="SeparateEx.MetaSlots.TweenAll"/>) when relevant.</item>
        /// <item>Tweaks the XHB's node in the HUD editor to match the bar's altered state</item>
        /// <item>Hides the Separated EXHB if the main menu is open</item>
        /// </list>
        /// </summary>
        private static unsafe void OnFrameUpdate(IFramework framework)
        {
            try
            {
                if (IsSetUp)
                {
                    if (Bars.Cross.Exists)
                    {
                        if (SeparateEx.MetaSlots.TweensExist) SeparateEx.MetaSlots.TweenAll(); // animate button sizes if needed
                        if (Profile.CombatFadeInOut && CombatFader.FadeTween.Active) { CombatFader.FadeTween.Run(); } // run fader animation if needed

                        HudChecked = HudLayout->AgentInterface.IsAgentActive() && (HudChecked || AdjustHudNode()); // if HUD layout editor is open, perform this fix once

                        if (SeparateEx.Ready && Bars.MainMenu.Exists) SeparateEx.HideForMenu();
                    }
                    else
                    {
                        Log.Debug("Cross Hotbar nodes not found; disabling plugin features");
                        IsSetUp = false;
                    }
                }
                else if (Bars.AllExist && Job.Current != 0)
                {
                    Log.Debug("Cross Hotbar nodes found; setting up plugin features");
                    HudSlot = GetHudSlot();
                    Setup();
                }
            }
            catch (Exception ex)
            {
                if (LogFrameCatch)
                {
                    LogFrameCatch = false;
                    Log.Error($"Exception: Framework Update Function Failed!\n{ex}");
                    Task.Delay(5000).ContinueWith(static delegate { LogFrameCatch = true; }); // So we aren't spamming if something goes terribly wrong
                }
            }
        }

        /// <summary>Runs the fader feature when the player's condition changes</summary>
        public static void OnConditionChange(ConditionFlag flag = 0, bool value = true)
        {
            if (Profile.CombatFadeInOut) CombatFader.FadeTween.Begin(Condition[ConditionFlag.InCombat]);
        }
    }
}