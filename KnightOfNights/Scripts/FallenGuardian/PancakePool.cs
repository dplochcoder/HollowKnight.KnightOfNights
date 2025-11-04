using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal class PancakePool : MonoBehaviour
{
    private HashSet<GameObject> active = [];
    private HashSet<GameObject> inactiveOneFrame = [];
    private Queue<GameObject> inactive = [];

    private readonly GameObject prefab = KnightOfNightsPreloader.Instance.ElderHuPancake!;

    private GameObject SpawnNew(Vector2 pos) => Instantiate(prefab, pos, Quaternion.identity);

    public void SpawnPancake(Vector2 pos)
    {
        var obj = inactive.Count > 0 ? inactive.Dequeue() : SpawnNew(pos);

        active.Add(obj);
        obj.SetActive(true);
    }

    private void Update()
    {
        foreach (var obj in inactiveOneFrame) inactive.Enqueue(obj);
        inactiveOneFrame.Clear();

        foreach (var obj in active) if (!obj.activeSelf) inactiveOneFrame.Add(obj);
        foreach (var obj in inactiveOneFrame) active.Remove(obj);
    }
}
