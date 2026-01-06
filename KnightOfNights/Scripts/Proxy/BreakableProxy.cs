using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.Proxy;

[Shim]
[RequireComponent(typeof(Collider2D))]
internal class BreakableProxy : Breakable
{
    private static readonly MonobehaviourPatcher<Breakable> patcher = new(
        () => KnightOfNightsPreloader.Instance.DirectionPoleStag!.GetComponent<Breakable>(),
        "audioSourcePrefab",
        "breakAudioEvent",
        "nailHitEffectPrefab",
        "spellHitEffectPrefab",
        "strikeEffectPrefab");

    [ShimField] public List<GameObject> WholeParts = [];
    [ShimField] public List<GameObject> RemnantParts = [];
    [ShimField] public List<GameObject> DebrisParts = [];
    [ShimField("-60")] public float AngleOffset;
    [ShimField] public Vector3 EffectOffset;
    [ShimField] public float FlingSpeedMin;
    [ShimField] public float FlingSpeedMax;

    public new void Awake()
    {
        containingParticles = [];
        flingObjectRegister = [];
        this.SetAttr<Breakable, GameObject[]>("wholeParts", [.. WholeParts]);
        this.SetAttr<Breakable, GameObject[]>("remnantParts", [.. RemnantParts]);
        this.SetAttr<Breakable, List<GameObject>>("debrisParts", DebrisParts);
        this.SetAttr<Breakable, float>("angleOffset", AngleOffset);
        this.SetAttr<Breakable, float>("inertBackgroundThreshold", 1f);
        this.SetAttr<Breakable, float>("inertForegroundThreshold", -1f);
        this.SetAttr<Breakable, Vector3>("effectOffset", EffectOffset);
        this.SetAttr<Breakable, float>("flingSpeedMin", FlingSpeedMin);
        this.SetAttr<Breakable, float>("flingSpeedMax", FlingSpeedMax);
        patcher.Patch(this);

        base.Awake();
    }
}
