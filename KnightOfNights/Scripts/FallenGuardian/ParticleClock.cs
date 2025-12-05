using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.GOUtil;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class ParticleClock : MonoBehaviour
{
    [ShimField] public List<GameObject> Prefabs = [];
    [ShimField] public float ClockRadius;
    [ShimField] public float SpokeRadius;
    [ShimField] public float FadeSpeed;
    [ShimField] public int NumSpokes;

    private Transform? parentTransform;
    private float animTime;
    private float circleTime;
    private float fadeTime;

    internal static ParticleClock Spawn(Transform parentTransform, float animTime, float circleTime, float fadeTime)
    {
        var prefab = KnightOfNightsBundleAPI.LoadPrefab<GameObject>("ParticleClock");
        prefab.SetActive(false);

        var obj = Instantiate(prefab, parentTransform.position, Quaternion.identity);

        var clock = obj.GetComponent<ParticleClock>();
        clock.parentTransform = parentTransform;
        clock.animTime = animTime;
        clock.circleTime = circleTime;
        clock.fadeTime = fadeTime;

        obj.SetActive(true);
        return clock;
    }

    private bool tracking = true;

    private void Update()
    {
        if (!tracking || parentTransform == null) return;
        transform.position = parentTransform.position;
    }

    private bool cancelled = false;

    internal void Cancel() => cancelled = true;

    private void OnEnable() => this.StartLibCoroutine(Run());

    private IEnumerator<CoroutineElement> Finish()
    {
        tracking = false;

        yield return Coroutines.SleepSeconds(fadeTime);
        yield return Coroutines.SleepFrame();

        spokes.ForEach(s => s.Recycle());
        Destroy(gameObject);
    }

    private IEnumerator<CoroutineElement> Run()
    {
        var oneof = Coroutines.OneOf(Coroutines.Sequence(RunImpl()), Coroutines.SleepUntil(() => cancelled));
        yield return oneof;

        if (oneof.Choice == 0) yield break;
        
        // Cancelled.
        foreach (var spoke in spokes) spoke.AlphaFade = 1f / fadeTime;
        yield return Coroutines.Sequence(Finish());
    }

    private readonly List<ParticleClockSpoke> spokes = [];

    private IEnumerator<CoroutineElement> RunImpl()
    {
        float circleTimeGap = circleTime / (NumSpokes - 1);
        float circleAngleGap = 360f / NumSpokes;

        GameObject prefab = Prefabs.Choose();
        for (int i = 0; i < NumSpokes; i++)
        {
            var pos = transform.position + Quaternion.Euler(0, 0, 90 - (i + 1) * circleAngleGap) * new Vector3(ClockRadius, 0, 0);
            var obj = prefab.Spawn(pos, Quaternion.identity);
            obj.transform.SetParent(transform, true);
            obj.transform.localScale = new(SpokeRadius, SpokeRadius, 1);

            var spoke = obj.GetComponent<ParticleClockSpoke>();
            spoke.SetAnimSpeed(1f / animTime);
            spokes.Add(spoke);
            obj.SetActive(true);

            if (i != NumSpokes - 1) yield return Coroutines.SleepSeconds(circleTimeGap);
        }

        yield return Coroutines.SleepSeconds(animTime);
        for (int i = 0; i < NumSpokes; i++)
        {
            var spoke = spokes[i];
            spoke.AlphaFade = 1f / fadeTime;
            spoke.Velocity = Quaternion.Euler(0, 0, 90 - (i + 1) * circleAngleGap) * new Vector3(FadeSpeed, 0, 0);
        }

        yield return Coroutines.Sequence(Finish());
    }
}

[Shim]
internal class ParticleClockSpoke : MonoBehaviour
{
    [ShimField] public Animator? Animator;
    [ShimField] public SpriteRenderer? spriteRenderer;

    internal Vector2 Velocity;
    internal float AlphaFade;

    private float alpha = 1;
    private float rotation;
    private float rotSpeed;

    internal void SetAnimSpeed(float speed)
    {
        Animator!.Rebind();
        Animator.Update(0);
        Animator.speed = speed;
    }

    private void OnEnable()
    {
        Velocity = Vector2.zero;
        AlphaFade = 0;
        alpha = 1;
        spriteRenderer!.SetAlpha(1);
        rotation = Random.Range(0f, 360f);
        rotSpeed = Random.Range(-180f, 180f);
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
    }

    private void Update()
    {
        rotation += Time.deltaTime * rotSpeed;
        transform.localRotation = Quaternion.Euler(0, 0, rotation);

        var pos = transform.position.To2d();
        pos += Velocity * Time.deltaTime;
        transform.position = pos;

        if (AlphaFade > 0)
        {
            alpha -= Time.deltaTime * AlphaFade;
            if (alpha < 0) alpha = 0;
        }
        spriteRenderer!.SetAlpha(alpha);
    }
}
