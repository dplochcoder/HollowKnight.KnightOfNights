using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.SharedLib;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal class MarkothNail(PlayMakerFSM fsm)
{
    internal static MarkothNail Spawn(ShieldCycloneStats stats, BoxCollider2D arena)
    {
        var obj = Object.Instantiate(KnightOfNightsPreloader.Instance.MarkothNail!);

        var fsm = obj.LocateMyFSM("Control");

        var initState = fsm.GetFsmState("Init");
        initState.ClearActions();
        initState.ClearTransitions();
        initState.AddFsmTransition("FINISHED", "Antic Point");
        initState.AddFirstAction(new Lambda(() =>
        {
            fsm.FsmVariables.GetFsmGameObject("Self").Value = fsm.gameObject;
            fsm.gameObject.transform.position = SelectSpawn(stats, arena);
            fsm.SendEvent("FINISHED");
        }));

        var recycleState = fsm.GetFsmState("Recycle");
        recycleState.ClearActions();
        recycleState.AddFirstAction(new Lambda(() => Object.Destroy(obj)));

        obj.SetActive(true);
        return new(fsm);
    }

    internal void Despawn() => fsm.SendEvent("GHOST DEATH");

    private static Vector3 SelectSpawn(ShieldCycloneStats stats, BoxCollider2D arena)
    {
        var b = arena.bounds;
        var kPos = HeroController.instance.transform.position;
        kPos.x = MathExt.Clamp(kPos.x, b.min.x, b.max.x);
        kPos.y = MathExt.Clamp(kPos.y, b.min.y, b.max.y);

        var minX = Mathf.Max(b.min.x, kPos.x - stats.DaggerMaxDistance);
        var maxX = Mathf.Min(b.max.x, kPos.x + stats.DaggerMaxDistance);
        var minY = Mathf.Max(b.min.y, kPos.y - stats.DaggerMaxDistance);
        var maxY = Mathf.Min(b.max.y, kPos.y + stats.DaggerMaxDistance);

        while (true)
        {
            Vector3 pos = new(Random.Range(minX, maxX), Random.Range(minY, maxY));
            var dist = (kPos - pos).magnitude;
            if (dist < stats.DaggerMinDistance || dist > stats.DaggerMaxDistance) continue;

            return pos;
        }
    }
}
