using ItemChanger.Modules;
using PurenailCore.CollectionUtil;

namespace KnightOfNights.IC;

internal abstract class AbstractModule<M> : Module where M : AbstractModule<M>
{
    private static Deferred<M> Instance = new();

    internal static M? Get() => Instance.TryGet(out M value) ? value : null;

    internal static Deferred<M> GetDeferred() => Instance;

    internal M? GetStatic() => Get();

    protected abstract M Self();

    protected virtual void InitializeInternal() { }

    public sealed override void Initialize()
    {
        InitializeInternal();
        Instance.Set(Self());
    }

    protected virtual void UnloadInternal() { }

    public sealed override void Unload()
    {
        UnloadInternal();
        Instance = new();
    }
}
