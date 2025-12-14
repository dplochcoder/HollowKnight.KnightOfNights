using ItemChanger.Extensions;
using System;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class OnDestroyHelper : MonoBehaviour
{
    internal Action? Action;

    private void OnDestroy() => Action?.Invoke();
}

internal static class OnDestroyHelperExtensions
{
    internal static void DoOnDestroy(this GameObject self, Action action) => self.GetOrAddComponent<OnDestroyHelper>().Action += action;

    internal static void DoOnDestroy(this MonoBehaviour self, Action action) => self.gameObject.DoOnDestroy(action);
}
