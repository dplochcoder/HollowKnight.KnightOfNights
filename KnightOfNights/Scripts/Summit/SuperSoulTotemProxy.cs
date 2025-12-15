using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.ModUtil;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.Summit;

[Shim]
internal class SuperSoulTotemProxy : MonoBehaviour
{
    private void Awake()
    {
        var totem = Instantiate(KnightOfNightsPreloader.Instance.SoulTotem!, transform.position, Quaternion.identity);
        totem.transform.localScale = transform.localScale;
        totem.AddComponent<SuperSoulTotem>();
        totem.SetActive(true);

        Destroy(gameObject);
    }
}

internal class SuperSoulTotem : MonoBehaviour
{
    private const float EMISSION_RATE = 30;
    private const float PARTICLE_LIFETIME = 0.65f;
    private const float PARTICLE_SIZE = 0.75f;
    private const int PARTICLE_CAP = 100;

    private void Awake()
    {
        var data = GetComponent<PersistentIntItem>().persistentIntData;
        data.value = 3;
        data.semiPersistent = false;
        data.id = "SuperSoulTotem";
        data.sceneName = "BrettasHouse";

        var fsm = gameObject.LocateMyFSM("soul_totem");
        fsm.GetFsmState("Close").AddFirstAction(new Lambda(() => fsm.FsmVariables.GetFsmInt("Value").Value = 3));
        var hit = fsm.GetFsmState("Hit");
        hit.AddFirstAction(new Lambda(() => fsm.FsmVariables.GetFsmInt("Value").Value = 3));

        var flinger = hit.GetFirstActionOfType<FlingObjectsFromGlobalPool>();
        flinger.spawnMin.Value = 11;
        flinger.spawnMax.Value = 11;

        if (buffedFlingers.Add(flinger)) this.DoOnDestroy(() => buffedFlingers.Remove(flinger));

        var particles = gameObject.FindChild("Soul Particles")!.GetComponent<ParticleSystem>();
        var main = particles.main;
        main.startSize = PARTICLE_SIZE;
        main.maxParticles = PARTICLE_CAP;
        main.startLifetime = PARTICLE_LIFETIME;
        var emission = particles.emission;
        emission.rateOverTime = EMISSION_RATE;
    }

    private static readonly HashSet<FlingObjectsFromGlobalPool> buffedFlingers = [];
    private static readonly HashSet<SoulOrb> buffedOrbs = [];

    private static void OnFlingSoulOrb(FlingObjectsFromGlobalPool fsmAction, SoulOrb soulOrb)
    {
        if (!buffedFlingers.Contains(fsmAction)) return;

        buffedOrbs.Add(soulOrb);
        soulOrb.DoOnDestroy(() => buffedOrbs.Remove(soulOrb));
    }

    private static void OnGiveSoul(SoulOrb soulOrb)
    {
        if (!buffedOrbs.Remove(soulOrb))
            return;

        HeroController.instance.AddMPCharge(16);  // (16 + 2) * 11 = 198 = max MP
        HeroController.instance.AddHealth(1);  // 1 * 11 = max health
    }

    static SuperSoulTotem()
    {
        SoulOrbModifier.OnFlingSoulOrb += OnFlingSoulOrb;
        SoulOrbModifier.OnGiveSoul += OnGiveSoul;
    }
}
