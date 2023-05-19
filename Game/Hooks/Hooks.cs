using System;

namespace CrossUp.Game.Hooks;

internal sealed class Hooks : IDisposable
{
    private readonly Events Events = new();
    private readonly ActionBarHooks ActionBarHooks = new();
    private readonly HudHooks HudHooks = new();

    public void Dispose()
    {
        Events.Dispose();
        ActionBarHooks.Dispose();
        HudHooks.Dispose();
    }
}