using RoR2;
using System;

namespace PactOfPunishment.Conditions
{
    public abstract class ConditionDef : Module, IConditionDef
    {
        public abstract int MaxRank { get; }

        public abstract int GetHeatForRank(int rank);

        public override void Init()
        {
            throw new NotImplementedException($"{nameof(ConditionDef)} '{this.GetType().Name}' has no initialisation logic.");
        }

        public int GetRank(UnityEngine.Object context)
        {
            return Run.instance.GetComponent<PactOfPunishmentBehavior>().GetRank(this);
        }
    }
}