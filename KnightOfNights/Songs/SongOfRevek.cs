using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Internal;
using KnightOfNights.IC;
using PurenailCore.CollectionUtil;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Songs;

internal class RevekFixes : MonoBehaviour, IHitResponder
{
    private MeshRenderer? renderer;
    private PlayMakerFSM? fsm;

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        fsm = gameObject.LocateMyFSM("Control");
    }

    private void LateUpdate()
    {
        renderer!.sortingLayerName = "Over";
        renderer!.sortingOrder = 1;
    }

    private static readonly HashSet<string> VULNERABLE_STATES = ["Slash Idle", "Slash Antic", "Slash"];

    public void Hit(HitInstance damageInstance)
    {
        if (damageInstance.DamageDealt <= 0) return;
        if (fsm == null || !VULNERABLE_STATES.Contains(fsm.ActiveStateName)) return;

        switch (damageInstance.AttackType)
        {
            case AttackTypes.Nail:
            case AttackTypes.Spell:
                if (damageInstance.AttackType == AttackTypes.Nail) SpawnSoul();
                fsm.SetState("Hit");
                break;
            case AttackTypes.Acid:
            case AttackTypes.Generic:
            case AttackTypes.NailBeam:
            case AttackTypes.RuinsWater:
            case AttackTypes.SharpShadow:
                break;
        }
    }

    private void SpawnSoul()
    {
        var prefab = ObjectCache.SoulOrb;
        Destroy(prefab.Spawn());
        prefab.SetActive(true);

        // Give 34 soul per parry.
        FlingUtils.Config config = new()
        {
            Prefab = prefab,
            AmountMin = 16,
            AmountMax = 16,
            SpeedMin = 10,
            SpeedMax = 20,
            AngleMin = 0,
            AngleMax = 360,
        };
        FlingUtils.SpawnAndFling(config, transform, Vector3.zero);

        // Heal on parry.
        HeroController.instance.AddHealth(1);

        Destroy(prefab);
    }
}

internal class SongOfRevek : IFluteSong
{
    internal const string NAME = "Revek";

    public string Name() => NAME;

    public List<FluteNote> Notes() => [FluteNote.Up, FluteNote.Up, FluteNote.Left, FluteNote.Right];

    private static bool revekActive = false;

    public void Summon()
    {
        if (revekActive) return;

        var revek = Object.Instantiate(KnightOfNightsPreloader.Instance.Revek!);
        revek.AddComponent<RevekFixes>();

        revek.transform.position = new(-100, -100);
        revek.SetActive(true);

        var fsm = revek.LocateMyFSM("Control");
        fsm.Fsm.GlobalTransitions = [];
        foreach (var state in fsm.FsmStates) state.RemoveTransitionsOn("TAKE DAMAGE");

        fsm.GetState("Appear Pause").GetFirstActionOfType<Wait>().time.Value = 0.5f;

        Wrapped<int> consecutiveHits = new(0);
        var idleState = fsm.GetState("Slash Idle");
        var idleWait = idleState.GetFirstActionOfType<WaitRandom>();
        idleState.AddFirstAction(new Lambda(() =>
        {
            var wait = consecutiveHits.Value > 0 ? 0.25f : 0.6f;
            idleWait.timeMin.Value = wait;
            idleWait.timeMax.Value = wait;
        }));

        var attackPauseState = fsm.GetState("Attack Pause");
        var wait = attackPauseState.GetFirstActionOfType<WaitRandom>();
        wait.timeMin.Value = 1.2f;
        wait.timeMax.Value = 1.2f;
        attackPauseState.AddFirstAction(new Lambda(() => consecutiveHits.Value = 0));

        var ghostCheckPauseState = fsm.GetState("Ghost Check Pause");
        ghostCheckPauseState.ClearTransitions();
        ghostCheckPauseState.AddTransition("FINISHED", "Set Angle");

        GameObject audioSrc = new("RevekAudioSource");
        audioSrc.transform.parent = HeroController.instance.transform;
        fsm.GetState("Slash Tele In").GetFirstActionOfType<AudioPlayerOneShotSingle>().spawnPoint = audioSrc;

        var damagedPauseState = fsm.GetState("Damaged Pause");
        var damagedWait = damagedPauseState.GetFirstActionOfType<WaitRandom>();
        fsm.GetState("Damaged Pause").AddFirstAction(new Lambda(() =>
        {
            var wait = (++consecutiveHits.Value == 3) ? 4.5f : 0f;
            damagedWait.timeMin.Value = wait;
            damagedWait.timeMax.Value = wait;
        }));

        Wrapped<bool> right = new(false);
        fsm.GetState("Set Angle").AddLastAction(new Lambda(() =>
        {
            if (consecutiveHits.Value == 3)
            {
                revekActive = false;
                fsm.SendEvent("REVEK KILLED");
                Object.Destroy(fsm.gameObject);
                return;
            }
            else if (consecutiveHits.Value == 0) right.Value = HeroController.instance.cState.facingRight;

            audioSrc.transform.localPosition = new(right.Value ? -12 : 12, 0, 0);
            fsm.FsmVariables.GetFsmFloat("X Distance").Value = right.Value ? -12f : 12f;
            fsm.FsmVariables.GetFsmFloat("Y Distance").Value = 3f;
        }));

        var slashState = fsm.GetState("Slash");
        slashState.GetFirstActionOfType<FireAtTarget>().position.Value = new(0, -1.5f, 0);
        var decelerate = slashState.GetFirstActionOfType<DecelerateV2>();
        slashState.AddFirstAction(new Lambda(() => decelerate.deceleration.Value = consecutiveHits.Value > 0 ? 0.92f : 0.9f));
    }
}
