using ItemChanger;
using ItemChanger.UIDefs;

namespace KnightOfNights.IC;

internal class ShadelessItem : AbstractItem
{
    public override void ResolveItem(GiveEventArgs args)
    {
        UIDef = new MsgUIDef()
        {
            name = new BoxedString("Shadeless"),
            shopDesc = new BoxedString("This should cost at least 100k geo"),
            sprite = new EmbeddedSprite("shadeless"),
        };
    }

    public override void GiveImmediate(GiveInfo info)
    {
        var mod = ShadelessModule.Get();
        if (mod == null) return;

        mod.IsShadeless = true;
    }
}
