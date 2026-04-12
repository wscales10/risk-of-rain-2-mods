using RoR2;
using System;

namespace PactOfPunishment.Conditions
{
    public abstract class ConditionDef : Module, IConditionDef
    {
        public abstract int MaxRank { get; }

        public string NameToken => $"CONDITION_{this.GetType().Name.ToUpper()}_NAME";

        public string DescriptionToken => $"CONDITION_{this.GetType().Name.ToUpper()}_DESCRIPTION";

        public string Name => Language.GetString(this.NameToken);

        public virtual string Description => Language.GetString(this.DescriptionToken);

        public abstract int GetHeatForRank(int rank);

        public override void Init()
        {
            throw new NotImplementedException($"{nameof(ConditionDef)} '{this.GetType().Name}' has no initialisation logic.");
        }

        public int GetRank(UnityEngine.Object context)
        {
            var run = Run.instance;

            // TODO: if we decide to display conditions in the UI outside of the context of a run, we'll need to change this
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