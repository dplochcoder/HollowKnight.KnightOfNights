using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class XeroArmadaStats : MonoBehaviour
{
    [ShimField] public float BobPeriod;
    [ShimField] public float BobRadius;
    [ShimField] public float GracePeriod;
    [ShimField] public float HeightMax;
    [ShimField] public float HeightMin;
    [ShimField] public float NailXInitialSpace;
    [ShimField] public float NailXSpace;
    [ShimField] public float NailYInitialSpace;
    [ShimField] public float NailYSpace;
    [ShimField] public float ProjectileDecelerateTime;
    [ShimField] public float ProjectileInitTime;
    [ShimField] public float ProjectileHeightMax;
    [ShimField] public float ProjectileHeightMin;
    [ShimField] public float ProjectilePointTime;
    [ShimField] public float ProjectileReturnDeceleration;
    [ShimField] public float ProjectileReturnPauseDeceleration;
    [ShimField] public float ProjectileReturnPauseTime;
    [ShimField] public float ProjectileShootTime;
    [ShimField] public float ProjectileSpeed;
    [ShimField] public float ProjectileSpinTime;
    [ShimField] public float WaitBetweenNailFires;
    [ShimField] public float WaitBetweenNailSpawns;
    [ShimField] public float WaitInitial;
    [ShimField] public float WaitLastSpawnToMove;
    [ShimField] public float WaitLastSpawnToFire;
    [ShimField] public float XBuffer;
    [ShimField] public float XMoveAccel;
    [ShimField] public float XMoveDuration;
    [ShimField] public float XMoveSpeed;
    [ShimField] public float XRangeMin;
    [ShimField] public float XRangeMax;
}
