using RoR2;
using System.Collections.Generic;

namespace UntitledMod
{
    public class PickupWeightMultipliers : IPickupWeightMultipliers
    {
        private readonly IDictionary<PickupIndex, float> dictionary = new Dictionary<PickupIndex, float>();

        private readonly ICustomLogger logger;

        public PickupWeightMultipliers(ICustomLogger logger)
        {
            this.logger = logger;
        }

        public void Reset() => this.dictionary.Clear();

        public bool TryGetValue(PickupIndex pickupIndex, out float multiplier)
        {
            this.logger.LogMethodCall();
            return this.dictionary.TryGetValue(pickupIndex, out multiplier);
        }

        public void SetValue(PickupIndex pickupIndex, float? multiplier)
        {
            if (multiplier.HasValue)
            {
                this.dictionary[pickupIndex] = multiplier.Value;
            }
            else
            {
                this.dictionary.Remove(pickupIndex);
            }
        }
    }
}