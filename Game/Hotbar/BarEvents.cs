using System;
using CrossUp.Features;
using CrossUp.Features.Layout;
using CrossUp.Utility;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static CrossUp.CrossUp;
using static CrossUp.Game.HudData;
using static CrossUp.Utility.Service;

namespace CrossUp.Game.Hotbar
{
    internal static unsafe partial class Bars
    {
        /// <summary>Set to true when <see cref="OnReceiveEvent"/> detects a drag/drop change, signalling the next <see cref="Cross.OnUpdate"/> to handle it.</summary>
        private static bool DragDrop;

        public static void OnReceiveEvent(AddonEvent type, AddonArgs args)
        {
            try
            {
                var reArgs = (AddonReceiveEventArgs)args;
                var barBase = (AddonActionBarBase*)args.Addon;

                switch (reArgs.AtkEventType)
                {
                    case 50 or 54 when SeparateEx.Ready && GameConfig.Cross.Enabled:
                    {
                        var barID = barBase->RaptureHotbarId;
                        Log.Debug($"Drag/Drop Event on Bar #{barID} ({(barID > 9 ? $"Cross Hotbar Set {barID - 9}" : $"Hotbar {barID + 1}")}); Handling on next Update event");
                        CrossLayout.UnassignedSlotVis(Profile.HideUnassigned);
                        DragDrop = true;
                        break;
                    }
                    case 47 when IsSetUp:
                        CrossLayout.UnassignedSlotVis(true);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception: ActionBarReceiveEventDetour Failed!\n{ex}");
            }
        }

        internal partial class Cross
        {
            /// <summary><list type="bullet">
            /// <item>Checks if conditions are right to initialize the plugin, or (once initialized) if it needs to be disabled again.</item>
            /// <item>Calls animation function (<see cref="SeparateEx.MetaSlots.TweenAll"/>) when relevant.</item>
            /// <item>Tweaks the XHB's node in the HUD editor to match the bar's altered state</item>
            /// <item>Hides the Separated EXHB if the main menu is open</item>
            /// </list>
            /// </summary>
            public static void OnDraw(AddonEvent type, AddonArgs args)
            {
                try
                {
                    if (IsSetUp)
                    {
                        if (SeparateEx.MetaSlots.TweensExist) SeparateEx.MetaSlots.TweenAll(); // animate button sizes if needed
                        if (Profile.CombatFadeInOut && CombatFader.FadeTween.Active) CombatFader.FadeTween.Run();

                        HudCheck(); // if HUD layout editor is open, perform this fix once
                    }
                    else if (Job.Current != 0 && GetBases())
                    {
                        Log.Info("Cross Hotbar nodes found; setting up plugin features");
                        Setup();
                    }
                }
                catch (Exception ex)
                {
                    Events.LogPolitely($"Exception: XHB PreDraw Listener Failed!\n{ex}");
                }
            }

            /// <summary>When the XHB is disposed of</summary>
            public static void OnFinalize(AddonEvent type, AddonArgs args)
            {
                Log.Warning("Hotbar nodes disposed; disabling plugin features");
                IsSetUp = false;
            }

            /// <summary>
            /// Whenever hotbar data is updated for the XHB. (Plugin previously hooked ActionBarBase_Update for this)
            /// </summary>
            public static void OnUpdate(AddonEvent type, AddonArgs args)
            {
                if (!IsSetUp) return;
                var barBase = (AddonActionBarBase*)args.Addon;
                try
                {
                    if (DragDrop)
                    {
                        Hotbar.Actions.HandleDragDrop();
                        DragDrop = false;
                    }

                    if (Job.HasChanged) Job.HandleJobChange();
                    if (SetID.HasChanged()) SetSwitching.HandleSetChange(barBase->RaptureHotbarId);
                    Layout.Update(EnableStateChanged);
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception: XHB PostRequestedUpdate Listener Failed!\n{ex}");
                }
            }
        }

        internal sealed partial class ActionBar
        {
            /// <summary>
            /// Draw event for standard action bars. Currently only intended for bars borrowed by the Separate Expanded Hold feature.
            /// </summary>
            public static void OnDraw(AddonEvent type, AddonArgs args)
            {
                try
                {
                    if (!SeparateEx.Ready) return;

                    if (args.AddonName == LR.BorrowBar.Base.AddonName)
                    {
                        FlashCheck(new BaseWrapper((AtkUnitBase*)args.Addon, args.AddonName, true), ref CooldownPartIdsLR);
                    }
                    else if (args.AddonName == RL.BorrowBar.Base.AddonName)
                    {
                        FlashCheck(new BaseWrapper((AtkUnitBase*)args.Addon, args.AddonName, true), ref CooldownPartIdsRL);
                    }
                }
                catch (Exception ex)
                {
                    Events.LogPolitely($"Exception: Action Bar PreDraw Listener Failed!\n{ex}");
                }
            }

            private static ushort[] CooldownPartIdsRL = new ushort[12];
            private static ushort[] CooldownPartIdsLR = new ushort[12];

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
                    if (cdNode.Node->IsVisible() && cdPartID == 80 && cdPartIDs[i] < 75) cdNode.SetVis(false);
                    cdPartIDs[i] = cdPartID;
                }
            }

        }

        internal partial class MainMenu
        {
            /// <summary>Sets the visibility of the EXHB when the main menu is opened via gamepad</summary>
            public static void OnDraw(AddonEvent type, AddonArgs args)
            {
                try
                {
                    if (!SeparateEx.Ready) return;

                    var alpha = (byte)(Base.Visible ? 0 : 255);
                    LR.Root.SetAlpha(alpha);
                    RL.Root.SetAlpha(alpha);
                }
                catch (Exception ex)
                {
                    Events.LogPolitely($"Exception: Main Menu PreDraw Listener Failed!\n{ex}");
                }
            }
        }
    }
}