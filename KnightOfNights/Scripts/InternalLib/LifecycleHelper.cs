using KnightOfNights.Scripts.SharedLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class LifecycleOnceHelper : MonoBehaviour
{
    private readonly HashSet<Action> actions = [];
    private bool invoked = false;

    internal event Action OnEvent
    {
        add
        {
            if (invoked) value();
            else actions.Add(value);
        }
        remove => actions.Remove(value);
    }

    protected void Invoke()
    {
        if (invoked) return;

        invoked = true;
        actions.ForEach(a => a.Invoke());
        actions.Clear();
    }
}
