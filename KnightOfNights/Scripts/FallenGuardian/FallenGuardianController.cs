using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using KnightOfNights.Util;
using PurenailCore.CollectionUtil;
using PurenailCore.GOUtil;
using SFCore.Utils;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal enum AttackChoice
{
    AxeHopscotch,
    GorbStorm,
    PancakeWaveSlash,
    PancakeDiveStorm,
    ShieldCyclone,
    UltraInstinct,
    XeroArmada
}

[Shim]
internal class FallenGuardianAttack : MonoBehaviour
{
    [ShimField] public AttackChoice Choice;
    [ShimField] public float Weight;
    [ShimField] public int Cooldown;
    [ShimField] public float WeightIncrease;
    [ShimField] public List<AttackChoice> ForbiddenPredecessors = [];
}

[Shim]
internal class GorbStormStats : MonoBehaviour
{
    [ShimField] public float BobPeriod;
    [ShimField] public float BobRadius;
    [ShimField] public int BurstCountFinale;
    [ShimField] public int BurstCountSmall;
    [ShimField] public float FinaleMinDist;
    [ShimField] public float GracePeriod;
    [ShimField] public int NumSmallBursts;
    [ShimField] public float PitchIncrementFinale;
    [ShimField] public float PitchIncrementSmall;
    [ShimField] public float SmallXMin;
    [ShimField] public float SmallXMax;
    [ShimField] public float SmallYMin;
    [ShimField] public float SmallYMax;
    [ShimField] public float SpikeAccel;
    [ShimField] public int SpokeCountFinale;
    [ShimField] public int SpokeCountSmall;
    [ShimField] public float SpokeRotationFinale;
    [ShimField] public float SpokeRotationSmall;
    [ShimField] public float WaitAfterFinale;
    [ShimField] public float WaitAfterTeleport;
    [ShimField] public float WaitBeforeFinale;
    [ShimField] public float WaitBeforeTeleport;
    [ShimField] public float WaitFirst;
    [ShimField] public float WaitSpikeFinale;
    [ShimField] public float WaitSpikeSmall;
}

[Shim]
internal class StaggerStats : MonoBehaviour
{
    [ShimField] public float GracePeriod;
    [ShimField] public float Invuln;
    [ShimField] public float HitWait;
    [ShimField] public float MaxWait;
    [ShimField] public float NextAttackDelay;
    [ShimField] public float OscillationPeriod;
    [ShimField] public float OscillationRadius;
}

[Shim]
internal class UltraInstinctStats : MonoBehaviour
{
    [ShimField] public float Deceleration;
    [ShimField] public float Interval;
    [ShimField] public float Speed;
    [ShimField] public float Tail;
    [ShimField] public float Telegraph;
}

[Shim]
internal class FallenGuardianPhaseStats : MonoBehaviour
{
    [ShimField] public int MinHP;
    [ShimField] public AttackChoice FirstAttack;
    [ShimField] public List<FallenGuardianAttack> Attacks = [];
    [ShimField] public GorbStormStats? GorbStormStats;
    [ShimField] public StaggerStats? StaggerStats;
    [ShimField] public UltraInstinctStats? UltraInstinctStats;

    internal bool DidFirstAttack = false;
}

[Shim]
internal class FallenGuardianController : MonoBehaviour
{
    [ShimField] public FallenGuardianContainer? Container;

    [ShimField] public float SequenceDelay;
    [ShimField] public float Telegraph;
    [ShimField] public float Deceleration;
    [ShimField] public float LongWait;
    [ShimField] public float ShortWait;
    [ShimField] public float SplitOffset;
    [ShimField] public float EscalationPause;
    [ShimField] public int StaggerCount;
    [ShimField] public float StaggerDistance;

    [ShimField] public GameObject? StaggerBurst;
    [ShimField] public GameObject? TeleportBurst;

    [ShimField] public RuntimeAnimatorController? SpellCastToEndController;
    [ShimField] public RuntimeAnimatorController? SpellCastToLoopController;
    [ShimField] public RuntimeAnimatorController? SpellLoopController;
    [ShimField] public RuntimeAnimatorController? SpellLoopToSwordController;
    [ShimField] public RuntimeAnimatorController? SpellStartToLoopController;
    [ShimField] public RuntimeAnimatorController? StaggerController;
    [ShimField] public RuntimeAnimatorController? StaggerToRecoverController;
    [ShimField] public RuntimeAnimatorController? SwordToSpellController;
    [ShimField] public RuntimeAnimatorController? TeleportInController;
    [ShimField] public RuntimeAnimatorController? TeleportOutController;

