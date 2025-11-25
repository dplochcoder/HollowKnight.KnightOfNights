using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class DiveStats : MonoBehaviour
{
    [ShimField] public float ImpactScale;
    [ShimField] public float PlungeSpeed;
    [ShimField] public float RetreatSpeed;
    [ShimField] public float ShockwaveXScale;
    [ShimField] public float ShockwaveYScale;
    [ShimField] public float ShockwaveSpeed;
    [ShimField] public float TallShockwaveSpeed;
}

[Shim]
internal class EmptyTeleportStats : MonoBehaviour
{
    [ShimField] public float DistanceMax;
    [ShimField] public float DistanceMin;
    [ShimField] public float HeightMax;
    [ShimField] public float HeightMin;
    [ShimField] public float GracePeriod;
    [ShimField] public float WaitToTeleOutMax;
    [ShimField] public float WaitToTeleOutMin;
    [ShimField] public float XBuffer;
}

[Shim]
internal class StaggerStats : MonoBehaviour
{
    [ShimField] public float GracePeriod;
    [ShimField] public float Invuln;
    [ShimField] public float HitWait;
    [ShimField] public float MaxWait;
    [ShimField] public float NextAttackDelay;
    [ShimField] public float OscillationPeriod;
    [ShimField] public float OscillationRadius;
}

[Shim]
internal class FallenGuardianPhaseStats : MonoBehaviour
{
    [ShimField] public int MinHP;
    [ShimField] public AttackChoice FirstAttack;
    [ShimField] public List<FallenGuardianAttack> Attacks = [];

    [ShimField] public AxeHopscotchStats? AxeHopscotchStats;
    [ShimField] public DiveStats? DiveStats;
    [ShimField] public EmptyTeleportStats? EmptyTeleportStats;
    [ShimField] public GorbStormStats? GorbStormStats;
    [ShimField] public RainingPancakesStats? RainingPancakesStats;
    [ShimField] public ShieldCycloneStats? ShieldCycloneStats;
    [ShimField] public SlashAmbushStats? SlashAmbushStats;
    [ShimField] public StaggerStats? StaggerStats;
    [ShimField] public UltraInstinctStats? UltraInstinctStats;
    [ShimField] public XeroArmadaStats? XeroArmadaStats;

    internal bool DidFirstAttack = false;
}
