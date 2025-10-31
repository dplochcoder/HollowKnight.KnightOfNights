using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.Internal;
using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts;

internal class RevekAddons : MonoBehaviour, IHitResponder
{
    public bool HealOnNailParry = false;
    public System.Func<float, bool> DirectionFilter = _ => true;
    public HitInstance? HitInstance {  get; private set; }

    private MeshRenderer? renderer;
    private PlayMakerFSM? fsm;

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        fsm = gameObject.LocateMyFSM("Control");
    }

    private void LateUpdate()
    {
        renderer!.sortingLayerName = "Over";
        renderer!.sortingOrder = 1;
    }

    private static readonly HashSet<string> VULNERABLE_STATES = ["Slash Idle", "Slash Antic", "Slash"];

    public void Hit(HitInstance damageInstance)
    {
        if (damageInstance.DamageDealt <= 0) return;
        if (fsm == null || !VULNERABLE_STATES.Contains(fsm.ActiveStateName)) return;
        if (!DirectionFilter(damageInstance.Direction)) return;

        switch (damageInstance.AttackType)
        {
            case AttackTypes.Nail:
            case AttackTypes.Spell:
                if (damageInstance.AttackType == AttackTypes.Nail && HealOnNailParry) SpawnSoul(transform);

                HitInstance = damageInstance;
                fsm.SetState("Hit");
                break;
            case AttackTypes.Acid:
            case AttackTypes.Generic:
            case AttackTypes.NailBeam:
            case AttackTypes.RuinsWater:
            case AttackTypes.SharpShadow:
                break;
        }
    }

    internal static void SpawnSoul(Transform transform)
    {
        var prefab = ObjectCache.SoulOrb;
        Destroy(prefab.Spawn());
        prefab.SetActive(true);

        // Give 34 soul per parry.
        FlingUtils.Config config = new()
        {
            Prefab = prefab,
            AmountMin = 16,
            AmountMax = 16,
            SpeedMin = 10,
            SpeedMax = 20,
            AngleMin = 0,
            AngleMax = 360,
        };
        FlingUtils.SpawnAndFling(config, transform, Vector3.zero);

        // Heal on parry.
        HeroController.instance.AddHealth(1);

        Destroy(prefab);
    }

    internal static void SpawnSoul(Vector2 pos)
    {
        GameObject obj = new();
        obj.transform.position = pos;
        SpawnSoul(obj.transform);
    }

    private static Lazy<List<AudioClip>> hurtClips = new(() => [.. KnightOfNightsPreloader.Instance.Revek.LocateMyFSM("Control").GetFsmState("Hit").GetFirstActionOfType<AudioPlayerOneShot>().audioClips]);

    internal static AudioClip GetHurtClip() => hurtClips.Get().Choose();
}
