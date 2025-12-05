using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class SlashAmbushStats : MonoBehaviour
{
    [ShimField] public float BigSlashDelay;
    [ShimField] public float FirstTelegraph;
    [ShimField] public float GracePeriod;
    [ShimField] public float HitGracePeriod;
    [ShimField] public float SlashSpeed;
    [ShimField] public float SlashDeceleration;
    [ShimField] public float TelegraphStagger;
}
