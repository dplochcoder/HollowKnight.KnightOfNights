using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class AxeHopscotchStats : MonoBehaviour
{
    [ShimField] public GameObject? WallImpactLPrefab;
    [ShimField] public GameObject? SnowImpactPrefab;

    [ShimField] public float AxeAccel;
    [ShimField] public float AxeAnticTime;
    [ShimField] public float AxeBounceSpeed;
    [ShimField] public float AxeDecelTime;
    [ShimField] public float AxeAnticRiseY;
    [ShimField] public float AxeEmergeTime;
    [ShimField] public float AxeFloatHeight;
    [ShimField] public float AxeGravity;
    [ShimField] public float AxeMaxSpeed;
    [ShimField] public float AxeSpawnXSpaceInner;
    [ShimField] public float AxeSpawnXSpaceOuter;
    [ShimField] public float AxeSpinTime;
    [ShimField] public float AxeTimer;
    [ShimField] public float BobPeriod;
    [ShimField] public float BobRadius;
    [ShimField] public float GracePeriod;
    [ShimField] public float GracePeriod2;
    [ShimField] public int NumSlashAttacks;
    [ShimField] public float SpawnXBuffer;
    [ShimField] public float SpawnXDistance;
    [ShimField] public float WaitAfterLastSpawnToTeleportOut;
    [ShimField] public float WaitAfterTeleportToSlashAttack;
    [ShimField] public float WaitAnticAfterSpawn;
    [ShimField] public float WaitBetweenAxeSpawns;
    [ShimField] public float WaitBetweenSlashAttacks;
    [ShimField] public float WaitInitial;
}
