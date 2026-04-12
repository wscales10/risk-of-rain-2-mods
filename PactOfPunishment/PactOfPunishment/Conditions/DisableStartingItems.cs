using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class DisableStartingItems : DefaultConditionDef
    {
        private DisableStartingItems()
        {
        }

        // TODO: DRY
        private const int itemsDisabledPerRank = 3;

        public override string Description => string.Format(base.Description, itemsDisabledPerRank);

        public static DisableStartingItems Instance { get; } = new DisableStartingItems();

        public override int MaxRank => 4;

        public override int HeatPerRank => 2;

        public override void Init()
        {
            RebirthPlus.RebirthPlus.GetLevelCount = this.GetLevelCount;
        }

        internal int GetLevelCount(int maxCount, int rank)
        {
            return Mathf.RoundToInt(maxCount * (1 - rank / (float)this.MaxRank));
        }

        private int GetLevelCount(int maxCount)
        {
            return this.GetLevelCount(maxCount, this.GetRank(Run.instance));
        }
    }
}