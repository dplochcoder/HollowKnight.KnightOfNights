using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class ShadeModule : AbstractModule<ShadeModule>
{
    private static readonly FsmID deathAnimId = new("Hero Death", "Hero Death Anim");

    protected override ShadeModule Self() => this;

    public override void Initialize()
    {
        base.Initialize();
        Events.AddFsmEdit(deathAnimId, ModifyDeathAnim);
    }

    public override void Unload()
    {
        Events.RemoveFsmEdit(deathAnimId, ModifyDeathAnim);
        base.Unload();
    }

    private void ModifyDeathAnim(PlayMakerFSM fsm)
    {
        fsm.GetState("Remove Geo").ClearActions();
        fsm.GetState("Limit Soul").ClearActions();

        var setShadeState = fsm.GetState("Set Shade");
        setShadeState.AddTransition("SKIP", "Save");
        setShadeState.AddFirstAction(new Lambda(() =>
        {
            fsm.FsmVariables.GetFsmGameObject("Self").Value = fsm.gameObject;
            fsm.SendEvent("SKIP");
        }));
    }
}
