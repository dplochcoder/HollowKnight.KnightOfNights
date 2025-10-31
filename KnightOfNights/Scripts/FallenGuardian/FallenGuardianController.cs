using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using KnightOfNights.Util;
using PurenailCore.CollectionUtil;
using PurenailCore.GOUtil;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal enum AttackChoice
{
    AxeHopscotch,
    GorbFireworks,
    GorbNuclear,
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
internal class FallenGuardianPhaseStats : MonoBehaviour
{
    [ShimField] public int MinHP;
    [ShimField] public AttackChoice FirstAttack;
    [ShimField] public List<FallenGuardianAttack> Attacks = [];

    [ShimField] public float StaggerGracePeriod;
    [ShimField] public float StaggerInvuln;
    [ShimField] public float StaggerHitWait;
    [ShimField] public float StaggerMaxWait;
    [ShimField] public float StaggerNextAttackDelay;
    [ShimField] public float UltraInstinctInterval;
    [ShimField] public float UltraInstinctTail;
    [ShimField] public float UltraInstinctTelegraph;

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

    [ShimField] public RuntimeAnimatorController? SpellStartController;
    [ShimField] public RuntimeAnimatorController? SpellLoopController;
    [ShimField] public RuntimeAnimatorController? SpellEndController;
    [ShimField] public RuntimeAnimatorController? StaggerController;
    [ShimField] public RuntimeAnimatorController? StaggerToRecoverController;
    [ShimField] public RuntimeAnimatorController? SwordToSpellController;

    [ShimField] public List<FallenGuardianPhaseStats> PhaseStats = [];

    private HealthManager? healthManager;
    private Recoil? recoil;
    private Animator? animator;
    private AudioSource? audio;

    private FallenGuardianPhaseStats? stats;

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
        recoil = GetComponent<Recoil>();
        animator = GetComponent<Animator>();
        stats = PhaseStats[0];

        audio = gameObject.AddComponent<AudioSource>();
        audio.outputAudioMixerGroup = AudioMixerGroups.Actors();

        On.GameManager.FreezeMoment_int += NoFreezeMoment;
    }

    private void OnEnable() => this.StartLibCoroutine(RunBoss());

    private void OnDestroy() => On.GameManager.FreezeMoment_int -= NoFreezeMoment;

    private static void NoFreezeMoment(On.GameManager.orig_FreezeMoment_int orig, GameManager self, int type)
    {
        if (type == 3) return;
        orig(self, type);
    }

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
            (2 * ShortWait, SlashAttackSpec.HIGH_LEFT.WithTelegraph(Telegraph)),
            (0f, SlashAttackSpec.HIGH_RIGHT.WithTelegraph(Telegraph + ShortWait))
        ]);
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
        if (++multiParries < StaggerCount) return true;

        multiParries = 0;
        staggerAttack = attack;
        return false;
    }

    private IEnumerator<CoroutineElement> ExecuteStagger()
    {
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

        if (!recoil!.IsRecoiling && prevAttack.HitInstance != null) recoil.RecoilByDamage(prevAttack.HitInstance.Value);

        SetIntangible();
        animator!.runtimeAnimatorController = StaggerController!;
        yield return Coroutines.SleepSeconds(stats!.StaggerInvuln);

        SetTangible();
        yield return Coroutines.SleepSeconds(stats!.StaggerGracePeriod);

        var prev = healthManager!.hp;
        yield return Coroutines.OneOf(
            Coroutines.SleepUntil(() => healthManager!.hp < prev).Then(Coroutines.SleepSeconds(stats!.StaggerHitWait)),
            Coroutines.SleepSeconds(stats!.StaggerMaxWait));

        animator.runtimeAnimatorController = StaggerToRecoverController!;

        Wrapped<bool> teleportOut = new(false);
        void Listen() => teleportOut.Value = true;
        OnTeleportOut += Listen;
        yield return Coroutines.SleepUntil(() => teleportOut.Value);
        OnTeleportOut -= Listen;

        transform.position = new(-100, -100);
        yield return Coroutines.SleepSeconds(stats!.StaggerNextAttackDelay);

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
    private static readonly AttackChoice? ForceAttack = AttackChoice.UltraInstinct;
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
            case AttackChoice.GorbFireworks:
                return Coroutines.SleepSeconds(1);
            case AttackChoice.GorbNuclear:
                return Coroutines.SleepSeconds(1);
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
        static bool IsValid(List<SlashAttackSpec> permutation) => permutation.Pairs().All(p =>
        {
            var (a, b) = p;
            if (a.ApproxEqual(SlashAttackSpec.LEFT) && b.ApproxEqual(SlashAttackSpec.LEFT)) return false;
            if (a.ApproxEqual(SlashAttackSpec.RIGHT) && b.ApproxEqual(SlashAttackSpec.RIGHT)) return false;
            if (a.ApproxEqual(SlashAttackSpec.HIGH_LEFT) && b.ApproxEqual(SlashAttackSpec.HIGH_LEFT)) return false;
            if (a.ApproxEqual(SlashAttackSpec.HIGH_RIGHT) && b.ApproxEqual(SlashAttackSpec.HIGH_RIGHT)) return false;
            return true;
        });

        List<List<SlashAttackSpec>> ret = [];
        input.ForEachPermutation(p =>
        {
            if (IsValid(p)) ret.Add([.. p]);
        });

        return ret;
    }
    private static readonly List<List<SlashAttackSpec>> UltraInstinctPatterns = GenUltraInstinctPatterns([
        SlashAttackSpec.LEFT,
        SlashAttackSpec.LEFT,
        SlashAttackSpec.RIGHT,
        SlashAttackSpec.RIGHT,
        SlashAttackSpec.HIGH_LEFT,
        SlashAttackSpec.HIGH_RIGHT
    ]);

    private static readonly List<List<SlashAttackSpec>> SuperUltraInstinctPatterns = GenUltraInstinctPatterns([
        SlashAttackSpec.LEFT,
        SlashAttackSpec.LEFT,
        SlashAttackSpec.LEFT,
        SlashAttackSpec.RIGHT,
        SlashAttackSpec.RIGHT,
        SlashAttackSpec.RIGHT,
        SlashAttackSpec.HIGH_LEFT,
        SlashAttackSpec.HIGH_LEFT,
        SlashAttackSpec.HIGH_RIGHT,
        SlashAttackSpec.HIGH_RIGHT
    ]);

    private IEnumerator<CoroutineElement> UltraInstinct()
    {
        var specs = UltraInstinctPatterns.Choose();
        specs = [.. specs.Select(s => s.WithTelegraph(stats!.UltraInstinctTelegraph))];

        List<SlashAttack> attacks = [];
        HashSet<SlashAttack> activeAttacks = [];
        foreach (var spec in specs)
        {
            var attack = SlashAttack.Spawn(spec);
            attacks.Add(attack);
            activeAttacks.Add(attack);
            yield return Coroutines.SleepSeconds(stats!.UltraInstinctInterval);
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
            healthManager!.hp -= attacks.Select(a => a.DamageDealt()).Sum();

            if (MaybeStagger(attack)) RevekAddons.SpawnSoul(attack.ParryPos);
        }

        yield return Coroutines.SleepSeconds(stats!.UltraInstinctTail);
    }

    private event System.Action? OnCastSpell;

    [ShimMethod]
    public void CastSpell() => OnCastSpell?.Invoke();

    [ShimMethod]
    public void SetTangible() => SetTangible(true);

    [ShimMethod]
    public void SetIntangible() => SetTangible(false);

    [ShimMethod]
    public void TeleportOut()
    {
        SetIntangible();
        KnightOfNightsPreloader.Instance.MageTeleportClip?.PlayAtPosition(transform.position, 1.1f);
    }

    private void SetTangible(bool value)
    {
        healthManager!.IsInvincible = !value;
        healthManager.SetPreventInvincibleEffect(!value);
    }

    private event System.Action? OnTeleportOut;

    [ShimMethod]
    public void TeleportOutEvent() => OnTeleportOut?.Invoke();
}
