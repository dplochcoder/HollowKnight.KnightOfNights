using KnightOfNights.IC;
using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.Proxy;
using KnightOfNights.Scripts.SharedLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class FallenGuardianContainer : RevekSongSuppressor
{
    [ShimField] public HeroDetectorProxy? Trigger;
    [ShimField] public FallenGuardianController? Boss;
    [ShimField] public BoxCollider2D? Arena;
    [ShimField] public BoxCollider2D? DaggerBox;
    [ShimField] public BoxCollider2D? GorbStormFinaleBox;

    [ShimField] public List<GameObject> DeactivateOnFight = [];
    [ShimField] public List<GameObject> ActivateOnFight = [];
    [ShimField] public List<ParticleSystem> DetectionParticles = [];

    protected override void OnEnable()
    {
        base.OnEnable();

        fightStarted = false;
        Trigger?.Listen(() =>
        {
            if (!fightStarted) DetectionParticles.ForEach(p => p.Play());
        }, () => DetectionParticles.ForEach(p => p.Stop()));

        StartCoroutine(Run());
    }

    private bool fightStarted = false;

    private IEnumerator Run()
    {
        yield return new WaitUntil(() => fightStarted);

        DeactivateOnFight.ForEach(o => o.SetActive(false));
        DetectionParticles.ForEach(p => p.Stop());
        ActivateOnFight.ForEach(o => o.SetActive(true));

        Boss!.gameObject.SetActive(true);
    }

    protected override bool InterceptRevekSong(List<FluteNote> song)
    {
        if (!fightStarted && Trigger!.Detected()) fightStarted = true;
        return true;
    }
}
