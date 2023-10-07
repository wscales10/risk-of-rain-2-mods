using System;
using System.Linq;
using Utils;

namespace Music
{
    public class VolumeController : IVolumeProvider
    {
        private readonly ConcreteKeyedCollection<string, INamedVolumeProvider> volumeProviders = new ConcreteKeyedCollection<string, INamedVolumeProvider>(provider => provider.Name);

        public double Volume => volumeProviders.Select(k => k.Volume).Aggregate(1d, (v1, v2) => v1 * v2);

        public int VolumePercent => (int)Math.Ceiling(Volume * 100);

        public VolumeKnob GetOrAdd(string name)
        {
            if (volumeProviders.Contains(name))
            {
                var provider = volumeProviders[name];

                if (provider is VolumeKnob knob)
                {
                    return knob;
                }

                throw new ArgumentOutOfRangeException(nameof(name), name, "Not a name of a knob");
            }
            else
            {
                var knob = new VolumeKnob(name, 1);
                AddProviders(knob);
                return knob;
            }
        }

        public VolumeController AddProviders(params INamedVolumeProvider[] providers)
        {
            foreach (var provider in providers)
            {
                if (!volumeProviders.TryGetValue(provider.Name, out var existingProvider))
                {
                    volumeProviders.Add(provider);
                }
                else if (existingProvider != provider)
                {
                    throw new InvalidOperationException($"A knob with the name '{provider.Name}' already exists");
                }
            }

            return this;
        }

        public VolumeController RemoveProviders(params INamedVolumeProvider[] providers)
        {
            foreach (var provider in providers)
            {
                if (volumeProviders.TryGetValue(provider.Name, out var existingProvider) && existingProvider == provider)
                {
                    volumeProviders.Remove(provider);
                }
                else
                {
                    throw new InvalidOperationException($"'{provider.GetType().GetDisplayName()}' is not on this controller");
                }
            }

            return this;
        }
    }
}