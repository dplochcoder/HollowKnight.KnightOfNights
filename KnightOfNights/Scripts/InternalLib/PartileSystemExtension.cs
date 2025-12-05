using PurenailCore.SystemUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class PartileSystemExtension<T>(ParticleSystem system)
{
    private readonly ParticleSystem system = system;
    private readonly Dictionary<int, T> data = [];

    internal class Session(PartileSystemExtension<T> parent, IEnumerable<(ParticleSystem.Particle, T)> data) : IDisposable
    {
        private readonly PartileSystemExtension<T> parent = parent;
        private readonly List<(ParticleSystem.Particle, T)> data = [.. data];

        public int Count => data.Count;

        public (ParticleSystem.Particle, T) Get(int i) => data[i];

        public void Set(int i, ParticleSystem.Particle particle, T value) => data[i] = (particle, value);

        public void Dispose()
        {
            parent.data.Clear();

            var array = new ParticleSystem.Particle[data.Count];
            List<Vector4> customData = [];
            for (int i = 0; i < data.Count; i++)
            {
                var (p, v) = data[i];
                array[i] = p;
                customData.Add(new(i + 1, 0));
                parent.data.Add(i + 1, v);
            }

            parent.system.SetParticles(array);
            parent.system.SetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
        }
    }

    public Session NewSession(Func<ParticleSystem.Particle, T> factory)
    {
        var particles = new ParticleSystem.Particle[system.particleCount];
        system.GetParticles(particles);
        List<Vector4> customData = [];
        system.GetCustomParticleData(customData, ParticleSystemCustomData.Custom1);

        List<(ParticleSystem.Particle, T)> sessionData = [];
        return new(this, particles.Zip(customData, (p, d) => (p, data.GetOrDefault(Mathf.RoundToInt(d.x), () => factory(p)))));
    }
}
