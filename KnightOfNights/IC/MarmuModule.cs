using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using PurenailCore.CollectionUtil;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class MarmuModule : AbstractGhostWarriorModule<MarmuModule>
{
    protected override FsmID FsmID() => new("Ghost Warrior Marmu", "Control");

    protected override float HPBoost() => 2.5f;

    protected override void ModifyGhostWarrior(PlayMakerFSM fsm, Wrapped<int> baseHp)
    {
        void SetWaits(float minRoll, float maxRoll, float antic)
        {
            var anticState = fsm.GetFsmState("Antic");
            var rollTime = anticState.GetFirstActionOfType<RandomFloat>();
            rollTime.min = minRoll;
            rollTime.max = maxRoll;
            anticState.GetFirstActionOfType<Wait>().time = antic;
            fsm.GetFsmState("Unroll").GetFirstActionOfType<Wait>().time = antic;
        }

        void SetSpeedMultiplier(float multiplier)
        {
            fsm.GetFsmState("Fire").GetFirstActionOfType<SetVelocityAsAngle>().speed = 20 * multiplier;

            var chase = fsm.GetFsmState("Chase").GetFirstActionOfType<ChaseObjectV2>();
            chase.speedMax = 40 * multiplier;
            chase.accelerationForce = 85 * multiplier * Mathf.Sqrt(multiplier);

            fsm.GetFsmState("Hit Down").GetFirstActionOfType<SetVelocity2d>().y = -40 / Mathf.Sqrt(multiplier);
            fsm.GetFsmState("Hit Left").GetFirstActionOfType<SetVelocity2d>().x = -40 / Mathf.Sqrt(multiplier);
            fsm.GetFsmState("Hit Left").GetFirstActionOfType<SetVelocity2d>().y = 15 / Mathf.Sqrt(multiplier);
            fsm.GetFsmState("Hit Right").GetFirstActionOfType<SetVelocity2d>().x = 40 / Mathf.Sqrt(multiplier);
            fsm.GetFsmState("Hit Right").GetFirstActionOfType<SetVelocity2d>().y = 15 / Mathf.Sqrt(multiplier);
            fsm.GetFsmState("Hit Up").GetFirstActionOfType<SetVelocity2d>().y = 50 / Mathf.Sqrt(multiplier);

            var audio = fsm.GetFsmState("Hit Voice").GetFirstActionOfType<AudioPlayerOneShot>();
            audio.pitchMin = multiplier;
            audio.pitchMax = multiplier;
        }

        Wrapped<bool> phase2 = new(false);
        Wrapped<bool> phase3 = new(false);
        Wrapped<bool> phase4 = new(false);
        fsm.GetFsmState("Antic").AddFirstAction(new Lambda(() =>
        {
            if (UpdatePhase(fsm, baseHp, phase2, 0.8f))
            {
                SetWaits(1.25f, 3f, 0.65f);
                SetSpeedMultiplier(1.1f);
            }
            else if (UpdatePhase(fsm, baseHp, phase3, 0.6f))
            {
                SetWaits(1f, 2.5f, 0.5f);
                SetSpeedMultiplier(1.15f);
            }
            else if (UpdatePhase(fsm, baseHp, phase4, 0.4f))
            {
                SetWaits(0.8f, 2.2f, 0.35f);
                SetSpeedMultiplier(1.25f);
            }
        }));
    }

    protected override MarmuModule Self() => this;
}
