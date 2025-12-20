using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.Proxy;
using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.Summit;

[Shim]
internal class CharmBarrier : MonoBehaviour
{
    [ShimField] public GameObject? RespawnMarker;
    [ShimField] public HeroDetectorProxy? Trigger;

    private void OnEnable() => this.StartLibCoroutine(Routine());

    private IEnumerator<CoroutineElement> Routine()
    {
        // TODO
        yield break;
    }
}
