using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal record SlashAttackSpec
{
    public static readonly SlashAttackSpec LEFT = new(new(-12f, 3.5f), new(0, -0.5f));
    public static readonly SlashAttackSpec RIGHT = LEFT.Flipped();
    public static readonly SlashAttackSpec HIGH_LEFT = new(new(-2f, 10f), new(0, -0.5f));
    public static readonly SlashAttackSpec HIGH_RIGHT = HIGH_LEFT.Flipped();

    public readonly Vector2 SpawnOffset;
    public readonly Vector2 TargetOffset;
    public readonly float Deceleration;

    internal SlashAttackSpec(Vector2 spawnOffset, Vector2 targetOffset, float deceleration = 0.925f)
    {
        SpawnOffset = spawnOffset;
        TargetOffset = targetOffset;
        Deceleration = deceleration;
    }

    private SlashAttackSpec Flipped() => new(new(-SpawnOffset.x, SpawnOffset.y), new(-TargetOffset.x, TargetOffset.y), Deceleration);
}
