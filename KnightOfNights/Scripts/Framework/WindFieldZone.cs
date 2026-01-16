using KnightOfNights.Scripts.SharedLib;
using KnightOfNights.Util;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

internal delegate bool WindFieldZoneCallback(Vector2 pos, out Vector2 windSpeed);

internal abstract class WindFieldZone : MonoBehaviour
{
    [ShimField] public int Priority;

    internal abstract IEnumerable<(PurenailCore.CollectionUtil.Rect, WindFieldZoneCallback)> GetCallbacks();    
}

[RequireComponent(typeof(Collider2D))]
internal abstract class ColliderWindFieldZone : WindFieldZone
{
    protected abstract bool GetWindSpeed(Vector2 pos, out Vector2 windSpeed);

    internal override IEnumerable<(PurenailCore.CollectionUtil.Rect, WindFieldZoneCallback)> GetCallbacks()
    {
        foreach (var collider in GetComponentsInChildren<Collider2D>())
        {
            var containmentFunc = collider.QuantizedContainmentTest(0.25f);
            bool Callback(Vector2 pos, out Vector2 windSpeed)
            {
                windSpeed = Vector2.zero;
                return containmentFunc(pos) && GetWindSpeed(pos, out windSpeed);
            }

            yield return (new(collider.bounds), Callback);
        }
    }
}

[Shim]
internal class ConstantWindFieldZone : ColliderWindFieldZone
{
    [ShimField] public Vector2 WindSpeed;

    protected override bool GetWindSpeed(Vector2 pos, out Vector2 windSpeed)
    {
        windSpeed = WindSpeed;
        return true;
    }
}

[Shim]
internal class GradientWindFieldZone : ColliderWindFieldZone
{
    [ShimField] public Transform? PointA;
    [ShimField] public Vector2 SpeedA;
    [ShimField] public Transform? PointB;
    [ShimField] public Vector2 SpeedB;

    protected override bool GetWindSpeed(Vector2 pos, out Vector2 windSpeed)
    {
        if (PointA == null || PointB == null)
        {
            windSpeed = Vector2.zero;
            return false;
        }

        windSpeed = Vector2.Lerp(SpeedA, SpeedB, MathExt.ILerp(pos, PointA.position, PointB.position));
        return true;
    }
}
