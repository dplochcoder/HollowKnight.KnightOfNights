using KnightOfNights.Scripts.InternalLib;
using System;

namespace KnightOfNights.Scripts.Proxy;

public class HealthManagerProxy : HealthManager
{
    public bool SoulGain = true;

    public event Action? OnDamageTaken;
    public event Action? CustomOnDeath;

    private static readonly MonobehaviourPatcher<HealthManager> Patcher = new(() =>
        KnightOfNightsPreloader.Instance.CrystalCrawler!.GetComponent<HealthManager>(),
        "audioPlayerPrefab",
        "regularInvincibleAudio",
        "blockHitPrefab",
        "strikeNailPrefab",
        "slashImpactPrefab",
        "fireballHitPrefab",
        "sharpShadowImpactPrefab",
        "corpseSplatPrefab",
        "enemyDeathSwordAudio",
        "enemyDamageAudio",
        "smallGeoPrefab",
        "mediumGeoPrefab",
        "largeGeoPrefab");

    private bool triggeredCustomDeath = false;

    protected new void Awake()
    {
        if (gameObject.IsPBActivated())
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();
        Patcher.Patch(this);

        CustomOnDeath += () => gameObject.ActivatePB();
        OnDeath += TriggerDeath;
    }

    private int? prevHp;

    protected new void Update()
    {
        base.Update();

        if (prevHp == null) prevHp = hp;
        else if (prevHp.Value != hp)
        {
            if (prevHp.Value > hp) OnDamageTaken?.Invoke();
            prevHp = hp;
        }
    }

    public void TriggerDeath()
    {
        if (triggeredCustomDeath) return;

        triggeredCustomDeath = true;
        isDead = true;
        CustomOnDeath?.Invoke();
    }
}
