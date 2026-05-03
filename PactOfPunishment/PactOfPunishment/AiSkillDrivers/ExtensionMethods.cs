using RoR2;
using RoR2.CharacterAI;
using System.Collections.Generic;
using System.Linq;

namespace PactOfPunishment.AiSkillDrivers
{
    public static class ExtensionMethods
    {
        public static IEnumerable<AISkillDriver> GetSkillDrivers(this CharacterMaster master, SkillDriverPredicate? predicate = null)
        {
            return master.GetSkillDriversInternal(predicate).LogIfEmpty();
        }

        public static IEnumerable<AISkillDriver> GetSkillDrivers(this BaseAI ai, SkillDriverPredicate? predicate = null)
        {
            return ai.GetSkillDriversInternal(predicate).LogIfEmpty();
        }

        public static IEnumerable<AISkillDriver> GetSkillDrivers(this CharacterBody body, SkillDriverPredicate? predicate = null)
        {
            return body.GetSkillDriversInternal(predicate).LogIfEmpty();
        }

        private static IEnumerable<AISkillDriver> GetSkillDriversInternal(this CharacterMaster master, SkillDriverPredicate? predicate)
        {
            if (!master)
            {
                return Enumerable.Empty<AISkillDriver>();
            }

            return master.AiComponents.SelectMany(x => x.GetSkillDriversInternal(predicate));
        }

        private static IEnumerable<AISkillDriver> GetSkillDriversInternal(this CharacterBody body, SkillDriverPredicate? predicate)
        {
            if (!body)
            {
                return Enumerable.Empty<AISkillDriver>();
            }

            return body.master.GetSkillDriversInternal(predicate);
        }

        private static IEnumerable<AISkillDriver> GetSkillDriversInternal(this BaseAI ai, SkillDriverPredicate? predicate)
        {
            if (predicate is null)
            {
                return ai.skillDrivers;
            }
            else
            {
                return ai.skillDrivers.Where(predicate.IsMatch);
            }
        }

        public abstract class SkillDriverPredicate
        {
            public static implicit operator SkillDriverPredicate(string customName) => new CustomNameSkillDriverPredicate(customName);

            public static implicit operator SkillDriverPredicate(SkillSlot skillSlot) => new SkillSlotSkillDriverPredicate(skillSlot);

            public abstract bool IsMatch(AISkillDriver skillDriver);
        }

        public class CustomNameSkillDriverPredicate : SkillDriverPredicate
        {
            private readonly string customName;

            public CustomNameSkillDriverPredicate(string customName) => this.customName = customName;

            public override bool IsMatch(AISkillDriver skillDriver) => skillDriver.customName == this.customName;
        }

        public class SkillSlotSkillDriverPredicate : SkillDriverPredicate
        {
            private readonly SkillSlot skillSlot;

            public SkillSlotSkillDriverPredicate(SkillSlot skillSlot) => this.skillSlot = skillSlot;

            public override bool IsMatch(AISkillDriver skillDriver) => skillDriver.skillSlot == this.skillSlot;
        }
    }
}