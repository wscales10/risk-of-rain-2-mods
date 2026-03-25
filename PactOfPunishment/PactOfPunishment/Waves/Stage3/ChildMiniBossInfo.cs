using HG;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1;
using RoR2;
using static PactOfPunishment.Waves.Stage3.Summoner2;

namespace PactOfPunishment.Waves.Stage3
{
    public class ChildMiniBossInfo : PortableMiniBossInfo<Summoner2BossFightBehavior>
    {
        private readonly AssetPromise<CharacterSpawnCard> childSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Child/cscChild.asset");

        public override InfiniteTowerExplicitSpawnWaveController.SpawnInfo SpawnInfo => new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
        {
            count = 1,
            spawnCard = this.childSpawnCard.Value,
        };

        public override void SetupBossBody(CharacterBody body, Summoner2BossFightBehavior bossFightBehavior)
        {
            body.EnsureComponent<Summoner2BossBodyBehavior>();
            body.ScaleDifficultyAsBoss(2.5f, 30, true, false);
            body.inventory.GiveItemPermanent(RoR2Content.Items.AdaptiveArmor);
            body.ResistNonTargetedDamage();

            if (bossFightBehavior.disableTeleport)
            {
                body.EnsureComponent<DisableChildMonsterTeleport>();
            }
        }
    }
}