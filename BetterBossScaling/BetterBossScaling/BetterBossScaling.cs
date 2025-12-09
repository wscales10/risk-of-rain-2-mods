using BepInEx;
using RoR2;

namespace BetterBossScaling
{
    [BepInPlugin("com.woodyscales.betterbossscaling", "Better Boss Scaling", "1.0.0")]
    public class BetterBossScaling : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.BossGroup.OnMemberAddedServer += this.BossGroup_OnMemberAddedServer;
            On.RoR2.BossGroup.OnDefeatedServer += this.BossGroup_OnDefeatedServer;
        }

        private void BossGroup_OnDefeatedServer(On.RoR2.BossGroup.orig_OnDefeatedServer orig, BossGroup self)
        {
            var tp = TeleporterInteraction.instance;

            if (self == tp?.bossGroup)
            {
                this.Logger.LogDebug($"Boss group defeated on stage {Run.instance.stageClearCount + 1} with diff coef {Run.instance.difficultyCoefficient} when teleporter {tp.chargePercent}% charged.");
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

            var hpDivisor = 2.5f;
            var damageDivisor = 30f;
            this.Logger.LogDebug($"Scaling boss '{memberMaster.name}'. Diff coef: {Run.instance.difficultyCoefficient}. HP divisor: {hpDivisor}. Damage divisor: {damageDivisor}.");
            memberMaster.ScaleDifficultyAsBoss(hpDivisor, damageDivisor);

            if (difficulty < DifficultyIndex.Hard)
            {
                return;
            }

            this.Logger.LogDebug("Giving boss adaptive armor: " + memberMaster.name);
            memberMaster.inventory.GiveItemPermanent(RoR2Content.Items.AdaptiveArmor);
        }
    }
}