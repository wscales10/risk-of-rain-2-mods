using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using System;
using UnityEngine.Events;

namespace Ror2Mod2
{
    public class Configuration
    {
        private const string PATHS = "Paths";

        private const string MUSIC = "Music";

        private readonly ConfigFile config;

        private readonly ConfigEntry<string> ruleLocation;

        public Configuration(ConfigFile configFile)
        {
            config = configFile;

            ruleLocation = config.Bind(PATHS, "Rule Location", string.Empty, "Path to rule xml file");
            ModSettingsManager.AddOption(new StringInputFieldOption(ruleLocation, false));
            ruleLocation.SettingChanged += (s, e) => RuleLocationChanged?.Invoke();

            ModSettingsManager.AddOption(new GenericButtonOption("Configuration", MUSIC, "Configure music connection", "Configuration", new UnityAction(() => ConfigurationPageRequested?.Invoke())));
        }

        public event Action RuleLocationChanged;

        public event Action ConfigurationPageRequested;

        public string RuleLocation => ruleLocation.Value;
    }
}