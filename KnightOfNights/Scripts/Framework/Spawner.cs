using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class Spawner : MonoBehaviour
{
    [ShimField] public GameObject? Prefab;
    [ShimField] public Vector3 Offset;

    private float delay;

    private void OnEnable()
    {
        Prefab?.Spawn(transform.position + Offset, transform.rotation);
        delay = 0;
    }

    private void Update()
    {
        delay += Time.deltaTime;
        if (delay > 10f) gameObject.Recycle();
    }
}
