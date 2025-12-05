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
        public Vector2 PrevPos = pos;
        public readonly CompactDict<Vector2> WindSpeed = new();
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
            if (particle.remainingLifetime <= 0) continue;

            Vector2 pPos = particle.position;
            Vector2 windDelta = Vector2.zero;
            foreach (var windField in WindField.ActiveWindFields())
            {
                bool initialized = data.WindSpeed.TryGetValue(windField.Id, out var windSpeed);
                var target = windField.WindSpeedAtPos(pPos, WindTargetType.Particle);
                if (initialized) windSpeed.AdvanceVecAbs(Time.deltaTime * windField.ParticleWindAccel, target);
                else windSpeed = target;

                data.WindSpeed[windField.Id] = windSpeed;
                windDelta += windSpeed;
            }

            particle.position = pPos + windDelta * Time.deltaTime;
            Vector2 prevTarget = WindField.ActiveWindEffects(data.PrevPos, WindTargetType.Particle);
            Vector2 newTarget = WindField.ActiveWindEffects(particle.position, WindTargetType.Particle);
            if (prevTarget.sqrMagnitude > 0)
            {
                var ratio = newTarget.magnitude / prevTarget.magnitude;
                if (ratio < 0.5f)
                {
                    // Check if this is due to a horizontal wall.
                    newTarget = WindField.ActiveWindEffects(particle.position with { y = data.PrevPos.y }, WindTargetType.Particle) * ratio;
                    var liveProb = newTarget.magnitude / prevTarget.magnitude;
                    if (liveProb < 0.5f && Random.Range(0f, 1f) >= liveProb)
                    {
                        particle.position = new(-1000, -1000);
                        particle.remainingLifetime = 0;
                    }
                }
            }

            data.PrevPos = particle.position;
            session.Set(i, particle, data);
        }
    }
}
