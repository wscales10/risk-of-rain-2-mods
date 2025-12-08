using RoR2;

namespace PerEnvironmentElites
{
    public interface IWeightGetter
    {
        decimal GetWeight(EliteDef eliteDef, SceneDef env);

        void Init();
    }
}