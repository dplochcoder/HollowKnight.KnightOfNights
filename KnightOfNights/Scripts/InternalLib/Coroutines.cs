using Satchel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

public delegate bool CoroutinePercentUpdate(float pct);

public delegate bool CoroutineTimeUpdate(float deltaTime);

public record CoroutineUpdate
{
    public CoroutineUpdate(bool done, float extraTime)
    {
        this.done = done;
        this.extraTime = extraTime;
    }

    public bool done;
    public float extraTime;
}

public abstract class CoroutineElement
{
    public float ExtraTime { get; private set; }

    public CoroutineUpdate Update(float deltaTime)
    {
        var update = UpdateImpl(deltaTime);
        if (update.done) ExtraTime = update.extraTime;
        return update;
    }

    // Returns false when incomplete.
    // Returns (true, remainingTime) when complete.
    protected abstract CoroutineUpdate UpdateImpl(float deltaTime);

    public CoroutineDisposable WithDisposable(params CoroutineElement[] choices) => new(this, Coroutines.AllOf(choices));

    public CoroutineElement Then(CoroutineElement next)
    {
        List<CoroutineElement> list = [this, next];
        return Coroutines.Sequence(list.GetEnumerator());
    }

    public CoroutineElement Then(IEnumerator<CoroutineElement> next) => Then(Coroutines.Sequence(next));
}

public class CoroutineInstant(Action action) : CoroutineElement
{
    private readonly Action action = action;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        action();
        return new(true, deltaTime);
    }
}

public class CoroutineNever : CoroutineElement
{
    protected override CoroutineUpdate UpdateImpl(float deltaTime) => new(false, 0);
}

public class CoroutineLoop(Action<float> action) : CoroutineElement
{
    private readonly Action<float> action = action;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        action(deltaTime);
        return new(false, 0);
    }
}

public class SleepSeconds(float remaining) : CoroutineElement
{
    private readonly float orig = remaining;
    private float remaining = remaining;
    private readonly CoroutinePercentUpdate? percentUpdate;
    private readonly CoroutineTimeUpdate? timeUpdate;

    public SleepSeconds(float remaining, CoroutinePercentUpdate percentUpdate) : this(remaining) => this.percentUpdate = percentUpdate;

    public SleepSeconds(float remaining, CoroutineTimeUpdate timeUpdate) : this(remaining) => this.timeUpdate = timeUpdate;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        if (remaining <= deltaTime)
        {
            percentUpdate?.Invoke(1.0f);
            timeUpdate?.Invoke(remaining);
            return new(true, deltaTime - remaining);
        }
        remaining -= deltaTime;

        bool done = false;
        if (percentUpdate != null) done = percentUpdate.Invoke(1.0f - (remaining / orig));
        if (timeUpdate != null) done = timeUpdate.Invoke(deltaTime);
        return new(done, 0);
    }
}

public class SleepFrames(int remaining, Action<float>? deltaConsumer = null) : CoroutineElement
{
    private int remaining = remaining;
    private readonly Action<float>? deltaConsumer = deltaConsumer;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        deltaConsumer?.Invoke(deltaTime);
        if (remaining <= 0) return new(true, deltaTime);

        --remaining;
        return new(false, 0);
    }
}

public class SleepUntil : CoroutineElement
{
    private readonly Func<float, bool> condition;

    public SleepUntil(Func<bool> condition) => this.condition = _ => condition();
    public SleepUntil(Func<float, bool> condition) => this.condition = condition;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        bool cond = condition(deltaTime);
        return new(cond, cond ? deltaTime : 0);
    }
}

public class SleepUntilTimeout(SleepUntil sleepUntil, SleepSeconds timeout) : CoroutineElement
{
    private readonly CoroutineOneOf choice = new([sleepUntil, timeout]);

    public bool TimedOut { get; private set; }

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        var update = choice.Update(deltaTime);
        TimedOut = update.done && choice.Choice == 1;
        return update;
    }
}

