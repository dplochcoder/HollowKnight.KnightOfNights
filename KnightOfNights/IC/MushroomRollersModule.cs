﻿using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using PurenailCore.CollectionUtil;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class MushroomRollersModule : AbstractModule<MushroomRollersModule>
{
    protected override MushroomRollersModule Self() => this;

    public override void Initialize()
    {
        base.Initialize();
        Events.AddSceneChangeEdit(SceneNames.Fungus2_23, MakeConsistentRollers);
    }

    public override void Unload()
    {
        Events.RemoveSceneChangeEdit(SceneNames.Fungus2_23, MakeConsistentRollers);
        base.Unload();
    }

    private void MakeConsistent(GameObject roller, bool jumpFirst)
    {
        var fsm = roller.LocateMyFSM("Mush Roller");

        Wrapped<bool> jump = new(jumpFirst);

        var attackState = fsm.GetState("Attack Choice");
        attackState.RemoveActionsOfType<SendRandomEventV2>();
        attackState.AddLastAction(new Lambda(() =>
        {
            bool jumpNow = jump.Value;
            jump.Value = !jumpNow;

            fsm.SendEvent(jumpNow ? "JUMP" : "ROLL");
        }));
    }

    private void MakeConsistentRollers(Scene scene)
    {
        MakeConsistent(scene.FindGameObject("Mushroom Roller")!, true);
        MakeConsistent(scene.FindGameObject("Mushroom Roller (1)")!, false);
    }
}
