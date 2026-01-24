using RoR2;
using UnityEngine;

namespace AssortedExperiments.ItemAdjustments
{
    public class EccentricVase : Module
    {
        public override void Init()
        {
            // Scale Eccentric Vase / Ziprail speed with move speed
            On.RoR2.ZiplineVehicle.OnPassengerEnter += this.ZiplineVehicle_OnPassengerEnter;
        }

        private void ZiplineVehicle_OnPassengerEnter(On.RoR2.ZiplineVehicle.orig_OnPassengerEnter orig, ZiplineVehicle self, GameObject passenger)
        {
            orig(self, passenger);
            var characterBody = passenger.GetComponent<CharacterBody>();

            if (!characterBody) return;

            var multiplier = characterBody.moveSpeed / 7f;
            this.Logger.LogInfo($"Multiplying eccentric vase acceleration and max speed by {multiplier}");

            // TODO: these numbers 30 and 10 are copied from the ZiplineVehicle prefab; this is not great, as it will override any changes made to this prefab or by other mods
            self.acceleration = 30f * multiplier;
            self.maxSpeed = 10f * multiplier;
        }
    }
}