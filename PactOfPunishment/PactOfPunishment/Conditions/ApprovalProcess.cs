using HG;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class ApprovalProcess : ConditionDef
    {
        public override int MaxRank => 2;

        public override int GetHeatForRank(int rank) => rank + 1;

        public override void Init()
        {
            On.RoR2.InfiniteTowerWaveController.DropRewards += this.InfiniteTowerWaveController_DropRewards; // TODO: also apply to more stuff like multishops?
        }

        private void InfiniteTowerWaveController_DropRewards(On.RoR2.InfiniteTowerWaveController.orig_DropRewards orig, RoR2.InfiniteTowerWaveController self)
        {
            if (this.GetRank(self) > 0)
            {
                this.Logger.LogDebug("Reducing reward option count...");
                var behavior = self.EnsureComponent<ApprovalProcessBehavior>();
                behavior.originalRewardOptionCount ??= self.rewardOptionCount;
                self.rewardOptionCount = Mathf.Max(1, behavior.originalRewardOptionCount.Value - this.GetRank(self));
            }

            orig(self);
        }

        public class ApprovalProcessBehavior : MonoBehaviour
        {
            public int? originalRewardOptionCount;
        }
    }
}