using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.CollectionUtil;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal enum SlashAttackResult
{
    PENDING,
    PARRIED,
    NOT_PARRIED,
}

internal class SlashAttack(PlayMakerFSM fsm)
{
    public SlashAttackResult Result { get; private set; }
    public Vector2 ParryPos { get; private set; }

    private bool cancelled = false;
    private ParticleClock? clock;

    public static SlashAttack Spawn(SlashAttackSpec spec)
    {
        var revek = Object.Instantiate(KnightOfNightsPreloader.Instance.Revek!);
        revek.transform.position = new(-100, -100);

        SlashAttack attack = new(revek.LocateMyFSM("Control"));
        attack.SpawnImpl(spec);
        return attack;
    }

    private const float ANIM_TIME = 0.1f;
    private const float CIRCLE_TIME = 0.4f;
    private const float FADE_TIME = 0.4f;

    private void SpawnImpl(SlashAttackSpec spec)
    {
        var revek = fsm.gameObject;
        revek.AddComponent<RevekAddons>();

        var timeToStrike = (5f / 18f) + spec.Telegraph;
        float clockDuration = ANIM_TIME + CIRCLE_TIME;
        Wrapped<float> compress = new(1);

        void SpawnClock()
        {
            if (cancelled) return;
            clock = ParticleClock.Spawn(revek.transform, compress.Value * ANIM_TIME, compress.Value * CIRCLE_TIME, compress.Value * FADE_TIME);
        }

        if (clockDuration >= timeToStrike)
        {
            compress.Value = timeToStrike / clockDuration;
            SpawnClock();
        }
        else revek.DoAfter(SpawnClock, timeToStrike - clockDuration);

        fsm.Fsm.GlobalTransitions = [];
        foreach (var state in fsm.FsmStates) state.RemoveTransitionsOn("TAKE DAMAGE");

        var slashTeleInState = fsm.GetFsmState("Slash Tele In");

        var initState = fsm.GetFsmState("Init");
        initState.ClearTransitions();
        initState.AddFsmTransition("FINISHED", "Slash Tele In");

        GameObject audioSrc = new("RevekAudioSource");
        audioSrc.transform.parent = HeroController.instance.transform;
        slashTeleInState.GetFirstActionOfType<AudioPlayerOneShotSingle>().spawnPoint = audioSrc;

        slashTeleInState.AddFirstAction(new Lambda(() =>
        {
            audioSrc.transform.localPosition = spec.SpawnOffset;
            fsm.FsmVariables.GetFsmFloat("X Distance").Value = spec.SpawnOffset.x;
            fsm.FsmVariables.GetFsmFloat("Y Distance").Value = spec.SpawnOffset.y;
        }));

        var idleWait = fsm.GetFsmState("Slash Idle").GetFirstActionOfType<WaitRandom>();
        idleWait.timeMin = spec.Telegraph;
        idleWait.timeMax = spec.Telegraph;

        var slashState = fsm.GetFsmState("Slash");
        slashState.AddFsmTransition("PARRIED", "Hit");
        slashState.GetFirstActionOfType<FireAtTarget>().position.Value = spec.TargetOffset;
        slashState.GetFirstActionOfType<DecelerateV2>().deceleration.Value = spec.Deceleration;

        fsm.GetFsmState("Slash Tele Out").AddFirstAction(new Lambda(() => Result = SlashAttackResult.NOT_PARRIED));
        fsm.GetFsmState("Damaged Pause").AddFirstAction(new Lambda(() => fsm.gameObject.DestroyAfter(3f)));

        var attackPause = fsm.GetFsmState("Attack Pause");
        var attackWait = attackPause.GetFirstActionOfType<WaitRandom>();
        attackWait.timeMin = 5;
        attackWait.timeMax = 5;
        attackPause.AddFirstAction(new Lambda(() => fsm.gameObject.DestroyAfter(3f)));

        var hitState = fsm.GetFsmState("Hit");
        hitState.AddFirstAction(new Lambda(() =>
        {
            ParryPos = fsm.gameObject.transform.position;
            Result = SlashAttackResult.PARRIED;
        }));
        hitState.GetFirstActionOfType<AudioPlayerOneShot>().Enabled = false;

        revek.SetActive(true);
    }

    internal void Cancel()
    {
        Result = SlashAttackResult.NOT_PARRIED;

        void TeleOut()
        {
            clock?.Cancel();
            cancelled = true;

            fsm.GetFsmState("Slash Tele Out").AddLastAction(new LambdaEveryFrame(() =>
            {
                // Fix clip fighting.
                var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
                if (animator.CurrentClip.name != "Tele Out") animator.Play("Tele Out");
            }));
            fsm.SetState("Slash Tele Out");

            var emission = fsm.FsmVariables.GetFsmGameObject("Idle Pt").Value.GetComponent<ParticleSystem>().emission;
            emission.enabled = false;
        }

        if (fsm.ActiveStateName == "Slash Idle" || fsm.ActiveStateName == "Slash Antic") TeleOut();
        else fsm.GetFsmState("Slash Idle").AddFirstAction(new Lambda(TeleOut));
    }
}
