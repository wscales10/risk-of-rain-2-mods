using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;

namespace BetterBossScaling
{
    internal class Config
    {
        public Config(ConfigFile config)
        {
            this.HpDivisor = config.Bind("Better Boss Scaling", "Teleporter Boss HP Bonus Divisor", 11.5f, "The divisor for bonus HP scaling of teleporter bosses.");
            this.DamageDivisor = config.Bind("Better Boss Scaling", "Teleporter Boss Damage Bonus Divisor", 35f, "The divisor for bonus damage scaling of teleporter bosses.");
            this.EnableAdaptiveArmor = config.Bind("Better Boss Scaling", "Enable Adaptive Armor on Monsoon+", true, "If true, teleporter bosses will gain Adaptive Armor on Monsoon difficulty and above.");
            this.DamageReducesBossMaxHealth = config.Bind("Better Boss Scaling", "Damage Reduces TP Boss Max HP", false, "If true, the max health of teleporter bosses will be reduced as they take damage.");
            ModSettingsManager.AddOption(new StepSliderOption(this.HpDivisor, new StepSliderConfig() { min = 1, max = 100, increment = 0.5f }));
            ModSettingsManager.AddOption(new StepSliderOption(this.DamageDivisor, new StepSliderConfig() { min = 1, max = 100, increment = 0.5f }));
            ModSettingsManager.AddOption(new CheckBoxOption(this.EnableAdaptiveArmor));
            ModSettingsManager.AddOption(new CheckBoxOption(this.DamageReducesBossMaxHealth, restartRequired: true));
        }

        public ConfigEntry<float> HpDivisor { get; }

        public ConfigEntry<float> DamageDivisor { get; }

        public ConfigEntry<bool> EnableAdaptiveArmor { get; }

        public ConfigEntry<bool> DamageReducesBossMaxHealth { get; }
    }
}
