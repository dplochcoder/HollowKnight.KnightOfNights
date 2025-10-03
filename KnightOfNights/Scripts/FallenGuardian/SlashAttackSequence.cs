using KnightOfNights.Scripts.SharedLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal abstract class SlashAttackSequence
{
    public abstract IEnumerable<(float, SlashAttackSpec)> AttackSequence();

    // Returns a cancellation callback.
    public Action Play(Action<SlashAttackResult> callback)
    {
        GameObject obj = new("SlashAttackSequence");
        var b = obj.AddComponent<SlashAttackSequenceBehaviour>();
        b.Attacks = [.. AttackSequence()];
        b.Callback = callback;
        return () => UnityEngine.Object.Destroy(b);
    }
}

internal class SlashAttackSequenceBehaviour : MonoBehaviour
{
    internal List<(float, SlashAttackSpec)> Attacks = [];
    internal Action<SlashAttackResult>? Callback;

    private float currentWait;
    private int currentIndex;
    private HashSet<SlashAttack> activeAttacks = [];

    private void Update()
    {
        currentWait += Time.deltaTime;
    }

    private void OnDestroy() => activeAttacks.ForEach(c => c.Cancel());
}
