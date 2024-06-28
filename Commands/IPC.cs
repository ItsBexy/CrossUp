using System;
using System.Numerics;
using Dalamud.Plugin.Ipc;
using static CrossUp.CrossUp;

namespace CrossUp.Commands;

internal sealed class IPC : IDisposable
{
    private struct ProviderSet
    {
        internal ICallGateProvider<bool>? Available;
        internal ICallGateProvider<bool>? OpenSettings;
        internal ICallGateProvider<(bool, int, int), bool>? SplitBar;
        internal ICallGateProvider<(int, int, bool), bool>? Padlock;
        internal ICallGateProvider<(int, int, bool), bool>? SetNumText;
        internal ICallGateProvider<(int, int), bool>? ChangeSet;
        internal ICallGateProvider<bool, bool>? TriggerText;
        internal ICallGateProvider<bool, bool>? EmptySlots;
        internal ICallGateProvider<(int, int, Vector3), bool>? SelectBG;
        internal ICallGateProvider<(Vector3, Vector3), bool>? ButtonGlow;
        internal ICallGateProvider<(Vector3, Vector3, Vector3), bool>? TextAndBorders;
        internal ICallGateProvider<(bool, bool), bool>? ExBar;
        internal ICallGateProvider<(int, int), bool>? LRpos;
        internal ICallGateProvider<(int, int), bool>? RLpos;
    }

    private ProviderSet Provider;

    public IPC()
    {
        Dispose();

        Provider.Available = PluginInterface.GetIpcProvider<bool>("CrossUp.Available");
        Provider.Available.RegisterFunc(static () => true);

        Provider.OpenSettings = PluginInterface.GetIpcProvider<bool>("CrossUp.Open");
        Provider.OpenSettings.RegisterAction(static () => CrossUp.UI.SettingsWindow.IsOpen=true);

        Provider.SplitBar = PluginInterface.GetIpcProvider<(bool, int, int), bool>("CrossUp.SplitBar");
        Provider.SplitBar.RegisterAction(InternalCmd.SplitBar);

        Provider.Padlock = PluginInterface.GetIpcProvider<(int, int, bool), bool>("CrossUp.Padlock");
        Provider.Padlock.RegisterAction(InternalCmd.Padlock);

        Provider.SetNumText = PluginInterface.GetIpcProvider<(int, int, bool), bool>("CrossUp.SetNumText");
        Provider.SetNumText.RegisterAction(InternalCmd.SetNumText);

        Provider.ChangeSet = PluginInterface.GetIpcProvider<(int, int), bool>("CrossUp.ChangeSet");
        Provider.ChangeSet.RegisterAction(InternalCmd.ChangeSet);

        Provider.TriggerText = PluginInterface.GetIpcProvider<bool, bool>("CrossUp.TriggerText");
        Provider.TriggerText.RegisterAction(InternalCmd.TriggerText);

        Provider.EmptySlots = PluginInterface.GetIpcProvider<bool, bool>("CrossUp.EmptySlots");
        Provider.EmptySlots.RegisterAction(InternalCmd.EmptySlots);

        Provider.SelectBG = PluginInterface.GetIpcProvider<(int, int, Vector3), bool>("CrossUp.SelectBG");
        Provider.SelectBG.RegisterAction(InternalCmd.SelectBG);

        Provider.ButtonGlow = PluginInterface.GetIpcProvider<(Vector3, Vector3), bool>("CrossUp.ButtonGlow");
        Provider.ButtonGlow.RegisterAction(InternalCmd.ButtonGlow);

        Provider.TextAndBorders = PluginInterface.GetIpcProvider<(Vector3, Vector3, Vector3), bool>("CrossUp.TextAndBorders");
        Provider.TextAndBorders.RegisterAction(InternalCmd.TextAndBorders);

        Provider.ExBar = PluginInterface.GetIpcProvider<(bool, bool), bool>("CrossUp.ExBar");
        Provider.ExBar.RegisterAction(InternalCmd.ExBar);

        Provider.LRpos = PluginInterface.GetIpcProvider<(int, int), bool>("CrossUp.LRpos");
        Provider.LRpos.RegisterAction(InternalCmd.LRpos);

        Provider.RLpos = PluginInterface.GetIpcProvider<(int, int), bool>("CrossUp.RLpos");
        Provider.RLpos.RegisterAction(InternalCmd.RLpos);
    }

    public void Dispose()
    {
        Provider.Available?.UnregisterFunc();
        Provider.Available = null;

        Provider.OpenSettings?.UnregisterAction();
        Provider.OpenSettings = null;

        Provider.SplitBar?.UnregisterAction();
        Provider.SplitBar = null;

        Provider.Padlock?.UnregisterAction();
        Provider.Padlock = null;

        Provider.SetNumText?.UnregisterAction();
        Provider.SetNumText = null;

        Provider.ChangeSet?.UnregisterAction();
        Provider.ChangeSet = null;

        Provider.TriggerText?.UnregisterAction();
        Provider.TriggerText = null;

        Provider.EmptySlots?.UnregisterAction();
        Provider.EmptySlots = null;

        Provider.SelectBG?.UnregisterAction();
        Provider.SelectBG = null;

        Provider.ButtonGlow?.UnregisterAction();
        Provider.ButtonGlow = null;

        Provider.TextAndBorders?.UnregisterAction();
        Provider.TextAndBorders = null;

        Provider.ExBar?.UnregisterAction();
        Provider.ExBar = null;

        Provider.LRpos?.UnregisterAction();
        Provider.LRpos = null;

        Provider.RLpos?.UnregisterAction();
        Provider.RLpos = null;
    }
}