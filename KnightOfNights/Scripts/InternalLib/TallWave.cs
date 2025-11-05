using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal static class TallWave
{
    static TallWave() => KnightOfNightsPreloader.Instance.TraitorLordWave?.FixSpawnBug();

    public static GameObject SpawnOne(Vector3 pos, bool right, float speed)
    {
        var obj = KnightOfNightsPreloader.Instance.TraitorLordWave!.Spawn(pos with { z = 6.2f });

        obj.SetActive(true);
        obj.transform.localScale = new(right ? 1 : -1, 1, 1);
        obj.GetComponent<Rigidbody2D>().velocity = new(right ? speed : -speed, 0);

        return obj;
    }

    public static List<GameObject> SpawnTwo(Vector3 pos, float speed) => [SpawnOne(pos, false, speed), SpawnOne(pos, true, speed)];
}
