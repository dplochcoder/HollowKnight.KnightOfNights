using HutongGames.PlayMaker;
using System;
using UnityEngine;

namespace KnightOfNights.Util;

internal class DelayedLambda(float delay, Action action) : FsmStateAction
{
    private float remaining;

    public override void OnEnter()
    {
        base.OnEnter();
        remaining = delay;
        if (remaining <= 0) action();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (remaining <= 0) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0) action();
    }
}

internal static class FsmExtensions
{
}