    [ShimField] public List<FallenGuardianPhaseStats> PhaseStats = [];

    private HealthManager? healthManager;
    private NonBouncer? nonBouncer;
    private BoxCollider2D? collider;
    private DamageHero? damageHero;
    private Recoil? recoil;
    private Animator? animator;
    private AudioSource? audio;
    private Bobber? bobber;

    private ParticleSystem? idleParticles;

    private FallenGuardianPhaseStats? stats;

    private void InitParticles()
    {
        GameObject particles = Instantiate(KnightOfNightsPreloader.Instance.Revek!.FindChild("Idle Pt")!);
        particles.SetActive(true);
        particles.SetParent(gameObject);
        particles.transform.localPosition = Vector3.zero;
        idleParticles = particles.GetComponent<ParticleSystem>();
    }

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
        nonBouncer = GetComponent<NonBouncer>();
        collider = GetComponent<BoxCollider2D>();
        damageHero = GetComponent<DamageHero>();
        recoil = GetComponent<Recoil>();
        animator = GetComponent<Animator>();
        stats = PhaseStats[0];

        audio = gameObject.AddComponent<AudioSource>();
        audio.outputAudioMixerGroup = AudioMixerGroups.Actors();
        bobber = gameObject.AddComponent<Bobber>();
        bobber.enabled = false;

