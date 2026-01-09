using DebugMod;
using ItemChanger;
using KnightOfNights.IC;

namespace KnightOfNights.Debug;

internal static class DebugInterop
{
    private const string CATEGORY = "Knight of Nights";

    internal static void Setup() => DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));

    internal static bool GetModule<M>(out M module) where M : AbstractModule<M>, new()
    {
        var m = new M();
#pragma warning disable CS8601 // Possible null reference assignment.
        module = m.GetStatic();
#pragma warning restore CS8601 // Possible null reference assignment.
        return module != null;
    }

    [BindableMethod(name = "Give Revek Song", category = CATEGORY)]
    public static void GiveRevekSong() => ItemChangerMod.Modules.GetOrAdd<RevekSongModule>().HasRevekSong = true;

    [BindableMethod(name = "Take Revek Song", category = CATEGORY)]
    public static void TakeRevekSong()
    {
        if (!GetModule<RevekSongModule>(out var mod)) return;
        mod.HasRevekSong = false;
    }

    [BindableMethod(name = "Give Warriors Notes", category = CATEGORY)]
    public static void GiveWarriorsNotes() => ItemChangerMod.Modules.GetOrAdd<WarriorsNotesModule>().HasWarriorsNotes = true;

    [BindableMethod(name = "Take Warriors Notes", category = CATEGORY)]
    public static void TakeWarriorsNotes()
    {
        if (!GetModule<WarriorsNotesModule>(out var mod)) return;
        mod.HasWarriorsNotes = false;
    }

    [BindableMethod(name = "Reveal Benches", category = CATEGORY)]
    public static void RevealBenches() => BenchesModule.Get()?.RevealBenches();

    [BindableMethod(name = "Toggle Summit", category = CATEGORY)]
    public static void ToggleSummit()
    {
        if (!GetModule<FallenGuardianModule>(out var mod)) return;
        mod.DefeatedBoss = !mod.DefeatedBoss;
    }
}
