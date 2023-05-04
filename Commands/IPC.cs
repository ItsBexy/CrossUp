using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace CrossUp;

public sealed partial class CrossUp
{
    public static class IpcManager
    {
        public class Provider
        {
            internal static ICallGateProvider<bool>? Available;
            internal static ICallGateProvider<(bool, int, int), bool>? SplitBar;
            internal static ICallGateProvider<(int, int, bool), bool>? Padlock;
            internal static ICallGateProvider<(int, int, bool), bool>? SetNumText;
            internal static ICallGateProvider<(int, int), bool>? ChangeSet;
            internal static ICallGateProvider<bool, bool>? TriggerText;
            internal static ICallGateProvider<bool, bool>? EmptySlots;
            internal static ICallGateProvider<(int, int, Vector3), bool>? SelectBG;
            internal static ICallGateProvider<(Vector3, Vector3), bool>? ButtonGlow;
            internal static ICallGateProvider<(Vector3, Vector3, Vector3), bool>? TextAndBorders;
            internal static ICallGateProvider<(bool, bool), bool>? ExBar;
            internal static ICallGateProvider<(int, int), bool>? LRpos;
            internal static ICallGateProvider<(int, int), bool>? RLpos;
        }

        internal static void Register(DalamudPluginInterface pluginInterface)
        {
            Unregister();

            Provider.Available = pluginInterface.GetIpcProvider<bool>("CrossUp.Available");
            Provider.Available.RegisterFunc(static () => true);

            Provider.SplitBar = pluginInterface.GetIpcProvider<(bool, int, int), bool>("CrossUp.SplitBar");
            Provider.SplitBar.RegisterAction(Commands.SplitBar);

            Provider.Padlock = pluginInterface.GetIpcProvider<(int, int, bool), bool>("CrossUp.Padlock");
            Provider.Padlock.RegisterAction(Commands.Padlock);

            Provider.SetNumText = pluginInterface.GetIpcProvider<(int, int, bool), bool>("CrossUp.SetNumText");
            Provider.SetNumText.RegisterAction(Commands.SetNumText);

            Provider.ChangeSet = pluginInterface.GetIpcProvider<(int, int), bool>("CrossUp.ChangeSet");
            Provider.ChangeSet.RegisterAction(Commands.ChangeSet);

            Provider.TriggerText = pluginInterface.GetIpcProvider<bool, bool>("CrossUp.TriggerText");
            Provider.TriggerText.RegisterAction(Commands.TriggerText);

            Provider.EmptySlots = pluginInterface.GetIpcProvider<bool, bool>("CrossUp.EmptySlots");
            Provider.EmptySlots.RegisterAction(Commands.EmptySlots);

            Provider.SelectBG = pluginInterface.GetIpcProvider<(int, int, Vector3), bool>("CrossUp.SelectBG");
            Provider.SelectBG.RegisterAction(Commands.SelectBG);

            Provider.ButtonGlow = pluginInterface.GetIpcProvider<(Vector3, Vector3), bool>("CrossUp.ButtonGlow");
            Provider.ButtonGlow.RegisterAction(Commands.ButtonGlow);

            Provider.TextAndBorders = pluginInterface.GetIpcProvider<(Vector3, Vector3, Vector3), bool>("CrossUp.TextAndBorders");
            Provider.TextAndBorders.RegisterAction(Commands.TextAndBorders);

            Provider.ExBar = pluginInterface.GetIpcProvider<(bool, bool), bool>("CrossUp.ExBar");
            Provider.ExBar.RegisterAction(Commands.ExBar);

            Provider.LRpos = pluginInterface.GetIpcProvider<(int, int), bool>("CrossUp.LRpos");
            Provider.LRpos.RegisterAction(Commands.LRpos);

            Provider.RLpos = pluginInterface.GetIpcProvider<(int, int), bool>("CrossUp.RLpos");
            Provider.RLpos.RegisterAction(Commands.RLpos);
        }

        internal static void Unregister()
        {
            Provider.Available?.UnregisterFunc();
            Provider.Available = null;

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
}