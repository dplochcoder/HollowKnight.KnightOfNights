using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class ProximityFader : MonoBehaviour
{
    [ShimField] public Color ActiveColor;
    [ShimField] public Color InactiveColor;
    [ShimField] public float TransitionDuration;
    [ShimField] public float ActiveDistance;
    [ShimField] public float InactivateDistance;

    private SpriteRenderer? renderer;
    private float fadeProgress;

    private void Awake() => renderer = GetComponent<SpriteRenderer>();

    private void OnEnable() => this.StartLibCoroutine(Routine());

    private bool show;

    private void Update()
    {
        if (renderer == null) return;

        fadeProgress.AdvanceFloatAbs(Time.deltaTime / TransitionDuration, show ? 1 : 0);
        renderer.color = Color.Lerp(InactiveColor, ActiveColor, fadeProgress);
    }

    private IEnumerator<CoroutineElement> Routine()
    {
        var activeSquared = ActiveDistance * ActiveDistance;
        var inactiveSquared = InactivateDistance * InactivateDistance;

        var heroTransform = HeroController.instance.transform;

        while (true)
        {
            show = false;
            yield return Coroutines.SleepUntil(() => (heroTransform.position - transform.position).sqrMagnitude <= activeSquared);

            show = true;
            yield return Coroutines.SleepUntil(() => (heroTransform.position - transform.position).sqrMagnitude > inactiveSquared);
        }
    }
}
