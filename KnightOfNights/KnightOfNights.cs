using Modding;

namespace KnightOfNights;

public class KnightOfNightsMod : Mod
{
    public static KnightOfNightsMod? Instance { get; private set; }

    private static readonly string VERSION = PurenailCore.ModUtil.VersionUtil.ComputeVersion<KnightOfNightsMod>();

    public override string GetVersion() => VERSION;

    public KnightOfNightsMod() : base("KnightOfNights") { Instance = this; }

    public override void Initialize() { }

    public static new void Log(string msg) => (Instance as ILogger)!.Log(msg);

    public static new void LogError(string msg) => (Instance as ILogger)!.LogError(msg);
}
