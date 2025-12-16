using KnightOfNights.Scripts.SharedLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class AwakeHelper : LifecycleOnceHelper
{
    private void Awake() => Invoke();
}

internal static class AwakeExtensions
{
    internal static void DoOnAwake(this GameObject self, Action action) => self.GetOrAddComponent<AwakeHelper>().OnEvent += action;

    internal static void DoOnAwake(this MonoBehaviour self, Action action) => self.gameObject.DoOnAwake(action);
}
