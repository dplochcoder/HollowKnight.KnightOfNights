using ItemChanger.Modules;

namespace KnightOfNights.IC;

internal abstract class AbstractModule<M> : Module where M : AbstractModule<M>
{
    private static M? Instance;

    internal static M? Get() => Instance;

    internal M? GetStatic() => Get();

    protected abstract M Self();

    public override void Initialize() => Instance = Self();

    public override void Unload() => Instance = null;
}
