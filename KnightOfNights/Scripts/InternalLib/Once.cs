using System;

namespace KnightOfNights.Scripts.InternalLib;

internal class Once(Action action)
{
    private bool done = false;

    public void Invoke()
    {
        if (done) return;

        done = true;
        action();
    }

    public static implicit operator Action(Once once) => once.Invoke;
}
