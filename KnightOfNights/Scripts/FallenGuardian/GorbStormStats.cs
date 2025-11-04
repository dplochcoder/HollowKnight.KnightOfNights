using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class GorbStormStats : MonoBehaviour
{
    [ShimField] public float BobPeriod;
    [ShimField] public float BobRadius;
    [ShimField] public int BurstCountFinale;
    [ShimField] public int BurstCountSmall;
    [ShimField] public float FinaleMinDist;
    [ShimField] public float GracePeriod;
    [ShimField] public int NumSmallBursts;
    [ShimField] public float PitchIncrementFinale;
    [ShimField] public float PitchIncrementSmall;
    [ShimField] public float SmallXMin;
    [ShimField] public float SmallXMax;
    [ShimField] public float SmallYMin;
    [ShimField] public float SmallYMax;
    [ShimField] public float SpikeAccel;
    [ShimField] public int SpokeCountFinale;
    [ShimField] public int SpokeCountSmall;
    [ShimField] public float SpokeRotationFinale;
    [ShimField] public float SpokeRotationSmall;
    [ShimField] public float WaitAfterFinale;
    [ShimField] public float WaitAfterTeleport;
    [ShimField] public float WaitBeforeFinale;
    [ShimField] public float WaitBeforeTeleport;
    [ShimField] public float WaitFirst;
    [ShimField] public float WaitSpikeFinale;
    [ShimField] public float WaitSpikeSmall;
}
