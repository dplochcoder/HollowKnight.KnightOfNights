using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;

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

    protected new void Awake()
    {
        base.Awake();
        Patcher.Patch(this);
    }
}
