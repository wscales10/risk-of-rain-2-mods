using RoR2;
using UnityEngine;

namespace AssortedExperiments.Items
{
    public class SoulShardController : MonoBehaviour
    {
        private static readonly Color materialColor = new Color(0f, 3.94117641f, 5f, 1f);

        private static readonly float rampUpTime = 5f;

        private static readonly float startupDelay = 3f;

        private static readonly int cap = 3;

        private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private float currentValue;

        private HoldoutZoneController holdoutZoneController;

        private Run.FixedTimeStamp enabledTime;

        public int CurrentItemCount { get; private set; }

        // Consider smoothing out into a curve, but it's not super important.
        private float ChargeRateBonus => Mathf.Min(this.holdoutZoneController.baseChargeDuration, 60f) / 180f;

        public float GetChargeRateBonus()
        {
            int currentItemCount = this.CurrentItemCount;

            if (currentItemCount > 0)
            {
                return this.ChargeRateBonus * this.CurrentItemCount;
            }

            return 0;
        }

        private void Awake()
        {
            this.holdoutZoneController = base.GetComponent<HoldoutZoneController>();
        }

        private void OnEnable()
        {
            this.enabledTime = Run.FixedTimeStamp.now;
            this.holdoutZoneController.calcColor += this.ApplyColor;
        }

        private void OnDisable()
        {
            this.holdoutZoneController.calcColor -= this.ApplyColor;
        }

        private void ApplyColor(ref Color color)
        {
            // TODO: edit to match soul pillar zone color?
            color = Color.Lerp(color, SoulShardController.materialColor, SoulShardController.colorCurve.Evaluate(this.currentValue));
        }

        private void Update()
        {
            this.DoUpdate(Time.deltaTime);
        }

        private void DoUpdate(float deltaTime)
        {
            this.CurrentItemCount = Util.GetItemCountForTeam(this.holdoutZoneController.chargingTeam, Content.Items.SoulShard.itemIndex, true, false);
            if (this.enabledTime.timeSince < SoulShardController.startupDelay)
            {
                this.CurrentItemCount = 0;
            }
            this.CurrentItemCount = Mathf.Min(this.CurrentItemCount, SoulShardController.cap);
            float num = (this.CurrentItemCount > 0f) ? 1f : 0f;
            float num2 = Mathf.MoveTowards(this.currentValue, num, SoulShardController.rampUpTime * deltaTime);
            if (this.currentValue <= 0f && num2 > 0f)
            {
                Util.PlaySound("Play_item_lunar_focusedConvergence", base.gameObject);
            }
            this.currentValue = num2;
        }
    }
}