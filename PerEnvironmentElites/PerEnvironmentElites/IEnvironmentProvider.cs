using RoR2;

namespace PerEnvironmentElites
{
    public interface IEnvironmentProvider
    {
        SceneDef GetCurrentEnvironment();
    }
}