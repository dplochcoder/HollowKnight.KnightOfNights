using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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
    private readonly HashSet<GameObject> active = [];
    private readonly HashSet<GameObject> inactiveOneFrame = [];
    private readonly Queue<GameObject> inactive = [];

    private readonly GameObject prefab = KnightOfNightsPreloader.Instance.ElderHuPancake!;

    private GameObject SpawnNew(Vector2 pos)
    {
        var obj = Instantiate(prefab, pos, Quaternion.identity);

        var fsm = obj.LocateMyFSM("Control");

        fsm.GetFsmState("Check Pos").ClearActions();

        // Immediately contract.
        // fsm.GetFsmState("Antic").AddLastAction(new Lambda(() => fsm.SendEvent("FINISHED")));

        var antic2State = fsm.GetFsmState("Antic 2");
        antic2State.ClearTransitions();
        antic2State.AddFsmTransition("FIRE", "Down");

        var fire = fsm.AddFsmBool("Fire", false);
        var playSound = fsm.AddFsmBool("PlaySound", true);
        antic2State.AddLastAction(new LambdaEveryFrame(() =>
        {
            if (!fire.Value) return;

            fire.Value = false;
            fsm.SendEvent("FIRE");
        }));

        fsm.GetFsmState("Land").AddFirstAction(new Lambda(() =>
        {
            if (!playSound.Value) return;
            KnightOfNightsPreloader.Instance.ElderHuImpactClip?.PlayAtPosition(new(HeroController.instance.transform.position.x, fsm.gameObject.transform.position.y));
        }));

        return obj;
    }

    // Invoke the returned action to fire the pancake.
    public Pancake SpawnPancake(Vector3 pos, float launchPitch, float speed, float minY, bool playSound)
    {
        if (playSound)
            KnightOfNightsPreloader.Instance.MageShotClip?.PlayAtPosition(new(HeroController.instance.transform.position.x, pos.y), launchPitch);

        var obj = inactive.Count > 0 ? inactive.Dequeue() : SpawnNew(pos);

        active.Add(obj);
        obj.transform.position = pos;

        obj.SetActive(true);

        var fsm = obj.LocateMyFSM("Control");

        var downState = fsm.GetFsmState("Down");
        downState.GetFirstActionOfType<FloatCompare>().float2 = minY + 4.01f;
        downState.GetFirstActionOfType<SetVelocity2d>().y = -speed;

        fsm.GetFsmState("Land").GetFirstActionOfType<SetPosition>().y = minY + 3.72f;
        fsm.FsmVariables.GetFsmBool("PlaySound").Value = playSound;

        return new(obj.LocateMyFSM("Control").FsmVariables.GetFsmBool("Fire"));
    }

    private void Update()
    {
        foreach (var obj in inactiveOneFrame) inactive.Enqueue(obj);
        inactiveOneFrame.Clear();

        foreach (var obj in active) if (!obj.activeSelf) inactiveOneFrame.Add(obj);
        foreach (var obj in inactiveOneFrame) active.Remove(obj);
    }
}
