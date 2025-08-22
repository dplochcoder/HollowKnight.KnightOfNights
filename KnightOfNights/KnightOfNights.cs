using Modding;

namespace KnightOfNights;

public class KnightOfNightsMod : Mod
{
    public static KnightOfNightsMod? Instance { get; private set; }

    private static readonly string VERSION = PurenailCore.ModUtil.VersionUtil.ComputeVersion<KnightOfNightsMod>();

    public override string GetVersion() => VERSION;

    public KnightOfNightsMod() : base("KnightOfNights") { Instance = this; }

    public override void Initialize() { }
}
