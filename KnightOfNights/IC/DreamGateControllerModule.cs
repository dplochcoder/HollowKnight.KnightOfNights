using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.Framework;
using KnightOfNights.Scripts.SharedLib.Data;
using KnightOfNights.Util;
using UnityEngine;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class DreamGateControllerModule : AbstractModule<DreamGateControllerModule>
{
    private static readonly FsmID dreamnailId = new("Dream Nail");

    protected override void InitializeInternal() => ItemChanger.Events.AddFsmEdit(dreamnailId, ModifyDreamnail);

    protected override void UnloadInternal() => ItemChanger.Events.RemoveFsmEdit(dreamnailId, ModifyDreamnail);

    private void ModifyDreamnail(PlayMakerFSM fsm)
    {
        fsm.GetState("Can Warp?").AddFirstAction(new Lambda(() =>
        {
            if (PlayerData.instance.GetInt(nameof(PlayerData.dreamOrbs)) <= 0) fsm.SendEvent("NO ESSENCE");
            else if (!CanWarp()) fsm.SendEvent("FAIL");
        }));
        fsm.GetState("Can Set?").AddFirstAction(new Lambda(() =>
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
