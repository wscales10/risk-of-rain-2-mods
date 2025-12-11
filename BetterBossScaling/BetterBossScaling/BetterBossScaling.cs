using BepInEx;
using RoR2;

namespace BetterBossScaling
{
    [BepInPlugin("com.woodyscales.betterbossscaling", "Better Boss Scaling", "1.0.0")]
    [BepInDependency("com.rune580.riskofoptions")]
    public class BetterBossScaling : BaseUnityPlugin
    {
        private Config settings;

        private float? teleporterStartTime;

        public void Awake()
        {
            this.settings = new Config(this.Config);
            On.RoR2.SceneCatalog.OnActiveSceneChanged += this.SceneCatalog_OnActiveSceneChanged;
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += this.ChargingState_OnEnter;
            On.RoR2.BossGroup.OnMemberAddedServer += this.BossGroup_OnMemberAddedServer;
            On.RoR2.BossGroup.OnDefeatedServer += this.BossGroup_OnDefeatedServer;

        }

        private static int? GetCurrentStageNumber()
        {
            int? stageClearCount = Run.instance.stageClearCount;
            return stageClearCount is null ? null : stageClearCount + 1;
        }

        private static string str(object obj, string nullString = "??")
        {
            return obj is null ? nullString : obj.ToString();
        }
        private bool IsTeleporterBossHealthComponent(HealthComponent hc)
        {
            var tp = TeleporterInteraction.instance;

            var teleporterBosses = tp?.bossGroup?.combatSquad?.readOnlyMembersList;
            var characterMaster = hc?.body?.master;

            if (teleporterBosses is null || characterMaster is null)
            {
                return false;
            }

            if (teleporterBosses.Contains(characterMaster))
            {
                this.Logger.LogDebug("Identified teleporter boss health component: " + str(characterMaster.name));
                return true;
            }
            else
            {
                if (characterMaster.isBoss)
                {
                    this.Logger.LogDebug("Non-teleporter boss health component: " + str(characterMaster.name));
                }

                return false;
            }
        }

        private void SceneCatalog_OnActiveSceneChanged(On.RoR2.SceneCatalog.orig_OnActiveSceneChanged orig, UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
        {
            orig(oldScene, newScene);
            this.teleporterStartTime = null;
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, TeleporterInteraction.ChargingState self)
        {
            orig(self);
            this.teleporterStartTime = Run.instance.GetRunStopwatch();
        }

        private void BossGroup_OnDefeatedServer(On.RoR2.BossGroup.orig_OnDefeatedServer orig, BossGroup self)
        {
            var tp = TeleporterInteraction.instance;

            if (self == tp?.bossGroup)
            {
                TimeSpan? timeDiff = null;

                if (this.teleporterStartTime is float tpStartTime)
                {
                    float? currentTime = Run.instance?.GetRunStopwatch();

                    if (currentTime != null)
                    {
                        timeDiff = TimeSpan.FromSeconds(currentTime.Value - tpStartTime);
                    }
                }

                this.Logger.LogDebug($"Boss group defeated on stage {str(GetCurrentStageNumber())} with diff coef {str(Run.instance?.difficultyCoefficient)} when teleporter {str(tp.chargePercent)}% charged (took {str(timeDiff)}).");
            }

            orig(self);
        }

        private void BossGroup_OnMemberAddedServer(On.RoR2.BossGroup.orig_OnMemberAddedServer orig, BossGroup self, CharacterMaster memberMaster)
        {
            orig(self, memberMaster);

            if (self != TeleporterInteraction.instance?.bossGroup)
            {
                return;
            }

            var difficulty = Run.instance.selectedDifficulty;

            if (difficulty < DifficultyIndex.Normal)
            {
                return;
            }

            var hpDivisor = this.settings.HpDivisor.Value;
            var damageDivisor = this.settings.DamageDivisor.Value;

            this.Logger.LogDebug($"Scaling boss '{memberMaster.name}'. Diff coef: {Run.instance.difficultyCoefficient}. HP divisor: {hpDivisor}. Damage divisor: {damageDivisor}.");
            memberMaster.ScaleDifficultyAsBoss(hpDivisor, damageDivisor);

            if (difficulty < DifficultyIndex.Hard || !this.settings.EnableAdaptiveArmor.Value)
            {
                return;
            }

            this.Logger.LogDebug("Giving boss adaptive armor: " + memberMaster.name);
            memberMaster.inventory.GiveItemPermanent(RoR2Content.Items.AdaptiveArmor);
        }
    }
}