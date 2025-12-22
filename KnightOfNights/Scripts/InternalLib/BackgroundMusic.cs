using KnightOfNights.Scripts.SharedLib;
using PurenailCore.GOUtil;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class BackgroundMusic : MonoBehaviour, IPersistentBehaviour<BackgroundMusic, BackgroundMusicManager>
{
    [ShimField] public AudioClip? Music;

    private AudioSource? audio;

    public void AwakeWithManager(BackgroundMusicManager initManager)
    {
        if (Music == null) return;

        audio = gameObject.GetOrAddComponent<AudioSource>();
        audio.clip = Music!;
        audio.loop = true;
        audio.outputAudioMixerGroup = AudioMixerGroups.Music();
        audio.Play();
    }

    public void SceneChanged(BackgroundMusicManager newManager) { }

    public void Stop()
    {
        this.StartLibCoroutine(FadeOut(1f));
        gameObject.DestroyAfter(10f);
    }

    private float fade = 1f;

    private CoroutineElement FadeOut(float duration) => Coroutines.SleepSecondsUpdatePercent(duration, pct =>
    {
        fade = Mathf.Min(fade, 1 - pct);
        return false;
    });

    private void Update()
    {
        if (audio == null) return;
        audio.volume = fade;
    }
}

[Shim]
internal class BackgroundMusicManager : PersistentBehaviourManager<BackgroundMusic, BackgroundMusicManager>
{
    public override BackgroundMusicManager Self() => this;
}
