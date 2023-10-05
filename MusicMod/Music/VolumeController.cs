using System;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace Music
{
    public class VolumeController
    {
        private readonly ConcreteKeyedCollection<string, VolumeKnob> volumeKnobs = new ConcreteKeyedCollection<string, VolumeKnob>(knob => knob.Name);

        public double Volume => volumeKnobs.Select(k => k.Volume).Aggregate(1d, (v1, v2) => v1 * v2);

        public VolumeKnob GetOrAdd(string name)
        {
            if (volumeKnobs.Contains(name))
            {
                return volumeKnobs[name];
            }
            else
            {
                var knob = new VolumeKnob(name, 1);
                AddKnobs(knob);
                return knob;
            }
        }

        public VolumeController AddKnobs(params VolumeKnob[] volumeKnobs)
        {
            foreach (var knob in volumeKnobs)
            {
                if (!this.volumeKnobs.TryGetValue(knob.Name, out var existingKnob))
                {
                    this.volumeKnobs.Add(knob);
                }
                else if (existingKnob != knob)
                {
                    throw new InvalidOperationException($"A knob with the name '{knob.Name}' already exists");
                }
            }

            return this;
        }

        public VolumeController RemoveKnobs(params VolumeKnob[] volumeKnobs)
        {
            foreach (var knob in volumeKnobs)
            {
                if (this.volumeKnobs.TryGetValue(knob.Name, out var existingKnob) && existingKnob == knob)
                {
                    this.volumeKnobs.Remove(knob);
                }
                else
                {
                    throw new InvalidOperationException("Knob is not on this controller");
                }
            }

            return this;
        }
    }
}