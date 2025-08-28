using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.SharedLib;
using System;
using System.Linq;
using UnityEngine.PlayerLoop;

namespace KnightOfNights.IC;

internal class PlandoSubmodule : Attribute { }

internal class PlandoModule : AbstractModule<PlandoModule>
{
    private static readonly FsmID deathAnimId = new("Hero Death", "Hero Death Anim");

    internal static event Action? OnEveryFrame;

    protected override PlandoModule Self() => this;

    public override void Initialize()
    {
        base.Initialize();

        // Add any other submodules.
        typeof(PlandoModule).Assembly.GetTypes().Where(t => t.IsDefined(typeof(PlandoSubmodule), false)).ForEach(t => ItemChangerMod.Modules.GetOrAdd(t));

        // Shade always spawns in glade.
        On.GameManager.Update += Update;
        Events.AddFsmEdit(deathAnimId, ModifySetShade);
    }

    public override void Unload()
    {
        On.GameManager.Update -= Update;
        Events.RemoveFsmEdit(deathAnimId, ModifySetShade);

        base.Unload();
    }

    private void Update(On.GameManager.orig_Update orig, GameManager self)
    {
        orig(self);
        OnEveryFrame?.Invoke();
    }

    private void ModifySetShade(PlayMakerFSM fsm) => fsm.GetState("Set Shade").AddLastAction(new Lambda(() =>
    {
        var pd = PlayerData.instance;
        pd.SetString(nameof(pd.shadeScene), SceneNames.RestingGrounds_08);
        pd.SetFloat(nameof(pd.shadePositionX), 196.5f);
        pd.SetFloat(nameof(pd.shadePositionY), 33f);
    }));
}
