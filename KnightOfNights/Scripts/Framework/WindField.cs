using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.CollectionUtil;
using PurenailCore.ModUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

internal record WFZCallbackRecord(int Priority, WindFieldZoneCallback Callback) { }

[Shim]
internal enum WindFieldAggregator
{
    Sum = 0,
    Average = 1,
    MaxMagnitude = 2,
}

internal enum WindTargetType
{
    Hero,
    Particle
}

[Shim]
internal class WindField : MonoBehaviour
{
    [ShimField] public WindFieldAggregator Aggregator;
    [ShimField] public float Prewarm;
    [ShimField] public float DownTime;
    [ShimField] public float TransitionTime;
    [ShimField] public float UpTime;
    [ShimField] public float HeroWindAccel;
    [ShimField] public float ParticleWindAccel;
    [ShimField] public float ParticleTargetMultiplier;

    private readonly RectMultimap<WFZCallbackRecord> windZoneCallbacks = [];
    private float windAccum;

    private static readonly HashSet<WindField> windFields = [];

    internal static IEnumerable<WindField> ActiveWindFields() => windFields;

    internal static Vector2 ActiveWindEffects(Vector2 pos, WindTargetType windTargetType) => windFields.Select(w => w.WindSpeedAtPos(pos, windTargetType)).Sum();

    internal static Vector2 HeroWindEffects() => windFields.Select(w => w.heroWindEffect).Sum();

    private static readonly CompactIdGen idGen = new();

    internal int Id { get; private set; } = -1;

    internal Vector2 WindSpeedAtPos(Vector2 p, WindTargetType windTargetType)
    {
        Vector2 sum = Vector2.zero;
        int priority = int.MinValue;
        List<Vector2> vectors = [];
        foreach (var cb in windZoneCallbacks.Get(p).OrderByDescending(c => c.Priority))
        {
            if (cb.Priority < priority) break;
            else if (cb.Priority > priority) vectors.Clear();
            if (!cb.Callback(p, out var windSpeed)) continue;

            priority = cb.Priority;
            vectors.Add(windSpeed);
        }

        float multiplier = windTargetType switch { WindTargetType.Hero => 1, WindTargetType.Particle => ParticleTargetMultiplier, _ => throw windTargetType.InvalidEnum() };
        return Aggregator switch
        {
            WindFieldAggregator.Sum => vectors.Sum(),
            WindFieldAggregator.Average => vectors.Sum() / Math.Max(1, vectors.Count),
            WindFieldAggregator.MaxMagnitude => vectors.SelectMin(v => -v.sqrMagnitude),
            _ => throw Aggregator.InvalidEnum()
        } * windAccum * multiplier;
    }

    private void OnEnable()
    {
        Id = idGen.Acquire();
        windFields.Add(this);
        foreach (var windZone in gameObject.GetComponentsInChildren<WindFieldZone>())
            foreach (var (rect, cb) in windZone.GetCallbacks())
                windZoneCallbacks.Add(rect, new WFZCallbackRecord(windZone.Priority, cb));

        var routine = Coroutines.Sequence(Routine());
        routine.Update(Prewarm);
        this.StartLibCoroutine(routine);
    }

    private void OnDisable()
    {
        windFields.Remove(this);
        idGen.Release(Id);
        Id = -1;
    }

    private IEnumerator<CoroutineElement> Routine()
    {
        while (true)
        {
            yield return Coroutines.SleepSeconds(DownTime);
            yield return Coroutines.SleepSecondsUpdatePercent(TransitionTime, p =>
            {
                windAccum = (1 + Mathf.Sin((p - 0.5f) * Mathf.PI)) / 2;
                return false;
            });
            yield return Coroutines.SleepSeconds(UpTime);
            yield return Coroutines.SleepSecondsUpdatePercent(TransitionTime, p =>
            {
                windAccum = (1 + Mathf.Sin((0.5f - p) * Mathf.PI)) / 2;
                return false;
            });
        }
    }

    private Vector2 heroWindEffect;

    private void FixedUpdate()
    {
        heroWindEffect.AdvanceVecAbs(HeroWindAccel * Time.fixedDeltaTime, WindSpeedAtPos(HeroController.instance.transform.position, WindTargetType.Hero));

        var cState = HeroController.instance.cState;
        if (cState.hazardRespawning || cState.dead) heroWindEffect = Vector2.zero;
    }

    private static Vector2 ModifyHeroVelocity(Vector2 velocity)
    {
        foreach (var windField in ActiveWindFields()) velocity += windField.heroWindEffect;
        return velocity;
    }

    private static bool loaded = false;
    internal static void Load()
    {
        if (loaded) return;

        loaded = true;
        HeroVelocityModifier.AddModifier(0, ModifyHeroVelocity);
    }

    static WindField() => Load();
}
