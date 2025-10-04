using ItemChanger;
using PurenailCore.CollectionUtil;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class HuModule : AbstractGhostWarriorModule<HuModule>
{
    protected override FsmID FsmID() => new("Ghost Warrior Hu", "Attacking");

    protected override void ModifyGhostWarrior(PlayMakerFSM fsm, Wrapped<int> baseHP)
    {
        // FIXME
        throw new System.NotImplementedException();
    }

    protected override HuModule Self() => this;
}
