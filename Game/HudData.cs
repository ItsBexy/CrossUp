using System;
using CrossUp.Features.Layout;
using CrossUp.Game.Hotbar;
using CrossUp.Utility;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace CrossUp.Game;

internal sealed class HudData : IDisposable
{
    private unsafe delegate nint SetHudLayout(AddonConfig* addonConfig, uint layoutIndex, bool unk1 = false, bool unk2 = true);

    [Signature("E8 ?? ?? ?? ?? 33 C0 EB 12", DetourName = nameof(OnSetHudLayout))]
    private readonly Hook<SetHudLayout>? SetHudLayoutHook = null;

    public HudData()
    {
       Service.GameInteropProvider.InitializeFromAttributes(this);
       SetHudLayoutHook?.Enable();
    }

    public void Dispose() => SetHudLayoutHook?.Dispose();

    private static readonly unsafe UIModule* UIModule = Framework.Instance()->GetUIModule();
    private static readonly unsafe AgentHUDLayout* HudLayout = UIModule->GetAgentModule()->GetAgentHUDLayout();

    /// <summary>Responds to the HUD layout being changed/set/saved</summary>
    public unsafe nint OnSetHudLayout(AddonConfig* addonConfig, uint hudSlot, bool unk1 = false, bool unk2 = true)
    {
        if (CrossUp.IsSetUp) Layout.ScheduleNudges(3, 10);
        return SetHudLayoutHook!.Original(addonConfig, hudSlot, unk1, unk2);
    }

    public static unsafe int CurrentSlot => UIModule->GetAddonConfig()->ModuleData->CurrentHudLayout + 1;

    /// <summary>Whether <see cref="AdjustXHBHudNode"/> has already run and completed</summary>
    private static bool HudChecked;

    /// <summary>Runs AdjustHudNode() if necessary</summary>
    public static unsafe void HudCheck() => HudChecked = HudLayout->AgentInterface.IsAgentActive() && (HudChecked || AdjustXHBHudNode());

    /// <summary>Fix for misaligned frame around XHB when using the HUD Layout Interface</summary>
    private static unsafe bool AdjustXHBHudNode()
    {
        var hudScreen = new BaseWrapper("_HudLayoutScreen");
        var root = Bars.Cross.Root.Node;

        if (hudScreen == null || root == null) return false;

        var scale = root->ScaleX;
        var hudNodes = hudScreen.NodeList;

        for (var i = 1; i < hudScreen.NodeListCount; i++)
        {
            var node = hudNodes[i];
            if (!node->IsVisible() ||
                Math.Abs(node->Y - root->Y) > 1 ||
                Math.Abs(node->Width - root->Width * scale) > 1 ||
                Math.Abs(node->Height - root->Height * scale) > 1 ||
                Math.Abs(node->X - root->X) < 1) continue;
            node->X = root->X;
            node->DrawFlags |= 0x5;
            return true;
        }

        return false;
    }
}