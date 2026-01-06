using KnightOfNights.Scripts.SharedLib;
using PurenailCore.GOUtil;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class BreakableFader : MonoBehaviour
{
    [ShimField] public float MinDelay;
    [ShimField] public float MaxDelay;
    [ShimField] public float FadeTime;

    private readonly List<SpriteRenderer> spriteRenderers = [];
    private float elapsed;
    private float target;

    private void Awake() => spriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>(true));

    private void OnEnable()
    {
        elapsed = 0;
        target = Random.Range(MinDelay, MaxDelay);
        spriteRenderers.ForEach(sr => sr.SetAlpha(1));
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= target + FadeTime)
        {
            gameObject.SetActive(false);
            return;
        }
        if (elapsed <= target) return;

        float progress = MathExt.Clamp((elapsed - target) / FadeTime, 0, 1);
        float alpha = 1 - progress;
        spriteRenderers.ForEach(sr => sr.SetAlpha(alpha));
    }
}