public class SleepUntilCondHolds(Func<bool> condition, float time) : CoroutineElement
{
    private readonly Func<bool> condition = condition;
    private readonly float time = time;
    private SleepSeconds? timer;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        if (condition())
        {
            timer ??= new(time);
            return timer.Update(deltaTime);
        }

        timer = null;
        return new(false, 0);
    }
}

public class CoroutineGenerator(Func<float, CoroutineElement> generator) : CoroutineElement
{
    private CoroutineElement? elem;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        elem ??= generator(deltaTime);
        return elem.Update(deltaTime);
    }
}

public class CoroutineTime
{
    public float DeltaTime { get; private set; }

    public CoroutineTime(out Action<float> setter) => setter = t => DeltaTime = t;
}

public class CoroutineSequence(IEnumerator<CoroutineElement> coroutine, CoroutineSequence.StopCondition? stopCondition = null) : CoroutineElement
{
    public delegate bool StopCondition();

    private readonly IEnumerator<CoroutineElement> coroutine = coroutine;
    private readonly StopCondition? stopCondition = stopCondition;
    private CoroutineElement? current;

    public static CoroutineSequence Create(IEnumerator<CoroutineElement> coroutine, StopCondition? stopCondition = null) => new(coroutine, stopCondition);

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        if (stopCondition?.Invoke() ?? false) return new(true, deltaTime);

        if (current == null)
        {
            SetCurrentDelta(deltaTime);
            current = coroutine.MaybeMoveNext();
            if (current == null) return new(true, deltaTime);
        }

        while (current != null && deltaTime > 0)
        {
            var update = current.Update(deltaTime);
            if (update.done)
            {
                deltaTime = update.extraTime;
                SetCurrentDelta(deltaTime);
                current = coroutine.MaybeMoveNext();
            }
            else break;
        }

        if (current == null) return new(true, deltaTime);
        else return new(false, 0);
    }

    protected virtual void SetCurrentDelta(float deltaTime) { }
}

public class DeltaAwareCoroutineSequence : CoroutineSequence
{
    private readonly Action<float> deltaSetter;

    private DeltaAwareCoroutineSequence(IEnumerator<CoroutineElement> iter, StopCondition? stopCondition, Action<float> deltaSetter) : base(iter, stopCondition) => this.deltaSetter = deltaSetter;

    public static DeltaAwareCoroutineSequence Create(Func<CoroutineTime, IEnumerator<CoroutineElement>> generator, StopCondition? stopCondition)
    {
        CoroutineTime time = new(out var setter);
        var iter = generator(time);

        return new(iter, stopCondition, setter);
    }

    protected override void SetCurrentDelta(float deltaTime) => deltaSetter(deltaTime);
}

public class CoroutineDisposable(CoroutineElement required, CoroutineElement disposable) : CoroutineElement
{
    private readonly CoroutineElement required = required;
    private CoroutineElement? disposable = disposable;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        var update = required.Update(deltaTime);
        if (update.done)
        {
            disposable?.Update(deltaTime - update.extraTime);
            return update;
        }

        if (disposable != null)
        {
            var dUpdate = disposable.Update(deltaTime);
            if (dUpdate.done) disposable = null;
        }

        return update;
    }
}

public class CoroutineOneOf(List<CoroutineElement> choices) : CoroutineElement
{
    private readonly List<CoroutineElement> choices = choices;

    public int Choice { get; private set; } = -1;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        CoroutineUpdate update = new(false, 0);
        for (int i = 0; i < choices.Count; i++)
        {
            var u = choices[i].Update(deltaTime);
            if (u.done && (!update.done || u.extraTime < update.extraTime))
            {
                update = u;
                Choice = i;
            }
        }

        return update;
    }
}

public class CoroutineAllOf(List<CoroutineElement> requirements) : CoroutineElement
{
    private readonly List<CoroutineElement> requirements = requirements;

