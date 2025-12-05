using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class ShieldCycloneStats : MonoBehaviour
{
    [ShimField] public float BobPeriod;
    [ShimField] public float BobRadius;
    [ShimField] public float CenterHeight;
    [ShimField] public float CenterXRange;
    [ShimField] public float CycloneMinDistance;
    [ShimField] public float DaggerMaxDistance;
    [ShimField] public float DaggerMinDistance;
    [ShimField] public float ExpansionAccel;
    [ShimField] public float ExpansionLimit;
    [ShimField] public float ExpansionStart;
    [ShimField] public float ExpansionStartSpeed;
    [ShimField] public float ExpansionTopSpeed;
    [ShimField] public float GracePeriod;
    [ShimField] public int NumDaggerSpawns;
    [ShimField] public int NumShieldWaves;
    [ShimField] public float RotationOffsetPerWave;
    [ShimField] public float RotationSpeedDecel;
    [ShimField] public float RotationSpeedMinimum;
    [ShimField] public float RotationSpeedStart;
    [ShimField] public int ShieldsPerWave;
    [ShimField] public float WaitAfterShieldTeleport;
    [ShimField] public float WaitAfterLastShieldSpawn;
    [ShimField] public float WaitBetweenDaggerSpawns;
    [ShimField] public float WaitBetweenShieldWaves;
    [ShimField] public float WaitLastDaggerSpawnToShieldTeleport;
}
