using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.CollectionUtil;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal enum SlashAttackResult
{
    PARRIED,
    NOT_PARRIED,
}

internal class SlashAttack(SlashAttackSpec spec, PlayMakerFSM fsm)
{
    public SlashAttackSpec Spec => spec;
    public SlashAttackResult? Result { get; private set; }
    public Vector2 ParryPos { get; private set; }
    public int DamageDealt { get; private set; }
    public float? DamageDirection { get; private set; }
    public float? MagnitudeMultiplier { get; private set; }

    internal event System.Action<SlashAttackResult>? OnResult;

    private bool cancelled = false;
    private ParticleClock? clock;
    private RevekAddons? revekAddons;

    public static SlashAttack Spawn(SlashAttackSpec spec)
    {
        var revek = Object.Instantiate(KnightOfNightsPreloader.Instance.Revek!);
        revek.transform.position = new(-100, -100);

        SlashAttack attack = new(spec, revek.LocateMyFSM("Control"));
        attack.SpawnImpl(spec);
        return attack;
    }

    private const float ANIM_TIME = 0.1f;
    private const float CIRCLE_TIME = 0.4f;
    private const float FADE_TIME = 0.4f;

    private void SpawnImpl(SlashAttackSpec spec)
    {
        var revek = fsm.gameObject;
        revekAddons = revek.AddComponent<RevekAddons>();
        revekAddons.DirectionFilter = dir =>
        {
            dir = MathExt.ClampAngle(dir, -45, 315);
            if (dir <= 45) return spec.AllowedHits.Contains("RIGHT");
            else if (dir <= 135) return spec.AllowedHits.Contains("UP");
            else if (dir <= 225) return spec.AllowedHits.Contains("LEFT");
            else return spec.AllowedHits.Contains("DOWN");
        };
        revekAddons.OnParry += hit =>
        {
            DamageDealt = hit.DamageDealt;
            DamageDirection = hit.Direction;
            MagnitudeMultiplier = hit.MagnitudeMultiplier;
        };

        fsm.FsmVariables.GetFsmFloat("Speed").Value = spec.Speed;

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
        else revek.DoOnAwake(() => revek.DoAfter(SpawnClock, timeToStrike - clockDuration));

        fsm.Fsm.GlobalTransitions = [];
        foreach (var state in fsm.FsmStates) state.RemoveTransitionsOn("TAKE DAMAGE");

        var slashTeleInState = fsm.GetState("Slash Tele In");

        var initState = fsm.GetState("Init");
        initState.ClearTransitions();
        initState.AddTransition("FINISHED", "Slash Tele In");

        GameObject audioSrc = new("RevekAudioSource");
        audioSrc.transform.parent = HeroController.instance.transform;
        slashTeleInState.GetFirstActionOfType<AudioPlayerOneShotSingle>().spawnPoint = audioSrc;

        slashTeleInState.AddFirstAction(new Lambda(() =>
        {
            audioSrc.transform.localPosition = spec.SpawnOffset;
            fsm.FsmVariables.GetFsmFloat("X Distance").Value = spec.SpawnOffset.x;
            fsm.FsmVariables.GetFsmFloat("Y Distance").Value = spec.SpawnOffset.y;
        }));

        var idleWait = fsm.GetState("Slash Idle").GetFirstActionOfType<WaitRandom>();
        idleWait.timeMin = spec.Telegraph;
        idleWait.timeMax = spec.Telegraph;

        var slashHit = revek.FindChild("Slash Hit")!;
        slashHit.DoOnAwake(() =>
        {
            var tink = slashHit.LocateMyFSM("nail_clash_tink");
            var blockedHitState = tink.GetState("Blocked Hit");
            blockedHitState.AddTransition("ABORT", "Detecting");

            // Move freeze moment and Nail parry calls to after direction validation.
            var actions = blockedHitState.Actions;
            (actions[0], actions[1], actions[2], actions[3]) = (actions[2], actions[3], actions[0], actions[1]);
            actions[2].Enabled = false;  // Disable FreezeMoment.
            // Prevent parries from disallowed directions.
            blockedHitState.InsertAction(new Lambda(() =>
            {
                var dir = tink.FsmVariables.GetFsmFloat("Attack Direction").Value;
                if (!revekAddons.DirectionFilter(dir)) tink.SendEvent("ABORT");
                else
                {
                    var damagesEnemy = tink.FsmVariables.GetFsmGameObject("Slash").Value.LocateMyFSM("damages_enemy");
                    DamageDealt = damagesEnemy.FsmVariables.GetFsmInt("damageDealt").Value;
                    DamageDirection = damagesEnemy.FsmVariables.GetFsmFloat("direction").Value;
                    MagnitudeMultiplier = damagesEnemy.FsmVariables.GetFsmFloat("magnitudeMult").Value;
                }
            }), 2);
        });

        var slashState = fsm.GetState("Slash");
        slashState.AddTransition("PARRIED", "Hit");
        slashState.GetFirstActionOfType<FireAtTarget>().position.Value = spec.TargetOffset;
        slashState.GetFirstActionOfType<DecelerateV2>().deceleration.Value = spec.Deceleration;

        fsm.GetState("Slash Tele Out").AddFirstAction(new Lambda(() => SetResult(SlashAttackResult.NOT_PARRIED)));
        fsm.GetState("Damaged Pause").AddFirstAction(new Lambda(() => fsm.gameObject.DestroyAfter(3f)));

        var attackPause = fsm.GetState("Attack Pause");
        var attackWait = attackPause.GetFirstActionOfType<WaitRandom>();
        attackWait.timeMin = 5;
        attackWait.timeMax = 5;
        attackPause.AddFirstAction(new Lambda(() => fsm.gameObject.DestroyAfter(3f)));

        var hitState = fsm.GetState("Hit");
        hitState.AddFirstAction(new Lambda(() =>
        {
            ParryPos = fsm.gameObject.transform.position;
            SetResult(SlashAttackResult.PARRIED);
        }));
        hitState.GetFirstActionOfType<AudioPlayerOneShot>().Enabled = false;

        revek.SetActive(true);
    }

    internal void Cancel()
    {
        SetResult(SlashAttackResult.NOT_PARRIED);

        void TeleOut()
        {
            clock?.Cancel();
            cancelled = true;

            fsm.GetState("Slash Tele Out").AddLastAction(new LambdaEveryFrame(() =>
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
        else fsm.GetState("Slash Idle").AddFirstAction(new Lambda(TeleOut));
    }

    private void SetResult(SlashAttackResult result)
    {
        if (Result.HasValue) return;

        Result = result;
        OnResult?.Invoke(result);
    }
}
