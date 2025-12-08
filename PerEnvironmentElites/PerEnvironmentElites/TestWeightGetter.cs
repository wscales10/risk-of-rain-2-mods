using RoR2;

namespace PerEnvironmentElites
{
    internal class TestWeightGetter : IWeightGetter
    {
        public decimal GetWeight(EliteDef eliteDef, SceneDef env)
        {
            switch (env.cachedName)
            {
                case "SnowyForest":
                case "FrozenWall":

                    if (eliteDef == RoR2Content.Elites.Fire || eliteDef == RoR2Content.Elites.FireHonor)
                    {
                        return 0;
                    }
                    else if (eliteDef == RoR2Content.Elites.Ice || eliteDef == RoR2Content.Elites.IceHonor)
                    {
                        return 4;
                    }
                    else
                    {
                        return 1;
                    }
            }

            return 1;
        }

        public void Init()
        {
            // Do nothing.
        }
    }
}