        SetTangible(false);
    }

    private void OnEnable() => this.StartLibCoroutine(RunBoss());

    private readonly HashSet<GameObject> recyclables = [];

    private void OnDestroy() => recyclables.ForEach(go =>
    {
        if (go.activeSelf) go.Recycle();
    });

    private Vector2 lastPos;

    private void Update()
    {
        stats = PhaseStats.Where(s => healthManager!.hp >= s.MinHP).First();

        var pos = transform.position;
        if (pos.x > 0 && pos.y > 0) lastPos = pos;
    }

    private IEnumerator<SlashAttackSequence> SpecTutorial()
    {
        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.LEFT.WithTelegraph(Telegraph)),
            (LongWait, SlashAttackSpec.LEFT.WithTelegraph(Telegraph)),
        ]);

        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.LEFT.WithTelegraph(Telegraph)),
            (LongWait, SlashAttackSpec.RIGHT.WithTelegraph(Telegraph)),
            (LongWait, SlashAttackSpec.LEFT.WithTelegraph(Telegraph))
        ]);

        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.LEFT.Up(SplitOffset).WithTelegraph(Telegraph)),
            (LongWait, SlashAttackSpec.LEFT.Down(SplitOffset).WithTelegraph(Telegraph)),
            (LongWait, SlashAttackSpec.RIGHT.Up(SplitOffset).WithTelegraph(Telegraph)),
            (0f, SlashAttackSpec.RIGHT.Down(SplitOffset).WithTelegraph(Telegraph + ShortWait))
        ], ShortWait);

        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.RIGHT.WithTelegraph(Telegraph)),
            (LongWait, SlashAttackSpec.LEFT.Up(SplitOffset).WithTelegraph(Telegraph)),
            (0f, SlashAttackSpec.LEFT.Down(SplitOffset).WithTelegraph(Telegraph + ShortWait)),
            (LongWait + ShortWait, SlashAttackSpec.RIGHT.Down(SplitOffset).WithTelegraph(Telegraph)),
            (0f, SlashAttackSpec.RIGHT.Up(SplitOffset).WithTelegraph(Telegraph + ShortWait))
        ], ShortWait);

        yield return new FlippableSlashAttackSequence([
            (0f, SlashAttackSpec.LEFT.WithTelegraph(Telegraph)),
            (0f, SlashAttackSpec.RIGHT.WithTelegraph(Telegraph + ShortWait)),
            (LongWait + ShortWait, SlashAttackSpec.RIGHT.WithTelegraph(Telegraph)),
            (0f, SlashAttackSpec.LEFT.WithTelegraph(Telegraph + ShortWait)),
            (3 * ShortWait, SlashAttackSpec.HIGH_LEFT.WithTelegraph(Telegraph)),
            (0f, SlashAttackSpec.HIGH_RIGHT.WithTelegraph(Telegraph + ShortWait))
        ], ShortWait);
    }

    private IEnumerator<CoroutineElement> RunBoss()
    {
        // TODO: Aura farm intro.

        IEnumerator<SlashAttackSequence> tutorial = SpecTutorial();
        if (SkipTutorial)
        {
            List<SlashAttackSequence> empty = [];
            tutorial = empty.GetEnumerator();
        }

        while (tutorial.MoveNext())
        {
            var sequence = tutorial.Current;

            while (true)
            {
                yield return Coroutines.SleepSeconds(SequenceDelay);

                Wrapped<SlashAttackResult> result = new(SlashAttackResult.PENDING);
                sequence.Play(r => result.Value = r);

                yield return Coroutines.SleepUntil(() => result.Value != SlashAttackResult.PENDING);
                if (result.Value == SlashAttackResult.NOT_PARRIED) continue;
                else break;
            }
        }

        // TODO: Aura farm escalation pause.

        if (SkipTutorial) EscalationPause = 1f;
        yield return Coroutines.SleepSeconds(EscalationPause);

        if (CharmIds.HeavyBlow.IsEquipped()) --StaggerCount;

        AttackChoice previousAttack = AttackChoice.UltraInstinct;
        while (true)
        {
            var attack = ChooseAttack(previousAttack);

            RecordChoice(attack);
            var oneof = Coroutines.OneOf(
                ExecuteAttack(attack),
                Coroutines.SleepUntil(() => healthManager!.hp <= 0),
                Coroutines.SleepUntil(() => staggerAttack != null));
            yield return oneof;

            if (oneof.Choice == 1)
            {
                OnDeath?.Invoke();
                yield break;
            }
            else if (oneof.Choice == 2) yield return Coroutines.Sequence(ExecuteStagger());

            // Continue to next attack.
        }
    }

    private int multiParries;
    private SlashAttack? staggerAttack;

    private bool MaybeStagger(SlashAttack attack)
    {
        if (++multiParries < StaggerCount) return false;

        multiParries = 0;
        staggerAttack = attack;
        return true;
    }

    private IEnumerator<CoroutineElement> ExecuteStagger()
    {
        var stats = this.stats!.StaggerStats!;

        var prevAttack = staggerAttack!;
        staggerAttack = null;
        OnStagger?.Invoke();
        OnStagger = null;

        var pos = prevAttack.ParryPos.To3d();

        // Force hero distance.
        var kPos = HeroController.instance.transform.position;
        if ((pos - kPos).magnitude < StaggerDistance) pos = kPos + (pos - kPos).normalized * StaggerDistance;

        // Force above grounds.
        if (pos.y <= Container!.Arena!.bounds.min.y + 1)
        {
            pos.y = Container!.Arena!.bounds.min.y + 1;

            if ((pos - kPos).magnitude < StaggerDistance)
            {
                var s = Mathf.Sign(pos.x - kPos.x);
                pos.x = kPos.x + s * StaggerDistance;
            }
        }

        transform.position = pos;
        transform.localScale = new(prevAttack.Spec.SpawnOffset.x >= 0 ? 1 : -1, 1, 1);
        StaggerBurst?.Spawn(pos);
        KnightOfNightsPreloader.Instance.StunEffect!.Spawn(pos);

        if (!recoil!.IsRecoiling && prevAttack.DamageDirection.HasValue && prevAttack.MagnitudeMultiplier.HasValue)
            recoil.RecoilByDirection(DirectionUtils.GetCardinalDirection(prevAttack.DamageDirection.Value), prevAttack.MagnitudeMultiplier.Value);

        SetTangible(false);
        animator!.runtimeAnimatorController = StaggerController!;
        yield return Coroutines.SleepSeconds(stats.Invuln);

        bobber?.ResetRandom(stats.OscillationRadius, stats.OscillationPeriod);
        SetTangible(true);
        yield return Coroutines.SleepSeconds(stats.GracePeriod);

        var prev = healthManager!.hp;
        yield return Coroutines.OneOf(
            Coroutines.SleepUntil(() => healthManager!.hp < prev).Then(Coroutines.SleepSeconds(stats.HitWait)),
            Coroutines.SleepSeconds(stats.MaxWait));

        bobber!.enabled = false;
        animator.runtimeAnimatorController = StaggerToRecoverController!;

        Wrapped<bool> teleportOut = new(false);
        void Listen() => teleportOut.Value = true;
        OnTeleportOut += Listen;
        yield return Coroutines.SleepUntil(() => teleportOut.Value);
        OnTeleportOut -= Listen;

        transform.position = new(-100, -100);
        yield return Coroutines.SleepSeconds(stats.NextAttackDelay);

        staggerAttack = null;
    }

    internal event System.Action? OnDeath;  // Invoked once.
    private event System.Action? OnStagger;  // Cleared after use.

    private readonly HashMultiset<AttackChoice> Cooldowns = new();
    private readonly HashMultiset<AttackChoice> WeightAdditions = new();

    private void RecordChoice(AttackChoice choice)
    {
        stats!.DidFirstAttack = true;

        foreach (var c in stats!.Attacks)
        {
            if (c.Choice == choice)
            {
                WeightAdditions.RemoveAll(c.Choice);
                Cooldowns.Add(c.Choice, c.Cooldown);
            }
            else
            {
                if (Cooldowns.CountOf(c.Choice) > 0) Cooldowns.Remove(c.Choice);
                else WeightAdditions.Add(c.Choice);
            }
        }
    }

    private AttackChoice ChooseAttack(AttackChoice previous)
    {
        if (ForceAttack.HasValue) return ForceAttack.Value;
        if (!stats!.DidFirstAttack) return stats.FirstAttack;

        IndexedWeightedSet<AttackChoice> choices = new();
        foreach (var c in stats!.Attacks)
        {
            if (previous == c.Choice) continue;
            if (Cooldowns.CountOf(c.Choice) > 0) continue;
            if (c.ForbiddenPredecessors.Contains(previous)) continue;

            choices.Add(c.Choice, c.Weight + c.WeightIncrease * WeightAdditions.CountOf(c.Choice));
        }
        return choices.Sample();
    }

