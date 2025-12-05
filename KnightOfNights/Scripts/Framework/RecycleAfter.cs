using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class RecycleAfter : MonoBehaviour
{
    [ShimField] public float Seconds;

    private float timer;

    private void OnEnable() => timer = 0;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= Seconds) gameObject.Recycle();
    }
}
