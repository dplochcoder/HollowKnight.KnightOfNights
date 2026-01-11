using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class EnvironmentProvider : MonoBehaviour, IOnAssetLoad
{
    [ShimField] public AudioClip? SnowWalkClip;
    [ShimField] public GameObject? SnowDashEffect;
    [ShimField] public GameObject? SnowRunEffect;
    [ShimField] public GameObject? SnowSoftLandEffect;
    [ShimField] public GameObject? SnowHardLandEffect;

    public void OnAssetLoad()
    {
        var snowId = CustomEnvironmentType.SNOW.ToIntId();
        SFCore.EnviromentParticleHelper.AddWalkAudio(snowId, SnowWalkClip!);
        SFCore.EnviromentParticleHelper.AddRunAudio(snowId, SnowWalkClip!);
        SFCore.EnviromentParticleHelper.AddDashEffects(snowId, SnowDashEffect!);
        SFCore.EnviromentParticleHelper.AddRunEffects(snowId, SnowRunEffect!);
        SFCore.EnviromentParticleHelper.AddSoftLandEffects(snowId, SnowSoftLandEffect!);
        SFCore.EnviromentParticleHelper.AddHardLandEffects(snowId, SnowHardLandEffect!);

        var iceId = CustomEnvironmentType.ICE.ToIntId();
        SFCore.EnviromentParticleHelper.AddCustomWalkAudioHook += hc => (iceId, hc.footstepsWalkMetal);
        SFCore.EnviromentParticleHelper.AddCustomWalkAudioHook += hc => (iceId, hc.footstepsRunMetal);
        SFCore.EnviromentParticleHelper.AddDashEffects(iceId, SnowRunEffect!);
        SFCore.EnviromentParticleHelper.AddCustomSoftLandEffectsHook += sle => (iceId, sle.dustEffects);
        SFCore.EnviromentParticleHelper.AddCustomHardLandEffectsHook += hle => (iceId, hle.dustObj);
    }
}
