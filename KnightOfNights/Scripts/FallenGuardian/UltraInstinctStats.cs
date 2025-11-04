using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class UltraInstinctStats : MonoBehaviour
{
    [ShimField] public float Deceleration;
    [ShimField] public float Interval;
    [ShimField] public float Speed;
    [ShimField] public float Tail;
    [ShimField] public float Telegraph;
}
