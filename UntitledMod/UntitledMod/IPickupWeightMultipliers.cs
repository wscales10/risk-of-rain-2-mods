using RoR2;

namespace UntitledMod
{
    public interface IPickupWeightMultipliers
    {
        void Reset();

        bool SetValue(PickupIndex pickupIndex, float? multiplier);

        bool TryGetValue(PickupIndex pickupIndex, out float multiplier);
    }
}