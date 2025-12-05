using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.Scripts.Proxy;

[Shim]
internal class EnemyHitEffectsGhostProxy : EnemyHitEffectsGhost
{
    static EnemyHitEffectsGhostProxy() => On.EnemyHitEffectsGhost.RecieveHitEffect += OnRecieveHitEffect;

    private static readonly MonobehaviourPatcher<EnemyHitEffectsGhost> Patcher = new(() =>
        KnightOfNightsPreloader.Instance.Xero!.GetComponent<EnemyHitEffectsGhost>(),
        "audioPlayerPrefab",
        "enemyDamage",
        "ghostHitPt",
        "slashEffectGhost1",
        "slashEffectGhost2");

    [ShimField] public CustomSpriteFlash? SpriteFlash;

    protected new void Awake()
    {
        base.Awake();
        Patcher.Patch(this);
    }

    private static void OnRecieveHitEffect(On.EnemyHitEffectsGhost.orig_RecieveHitEffect orig, EnemyHitEffectsGhost self, float attackDirection)
    {
        if (self is EnemyHitEffectsGhostProxy proxy && proxy.SpriteFlash != null && !self.GetAttr<EnemyHitEffectsGhost, bool>("didFireThisFrame")) proxy.SpriteFlash.Flash(Color.white, 0.85f, 0.35f);
        orig(self, attackDirection);
    }
}
