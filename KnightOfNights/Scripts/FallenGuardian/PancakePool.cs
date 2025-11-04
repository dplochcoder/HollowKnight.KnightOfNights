using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.InternalLib;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal class Pancake(FsmBool fire)
{
    internal void Fire() => fire.Value = true;
}

internal class PancakePool : MonoBehaviour
{
    private HashSet<GameObject> active = [];
    private HashSet<GameObject> inactiveOneFrame = [];
    private Queue<GameObject> inactive = [];

    private readonly GameObject prefab = KnightOfNightsPreloader.Instance.ElderHuPancake!;

    private GameObject SpawnNew(Vector2 pos)
    {
        var obj = Instantiate(prefab, pos, Quaternion.identity);

        var fsm = obj.LocateMyFSM("Control");

        fsm.GetFsmState("Check Pos").ClearActions();

        var antic2State = fsm.GetFsmState("Antic 2");
        antic2State.ClearTransitions();
        antic2State.AddFsmTransition("FIRE", "Down");

        var fire = fsm.AddFsmBool("Fire", false);
        antic2State.AddLastAction(new LambdaEveryFrame(() =>
        {
            if (fire.Value)
            {
                fire.Value = false;
                fsm.SendEvent("FIRE");
            }
        }));

        fsm.GetFsmState("Land").AddFirstAction(new Lambda(() =>
        {
            if (landClipTimer > 0) return;

            KnightOfNightsPreloader.Instance.ElderHuImpactClip?.PlayAtPosition(new(HeroController.instance.transform.position.x, fsm.gameObject.transform.position.y));
            landClipTimer = 0.1f;
        }));

        return obj;
    }

    // Invoke the returned action to fire the pancake.
    public Pancake SpawnPancake(Vector2 pos, float pitch)
    {
        if (spawnClipTimer == 0)
        {
            KnightOfNightsPreloader.Instance.MageShotClip?.PlayAtPosition(new(HeroController.instance.transform.position.x, pos.y), pitch);
            spawnClipTimer = 0.1f;
        }

        var obj = inactive.Count > 0 ? inactive.Dequeue() : SpawnNew(pos);

        active.Add(obj);
        obj.SetActive(true);

        return new(obj.LocateMyFSM("Control").FsmVariables.GetFsmBool("Fire"));
    }

    private float spawnClipTimer;
    private float landClipTimer;

    private void Update()
    {
        spawnClipTimer -= Time.deltaTime;
        if (spawnClipTimer < 0) spawnClipTimer = 0;
        landClipTimer -= Time.deltaTime;
        if (landClipTimer < 0) landClipTimer = 0;

        foreach (var obj in inactiveOneFrame) inactive.Enqueue(obj);
        inactiveOneFrame.Clear();

        foreach (var obj in active) if (!obj.activeSelf) inactiveOneFrame.Add(obj);
        foreach (var obj in inactiveOneFrame) active.Remove(obj);
    }
}
