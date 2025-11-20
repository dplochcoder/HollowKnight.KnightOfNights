using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class Bobber : MonoBehaviour
{
    private Rigidbody2D? rigidbody;

    private float radius;
    private float period;
    private float timer;

    private void Awake() => rigidbody = GetComponent<Rigidbody2D>();

    internal void Reset(float radius, float period, bool upFirst = true)
    {
        this.radius = radius;
        this.period = period;
        timer = upFirst ? 0 : (period / 2);

        enabled = true;
    }

    internal void ResetRandom(float radius, float period) => Reset(radius, period, MathExt.CoinFlip());

    private void Update()
    {
        float y1 = ComputeY(timer);
        timer += Time.deltaTime;
        float y2 = ComputeY(timer);

        transform.Translate(new(0, y2 - y1), Space.World);
    }

    internal float OffsetY() => ComputeY(timer);

    private float ComputeY(float time) => radius * Mathf.Sin(2 * Mathf.PI * time / period);
}
