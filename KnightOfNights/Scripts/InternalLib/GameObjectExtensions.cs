using KnightOfNights.Scripts.SharedLib;
using PurenailCore.GOUtil;
using System.Collections;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal static class GameObjectExtensions
{
    public static void PlayAtPosition(this AudioClip self, Vector2 pos)
    {
        var obj = new GameObject();
        obj.transform.position = pos;
        var audio = obj.AddComponent<AudioSource>();
        audio.outputAudioMixerGroup = AudioMixerGroups.Actors();

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
