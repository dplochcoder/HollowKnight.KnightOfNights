using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using KnightOfNights.Util;
using PurenailCore.CollectionUtil;
using PurenailCore.GOUtil;
using RandomizerMod.IC;
using SFCore.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal enum AttackChoice
{
    AxeHopscotch,
    GorbStorm,
    RainingPancakes,
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

    [ShimField] public RuntimeAnimatorController? DiveAnticLoopController;
    [ShimField] public RuntimeAnimatorController? DiveAnticToDiveLoopController;
    [ShimField] public RuntimeAnimatorController? DiveImpactController;
    [ShimField] public RuntimeAnimatorController? DiveLoopController;
    [ShimField] public RuntimeAnimatorController? SpellCastToEndController;
    [ShimField] public RuntimeAnimatorController? SpellCastToLoopController;
    [ShimField] public RuntimeAnimatorController? SpellLoopController;
    [ShimField] public RuntimeAnimatorController? SpellLoopToSwordController;
    [ShimField] public RuntimeAnimatorController? SpellStartToLoopController;
    [ShimField] public RuntimeAnimatorController? StaggerController;
    [ShimField] public RuntimeAnimatorController? StaggerToRecoverController;
    [ShimField] public RuntimeAnimatorController? SwordToDiveAnticController;
    [ShimField] public RuntimeAnimatorController? SwordToSpellController;
    [ShimField] public RuntimeAnimatorController? TeleportInController;
    [ShimField] public RuntimeAnimatorController? TeleportOutController;

    [ShimField] public List<FallenGuardianPhaseStats> PhaseStats = [];

    private HealthManager? healthManager;
    private Rigidbody2D? rigidbody;
    private NonBouncer? nonBouncer;
    private BoxCollider2D? collider;
    private DamageHero? damageHero;
    private Recoil? recoil;
    private Animator? animator;
    private AudioSource? audio;
    private Bobber? bobber;
    private PancakePool? pancakePool;

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
        rigidbody = GetComponent<Rigidbody2D>();
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
        pancakePool = gameObject.AddComponent<PancakePool>();

        StaggerBurst?.FixSpawnBug();
        TeleportBurst?.FixSpawnBug();

        SetTangible(false);
        InitParticles();
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
    private static readonly AttackChoice? ForceAttack = AttackChoice.RainingPancakes;
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
            case AttackChoice.RainingPancakes:
                return Coroutines.Sequence(RainingPancakes());
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

    private IEnumerator<CoroutineElement> LaunchGorbSpikes(int count, float offset, int numBursts, float wait, float pitchIncrement)
    {
        var prefab = KnightOfNightsPreloader.Instance.GorbSpear!;
        prefab.FixSpawnBug();

        float angle = Random.Range(0f, 360f);
        float off = 360f * offset / count * (MathExt.CoinFlip() ? 1 : -1);
        for (int i = 0; i < numBursts; i++)
        {
            var pos = transform.position;
            KnightOfNightsPreloader.Instance.MageShotClip!.PlayAtPosition(pos, 1f + i * pitchIncrement);
            for (int j = 0; j < count; j++)
            {
                var obj = prefab.Spawn(pos, Quaternion.Euler(0, 0, angle));
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
                FacePlayer();
                bobber?.ResetRandom(stats.BobRadius, stats.BobPeriod);

                this.StartLibCoroutine(Coroutines.PlayAnimations(animator!, [TeleportInController!, SwordToSpellController!, SpellStartToLoopController!]));
                yield return Coroutines.SleepSeconds(stats.WaitFirst);
            }
            else
            {
                TeleportInstant(pos);
                FacePlayer();
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
        FacePlayer();

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

    private const int NUM_PANCAKES = 16;

    private static readonly List<List<int>> HOLE_PERMUTATIONS = GenerateHolePermutations();
    private static List<List<int>> GenerateHolePermutations()
    {
        List<int> input = [3, 4, 4, 5];
        List<List<int>> ret = [];
        input.ForEachPermutation(ret.Add);
        return ret;
    }

    private static List<bool> GeneratePancakeSpawns()
    {
        List<bool> spawns = [];
        for (int i = 0; i < NUM_PANCAKES; i++) spawns.Add(true);

        int idx = 0;
        foreach (int span in HOLE_PERMUTATIONS.Choose())
        {
            idx += span;
            spawns[idx - 1] = false;
        }

        List<List<bool>> valid = [];
        for (int i = 0; i < NUM_PANCAKES; i++)
        {
            if (spawns[0] && spawns[NUM_PANCAKES - 1]) valid.Add([.. spawns]);
            spawns.Rotate(1);
        }
        return valid.Choose();
    }

    private static List<bool> UpdatePancakes(List<bool> prev)
    {
        List<bool> ret = [];
        for (int i = 0; i < NUM_PANCAKES; i++) ret.Add(true);

        for (int i = 0; i < NUM_PANCAKES; i++)
        {
            if (prev[i]) continue;

            List<int> choices = [];
            if (i < NUM_PANCAKES - 1 && !prev[i + 1])
            {
                choices = [i, i + 1];
                i++;
            }
            else choices = [i == 0 ? 1 : (i - 1), i == NUM_PANCAKES - 1 ? (NUM_PANCAKES - 2) : i + 1];
            ret[choices.Choose()] = false;
        }

        return ret;
    }

    private List<Pancake> SpawnPancakes(List<bool> spawns, float y, float z, float launchPitch)
    {
        var stats = this.stats!.RainingPancakesStats!;

        var b = Bounds();
        var span = (b.max.x - b.min.x) / NUM_PANCAKES;

        float X(int idx) => b.min.x + span / 2 + idx * span + stats.PancakeXOffset;

        List<Pancake> ret = [];
        var extra = stats.WingCount;
        for (int i = -extra; i < NUM_PANCAKES + extra; i++)
        {
            bool mainPlatform = i >= 0 && i < NUM_PANCAKES;
            if (mainPlatform && !spawns[i]) continue;

            var minY = Bounds().min.y - (mainPlatform ? 0 : 8);
            ret.Add(pancakePool!.SpawnPancake(new(X(i), y, z), launchPitch, stats.PancakeSpeed, minY, mainPlatform));
        }
        return ret;
    }

    private IEnumerator<CoroutineElement> RainingPancakes()
    {
        var stats = this.stats!.RainingPancakesStats!;

        recoil!.CancelRecoil();
        recoil.enabled = false;

        float ChooseX()
        {
            var b = Container!.Arena!.bounds;
            var kX = MathExt.Clamp(HeroController.instance.transform.position.x, b.min.x, b.max.x);

            return Random.Range(Mathf.Max(b.min.x + stats.DiveXBuffer, kX - stats.DiveXRange), Mathf.Min(b.max.x - stats.DiveXBuffer, kX + stats.DiveXRange));
        }

        for (int i = 0; i < stats.NumDives; i++)
        {
            var iCopy = i;
            gameObject.DoAfter(() =>
            {
                transform.position = new(ChooseX(), Bounds().min.y + Random.Range(stats.DiveHeightMin, stats.DiveHeightMax));
                SpawnTeleportBurst(1);
                PlayTeleportSound();

                if (iCopy == 0) this.StartLibCoroutine(Coroutines.PlayAnimations(animator!, [TeleportInController!, SwordToDiveAnticController!]));
                else animator!.runtimeAnimatorController = DiveAnticLoopController;

                SetTangible(true);
            }, i == 0 ? stats.WaitForFirstTeleport : stats.WaitForLaterTeleports);

            List<List<Pancake>> waves = [];
            var spawns = GeneratePancakeSpawns();
            float y = Bounds().min.y + stats.PancakeY;
            float pitch = 1;
            float z = 0;
            for (int j = 0; j < stats.NumWavesPerDive; j++)
            {
                waves.Add(SpawnPancakes(spawns, y, z, pitch));

                spawns = UpdatePancakes(spawns);
                y += stats.PancakeYIncrement;
                z += 0.01f;
                pitch += stats.PancakePitchIncrement;

                if (j != stats.NumWavesPerDive - 1) yield return Coroutines.SleepSeconds(stats.WaitBetweenWaveSpawns);
            }

            yield return Coroutines.SleepSeconds(stats.WaitAfterLastWaveSpawn);

            for (int j = 0; j < waves.Count; j++)
            {
                waves[j].ForEach(p => p.Fire());
                yield return Coroutines.SleepSeconds(j == waves.Count - 1 ? stats.WaitFromLastDropToDive : stats.WaitBetweenWaveDrops);
            }

            rigidbody!.velocity = new(0, stats.DiveRetreatSpeed);
            recoil!.CancelRecoil();
            recoil.enabled = false;

            Wrapped<bool> dive = new(false);
            OnDive += () => dive.Value = true;
            animator!.runtimeAnimatorController = DiveAnticToDiveLoopController;
            yield return Coroutines.SleepUntil(() => dive.Value);
            KnightOfNightsPreloader.Instance.RevekAttackClips.Choose().PlayAtPosition(transform.position);

            dive.Value = false;
            Wrapped<Vector2> landing = new(Vector2.zero);
            rigidbody!.velocity = new(0, -stats.DivePlungeSpeed);
            OnDiveLand += p =>
            {
                landing.Value = p;
                dive.Value = true;
            };
            yield return Coroutines.SleepUntil(() => dive.Value);

            rigidbody!.velocity = Vector2.zero;
            SetTangible(false);

            transform.position = landing.Value;
            transform.localScale = new(stats.DiveImpactScale, stats.DiveImpactScale, 1);
            this.StartLibCoroutine(Coroutines.PlayAnimation(animator!, DiveImpactController!).Then(Coroutines.Instant(() => transform.localScale = new(1, 1, 1))));

            var p = landing.Value;
            p.y -= 0.25f;
            Shockwave.SpawnTwo(p, new(stats.ShockwaveXScale, stats.ShockwaveYScale), stats.ShockwaveSpeed);
            KnightOfNightsPreloader.Instance.MageStrikeImpactClip?.PlayAtPosition(landing.Value);

            yield return Coroutines.SleepSeconds(stats.WaitFromDiveToNextSpawn);
        }

        // TODO: Finale
        recoil!.enabled = true;
    }

    private static List<List<SlashAttackSpec>> GenUltraInstinctPatterns(List<SlashAttackSpec> input)
    {
        static bool MatchingDirections(SlashAttackSpec a, SlashAttackSpec b)
        {
            if (a.AllowedHits.Count != b.AllowedHits.Count) return false;

            HashSet<string> bSet = [.. b.AllowedHits];
            return a.AllowedHits.All(bSet.Contains);
        }

        List<List<SlashAttackSpec>> ret = [];
        input.ForEachPermutation(p =>
        {
            if (p.Pairs().All(pair => !MatchingDirections(pair.Item1, pair.Item2))) ret.Add([.. p]);
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

    private UnityEngine.Bounds Bounds() => Container!.Arena!.bounds;

    private void FacePlayer()
    {
        var kPos = HeroController.instance.transform.position;
        var pos = transform.position;

        bool left = pos.x >= kPos.x;
        transform.localScale = new(left ? 1 : -1, 1, 1);
    }

    private void SpawnTeleportBurst(float scale, Vector3? pos = null)
    {
        pos ??= transform.position;
        TeleportBurst!.Spawn(pos.Value).transform.localScale = new(scale, scale, 1);
    }

    private void PlayTeleportSound(Vector3? pos = null) => KnightOfNightsPreloader.Instance.MageTeleportClip?.PlayAtPosition(pos ?? transform.position, 1.1f);

    private void TeleportInstant(Vector2 newPos)
    {
        SpawnTeleportBurst(0.5f);

        transform.position = newPos;
        PlayTeleportSound();
        SpawnTeleportBurst(1);
    }

    private event System.Action? OnCastSpell;

    [ShimMethod]
    public void CastSpellEvent()
    {
        var prev = OnCastSpell;
        OnCastSpell = null;
        prev?.Invoke();
    }

    private event System.Action? OnDive;

    [ShimMethod]
    public void DiveEvent()
    {
        var prev = OnDive;
        OnDive = null;
        prev?.Invoke();
    }

    [ShimMethod]
    public void TeleportIn()
    {
        SetTangible(true);
        PlayTeleportSound();
        SpawnTeleportBurst(1);
    }

    [ShimMethod]
    public void TeleportOut()
    {
        SetTangible(false);
        PlayTeleportSound();
        SpawnTeleportBurst(0.5f);
        bobber!.enabled = false;
    }

    private void SetTangible(bool value)
    {
        healthManager!.IsInvincible = !value;
        healthManager.SetPreventInvincibleEffect(!value);
        nonBouncer!.active = !value;
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

    private event System.Action<Vector2>? OnDiveLand;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer != (int)PhysLayers.TERRAIN) return;

        var b = collider!.bounds;

        var prev = OnDiveLand;
        OnDiveLand = null;
        prev?.Invoke(new(b.center.x, Bounds().min.y));
    }
}
