using RoR2;

namespace PerEnvironmentElites
{
    internal class SceneCatalogEnvironmentProvider : IEnvironmentProvider
    {
        public SceneDef GetCurrentEnvironment()
        {
            return SceneCatalog.currentSceneDef;
        }
    }
}
