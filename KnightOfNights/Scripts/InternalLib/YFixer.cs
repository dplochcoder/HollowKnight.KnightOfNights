using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class YFixer : MonoBehaviour
{
    private float targetY;
    private float ratioPerSecond;

    internal void Reset(float targetY, float ratioPerSecond)
    {
        this.targetY = targetY;
        this.ratioPerSecond = ratioPerSecond;
        enabled = true;
    }

    private void FixedUpdate()
    {
        var target = targetY;
        if (gameObject.TryGetComponent<Bobber>(out var bobber) && bobber.enabled) target += bobber.OffsetY();

        var pos = transform.position;
        pos.y += (targetY - pos.y) * ratioPerSecond * Time.fixedDeltaTime;
        transform.Translate(pos - transform.position, Space.World);
    }
}
