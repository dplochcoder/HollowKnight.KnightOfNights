using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal static class Shockwave
{
    static Shockwave() => KnightOfNightsPreloader.Instance.Shockwave?.FixSpawnBug();


    public static void SpawnOne(Vector2 pos, Vector2 scale, bool right, float speed)
    {
        var obj = KnightOfNightsPreloader.Instance.Shockwave!.Spawn(pos);
        obj.transform.localScale = scale;
        obj.SetActive(true);

        var fsm = obj.LocateMyFSM("shockwave");
        fsm.FsmVariables.GetFsmBool("Facing Right").Value = right;
        fsm.FsmVariables.GetFsmFloat("Speed").Value = speed;
    }

    public static void SpawnTwo(Vector2 pos, Vector2 scale, float speed)
    {
        SpawnOne(pos, scale, false, speed);
        SpawnOne(pos, scale, true, speed);
    }
}
