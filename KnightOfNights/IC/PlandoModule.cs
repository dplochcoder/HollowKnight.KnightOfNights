using ItemChanger;
using KnightOfNights.Scripts.SharedLib;
using System;
using System.Linq;

namespace KnightOfNights.IC;

internal class PlandoSubmodule : Attribute { }

internal class PlandoModule : AbstractModule<PlandoModule>
{
    internal static event Action? OnEveryFrame;

    protected override PlandoModule Self() => this;

    protected override void InitializeInternal()
    {
        // Add any other submodules.
        typeof(PlandoModule).Assembly.GetTypes().Where(t => t.IsDefined(typeof(PlandoSubmodule), false)).ForEach(t => ItemChangerMod.Modules.GetOrAdd(t));

        // Shade always spawns in glade.
        On.GameManager.Update += Update;
    }

    protected override void UnloadInternal() => On.GameManager.Update -= Update;

    private void Update(On.GameManager.orig_Update orig, GameManager self)
    {
        orig(self);
        OnEveryFrame?.Invoke();
    }
}
