using ItemChanger;
using ItemChanger.UIDefs;

namespace KnightOfNights.IC;

internal class RevekSongItem : AbstractItem
{
    public override void ResolveItem(GiveEventArgs args)
    {
        UIDef = new BigUIDef()
        {
            bigSprite = new EmbeddedSprite("reveksong_big"),
            take = new BoxedString("Absorbed the"),
            press = null,
            buttonSkin = null,
            descOne = new BoxedString("Press left or right three times while swinging the Dream Nail."),
            descTwo = new BoxedString("Revek will answer the call until parried three times in a row."),
            name = new BoxedString("Revek Song"),
            shopDesc = new BoxedString("Revek Song"),
            sprite = new EmbeddedSprite("reveksong"),
        };
    }

    public override void GiveImmediate(GiveInfo info)
    {
        var mod = RevekSongModule.Get();
        if (mod == null) return;

        mod.HasRevekSong = true;
    }
}
