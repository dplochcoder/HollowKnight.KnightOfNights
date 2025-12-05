using KnightOfNights.Scripts.InternalLib;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.Scripts.Proxy;

public class TinkEffectProxy : TinkEffect
{
    private static readonly MonobehaviourPatcher<TinkEffect> Patcher = new(
        () => KnightOfNightsPreloader.Instance.Goam.GetComponent<TinkEffect>(),
        "blockEffect");

    private void Awake()
    {
        Patcher.Patch(this);
        this.SetAttr<TinkEffect, BoxCollider2D>("boxCollider", gameObject.GetComponent<BoxCollider2D>());
    }
}