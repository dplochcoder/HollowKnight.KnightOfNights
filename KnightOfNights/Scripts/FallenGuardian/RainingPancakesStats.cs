using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class RainingPancakesStats : MonoBehaviour
{
    [ShimField] public float DiveHeightMax;
    [ShimField] public float DiveHeightMin;
    [ShimField] public float DiveXBuffer;
    [ShimField] public float DiveXRange;
    [ShimField] public int NumDives;
    [ShimField] public float PancakeSoundDelay;
    [ShimField] public float PancakeY;
    [ShimField] public float PancakeYIncrement;
    [ShimField] public float WaitAfterDive;
    [ShimField] public float WaitAfterWave;
    [ShimField] public float WaitBetweenWaves;
    [ShimField] public float WaitFirstWave;
    [ShimField] public float WaitLaterWaves;
    [ShimField] public int WingCount;
}
