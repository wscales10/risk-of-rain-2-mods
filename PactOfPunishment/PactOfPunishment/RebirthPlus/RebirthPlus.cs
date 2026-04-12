using BepInEx.Configuration;
using HG;
using PactOfPunishment.Conditions;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Linq;
using UnityEngine.Networking;

namespace PactOfPunishment.RebirthPlus
{
    public class RebirthPlus : Module, IItemChoiceStrategy
    {
        private const string Section = "Starting Items";

        // TODO: this class should not be tied to the condition or settings, right?
        private ConfigEntry<int>? conditionConfigEntry;

        public static Func<int, int>? GetLevelCount { get; internal set; }

        public PickupInfo? ChoosePickup(ILevelInfo levelInfo)
        {
            if (this.Config.TryGetEntry<int>(Section, GetKey(levelInfo), out var configEntry))
            {
                return levelInfo.Options[configEntry.Value];
            }
            else
            {
                return null;
            }
        }

        public override void Init()
        {
            On.RoR2.Run.Awake += this.Run_Awake;
            RebirthPlusBehavior.itemChoiceStrategy = this;
            Utils.DoDuringGameLoad(this.AddConfigSettings);
        }

        private static string GetKey(ILevelInfo level)
        {
            return $"Level {level.Index + 1}";
        }

        private void AddConfigSettings()
        {
            var config = this.Config;
            var levels = RebirthPlusBehavior.GetLevels();
            foreach (var level in levels)
            {
                var configEntry = config.Bind(Section, GetKey(level), 0, string.Join(", " + Environment.NewLine, level.Options.Select((option, i) => $"{i} = {option}")));
                ModSettingsManager.AddOption(new IntSliderOption(configEntry, new IntSliderConfig
                {
                    name = string.Join(" / ", level.Options.Select(option => option.CountAgnosticString)),
                    restartRequired = false,
                    min = 0,
                    max = level.Options.Length - 1,
                    checkIfDisabled = () =>
                    {
                        if (Run.instance)
                        {
                            return true;
                        }

                        if (this.conditionConfigEntry is null && !config.TryGetEntry(PactOfPunishmentPlugin.Section, Utils.SplitPascalCaseString(typeof(DisableStartingItems).Name), out this.conditionConfigEntry))
                        {
                            return false;
                        }

                        var levelCount = DisableStartingItems.Instance.GetLevelCount(levels.Length, this.conditionConfigEntry.Value);
                        return level.Index >= levelCount;
                    }
                }));
            }
        }

        private void Run_Awake(On.RoR2.Run.orig_Awake orig, Run self)
        {
            if (NetworkServer.active)
            {
                self.EnsureComponent<RebirthPlusBehavior>();
            }

            orig(self);
        }
    }
}