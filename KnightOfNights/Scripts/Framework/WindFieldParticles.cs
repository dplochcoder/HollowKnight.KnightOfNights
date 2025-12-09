using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
[RequireComponent(typeof(ParticleSystem))]
internal class WindFieldParticles : MonoBehaviour
{
    private record ParticleData(Vector2 pos)
    {
        public bool Initialized = false;
        public Vector2 WindSpeed = WindField.ActiveWindEffects(pos, WindTargetType.Particle);
    }

    private PartileSystemExtension<ParticleData>? particleWindData;

    private void Awake()
    {
        if (TryGetComponent<ParticleSystem>(out var system)) particleWindData = new(system);
    }

    private void LateUpdate()
    {
        if (particleWindData == null) return;

        using var session = particleWindData.NewSession(p => new(p.position));
        for (int i = 0; i < session.Count; i++)
        {
            var (particle, data) = session.Get(i);

            if (!data.Initialized)
            {
                data.Initialized = true;
                particle.position -= data.WindSpeed.To3d() * particle.remainingLifetime / 2;
            }
            particle.position += data.WindSpeed.To3d() * Time.deltaTime;

            session.Set(i, particle, data);
        }
    }
}
