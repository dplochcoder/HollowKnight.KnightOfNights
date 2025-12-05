using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class ParticleSystemPatcher : MonoBehaviour
{
    private void OnEnable() => gameObject.GetComponent<ParticleSystemRenderer>().material.shader = Shader.Find("Sprites/Default");
}
