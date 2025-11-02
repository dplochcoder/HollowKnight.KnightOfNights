using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.Scripts.Proxy;

[Shim]
internal class EnemyHitEffectsGhostProxy : EnemyHitEffectsGhost
{
    private static readonly MonobehaviourPatcher<EnemyHitEffectsGhost> Patcher = new(() =>
        KnightOfNightsPreloader.Instance.Xero!.GetComponent<EnemyHitEffectsGhost>(),
        "audioPlayerPrefab",
        "enemyDamage",
        "ghostHitPt",
        "slashEffectGhost1",
        "slashEffectGhost2");

    [ShimField] public GameObject? SpriteFlashOverride;

    protected new void Awake()
    {
        base.Awake();
        Patcher.Patch(this);

        var spriteFlash = SpriteFlashOverride?.GetComponent<SpriteFlash>();
        if (spriteFlash != null) this.SetAttr("spriteFlash", spriteFlash);
    }
}
