using System.Collections.Generic;

namespace PactOfPunishment.Conditions
{
    public class ConditionEqualityComparer : IEqualityComparer<Condition?>
    {
        public bool Equals(Condition? x, Condition? y)
        {
            return x?.ConditionDef == y?.ConditionDef;
        }

        public int GetHashCode(Condition? obj)
        {
            return obj?.ConditionDef.GetHashCode() ?? 0;
        }
    }
}