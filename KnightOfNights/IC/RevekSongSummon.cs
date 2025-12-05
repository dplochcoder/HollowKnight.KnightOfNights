using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts;
using KnightOfNights.Scripts.Framework;
using PurenailCore.CollectionUtil;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.IC;

internal class RevekSongSummon
{
    internal static bool revekActive = false;

    internal static void MoveWithWind(FsmState state)
    {
        var t = state.Fsm.FsmComponent.transform;
        state.AddFirstAction(new LambdaEveryFrame(() => t.Translate(WindField.ActiveWindEffects(t.position, WindTargetType.Hero) * Time.deltaTime, Space.World)));
    }

    internal static void Summon(List<FluteNote> notes)
    {
        var mapZone = GameManager.instance.GetCurrentMapZone();
        if (mapZone == nameof(MapZone.DREAM_WORLD) || mapZone == nameof(MapZone.WHITE_PALACE) || mapZone == nameof(MapZone.GODS_GLORY)) return;

        if (notes.Count < 3 || revekActive) return;
        revekActive = true;

        var revek = Object.Instantiate(KnightOfNightsPreloader.Instance.Revek!);
        revek.AddComponent<RevekAddons>().HealOnNailParry = true;

        revek.transform.position = new(-100, -100);
        revek.SetActive(true);

        var fsm = revek.LocateMyFSM("Control");
        fsm.Fsm.GlobalTransitions = [];
        foreach (var state in fsm.FsmStates) state.RemoveTransitionsOn("TAKE DAMAGE");

        fsm.GetFsmState("Appear Pause").GetFirstActionOfType<Wait>().time.Value = 0.5f;

        Wrapped<int> consecutiveHits = new(0);
        var idleState = fsm.GetFsmState("Slash Idle");
        var idleWait = idleState.GetFirstActionOfType<WaitRandom>();
        idleState.AddFirstAction(new Lambda(() =>
        {
            var wait = consecutiveHits.Value > 0 ? 0.25f : 0.6f;
            idleWait.timeMin.Value = wait;
            idleWait.timeMax.Value = wait;
        }));

        var attackPauseState = fsm.GetFsmState("Attack Pause");
        var wait = attackPauseState.GetFirstActionOfType<WaitRandom>();
        wait.timeMin.Value = 1.2f;
        wait.timeMax.Value = 1.2f;
        attackPauseState.AddFirstAction(new Lambda(() => consecutiveHits.Value = 0));

        var ghostCheckPauseState = fsm.GetFsmState("Ghost Check Pause");
        ghostCheckPauseState.ClearTransitions();
        ghostCheckPauseState.AddFsmTransition("FINISHED", "Set Angle");

        GameObject audioSrc = new("RevekAudioSource");
        audioSrc.transform.parent = HeroController.instance.transform;
        fsm.GetFsmState("Slash Tele In").GetFirstActionOfType<AudioPlayerOneShotSingle>().spawnPoint = audioSrc;

        var damagedPauseState = fsm.GetFsmState("Damaged Pause");
        var damagedWait = damagedPauseState.GetFirstActionOfType<WaitRandom>();
        fsm.GetFsmState("Damaged Pause").AddFirstAction(new Lambda(() =>
        {
            var wait = (++consecutiveHits.Value == 3) ? 4.5f : 0f;
            damagedWait.timeMin.Value = wait;
            damagedWait.timeMax.Value = wait;
        }));

        fsm.GetFsmState("Set Angle").AddLastAction(new Lambda(() =>
        {
            if (consecutiveHits.Value == 3)
            {
                revekActive = false;
                fsm.SendEvent("REVEK KILLED");
                Object.Destroy(fsm.gameObject);
                return;
            }

            bool flyRight = notes[consecutiveHits.Value] == FluteNote.Right;
            audioSrc.transform.localPosition = new(flyRight ? -12 : 12, 0, 0);
            fsm.FsmVariables.GetFsmFloat("X Distance").Value = flyRight ? -12f : 12f;
            fsm.FsmVariables.GetFsmFloat("Y Distance").Value = 3f;
        }));

        MoveWithWind(fsm.GetFsmState("Slash Antic"));

        var slashState = fsm.GetFsmState("Slash");
        slashState.AddFsmTransition("PARRIED", "Hit");
        slashState.GetFirstActionOfType<FireAtTarget>().position.Value = new(0, -1.5f, 0);
        slashState.GetFirstActionOfType<DecelerateV2>().deceleration.Value = 0.925f;
        MoveWithWind(slashState);
    }
}
