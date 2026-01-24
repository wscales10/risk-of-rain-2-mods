using RoR2;
using UnityEngine;

namespace AssortedExperiments.Items
{
    public class LunarShardsController : MonoBehaviour
    {
        private HoldoutZoneController holdoutZoneController;

        private SoulShardController? soulShardController;

        private BloodShardController? bloodShardController;

        public float HoldoutZoneChargeRateMultiplier { get; set; } = 1f;

        private void Awake()
        {
            this.holdoutZoneController = base.GetComponent<HoldoutZoneController>();
        }

        private void Start()
        {
            this.soulShardController = this.gameObject.AddComponent<SoulShardController>();
            this.bloodShardController = this.gameObject.AddComponent<BloodShardController>();
        }

        private void OnEnable()
        {
            this.holdoutZoneController.calcChargeRate += this.ApplyChargeRate;
        }

        private void OnDisable()
        {
            this.holdoutZoneController.calcChargeRate -= this.ApplyChargeRate;
        }

        private void ApplyChargeRate(ref float rate)
        {
            float chargeRateBonus = (this.soulShardController.Then2(x => x.GetChargeRateBonus()) ?? 0) + (this.bloodShardController.Then2(x => x.GetChargeRateBonus()) ?? 0);

            if (chargeRateBonus > 0)
            {
                rate *= 1f + chargeRateBonus;
            }
        }
    }
}