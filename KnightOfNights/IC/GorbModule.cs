﻿using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.SharedLib;
using KnightOfNights.Util;
using PurenailCore.CollectionUtil;
using PurenailCore.SystemUtil;
using SFCore.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class GorbModule : AbstractGhostWarriorModule<GorbModule>
{
    protected override FsmID FsmID() => new("Ghost Warrior Slug", "Attacking");

    protected override float HPBoost() => 2.5f;

    private void SpawnSpears(PlayMakerFSM src, GameObject template, float baseAngle, int count, float speedup, AudioPlayerOneShotSingle audio, float pitch)
    {
        var (prevMin, prevMax) = (audio.pitchMin, audio.pitchMax);
        (audio.pitchMin, audio.pitchMax) = (pitch, pitch);
        audio.DoPlayRandomClip();
        (audio.pitchMin, audio.pitchMax) = (prevMin, prevMax);

        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + (i * 360f) / count;

            var obj = template.Spawn(src.gameObject.transform.position, Quaternion.Euler(0, 0, angle));

            var control = obj.LocateMyFSM("Control");
            var poke = control.GetFsmState("Poke Out");
            poke.GetFirstActionOfType<SetVelocityAsAngle>().speed = 30 * speedup;
            poke.GetFirstActionOfType<DecelerateV2>().deceleration = 0.88f * Mathf.Pow(speedup, 0.02f);
            poke.GetFirstActionOfType<Wait>().time = 0.5f / speedup;
            control.GetFsmState("Fire").GetFirstActionOfType<SetVelocityAsAngle>().speed = 25 * speedup;

            control.GetFsmState("Recycle").AddFirstAction(new Lambda(() =>
            {
                poke.GetFirstActionOfType<SetVelocityAsAngle>().speed = 30;
                poke.GetFirstActionOfType<DecelerateV2>().deceleration = 0.88f;
                poke.GetFirstActionOfType<Wait>().time = 0.5f;
                control.GetFsmState("Fire").GetFirstActionOfType<SetVelocityAsAngle>().speed = 25;
            }));

            obj.SetActive(true);
        }
    }


    protected override void ModifyGhostWarrior(PlayMakerFSM fsm, Wrapped<int> baseHp)
    {
        void SetSpeeds(float waitMin, float waitMax, float damagedWait, float attackSpeedup, float recoverWait)
        {
            var wait = fsm.GetFsmState("Wait").GetFirstActionOfType<WaitRandom>();
            wait.timeMin = waitMin;
            wait.timeMax = waitMax;

            fsm.GetFsmState("Damaged").GetFirstActionOfType<Wait>().time = damagedWait;
            fsm.GetFsmState("Recover").GetFirstActionOfType<Wait>().time = recoverWait;

            var accel = fsm.gameObject.GetOrAddComponentSharedLib<AnimationAccelerator>();
            fsm.GetFsmState("Antic").AccelerateAnimation(accel, attackSpeedup);
            fsm.GetFsmState("Attack").AccelerateAnimation(accel, attackSpeedup);
        }

        var movementFsm = fsm.gameObject.LocateMyFSM("Movement");
        void SetMoveSpeeds(float speed, float accel, float warpChance, float warpDelay)
        {
            var chase = movementFsm.GetFsmState("Hover").GetFirstActionOfType<ChaseObject>();
            chase.speedMax = speed;
            chase.acceleration = accel;

            var warpState = movementFsm.GetFsmState("Warp Check");
            warpState.GetFirstActionOfType<SendRandomEvent>().weights = [1 - warpChance, warpChance];
            warpState.GetFirstActionOfType<Wait>().time = warpDelay;
        }

        var attackState = fsm.GetFsmState("Attack");
        attackState.ClearTransitions();
        attackState.AddFsmTransition("DONE", "Recover");
        attackState.GetFirstActionOfType<Tk2dPlayAnimationWithEvents>().animationCompleteEvent = FsmEvent.GetFsmEvent("");
        var spear = attackState.GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
        attackState.Actions = [.. attackState.Actions.Take(5)];
        attackState.AddLastAction(new LambdaEveryFrame(() =>
        {
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            if (!animator.Playing) animator.Play("Idle");
        }));

        Wrapped<bool> phase2 = new(false);
        Wrapped<bool> phase3 = new(false);
        Wrapped<bool> phase4 = new(false);
        fsm.GetFsmState("Wait").AddFirstAction(new Lambda(() =>
        {
            if (UpdatePhase(fsm, baseHp, phase2, 0.8f))
            {
                SetSpeeds(0.75f, 1.75f, 0.25f, 1.1f, 0.5f);
                SetMoveSpeeds(5f, 0.35f, 0.25f, 0.25f);
            }
            else if (UpdatePhase(fsm, baseHp, phase3, 0.55f))
            {
                SetSpeeds(0.75f, 1.25f, 0.2f, 1.3f, 0.35f);
                SetMoveSpeeds(5.5f, 0.35f, 0.3f, 0.2f);
            }
            else if (UpdatePhase(fsm, baseHp, phase4, 0.35f))
            {
                SetSpeeds(0.7f, 1f, 0.2f, 1.5f, 0.25f);
                SetMoveSpeeds(6.5f, 0.4f, 0.45f, 0.15f);
            }
        }));

        var audio = fsm.GetFsmState("Double").GetFirstActionOfType<AudioPlayerOneShotSingle>();

        float Offset(int count, int reps) => (360f / reps) / count;
        attackState.AddLastAction(new Lambda(() =>
        {
            if (!phase2.Value)
            {
                IEnumerator Attack()
                {
                    SpawnSpears(fsm, spear, Random.Range(0, 360), Random.Range(0, 4) == 0 ? 12 : 8, 1f, audio, 1f);
                    yield return new WaitForSeconds(0.25f);
                    fsm.SendEvent("DONE");
                }
                fsm.StartCoroutine(Attack());
            }
            else if (!phase3.Value)
            {
                IEnumerator Attack()
                {
                    float angle = Random.Range(0, 360);
                    SpawnSpears(fsm, spear, angle, 8, 1.05f, audio, 1f);
                    yield return new WaitForSeconds(0.5f);
                    SpawnSpears(fsm, spear, angle + Offset(8, 2), 8, 1.05f, audio, 1.2f);
                    if (Random.Range(0, 3) == 0)
                    {
                        yield return new WaitForSeconds(0.25f);
                        SpawnSpears(fsm, spear, angle, 8, 1.05f, audio, 1.4f);
                    }
                    fsm.SendEvent("DONE");
                }
                fsm.StartCoroutine(Attack());
            }
            else if (!phase4.Value)
            {
                IEnumerator Attack()
                {
                    float angle = Random.Range(0, 360);
                    if (Random.Range(0, 3) == 0)
                    {
                        SpawnSpears(fsm, spear, angle, 16, 1.1f, audio, 1.1f);
                        yield return new WaitForSeconds(0.225f);
                    }
                    else
                    {
                        int count = Random.Range(0, 2) == 0 ? 8 : 10;
                        float off1, off2;
                        if (Random.Range(0, 3) == 0)
                        {
                            off1 = Offset(count, 3);
                            if (Random.Range(0, 2) == 0) off1 *= -1;
                            off2 = off1 * 2;
                        }
                        else
                        {
                            off1 = Offset(count, 2);
                            off2 = 0;
                        }

                        SpawnSpears(fsm, spear, angle, count, 1.15f, audio, 1f);
                        yield return new WaitForSeconds(0.4f);
                        SpawnSpears(fsm, spear, angle + off1, count, 1.15f, audio, 1.2f);
                        yield return new WaitForSeconds(0.2f);
                        SpawnSpears(fsm, spear, angle + off2, count, 1.15f, audio, 1.4f);
                    }
                    fsm.SendEvent("DONE");
                }
                fsm.StartCoroutine(Attack());
            }
            else
            {
                IEnumerator Attack()
                {
                    float angle = Random.Range(0, 360);
                    if (Random.Range(0, 2) == 0)
                    {
                        int count = Random.Range(0, 3) == 0 ? 16 : 12;
                        SpawnSpears(fsm, spear, angle, count, 1.15f, audio, 1.1f);
                        yield return new WaitForSeconds(0.35f);
                        SpawnSpears(fsm, spear, angle + Offset(count, 2), count, 1.15f, audio, 1.1f);
                    }
                    else
                    {
                        int count = Random.Range(7, 9);
                        int len = Random.Range(0, 3) == 0 ? 4 : 5;
                        List<float> offs = [];
                        for (int i = 0; i < len; i++) offs.Add(Offset(count, len) * i);
                        offs.Shuffle(new());

                        for (int i = 0; i < len; i++)
                        {
                            SpawnSpears(fsm, spear, angle + offs[i], count, 1.2f, audio, 1 + (i * 0.1f));
                            yield return new WaitForSeconds(0.7f / len);
                        }
                    }
                    fsm.SendEvent("DONE");
                }
                fsm.StartCoroutine(Attack());
            }
        }));
    }

    protected override GorbModule Self() => this;
}
