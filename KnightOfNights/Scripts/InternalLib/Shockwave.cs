using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal static class Shockwave
{
    static Shockwave() => KnightOfNightsPreloader.Instance.Shockwave?.FixSpawnBug();

    public static GameObject SpawnOne(Vector2 pos, Vector2 scale, bool right, float speed)
    {
        var obj = KnightOfNightsPreloader.Instance.Shockwave!.Spawn(pos);
        obj.transform.localScale = scale;
        obj.SetActive(true);

        var fsm = obj.LocateMyFSM("shockwave");
        fsm.FsmVariables.GetFsmBool("Facing Right").Value = right;
        fsm.FsmVariables.GetFsmFloat("Speed").Value = speed;

        return obj;
    }

    public static List<GameObject> SpawnTwo(Vector2 pos, Vector2 scale, float speed) => [SpawnOne(pos, scale, false, speed), SpawnOne(pos, scale, true, speed)];
}
