using KnightOfNights.Scripts.SharedLib;
using PurenailCore.GOUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal static class GameObjectExtensions
{
    public static void StartLibCoroutine(this MonoBehaviour self, CoroutineElement co) => self.StartCoroutine(EvaluateLibCoroutine(co));

    public static void StartLibCoroutine(this MonoBehaviour self, IEnumerator<CoroutineElement> enumerator) => self.StartCoroutine(EvaluateLibCoroutine(CoroutineSequence.Create(enumerator)));

    private static IEnumerator EvaluateLibCoroutine(CoroutineElement co)
    {
        while (!co.Update(Time.deltaTime).done) yield return 0;
    }

    public static void PlayAtPosition(this AudioClip self, Vector2 pos, float pitch = 1f)
    {
        var obj = new GameObject();
        obj.transform.position = pos;
        var audio = obj.AddComponent<AudioSource>();
        audio.outputAudioMixerGroup = AudioMixerGroups.Actors();
        audio.pitch = pitch;

        IEnumerator Routine()
        {
            yield return null;
            audio.PlayOneShot(self);

            yield return new WaitUntil(() => !audio.isPlaying);
            yield return null;
            Object.Destroy(obj);
        }
        obj.AddComponent<Dummy>().StartCoroutine(Routine());
    }
}
