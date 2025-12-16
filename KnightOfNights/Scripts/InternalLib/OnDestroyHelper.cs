using ItemChanger.Extensions;
using System;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class OnDestroyHelper : LifecycleOnceHelper
{
    private void OnDestroy() => Invoke();
}

internal static class OnDestroyHelperExtensions
{
    internal static void DoOnDestroy(this GameObject self, Action action) => self.GetOrAddComponent<OnDestroyHelper>().OnEvent += action;

    internal static void DoOnDestroy(this MonoBehaviour self, Action action) => self.gameObject.DoOnDestroy(action);
}
