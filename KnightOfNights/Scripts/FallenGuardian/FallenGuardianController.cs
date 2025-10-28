using KnightOfNights.Scripts.SharedLib;
using PurenailCore.CollectionUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class FallenGuardianController : MonoBehaviour
{
    [ShimField] public float SequenceDelay;
    [ShimField] public float NormalTelegraph;
    [ShimField] public float ShortTelegraph;
    [ShimField] public float Deceleration;
    [ShimField] public float LongWait;
    [ShimField] public float ShortWait;
    [ShimField] public float SplitOffset;

    private void OnEnable() => StartCoroutine(RunBoss());

    private IEnumerator<SlashAttackSequence> SpecTutorial()
    {
        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.LEFT.WithTelegraph(NormalTelegraph)),
            (LongWait, SlashAttackSpec.RIGHT.WithTelegraph(NormalTelegraph)),
            (LongWait, SlashAttackSpec.LEFT.WithTelegraph(NormalTelegraph))
        ]);

        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.LEFT.WithTelegraph(NormalTelegraph)),
            (LongWait, SlashAttackSpec.LEFT.WithTelegraph(NormalTelegraph)),
            (LongWait, SlashAttackSpec.RIGHT.Up(SplitOffset).WithTelegraph(NormalTelegraph)),
            (ShortWait, SlashAttackSpec.RIGHT.Down(SplitOffset).WithTelegraph(ShortTelegraph))
        ]);

        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.RIGHT.WithTelegraph(NormalTelegraph)),
            (LongWait, SlashAttackSpec.LEFT.Up(SplitOffset).WithTelegraph(NormalTelegraph)),
            (ShortWait, SlashAttackSpec.LEFT.Down(SplitOffset).WithTelegraph(ShortTelegraph)),
            (LongWait, SlashAttackSpec.RIGHT.Down(SplitOffset).WithTelegraph(NormalTelegraph)),
            (ShortWait, SlashAttackSpec.RIGHT.Up(SplitOffset).WithTelegraph(ShortTelegraph))
        ]);

        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.LEFT.WithTelegraph(NormalTelegraph)),
            (ShortWait, SlashAttackSpec.RIGHT.WithTelegraph(ShortTelegraph)),
            (LongWait, SlashAttackSpec.RIGHT.WithTelegraph(NormalTelegraph)),
            (ShortWait, SlashAttackSpec.LEFT.WithTelegraph(ShortTelegraph)),
            (ShortWait, SlashAttackSpec.HIGH_LEFT.WithTelegraph(ShortTelegraph)),
            (0f, SlashAttackSpec.HIGH_RIGHT.WithTelegraph(ShortTelegraph))
        ]);

        // FIXME: More
    }

    private IEnumerator RunBoss()
    {
        IEnumerator<SlashAttackSequence> tutorial = SpecTutorial();
        while (tutorial.MoveNext())
        {
            var sequence = tutorial.Current;

            while (true)
            {
                yield return new WaitForSeconds(SequenceDelay);

                Wrapped<SlashAttackResult> result = new(SlashAttackResult.PENDING);
                sequence.Play(r => result.Value = r);

                yield return new WaitUntil(() => result.Value != SlashAttackResult.PENDING);
                if (result.Value == SlashAttackResult.NOT_PARRIED) continue;
                else break;
            }
        }
    }
}
