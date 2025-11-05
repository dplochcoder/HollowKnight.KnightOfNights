using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class RainingPancakesStats : MonoBehaviour
{
    [ShimField] public float DiveHeightMax;
    [ShimField] public float DiveHeightMin;
    [ShimField] public float DiveImpactScale;
    [ShimField] public float DivePlungeSpeed;
    [ShimField] public float DiveRetreatSpeed;
    [ShimField] public float DiveXBuffer;
    [ShimField] public float DiveXRange;
    [ShimField] public int NumDives;
    [ShimField] public int NumWavesPerDive;
    [ShimField] public float PancakePitchIncrement;
    [ShimField] public float PancakeSpeed;
    [ShimField] public float PancakeXOffset;
    [ShimField] public float PancakeY;
    [ShimField] public float PancakeYIncrement;
    [ShimField] public float ShockwaveSpeed;
    [ShimField] public float ShockwaveXScale;
    [ShimField] public float ShockwaveYScale;
    [ShimField] public float WaitAfterLastWaveSpawn;
    [ShimField] public float WaitAfterFirstTeleport;
    [ShimField] public float WaitAfterSpawnForTeleport;
    [ShimField] public float WaitBetweenWaveDrops;
    [ShimField] public float WaitBetweenWaveSpawns;
    [ShimField] public float WaitFinal;
    [ShimField] public float WaitFromDiveToNextSpawn;
    [ShimField] public float WaitFromLastDropToDive;
    [ShimField] public int WingCount;
}
