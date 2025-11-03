using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class Bobber : MonoBehaviour
{
    private float radius;
    private float period;
    private float timer;

    internal void Reset(float radius, float period, bool upFirst = true)
    {
        this.radius = radius;
        this.period = period;
        timer = upFirst ? 0 : (period / 2);

        this.enabled = true;
    }

    internal void ResetRandom(float radius, float period) => Reset(radius, period, MathExt.CoinFlip());

    private void Update()
    {
        float y1 = ComputeY(timer);
        timer += Time.deltaTime;
        float y2 = ComputeY(timer);

        var p = transform.position;
        p.y += y2 - y1;
        transform.position = p;
    }

    private float ComputeY(float time) => radius * Mathf.Sin(2 * Mathf.PI * timer / period);
}
