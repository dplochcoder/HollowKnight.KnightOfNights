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

        var setShadeState = fsm.GetState("Set Shade");
        setShadeState.RemoveActionsOfType<SendEventByName>();
        setShadeState.AddLastAction(new Lambda(() =>
        {
            var pd = PlayerData.instance;
            pd.SetString(nameof(pd.shadeScene), SceneNames.RestingGrounds_08);
            pd.SetString(nameof(pd.shadeMapZone), nameof(MapZone.RESTING_GROUNDS));
            pd.SetFloat(nameof(pd.shadePositionX), 196.5f);
            pd.SetFloat(nameof(pd.shadePositionY), 33f);
        }));
    }
}
