using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrossUp.Features;
using CrossUp.Features.Layout;
using CrossUp.Game.Hotbar;
using CrossUp.Utility;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static CrossUp.CrossUp;
using static CrossUp.Game.Hooks.HudHooks;
using static CrossUp.Utility.Service;

namespace CrossUp.Game.Hooks
{
    internal sealed class Events : IDisposable
    {
        private static readonly List<string> ActionBarList = new() { "_ActionBar01", "_ActionBar02", "_ActionBar03", "_ActionBar04", "_ActionBar05", "_ActionBar06", "_ActionBar07", "_ActionBar08", "_ActionBar09" };

        static Events()
        {
            AddonLifecycle.RegisterListener(AddonEvent.PreDraw,"_MainCross", MainMenu.PreDraw);
            AddonLifecycle.RegisterListener(AddonEvent.PreDraw,"_ActionCross", Cross.PreDraw);
            AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "_ActionCross", Cross.Finalize);
            Condition.ConditionChange += OnConditionChange;
        }

        public void Dispose()
        {
            AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, ActionBarList, ActionBars.PreDraw);
            AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "_MainCross", MainMenu.PreDraw);
            AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "_ActionCross", Cross.PreDraw);
            AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, "_ActionCross", Cross.Finalize);
            Condition.ConditionChange -= OnConditionChange;
        }

        private static unsafe class Cross
        {
            /// <summary><list type="bullet">
            /// <item>Checks if conditions are right to initialize the plugin, or (once initialized) if it needs to be disabled again.</item>
            /// <item>Calls animation function (<see cref="SeparateEx.MetaSlots.TweenAll"/>) when relevant.</item>
            /// <item>Tweaks the XHB's node in the HUD editor to match the bar's altered state</item>
            /// <item>Hides the Separated EXHB if the main menu is open</item>
            /// </list>
            /// </summary>
            public static void PreDraw(AddonEvent type, AddonArgs args)
            {
                try
                {
                    if (IsSetUp)
                    {
                        if (SeparateEx.MetaSlots.TweensExist) SeparateEx.MetaSlots.TweenAll(); // animate button sizes if needed
                        if (Profile.CombatFadeInOut && CombatFader.FadeTween.Active) { CombatFader.FadeTween.Run(); } // run fader animation if needed

                        HudChecked = HudLayout->AgentInterface.IsAgentActive() && (HudChecked || AdjustHudNode()); // if HUD layout editor is open, perform this fix once
                    }
                    else if (Job.Current != 0 && Bars.GetBases())
                    {
                        Log.Info("Cross Hotbar nodes found; setting up plugin features");
                        HudSlot = GetHudSlot();
                        Setup();
                    }
                }
                catch (Exception ex) {
                    LogPolitely($"Exception: XHB PreDraw Listener Failed!\n{ex}"); }
            }

            /// <summary>When the XHB is disposed of</summary>
            public static void Finalize(AddonEvent type, AddonArgs args)
            {
                Log.Warning("Hotbar nodes disposed; disabling plugin features");
                IsSetUp = false;
            }
        }

        private static class MainMenu
        {
            /// <summary>Sets the visibility of the EXHB when the main menu is opened via gamepad</summary>
            public static void PreDraw(AddonEvent type, AddonArgs args)
            {
                try { SeparateEx.MainMenuCheck(); } catch (Exception ex) { LogPolitely($"Exception: Main Menu PreDraw Listener Failed!\n{ex}"); }
            }
        }

        public static unsafe class ActionBars
        {
            /// <summary>
            /// Draw event for standard action bars. Currently only intended for bars borrowed by the Separate Expanded Hold feature.
            /// </summary>
            public static void PreDraw(AddonEvent type, AddonArgs args)
            {
                try
                {
                    if (!SeparateEx.Ready) return;

                    if (args.AddonName == Bars.LR.BorrowBar.Base.AddonName)
                    { 
                        FlashCheck(new BaseWrapper((AtkUnitBase*)args.Addon, args.AddonName, true), ref CoolDownPartIDs.LR);
                    }
                    else if (args.AddonName == Bars.RL.BorrowBar.Base.AddonName)
                    {
                        FlashCheck(new BaseWrapper((AtkUnitBase*)args.Addon, args.AddonName, true), ref CoolDownPartIDs.RL);
                    }
                }
                catch (Exception ex) { LogPolitely($"Exception: Action Bar PreDraw Listener Failed!\n{ex}"); }
            }

            /// <summary>
            /// Records the last known states of the PartID of all the cooldown image nodes on each borrowed bar. 80 represents the "cooldown complete" flash.
            /// </summary>
            private static class CoolDownPartIDs
            {
                public static ushort[] LR = new ushort[12];
                public static ushort[] RL = new ushort[12];
            }

            /// <summary>
            /// Fix for a momentary visual flash that would occur when switching bars while cooldowns are ticking.<br/><br/>
            /// If an icon on one of the borrowed bars has a cooldown image node whose PartID just jumped up to 80 from a much lower value, we disable the node's visibility to hide the unwanted flash.
            /// </summary>
            private static void FlashCheck(BaseWrapper bar, ref ushort[] cdPartIDs)
            {
                for (uint i = 0; i < 12; i++)
                {
                    var cdNode = bar[i + 8u][3u][2u][14u];
                    var cdPartID = cdNode.Node->GetAsAtkImageNode()->PartId;
                    if (cdNode.Node->IsVisible && cdPartID == 80 && cdPartIDs[i] < 75) cdNode.SetVis(false);
                    cdPartIDs[i] = cdPartID;
                }
            }
        }

        /// <summary>Runs the fader feature when the player's condition changes</summary>
        public static void OnConditionChange(ConditionFlag flag = 0, bool value = true)
        {
            if (!Profile.CombatFadeInOut) return;

            var show = Condition.Any(ConditionFlag.InCombat, ConditionFlag.Crafting, ConditionFlag.PreparingToCraft, ConditionFlag.Crafting40, ConditionFlag.Fishing, ConditionFlag.Gathering,ConditionFlag.Gathering42,ConditionFlag.PvPDisplayActive);
            CombatFader.FadeTween.Begin(show);
        }

        private static bool LogFrameCatch = true;

        /// <summary>
        /// Logs a maximum of once every five seconds, so failed draw events don't instantly flood the log.
        /// </summary>
        private static void LogPolitely(string msg)
        {
            if (!LogFrameCatch) return;
            LogFrameCatch = false;
            Log.Error(msg);
            Task.Delay(5000).ContinueWith(static delegate { LogFrameCatch = true; });
        }
    }
}