using KnightOfNights.Scripts.Proxy;
using KnightOfNights.Scripts.SharedLib;
using System.Collections;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class FallenGuardianContainer : MonoBehaviour
{
    [ShimField] public HeroDetectorProxy? Trigger;
    [ShimField] public FallenGuardianController? Boss;
    [ShimField] public BoxCollider2D? Arena;

    private void OnEnable() => StartCoroutine(Run());

    private IEnumerator Run()
    {
        yield return new WaitUntil(() => Trigger!.Detected());
        // FIXME: Music

        Boss!.gameObject.SetActive(true);
        // FIXME: Camera, hazards, death
    }
}
