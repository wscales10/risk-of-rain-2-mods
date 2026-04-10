using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class RoutineInspection : DefaultConditionDef
    {
        private RoutineInspection()
        {
        }

        public static RoutineInspection Instance { get; } = new RoutineInspection();

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
            return GetLevelCount(maxCount, this.GetRank(Run.instance));
        }
    }
}