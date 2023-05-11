using System;

namespace CrossUp.Game.Hooks;

internal sealed class Hooks : IDisposable
{
    private readonly EventHooks EventHooks = new();
    private readonly ActionBarHooks ActionBarHooks = new();
    private readonly HudHooks HudHooks = new();

    public void Dispose()
    {
        EventHooks.Dispose();
        ActionBarHooks.Dispose();
        HudHooks.Dispose();
    }
}