    protected override CoroutineUpdate UpdateImpl(float deltaTime)
    {
        List<CoroutineElement> remaining = [];
        float minRemaining = Mathf.Infinity;

        foreach (var requirement in requirements)
        {
            var update = requirement.Update(deltaTime);
            if (update.done) minRemaining = Mathf.Min(minRemaining, update.extraTime);
            else remaining.Add(requirement);
        }

        if (remaining.Count == 0)
        {
            requirements.Clear();
            return new(true, minRemaining);
        }
        else if (remaining.Count != requirements.Count)
        {
            requirements.Clear();
            requirements.AddRange(remaining);
        }

        return new(false, 0);
    }
}

public static class Coroutines
{
    public static CoroutineInstant Instant(Action action) => new(action);

    public static CoroutineNever Never() => new();

    public static CoroutineGenerator Deferred(Func<float, CoroutineElement> generator) => new(generator);

    public static CoroutineLoop Loop(Action action) => new(_ => action());
    public static CoroutineLoop Loop(Action<float> action) => new(action);

    public static CoroutineSequence Sequence<T>(IEnumerator<T> enumerator, CoroutineSequence.StopCondition? stopCondition = null) where T : CoroutineElement => new(enumerator, stopCondition);

    public static CoroutineSequence Sequence<T>(IEnumerable<T> enumerable, CoroutineSequence.StopCondition? stopCondition = null) where T : CoroutineElement => Sequence(enumerable.GetEnumerator(), stopCondition);

    public static DeltaAwareCoroutineSequence Sequence(Func<CoroutineTime, IEnumerator<CoroutineElement>> generator, CoroutineSequence.StopCondition? stopCondition = null) => DeltaAwareCoroutineSequence.Create(generator, stopCondition);

    public static CoroutineOneOf OneOf(params CoroutineElement[] choices) => new(choices.ToList());

    public static CoroutineAllOf AllOf(params CoroutineElement[] choices) => new(choices.ToList());

    // Sleep N frames
    public static SleepFrames SleepFrames(int frames, Action<float>? deltaConsumer = null) => new(frames, deltaConsumer);

    // Sleep one frame
    public static SleepFrames SleepFrame(Action<float>? deltaConsumer = null) => SleepFrames(1, deltaConsumer);

    // Sleep the specified number of seconds
    public static SleepSeconds SleepSeconds(float seconds) => new(seconds);

    // Sleep until condition() holds
    public static SleepUntil SleepUntil(Func<bool> condition) => new(condition);
    public static SleepUntil SleepUntil(Func<float, bool> condition) => new(condition);

    // Sleep until condition(), or 
    public static SleepUntilTimeout SleepUntilTimeout(Func<bool> condition, float seconds) => new(new(condition), new(seconds));

    public static SleepUntilCondHolds SleepUntilCondHolds(Func<bool> condition, float seconds) => new(condition, seconds);

    public static SleepSeconds Noop() => SleepSeconds(0);

    public static SleepSeconds SleepSecondsUpdatePercent(float seconds, CoroutinePercentUpdate update) => new(seconds, update);

    public static SleepSeconds SleepSecondsUpdateDelta(float seconds, CoroutineTimeUpdate update) => new(seconds, update);

    public static SleepUntil PlayTk2dAnimation(tk2dSpriteAnimator animator, string name, float speedup = 1)
    {
        animator.Play(name);
        return SleepUntil(delta =>
        {
            animator.UpdateAnimation(delta * (speedup - 1));
            return !animator.Playing;
        });
    }

    public static SleepUntil PlayAnimation(Animator animator, RuntimeAnimatorController controller)
    {
        animator.runtimeAnimatorController = controller;
        return SleepUntil(() => !animator.IsPlaying());
    }

    public static CoroutineElement PlayAnimations(Animator animator, List<RuntimeAnimatorController> controllers) => Sequence(controllers.Select(c => PlayAnimation(animator, c)));
}
