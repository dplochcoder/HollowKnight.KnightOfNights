using ItemChanger;
using ItemChanger.UIDefs;

namespace KnightOfNights.IC;

internal class WarriorsNotesItem : AbstractItem
{
    public override void ResolveItem(GiveEventArgs args)
    {
        UIDef = new MsgUIDef()
        {
            name = new BoxedString("Warrior's Notes"),
            shopDesc = new BoxedString("Found this out in the glade, what was it doing there?"),
            sprite = new EmbeddedSprite("notes"),
        };
    }

    public override void GiveImmediate(GiveInfo info)
    {
        var mod = WarriorsNotesModule.Get();
        if (mod == null) return;

        mod.HasWarriorsNotes = true;
    }
}
