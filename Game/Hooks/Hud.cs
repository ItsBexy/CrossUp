using System;
using System.Runtime.InteropServices;
using CrossUp.Features.Layout;
using CrossUp.Game.Hotbar;
using CrossUp.Utility;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static CrossUp.CrossUp;

// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace CrossUp.Game.Hooks
{
    internal sealed class HudHooks : IDisposable
    {
        private delegate IntPtr GetFilePointerDelegate(byte index);

        private static GetFilePointerDelegate? GetFilePointer;

        public HudHooks()
        {
            SetHudLayoutHook ??= Hook<SetHudLayoutDel>.FromAddress(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 33 C0 EB 15"), SetHudLayoutDetour);
            SetHudLayoutHook?.Enable();

            var ptr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 85 C0 74 14 83 7B 44 00");
            if (ptr != IntPtr.Zero) GetFilePointer = Marshal.GetDelegateForFunctionPointer<GetFilePointerDelegate>(ptr);
        }

        public void Dispose() => SetHudLayoutHook?.Dispose();

        /// <summary>The current HUD slot</summary>
        internal static int HudSlot;

        /// <summary>Finds the HUD slot on setup</summary>
        public static unsafe int GetHudSlot()
        {
            try
            {
                var filePtr = GetFilePointer?.Invoke(0) ?? IntPtr.Zero;
                var dataPtr = filePtr + 0x50;
                return (int)*(uint*)(Marshal.ReadIntPtr(dataPtr) + 0x6378) + 1;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Responds to the HUD layout being changed/set/saved</summary>
        private static uint SetHudLayoutDetour(IntPtr filePtr, uint hudSlot, byte unk0, byte unk1)
        {
            HudSlot = (int)(hudSlot + 1);
            if (IsSetUp) Layout.ScheduleNudges(2, 10);
            return SetHudLayoutHook!.Original(filePtr, hudSlot, unk0, unk1);
        }

        /// <summary>Whether <see cref="AdjustHudNode"/> has already run and completed</summary>
        public static bool HudChecked;

        /// <summary>Fix for misaligned frame around XHB when using the HUD Layout Interface</summary>
        public static unsafe bool AdjustHudNode()
        {
            var hudScreen = new BaseWrapper("_HudLayoutScreen");
            var root = Bars.Cross.Root.Node;

            if (hudScreen == null || root == null) return false;

            var scale = root->ScaleX;
            var hudNodes = hudScreen.NodeList;

            for (var i = 1; i < hudScreen.NodeListCount; i++)
            {
                if (!hudNodes[i]->IsVisible ||
                    Math.Abs(hudNodes[i]->Y - root->Y) > 1 ||
                    Math.Abs(hudNodes[i]->Width - root->Width * scale) > 1 ||
                    Math.Abs(hudNodes[i]->Height - root->Height * scale) > 1 ||
                    Math.Abs(hudNodes[i]->X - root->X) < 1) continue;
                hudNodes[i]->X = root->X;
                hudNodes[i]->Flags_2 |= 0xD;
                return true;
            }

            return false;
        }

        private delegate uint SetHudLayoutDel(IntPtr filePtr, uint hudLayout, byte unk0, byte unk1);

        private static Hook<SetHudLayoutDel>? SetHudLayoutHook;
        public static readonly unsafe AgentHudLayout* HudLayout = Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentHudLayout();


    }
}