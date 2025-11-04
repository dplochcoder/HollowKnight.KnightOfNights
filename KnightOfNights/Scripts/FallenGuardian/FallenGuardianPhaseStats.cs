using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

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
    [ShimField] public GorbStormStats? GorbStormStats;
    [ShimField] public RainingPancakesStats? RainingPancakesStats;
    [ShimField] public StaggerStats? StaggerStats;
    [ShimField] public UltraInstinctStats? UltraInstinctStats;

    internal bool DidFirstAttack = false;
}
