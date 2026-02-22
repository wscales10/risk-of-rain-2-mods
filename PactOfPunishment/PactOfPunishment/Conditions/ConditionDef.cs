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
            var run = Run.instance;

            if (run && run.TryGetComponent<PactOfPunishmentBehavior>(out var behavior))
            {
                return behavior.GetRank(this);
            }

            return 0;
        }

        public bool IsEnabled(UnityEngine.Object context)
        {
            return this.GetRank(context) > 0;
        }
    }
}