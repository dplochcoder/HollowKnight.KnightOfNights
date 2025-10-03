using KnightOfNights.Scripts.SharedLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class FallenGuardianController : MonoBehaviour
{
    private void OnEnable() => StartCoroutine(RunBoss());

    private IEnumerator<SlashAttackSequence> SpecTutorial()
    {
        // FIXME
        yield break;
    }

    private IEnumerator RunBoss()
    {
        IEnumerator<SlashAttackSequence> tutorial = SpecTutorial();
        while (tutorial.MoveNext())
        {
            var segment = tutorial.Current;

            // FIXME
            yield break;
        }
    }
}
