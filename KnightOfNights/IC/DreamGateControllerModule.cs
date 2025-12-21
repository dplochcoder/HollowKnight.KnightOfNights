using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.Framework;
using KnightOfNights.Scripts.SharedLib.Data;
using KnightOfNights.Util;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class DreamGateControllerModule : AbstractModule<DreamGateControllerModule>
{
    private static readonly FsmID dreamnailId = new("Knight", "Dream Nail");

    public override void Initialize()
    {
        base.Initialize();
        Events.AddFsmEdit(dreamnailId, ModifyDreamnail);
    }

    public override void Unload()
    {
        Events.RemoveFsmEdit(dreamnailId, ModifyDreamnail);
        base.Unload();
    }

    private void ModifyDreamnail(PlayMakerFSM fsm)
    {
        fsm.GetFsmState("Can Warp?").AddFirstAction(new Lambda(() =>
        {
            var pd = PlayerData.instance;
            var essence = pd.GetInt(nameof(pd.dreamOrbs));
            if (essence == 0) fsm.SendEvent("NO ESSENCE");
            else if (!CanWarp()) fsm.SendEvent("FAIL");
        }));
        fsm.GetFsmState("Can Set?").AddFirstAction(new Lambda(() =>
        {
            if (!CanSet()) fsm.SendEvent("FAIL");
        }));
    }

    private bool CanCharmWarp()
    {
        var pd = PlayerData.instance;
        var targetScene = pd.GetString(nameof(pd.dreamGateScene));
        Vector2 pos = new(pd.GetFloat(nameof(pd.dreamGateX)), pd.GetFloat(nameof(pd.dreamGateY)));

        foreach (var forbid in SceneDataModule.Get()?.GetForScene<ForbidCharmWarp>(targetScene) ?? [])
        {
            if (forbid.WholeScene) return false;
            if (pos.x >= forbid.MinX && pos.x <= forbid.MaxX && pos.y >= forbid.MinY && pos.y <= forbid.MaxY) return false;
        }

        return true;
    }

    private bool CanWarp()
    {
        if (CharmIds.EquippedAnyCharmsBesidesVoidHeart() && !CanCharmWarp()) return false;

        return true;
    }

    private int setBlockers = 0;

    public void AddSetBlocker() => setBlockers++;
    public void RemoveSetBlocker() => setBlockers--;

    private bool CanSet()
    {
        if (setBlockers > 0) return false;
        if (WindField.ActiveWindEffects(HeroController.instance.transform.position, WindTargetType.Hero).sqrMagnitude > 0) return false;

        return true;
    }

    protected override DreamGateControllerModule Self() => this;
}
