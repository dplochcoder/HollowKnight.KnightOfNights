using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class XFixer : MonoBehaviour
{
    private float targetX;
    private float ratioPerSecond;

    internal void Reset(float targetX, float ratioPerSecond)
    {
        this.targetX = targetX;
        this.ratioPerSecond = ratioPerSecond;
        enabled = true;
    }

    private void FixedUpdate()
    {
        var pos = transform.position;
        pos.x += (targetX - pos.x) * ratioPerSecond * Time.fixedDeltaTime;
        transform.Translate(pos - transform.position, Space.World);
    }
}
