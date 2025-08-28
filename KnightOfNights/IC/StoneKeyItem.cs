using ItemChanger;

namespace KnightOfNights.IC;

internal class StoneKeyItem : AbstractItem
{
    // TODO: UI

    public override void GiveImmediate(GiveInfo info)
    {
        var module = StoneKeyModule.Get();
        if (module == null) return;

        module.HasStoneKey = true;
    }
}
