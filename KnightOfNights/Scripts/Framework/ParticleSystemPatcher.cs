using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class ParticleSystemPatcher : MonoBehaviour
{
    private void Awake() => gameObject.GetComponent<ParticleSystemRenderer>().material.shader = Shader.Find("Sprites/Default");
}
