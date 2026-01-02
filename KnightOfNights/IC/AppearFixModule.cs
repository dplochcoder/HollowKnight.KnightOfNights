using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class AppearFixModule : AbstractModule<AppearFixModule>
{
    private static readonly FsmID appearID = new("Appear");

    protected override AppearFixModule Self() => this;

    protected override void InitializeInternal() => Events.AddFsmEdit(appearID, FixAppearFSM);

    protected override void UnloadInternal() => Events.RemoveFsmEdit(appearID, FixAppearFSM);

    private void FixAppearFSM(PlayMakerFSM fsm)
    {
        var inertState = fsm.GetState("Inert");
        inertState.AddTransition("REACTIVATE", "Away");
        inertState.AddLastAction(new LambdaEveryFrame(() =>
        {
            var pd = PlayerData.instance;
            if (!pd.GetBool(nameof(pd.hasDreamNail))) return;

            fsm.FsmVariables.GetFsmGameObject("Dreamnail Hit").Value.SetActive(true);
            fsm.SendEvent("REACTIVATE");
        }));
    }
}
