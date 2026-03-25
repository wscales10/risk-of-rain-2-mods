using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage1
{
    public class ImpOverlordBossFightBehavior : PortableMiniBossFightBehavior<ImpOverlordBossFightBehavior>
    {
    }

    public partial class ImpOverlord : PortableMiniBossWaveDefinition<ImpOverlordBossFightBehavior>
    {
        public ImpOverlord() : base(ScriptableObject.CreateInstance<ImpOverlordMiniBossInfo>())
        {
        }

        protected override UpgradeEncounterStrategy GetUpgradeStrategy() => ScriptableObject.CreateInstance<PeriodicallySpawnGlacialJellyfish>();

        protected override PickupDropTable GetRewardDropTable(Run run) // TODO: keep an eye on this, it might be too good
        {
            return BetterExplicitPickupDropTable.ReplaceTierWithSingleItem(GetBaseDropTable(run), RoR2Content.Items.BleedOnHitAndExplode);
        }

        public class ImpOverlordMiniBossInfo : PortableMiniBossInfo<ImpOverlordBossFightBehavior>
        {
            private readonly AssetPromise<CharacterSpawnCard> impOverlordSpawnCard;

            public ImpOverlordMiniBossInfo()
            {
                this.impOverlordSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/ImpBoss/cscImpBoss.asset");
            }

            public override SpawnInfo SpawnInfo => new SpawnInfo
            {
                count = 1,
                spawnCard = this.impOverlordSpawnCard.Value,
            };

            public override void SetupBossBody(CharacterBody body, ImpOverlordBossFightBehavior bossFightBehavior)
            {
                body.ScaleMaxHealth(this, 0.4f);
                body.ScaleDifficultyAsBoss(39, 31, true, false);
                body.skillLocator.utility.cooldownOverride = 2;
            }
        }

        public class PeriodicallySpawnGlacialJellyfish : UpgradeEncounterStrategy
        {
            private readonly CharacterSpawnCard jellyfishSpawnCard;

            public PeriodicallySpawnGlacialJellyfish()
            {
                this.jellyfishSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Jellyfish/cscJellyfish.asset").WaitForCompletion();
            }

            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                var behavior = ctx.GameObject.AddComponent<DoSomethingAtFixedRate>();
                behavior.interval = 4;
                behavior.doSomething = () =>
                {
                    if (!ctx.SpawnTarget)
                    {
                        return;
                    }

                    DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Standard, out var minDistance, out var maxDistance);

                    var directorSpawnRequest = new DirectorSpawnRequest(this.jellyfishSpawnCard, new DirectorPlacementRule
                    {
                        minDistance = minDistance,
                        maxDistance = maxDistance,
                        position = ctx.SpawnTarget.transform.position
                    }, RoR2Application.rng)
                    {
                        teamIndexOverride = TeamIndex.Monster,
                    };

                    var jellyfish = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

                    if (!jellyfish)
                    {
                        return;
                    }

                    ctx.CombatSquad.AddMember(jellyfish.GetComponent<CharacterMaster>());

                    if (Utils.TryGetCharacterBody(jellyfish, out var jellyfishBody))
                    {
                        Utils.ScaleDeathRewards(jellyfishBody, 0);
                        Utils.MakeUnscaledEliteUsingEquipment(jellyfishBody, RoR2Content.Elites.Ice);
                    }
                };

                var bossFightBehavior = ctx.GameObject.GetComponent<ImpOverlordBossFightBehavior>();
                bossFightBehavior.OnSetEnabled += customEnabled => behavior.enabled = customEnabled;
                bossFightBehavior.ApplyEnabledState();

                ctx.GameObject.EliminateCombatSquadWhenLastMainMemberDies(ctx.CombatSquad, x => x.GetBody().Is(RoR2Content.BodyPrefabs.ImpBossBody), x => x.GetBody().Is(RoR2Content.BodyPrefabs.JellyfishBody), () => behavior.enabled = false);
            }
        }
    }
}