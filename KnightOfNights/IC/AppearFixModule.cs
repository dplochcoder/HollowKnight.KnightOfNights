using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using SFCore.Utils;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class AppearFixModule : AbstractModule<AppearFixModule>
{
    private static readonly FsmID appearID = new("Appear");

    protected override AppearFixModule Self() => this;

    public override void Initialize()
    {
        base.Initialize();
        Events.AddFsmEdit(appearID, FixAppearFSM);
    }

    public override void Unload()
    {
        Events.RemoveFsmEdit(appearID, FixAppearFSM);
        base.Unload();
    }

    private void FixAppearFSM(PlayMakerFSM fsm)
    {
        var inertState = fsm.GetFsmState("Inert");
        inertState.AddFsmTransition("REACTIVATE", "Away");
        inertState.AddLastAction(new LambdaEveryFrame(() =>
        {
            var pd = PlayerData.instance;
            if (!pd.GetBool(nameof(pd.hasDreamNail))) return;

            fsm.FsmVariables.GetFsmGameObject("Dreamnail Hit").Value.SetActive(true);
            fsm.SendEvent("REACTIVATE");
        }));
    }
}