#if DEBUG
    private const bool SkipTutorial = true;
    private static readonly AttackChoice? ForceAttack = AttackChoice.GorbStorm;
#else
    private const bool SkipTutorial = false;
    private static readonly AttackChoice? ForceAttack = null;
#endif

    private CoroutineElement ExecuteAttack(AttackChoice choice)
    {
        switch (choice)
        {
            case AttackChoice.AxeHopscotch:
                return Coroutines.SleepSeconds(1);
            case AttackChoice.GorbStorm:
                return Coroutines.Sequence(GorbStorm());
            case AttackChoice.PancakeDiveStorm:
                return Coroutines.SleepSeconds(1);
            case AttackChoice.PancakeWaveSlash:
                return Coroutines.SleepSeconds(1);
            case AttackChoice.ShieldCyclone:
                return Coroutines.SleepSeconds(1);
            case AttackChoice.UltraInstinct:
                return Coroutines.Sequence(UltraInstinct());
            case AttackChoice.XeroArmada:
                return Coroutines.SleepSeconds(1);
            default:
                KnightOfNightsMod.BUG($"Unhandled attack: {choice}");
                return Coroutines.SleepSeconds(1);
        }
    }

    private static List<List<SlashAttackSpec>> GenUltraInstinctPatterns(List<SlashAttackSpec> input)
    {
        static bool Equal(SlashAttackSpec a, SlashAttackSpec b)
        {
            if (a.AllowedHits.Count != b.AllowedHits.Count) return false;

            HashSet<string> bSet = [.. b.AllowedHits];
            return a.AllowedHits.All(bSet.Contains);
        }

        List<List<SlashAttackSpec>> ret = [];
        input.ForEachPermutation(p =>
        {
            if (p.Pairs().All(pair => !Equal(pair.Item1, pair.Item2))) ret.Add([.. p]);
        });
        return ret;
    }
    private static readonly List<List<SlashAttackSpec>> UltraInstinctPatterns = GenUltraInstinctPatterns([
        SlashAttackSpec.LEFT.Down(1.25f),
        SlashAttackSpec.LEFT.Up(1.25f),
        SlashAttackSpec.RIGHT.Down(1.25f),
        SlashAttackSpec.RIGHT.Up(1.25f),
        SlashAttackSpec.HIGH_LEFT,
        SlashAttackSpec.HIGH_RIGHT
    ]);

    private static readonly List<List<SlashAttackSpec>> SuperUltraInstinctPatterns = GenUltraInstinctPatterns([
        SlashAttackSpec.LEFT,
        SlashAttackSpec.LEFT,
        SlashAttackSpec.LEFT,
        SlashAttackSpec.RIGHT,
        SlashAttackSpec.RIGHT,
        SlashAttackSpec.HIGH_LEFT,
        SlashAttackSpec.HIGH_RIGHT,
        SlashAttackSpec.HIGH_RIGHT
    ]);

    private IEnumerator<CoroutineElement> UltraInstinct()
    {
        var stats = this.stats!.UltraInstinctStats!;

        var specs = UltraInstinctPatterns.Choose();
        specs = [.. specs.Select(s => s.WithTelegraph(stats.Telegraph).WithSpeed(stats.Speed).WithDeceleration(stats.Deceleration))];
        if (Random.Range(0, 2) == 0) specs = [.. specs.Select(s => s.Flipped())];

        List<SlashAttack> attacks = [];
        HashSet<SlashAttack> activeAttacks = [];
        foreach (var spec in specs)
        {
            var attack = SlashAttack.Spawn(spec);
            attacks.Add(attack);
            activeAttacks.Add(attack);
            yield return Coroutines.SleepSeconds(stats.Interval);
        }

        Wrapped<SlashAttack> lastAttack = new(activeAttacks.First());
        yield return Coroutines.SleepUntil(() => {
            if (activeAttacks.Count > 0)
            {
                lastAttack.Value = activeAttacks.First();
                activeAttacks.RemoveWhere(a => a.Result != SlashAttackResult.PENDING);
            }

            return activeAttacks.Count == 0;
        });

        if (attacks.All(a => a.Result == SlashAttackResult.PARRIED))
        {
            var attack = lastAttack.Value;

            RevekAddons.SpawnSoul(attack.ParryPos);
            RevekAddons.GetHurtClip().PlayAtPosition(attack.ParryPos);
            healthManager!.hp -= attacks.Select(a => a.DamageDealt).Sum();

            if (MaybeStagger(attack)) RevekAddons.SpawnSoul(attack.ParryPos);
        }

        yield return Coroutines.SleepSeconds(stats.Tail);
    }

    private IEnumerator<CoroutineElement> LaunchGorbSpikes(int count, float offset, int numBursts, float wait, float pitchIncrement)
    {
        float angle = Random.Range(0f, 360f);
        float off = 360f * offset / count * (MathExt.CoinFlip() ? 1 : -1);
        for (int i = 0; i < numBursts; i++)
        {
            var pos = transform.position;
            KnightOfNightsPreloader.Instance.MageShotClip!.PlayAtPosition(pos, 1f + i * pitchIncrement);
            for (int j = 0; j < count; j++)
            {
                var obj = KnightOfNightsPreloader.Instance.GorbSpear!.Spawn(pos, Quaternion.Euler(0, 0, angle));
                recyclables.Add(obj);

                angle += 360f / count;

                var accel = stats!.GorbStormStats!.SpikeAccel;
                var control = obj.LocateMyFSM("Control");
                var poke = control.GetFsmState("Poke Out");
                poke.GetFirstActionOfType<SetVelocityAsAngle>().speed = 30 * accel;
                poke.GetFirstActionOfType<DecelerateV2>().deceleration = 0.88f * Mathf.Pow(accel, 0.02f);
                poke.GetFirstActionOfType<Wait>().time = 0.5f / accel;
                control.GetFsmState("Fire").GetFirstActionOfType<SetVelocityAsAngle>().speed = 25 * accel;

                control.GetFsmState("Recycle").AddFirstAction(new Lambda(() =>
                {
                    poke.GetFirstActionOfType<SetVelocityAsAngle>().speed = 30;
                    poke.GetFirstActionOfType<DecelerateV2>().deceleration = 0.88f;
                    poke.GetFirstActionOfType<Wait>().time = 0.5f;
                    control.GetFsmState("Fire").GetFirstActionOfType<SetVelocityAsAngle>().speed = 25;
                }));

                obj.SetActive(true);
            }

            angle += off;
            if (i != numBursts - 1) yield return Coroutines.SleepSeconds(wait);
        }
    }

    private void TeleportInstant(Vector2 newPos)
    {
        TeleportBurst!.Spawn(transform.position).transform.localScale = new(0.5f, 0.5f, 1);

        transform.position = newPos;
        KnightOfNightsPreloader.Instance.MageTeleportClip!.PlayAtPosition(transform.position, 1.1f);
        TeleportBurst?.Spawn(transform.position);
    }

    private IEnumerator<CoroutineElement> GorbStorm()
    {
        var stats = this.stats!.GorbStormStats!;

        Vector2 PickSmallPos(bool left)
        {
            var xMin = left ? -stats.SmallXMax : stats.SmallXMin;
            var xMax = left ? -stats.SmallXMin : stats.SmallXMax;

            var kPos = HeroController.instance.transform.position;
            return new(kPos.x + Random.Range(xMin, xMax), Container!.Arena!.bounds.min.y + Random.Range(stats.SmallYMin, stats.SmallYMax));
        }

        var left = MathExt.CoinFlip();
        bool first = true;
        for (int i = 0; i < stats.NumSmallBursts; i++)
        {
            var prevPos = transform.position;
            var pos = PickSmallPos(left);
            left = !left;

            if (first)
            {
                first = false;

                transform.position = pos;
                bobber?.ResetRandom(stats.BobRadius, stats.BobPeriod);

                this.StartLibCoroutine(Coroutines.PlayAnimations(animator!, [TeleportInController!, SwordToSpellController!, SpellStartToLoopController!]));
                yield return Coroutines.SleepSeconds(stats.WaitFirst);
            }
            else
            {
                TeleportInstant(pos);
                yield return Coroutines.SleepSeconds(stats.WaitAfterTeleport);
            }

            yield return Coroutines.Sequence(LaunchGorbSpikes(stats.SpokeCountSmall, stats.SpokeRotationSmall, stats.BurstCountSmall, stats.WaitSpikeSmall, stats.PitchIncrementSmall));
            yield return Coroutines.SleepSeconds(stats.WaitBeforeTeleport);
        }

        Vector2 PickBigPos()
        {
            var bounds = Container!.GorbStormFinaleBox!.bounds;
            var kPos = HeroController.instance.transform.position.To2d();
            for (int i = 0; i < 100; i++)
            {
                var p = bounds.Random();
                if ((kPos - p).magnitude >= stats.FinaleMinDist) return p;
            }

            return bounds.center;
        }

        TeleportInstant(PickBigPos());

        yield return Coroutines.SleepSeconds(stats.WaitBeforeFinale);
        animator!.runtimeAnimatorController = SpellCastToLoopController!;

        Wrapped<bool> spell = new(false);
        OnCastSpell += () => spell.Value = true;
        yield return Coroutines.SleepUntil(() => spell.Value = true);

        yield return Coroutines.Sequence(LaunchGorbSpikes(stats.SpokeCountFinale, stats.SpokeRotationFinale, stats.BurstCountFinale, stats.WaitSpikeFinale, stats.PitchIncrementFinale));
        yield return Coroutines.SleepSeconds(stats.WaitAfterFinale);

        Wrapped<bool> teleport = new(false);
        OnTeleportOut += () => teleport.Value = true;
        this.StartLibCoroutine(Coroutines.PlayAnimations(animator, [SpellLoopToSwordController!, TeleportOutController!]));
        yield return Coroutines.SleepUntil(() => teleport.Value = true);

        yield return Coroutines.SleepSeconds(stats.GracePeriod);
    }

    private event System.Action? OnCastSpell;

    [ShimMethod]
    public void CastSpellEvent()
    {
        var prev = OnCastSpell;
        OnCastSpell = null;
        prev?.Invoke();
    }

    [ShimMethod]
    public void TeleportIn()
    {
        SetTangible(true);
        KnightOfNightsPreloader.Instance.MageTeleportClip?.PlayAtPosition(transform.position, 1.1f);
        TeleportBurst?.Spawn(transform.position);
    }

    [ShimMethod]
    public void TeleportOut()
    {
        SetTangible(false);
        KnightOfNightsPreloader.Instance.MageTeleportClip?.PlayAtPosition(transform.position, 1.1f);
        bobber!.enabled = false;
    }

    private void SetTangible(bool value)
    {
        healthManager!.IsInvincible = !value;
        healthManager.SetPreventInvincibleEffect(!value);
        nonBouncer!.active = value;
        collider!.enabled = value;
        damageHero!.enabled = value;

        if (value) idleParticles?.Play();
        else idleParticles?.Stop();
    }

    private event System.Action? OnTeleportOut;

    [ShimMethod]
    public void TeleportOutEvent()
    {
        var prev = OnTeleportOut;
        OnTeleportOut = null;
        prev?.Invoke();
    }
}
