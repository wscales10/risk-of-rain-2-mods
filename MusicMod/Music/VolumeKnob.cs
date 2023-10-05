using System;
using System.Threading.Tasks;

namespace Music
{
    public class VolumeKnob
    {
        private double volume;

        public VolumeKnob(string name, double volume)
        {
            Name = name;
            Volume = volume;
        }

        public string Name { get; }

        public double Volume
        {
            get => volume;

            set
            {
                if (volume == value)
                {
                    return;
                }

                if (double.IsNaN(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be a number");
                }

                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be between 0 and 1");
                }

                volume = value;
            }
        }
    }
}