using KnightOfNights.Scripts.SharedLib;
using PurenailCore.GOUtil;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
[RequireComponent(typeof(AudioSource))]
internal class ApplyActorGroup : MonoBehaviour
{
    private void Awake() => this.GetComponent<AudioSource>()?.outputAudioMixerGroup = AudioMixerGroups.Actors();
}
