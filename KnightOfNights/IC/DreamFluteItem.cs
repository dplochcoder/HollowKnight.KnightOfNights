using ItemChanger;

namespace KnightOfNights.IC;

internal class DreamFluteItem : AbstractItem
{
    // TODO: UI

    public override void GiveImmediate(GiveInfo info)
    {
        var mod = DreamFluteModule.Get();
        if (mod == null) return;

        mod.HasDreamFlute = true;
        mod.LearnedSongs.Add(Songs.SongOfRevek.NAME);
    }

    public override bool Redundant()
    {
        var mod = DreamFluteModule.Get();
        if (mod == null) return true;

        return mod.HasDreamFlute && mod.LearnedSongs.Contains(Songs.SongOfRevek.NAME);
    }
}
