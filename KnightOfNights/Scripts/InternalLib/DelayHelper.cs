using ItemChanger.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class DelayHelper : MonoBehaviour
{
    internal new void StartCoroutine(IEnumerator coroutine) => base.StartCoroutine(coroutine);
}

internal static class DelayHelperExtensions
{
    internal static void DoAfter(this GameObject self, Action action, float delay)
    {
        if (delay <= 0)
        {
            action();
            return;
        }

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(delay);
            action();
        }
        self.GetOrAddComponent<DelayHelper>().StartCoroutine(Routine());
    }

    internal static void DestroyAfter(this GameObject self, float delay) => self.DoAfter(() => UnityEngine.Object.Destroy(self), delay);

    internal static void DoAfter(this MonoBehaviour self, Action action, float delay) => self.gameObject.DoAfter(action, delay);
}
