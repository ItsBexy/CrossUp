using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Hooking;
using Dalamud.IoC;

namespace CrossUp;

// Services and Utilities from SimpleTweaks by Caraxi https://github.com/Caraxi/SimpleTweaksPlugin
public class Service    // from https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Service.cs
{
    [PluginService] public static ClientState ClientState { get; private set; }
    [PluginService] public static Framework Framework { get; private set; }
    [PluginService] public static GameGui GameGui { get; private set; }
    [PluginService] public static SigScanner SigScanner { get; private set; }
}
public class Common {  // from https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Utility/Common.cs
    public static SigScanner Scanner => Service.SigScanner;
    public static HookWrapper<T> Hook<T>(string signature, T detour, int addressOffset = 0) where T : Delegate {
        var addr = Scanner.ScanText(signature);
        var h = new Hook<T>(addr + addressOffset, detour);
        var wh = new HookWrapper<T>(h);
        HookList.Add(wh);
        return wh;
    }

    public static List<IHookWrapper> HookList = new();
}

public interface IHookWrapper : IDisposable // from https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Utility/HookWrapper.cs
{
    public void Enable();
    public void Disable();
    public bool IsEnabled { get; }
    public bool IsDisposed { get; }
}

public class HookWrapper<T> : IHookWrapper where T : Delegate
{
    private Hook<T> wrappedHook;
    private bool disposed;
    public HookWrapper(Hook<T> hook) { wrappedHook = hook; }
    public void Enable()
    {
        if (disposed) return;
        wrappedHook?.Enable();
    }

    public void Disable()
    {
        if (disposed) return;
        wrappedHook?.Disable();
    }

    public void Dispose()
    {
        Disable();
        disposed = true;
        wrappedHook?.Dispose();
    }

    public T Original => wrappedHook.Original;
    public bool IsEnabled => wrappedHook.IsEnabled;
    public bool IsDisposed => wrappedHook.IsDisposed;
}