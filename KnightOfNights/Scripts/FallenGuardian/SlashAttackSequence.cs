using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal abstract class SlashAttackSequence
{
    public abstract IEnumerable<(float, SlashAttackSpec)> AttackSequence();

    public abstract float Tail();

    // Returns a cancellation callback.
    public System.Action Play(System.Action<SlashAttackResult> callback)
    {
        GameObject obj = new();
        var b = obj.AddComponent<SlashAttackSequenceBehaviour>();
        b.Attacks = [.. AttackSequence()];
        b.Callback = callback;
        return b.CancelAndDestroy;
    }
}

internal class FlippableSlashAttackSequence(List<(float, SlashAttackSpec)> specs, float tail = 0) : SlashAttackSequence
{
    private bool flipped = Random.Range(0, 2) == 0;

    public override IEnumerable<(float, SlashAttackSpec)> AttackSequence()
    {
        flipped = !flipped;
        float total = 0;
        foreach (var (time, s) in specs)
        {
            SlashAttackSpec spec = s;
            if (flipped) spec = spec.Flipped();

            total += time;
            yield return (total, spec);
        }
    }

    public override float Tail() => tail;
}

internal class SlashAttackSequenceBehaviour : MonoBehaviour
{
    internal List<(float, SlashAttackSpec)> Attacks = [];
    internal System.Action<SlashAttackResult>? Callback;

    private float currentWait;
    private readonly HashSet<SlashAttack> activeAttacks = [];
    private int launchedAttacks;
    private int parriedAttacks;
    private bool cancelled = false;

    private void Update()
    {
        currentWait += Time.deltaTime;

        for (int i = launchedAttacks; i < Attacks.Count; i++)
        {
            var (time, spec) = Attacks[i];
            if (currentWait < time) break;
            else
            {
                ++launchedAttacks;
                var attack = SlashAttack.Spawn(spec);
                activeAttacks.Add(attack);

                attack.OnResult += result =>
                {
                    activeAttacks.Remove(attack);

                    switch (result)
                    {
                        case SlashAttackResult.NOT_PARRIED:
                            CancelAndDestroy();
                            break;
                        case SlashAttackResult.PARRIED:
                            if (++parriedAttacks == Attacks.Count)
                            {
                                RevekAddons.SpawnSoul(attack.ParryPos);
                                RevekAddons.GetHurtClip().PlayAtPosition(attack.ParryPos);

                                Callback?.Invoke(SlashAttackResult.PARRIED);
                                Callback = null;
                                Destroy(gameObject);
                            }
                            break;
                    }
                };
            }
        }
    }

    internal void CancelAndDestroy()
    {
        Cancel();
        Destroy(gameObject);
    }

    internal void Cancel()
    {
        if (cancelled) return;

        Callback?.Invoke(SlashAttackResult.NOT_PARRIED);
        Callback = null;
        cancelled = true;

        List<SlashAttack> attacks = [.. activeAttacks];
        attacks.ForEach(c => c.Cancel());
    }

    private void OnDestroy() => Cancel();
}
