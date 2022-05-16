using BepInEx.Configuration;

namespace Ror2Mod2
{
    public class Configuration
    {
        private const string PATHS = "paths";

        private readonly ConfigFile config;

        private readonly ConfigEntry<string> ruleLocation;

        public Configuration(ConfigFile configFile)
        {
            config = configFile;
            ruleLocation = config.Bind(PATHS, "ruleLocation", string.Empty, "Path to rule xml");
        }

        public string RuleLocation => ruleLocation.Value;
    }
}