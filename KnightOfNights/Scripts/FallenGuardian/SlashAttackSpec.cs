using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal record SlashAttackSpec
{
    public static readonly SlashAttackSpec LEFT = new(["LEFT"], new(-11f, 2.5f), new(0, -0.5f));
    public static readonly SlashAttackSpec RIGHT = LEFT.Flipped();
    public static readonly SlashAttackSpec HIGH_LEFT = new(["UP"], new(-2f, 10f), new(0, -0.5f));
    public static readonly SlashAttackSpec HIGH_RIGHT = HIGH_LEFT.Flipped();

    public readonly List<string> AllowedHits;
    public readonly Vector2 SpawnOffset;
    public readonly Vector2 TargetOffset;
    public readonly float Telegraph;
    public readonly float Deceleration;

    private static string FlipHit(string hit)
    {
        return hit switch
        {
            "RIGHT" => "LEFT",
            "LEFT" => "RIGHT",
            _ => hit
        };
    }

    internal SlashAttackSpec(List<string> allowedHits, Vector2 spawnOffset, Vector2 targetOffset, float telegraph = 0.6f, float deceleration = 0.925f)
    {
        AllowedHits = [.. allowedHits];
        SpawnOffset = spawnOffset;
        TargetOffset = targetOffset;
        Telegraph = telegraph;
        Deceleration = deceleration;
    }

    internal SlashAttackSpec Flipped() => new([.. AllowedHits.Select(FlipHit)], new(-SpawnOffset.x, SpawnOffset.y), new(-TargetOffset.x, TargetOffset.y), Telegraph, Deceleration);

    internal SlashAttackSpec Up(float y) => new(AllowedHits, new(SpawnOffset.x, SpawnOffset.y + y), TargetOffset, Telegraph, Deceleration);

    internal SlashAttackSpec Down(float y) => new(AllowedHits, new(SpawnOffset.x, SpawnOffset.y - y), TargetOffset, Telegraph, Deceleration);

    internal SlashAttackSpec Left(float x) => new(AllowedHits, new(SpawnOffset.x - x, SpawnOffset.y), TargetOffset, Telegraph, Deceleration);

    internal SlashAttackSpec Right(float x) => new(AllowedHits, new(SpawnOffset.x + x, SpawnOffset.y), TargetOffset, Telegraph, Deceleration);

    internal SlashAttackSpec WithTelegraph(float telegraph) => new(AllowedHits, SpawnOffset, TargetOffset, telegraph, Deceleration);

    internal SlashAttackSpec WithDeceleration(float deceleration) => new(AllowedHits, SpawnOffset, TargetOffset, Telegraph, Deceleration);
}
