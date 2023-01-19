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

		private readonly ConfigFile config;

		private readonly ConfigEntry<string> ruleLocation;

		public Configuration(ConfigFile configFile)
		{
			config = configFile;

			ruleLocation = config.Bind(PATHS, "Rule Location", string.Empty, "Path to rule xml file");
			ModSettingsManager.AddOption(new StringInputFieldOption(ruleLocation, false));
			ruleLocation.SettingChanged += (s, e) => RuleLocationChanged?.Invoke();
		}

		public event Action RuleLocationChanged;

		public string RuleLocation => ruleLocation.Value;
	}
}