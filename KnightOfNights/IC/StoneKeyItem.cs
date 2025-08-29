using ItemChanger;
using ItemChanger.UIDefs;

namespace KnightOfNights.IC;

internal class StoneKeyItem : AbstractItem
{
    public override void ResolveItem(GiveEventArgs args)
    {
        UIDef = new MsgUIDef()
        {
            name = new BoxedString("Stone Key"),
            shopDesc = new BoxedString("Stone Key"),
            sprite = new EmbeddedSprite("stonekey")
        };
    }

    public override void GiveImmediate(GiveInfo info)
    {
        var module = StoneKeyModule.Get();
        if (module == null) return;

        module.HasStoneKey = true;
    }
}
