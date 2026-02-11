using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class ConvenienceFee : DefaultConditionDef
    {
        public override int MaxRank => 2;

        public override void Init()
        {
            On.RoR2.TeamManager.AdjustCostForLongstandingSolitude += this.TeamManager_AdjustCostForLongstandingSolitude; // Used by pretty much every purchase interaction
            On.RoR2.HalcyoniteShrineInteractable.Awake += this.HalcyoniteShrineInteractable_Awake;
        }

        public int ScaleCost(Object context, float originalCost)
        {
            return Mathf.RoundToInt(originalCost * (1 + 0.4f * this.GetRank(context)));
        }

        private void HalcyoniteShrineInteractable_Awake(On.RoR2.HalcyoniteShrineInteractable.orig_Awake orig, HalcyoniteShrineInteractable self)
        {
            self.lowGoldCost = this.ScaleCost(self, self.lowGoldCost);
            self.midGoldCost = this.ScaleCost(self, self.midGoldCost);
            self.maxGoldCost = this.ScaleCost(self, self.maxGoldCost);
            orig(self);
        }

        private int TeamManager_AdjustCostForLongstandingSolitude(On.RoR2.TeamManager.orig_AdjustCostForLongstandingSolitude orig, RoR2.CostTypeIndex costType, int cost, RoR2.CharacterBody viewerBody)
        {
            var output = orig(costType, cost, viewerBody);

            if (costType != CostTypeIndex.Money || output == 0)
            {
                return output;
            }

            return this.ScaleCost(viewerBody, output);
        }
    }
}