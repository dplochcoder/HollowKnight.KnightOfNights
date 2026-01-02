using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using KnightOfNights.Scripts.Framework;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.ModUtil;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class NailParrySingleton : SceneSingleton<NailParrySingleton>
{
    private bool nailParryActive = false;

    internal void DoParry(GameObject src, Collider2D collider, IParryResponder? parryResponder)
    {
        if (collider.gameObject.layer != 16) return;
        if (nailParryActive) return;

        nailParryActive = true;
        this.StartLibCoroutine(DoParryImpl(src, collider, parryResponder));
    }

    private IEnumerator<CoroutineElement> DoParryImpl(GameObject src, Collider2D collider, IParryResponder? parryResponder)
    {
        GameManager.instance.FreezeMoment(3);

        var hc = HeroController.instance;
        hc.NailParry();

        var attackDir = collider.gameObject.Parent().LocateMyFSM("damages_enemy").FsmVariables.GetFsmFloat("direction").Value;
        attackDir = MathExt.ClampAngle(attackDir, -45, 315);
        GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");

        var audio = src.GetOrAddComponentSharedLib<AudioSource>();
        audio.pitch = Random.Range(0.85f, 1.15f);

        var fsm = KnightOfNightsPreloader.Instance.NailClashTinkFSM!;
        audio.PlayOneShot(fsm.GetState("Blocked Hit").GetFirstActionOfType<AudioPlayerOneShot>().audioClips[0]);

        Vector3 offset;
        if (attackDir <= 45)
        {
            hc.RecoilLeft();
            offset = new(1.5f, 0);
            parryResponder?.Parried(0);
        }
        else if (attackDir <= 135)
        {
            hc.RecoilDown();
            offset = new(0, 1.5f);
            parryResponder?.Parried(90);
        }
        else if (attackDir <= 225)
        {
            hc.RecoilRight();
            offset = new(-1.5f, 0);
            parryResponder?.Parried(180);
        }
        else
        {
            hc.Bounce();
            offset = new(0, -1.5f);
            parryResponder?.Parried(270);
        }

        var pos = hc.transform.position + offset;
        fsm.GetState("No Box Right").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value.Spawn(pos);

        yield return Coroutines.SleepSeconds(0.1f);

        hc.NailParryRecover();
        nailParryActive = false;

        // TODO: Send parry event to source.
    }
}

[Shim]
internal class NailClashTink : MonoBehaviour
{
    [ShimField] public GameObject? ParryResponder;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        NailParrySingleton.Get()?.DoParry(gameObject, collider, ParryResponder?.GetComponent<IParryResponder>());
    }
}