using System.Linq;

namespace KnightOfNights.Util;

internal class CharmId(int id)
{
    public readonly int Id = id;

    public bool IsEquipped() => PlayerData.instance.GetBool($"equippedCharm_{Id}");
}

internal static class CharmIds
{
    public static readonly CharmId GatheringSwarm = new(1);
    public static readonly CharmId WaywardCompass = new(2);
    public static readonly CharmId Grubsong = new(3);
    public static readonly CharmId StalwartShell = new(4);
    public static readonly CharmId BaldurShell = new(5);
    public static readonly CharmId FuryOfTheFallen = new(6);
    public static readonly CharmId QuickFocus = new(7);
    public static readonly CharmId LifebloodHeart = new(8);
    public static readonly CharmId LifebloodCore = new(9);
    public static readonly CharmId DefendersCrest = new(10);
    public static readonly CharmId Flukenest = new(11);
    public static readonly CharmId ThornsOfAgony = new(12);
    public static readonly CharmId MarkOfPride = new(13);
    public static readonly CharmId SteadyBody = new(14);
    public static readonly CharmId HeavyBlow = new(15);
    public static readonly CharmId SharpShadow = new(16);
    public static readonly CharmId SporeShroom = new(17);
    public static readonly CharmId Longnail = new(18);
    public static readonly CharmId ShamanStone = new(19);
    public static readonly CharmId SoulCatcher = new(20);
    public static readonly CharmId SoulEater = new(21);
    public static readonly CharmId GlowingWomb = new(22);
    public static readonly CharmId FragileHeart = new(23);
    public static readonly CharmId UnbreakableHeart = new(23);
    public static readonly CharmId FragileGreed = new(24);
    public static readonly CharmId UnbreakableGreed = new(24);
    public static readonly CharmId FragileStrength = new(25);
    public static readonly CharmId UnbreakableStrength = new(25);
    public static readonly CharmId NailmastersGlory = new(26);
    public static readonly CharmId JonisBlessing = new(27);
    public static readonly CharmId ShapeOfUnn = new(28);
    public static readonly CharmId HiveBlood = new(29);
    public static readonly CharmId DreamWielder = new(30);
    public static readonly CharmId Dashmaster = new(31);
    public static readonly CharmId QuickSlash = new(32);
    public static readonly CharmId SpellTwister = new(33);
    public static readonly CharmId DeepFocus = new(34);
    public static readonly CharmId GrubberflysElegy = new(35);
    public static readonly CharmId Kingsoul = new(36);
    public static readonly CharmId VoidHeart = new(36);
    public static readonly CharmId Sprintmaster = new(37);
    public static readonly CharmId Dreamshield = new(38);
    public static readonly CharmId Weaversong = new(39);
    public static readonly CharmId Grimmchild = new(40);

    internal static bool EquippedAnyCharmsBesidesVoidHeart()
    {
        var pd = PlayerData.instance;
        return pd.equippedCharms.Any(id => id != VoidHeart.Id || pd.GetInt(nameof(pd.royalCharmState)) == 3);
    }
}
