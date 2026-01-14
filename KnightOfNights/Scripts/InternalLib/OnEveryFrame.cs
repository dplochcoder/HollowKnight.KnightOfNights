using System;

namespace KnightOfNights.Scripts.InternalLib;

internal static class OnEveryFrame
{
    internal static event Action? Event;

    static OnEveryFrame() => On.GameManager.Update += (orig, self) =>
    {
        orig(self);
        try { Event?.Invoke(); } catch (Exception e) { KnightOfNightsMod.LogError($"{e}"); }
    };
}
