using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal record XeroNailSpec
{
    public float initTime;
    public float spinTime;
    public float pointTime;
    public float speed;
    public float shootTime;
    public float maxY;
    public float minY;
    public float decelerateTime;
    public float returnDeceleration;
    public float returnPauseDeceleration;
    public float returnPauseTime;
}

internal class XeroNail(PlayMakerFSM fsm)
{
    private bool despawned;

    internal Vector3 Position => fsm.gameObject.transform.position;

    internal static XeroNail Spawn(GameObject home, XeroNailSpec spec)
    {
        var obj = Object.Instantiate(KnightOfNightsPreloader.Instance.XeroNail!, home.transform.position, Quaternion.Euler(0, 0, -90));
        obj.SetActive(true);

        var fsm = obj.LocateMyFSM("xero_nail");

        var initState = fsm.GetFsmState("Init");
        fsm.FsmVariables.GetFsmGameObject("Home").Value = home;
        initState.RemoveFirstActionOfType<GetParent>();
        initState.RemoveFirstActionOfType<GetChild>();
        initState.GetFirstActionOfType<iTweenRotateBy>().time = spec.initTime;

        var anticSpinState = fsm.GetFsmState("Antic Spin");
        anticSpinState.GetFirstActionOfType<iTweenRotateBy>().time = spec.spinTime * 4f / 3f;
        anticSpinState.GetFirstActionOfType<Wait>().time = spec.spinTime;

        fsm.GetFsmState("Antic Point").GetFirstActionOfType<Wait>().time = spec.pointTime;

        var shootState = fsm.GetFsmState("Shoot");
        shootState.GetFirstActionOfType<Wait>().time = spec.shootTime;
        shootState.GetFirstActionOfType<FloatCompare>().float2 = spec.minY;

        var decelerateState = fsm.GetFsmState("Decelerate");
        decelerateState.GetFirstActionOfType<iTweenRotateBy>().time = spec.decelerateTime;
        decelerateState.GetFirstActionOfType<DecelerateV2>().deceleration = spec.returnDeceleration;

        var returnPauseState = fsm.GetFsmState("Return Pause");
        returnPauseState.GetFirstActionOfType<Wait>().time = spec.returnPauseTime;
        returnPauseState.GetFirstActionOfType<DecelerateV2>().deceleration = spec.returnPauseDeceleration;

        fsm.GetFsmState("Returning").GetFirstActionOfType<FloatCompare>().float2 = spec.maxY;

        return new(fsm);
    }

    internal bool Attack()
    {
        if (fsm.FsmVariables.GetFsmBool("Attacking").Value) return false;

        fsm.SendEvent("ATTACK");
        return true;
    }

    internal void Despawn()
    {
        if (despawned) return;
        despawned = true;

        fsm.SendEvent("GHOST DEAD");
        fsm.gameObject.DestroyAfter(3f);
    }
}
