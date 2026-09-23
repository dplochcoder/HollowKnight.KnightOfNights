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
    public static void ToggleRevekSong()
    {
        var mod = ItemChangerMod.Modules.GetOrAdd<RevekSongModule>();
        mod.HasRevekSong = !mod.HasRevekSong;
        Console.AddLine(mod.HasRevekSong ? "Gave Revek Song." : "Removed Revek Song.");
    }

    [BindableMethod(name = "Toggle Warriors Notes", category = CATEGORY)]
    public static void ToggleWarriorsNotes ()
    {
        var mod = ItemChangerMod.Modules.GetOrAdd<WarriorsNotesModule>();
        mod.HasWarriorsNotes = !mod.HasWarriorsNotes;

        Console.AddLine(mod.HasWarriorsNotes ? "Gave Warrior's Notes." : "Removed Warrior's Notes.");
    }

    [BindableMethod(name = "Reveal Benches", category = CATEGORY)]
    public static void RevealBenches() => BenchesModule.Get()?.RevealBenches();

    [BindableMethod(name = "Toggle Summit", category = CATEGORY)]
    public static void ToggleSummit()
    {
        if (!GetModule<FallenGuardianModule>(out var mod))
        {
            Console.AddLine($"{nameof(FallenGuardianModule)} not present.");
            return;
        }

        mod.DefeatedBoss = !mod.DefeatedBoss;
        Console.AddLine(mod.DefeatedBoss ? "Summit closed off." : "Summit re-opened.");
    }

    [BindableMethod(name = "Toggle Boss Intro", category = CATEGORY)]
    public static void ToggleBossIntro()
    {
        if (!GetModule<FallenGuardianModule>(out var mod))
        {
            Console.AddLine($"{nameof(FallenGuardianModule)} not present.");
            return;
        }

        mod.CompletedBossIntro = !mod.CompletedBossIntro;
        Console.AddLine(mod.CompletedBossIntro ? "Boss intro marked complete." : "Boss intro marked incomplete.");
    }
}
