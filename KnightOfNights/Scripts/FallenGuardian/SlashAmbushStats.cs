using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class SlashAmbushStats : MonoBehaviour
{
    [ShimField] public float BigSlashAngleRange;
    [ShimField] public float BigSlashDelay;
    [ShimField] public Vector2 BigSlashSpawnOffset;
    [ShimField] public float BigSlashSpeed;
    [ShimField] public Vector2 BigSlashTargetOffset;
    [ShimField] public float BigSlashXBuffer;
    [ShimField] public float BigSlashDeceleration;
    [ShimField] public float FirstTelegraph;
    [ShimField] public float GracePeriod;
    [ShimField] public float HitGracePeriod;
    [ShimField] public float SlashSpeed;
    [ShimField] public float SlashDeceleration;
    [ShimField] public float TelegraphStagger;
}
