using DebugMod;
using ItemChanger;
using KnightOfNights.IC;
using RandomizerMod.IC;
using System.Collections.Generic;

namespace KnightOfNights.Debug;

internal static class DebugInterop
{
    private const string CATEGORY = "Knight of Nights";

    internal static void Setup() => DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));

    internal static bool GetModule<M>(out M module) where M : AbstractModule<M>, new()
    {
        var m = new M();
        module = m.GetStatic();
        return module != null;
    }

    [BindableMethod(name = "Give Revek Song", category = CATEGORY)]
    public static void GiveRevekSong()
    {
        if (!GetModule<RevekSongModule>(out var mod)) return;

        mod.HasRevekSong = true;
    }

    [BindableMethod(name = "Take Revek Song", category = CATEGORY)]
    public static void TakeRevekSong()
    {
        if (!GetModule<RevekSongModule>(out var mod)) return;

        mod.HasRevekSong = false;
    }

    [BindableMethod(name = "Give Warriors Notes", category = CATEGORY)]
    public static void GiveWarriorsNotes()
    {
        if (!GetModule<WarriorsNotesModule>(out var mod)) return;

        mod.HasWarriorsNotes = true;
    }

    [BindableMethod(name = "Take Warriors Notes", category = CATEGORY)]
    public static void TakeWarriorsNotes()
    {
        if (!GetModule<WarriorsNotesModule>(out var mod)) return;

        mod.HasWarriorsNotes = false;
    }

    private static int GetAndCountAccessibleItems()
    {
        var rs = RandomizerMod.RandomizerMod.RS;
        var ctx = rs.Context;
        var pm = rs.TrackerData.pm;

        Dictionary<int, (AbstractPlacement, AbstractItem)> dict = [];
        foreach (var p in ItemChanger.Internal.Ref.Settings.Placements.Values)
        {
            foreach (var i in p.Items)
            {
                if (i.GetTag<RandoItemTag>() is RandoItemTag tag) dict[tag.id] = (p, i);
            }
        }

        List<(AbstractPlacement, AbstractItem)> toGive = [];
        foreach (var p in ctx.itemPlacements)
        {
            if (!dict.TryGetValue(p.Index, out var pair)) continue;

            var (ip, ii) = pair;
            if (ii.name == ItemNames.Mantis_Claw || ii.name == ItemNames.Crystal_Heart) continue;
            if (ii.WasEverObtained()) continue;
            if (!p.Location.CanGet(pm)) continue;

            toGive.Add((ip, ii));
        }

        foreach (var (p, i) in toGive)
        {
            i.Give(p, new()
            {
                Container = "TreasureHunt",
                FlingType = FlingType.DirectDeposit,
                MessageType = MessageType.Corner,
                Transform = null,
                Callback = null
            });
        }

        return toGive.Count;
    }

#if DEBUG
    [BindableMethod(name = "Get Accessible Items", category = CATEGORY)]
    public static void GetAccessibleItems() => GetAndCountAccessibleItems();

    [BindableMethod(name = "Get Accessible Items Recursively", category = CATEGORY)]
    public static void GetAccessibleItemsRecursively()
    {
        while (GetAndCountAccessibleItems() > 0) { }
    }
#endif
}
