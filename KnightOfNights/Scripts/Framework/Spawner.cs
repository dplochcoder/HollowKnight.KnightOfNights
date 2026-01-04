using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class Spawner : MonoBehaviour
{
    [ShimField] public GameObject? Prefab;

    private float delay;

    private void OnEnable()
    {
        Prefab?.Spawn(transform.position, transform.rotation);
        delay = 0;
    }

    private void Update()
    {
        delay += Time.deltaTime;
        if (delay > 10f) gameObject.Recycle();
    }
}
