using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage1
{
    public partial class ImpOverlord : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController> // TODO: custom reward drop table
    {
        private readonly AssetPromise<CharacterSpawnCard> impOverlordSpawnCard;

        public ImpOverlord()
        {
            this.impOverlordSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/ImpBoss/cscImpBoss.asset");
        }

        protected override UpgradeWaveStrategy GetUpgradeStrategy() => ScriptableObject.CreateInstance<PeriodicallySpawnGlacialJellyfish>();

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.impOverlordSpawnCard.Value,
                }
            };

            dir.EnsureComponent<ImpOverlordBossFightBehavior>();
        }

        public class ImpOverlordBossFightBehavior : BossFightBehavior
        {
            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                if (body.Is(RoR2Content.BodyPrefabs.ImpBossBody))
                {
                    body.ScaleDifficultyAsBoss(39, 31, true, false);
                    body.OverrideCooldown(x => x.utility, 2);
                }
            }
        }

        public class PeriodicallySpawnGlacialJellyfish : UpgradeWaveStrategy
        {
            private readonly CharacterSpawnCard jellyfishSpawnCard;

            public PeriodicallySpawnGlacialJellyfish()
            {
                this.jellyfishSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Jellyfish/cscJellyfish.asset").WaitForCompletion();
            }

            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                var behavior = wave.gameObject.AddComponent<DoSomethingAtFixedRate>();
                behavior.interval = 4;
                behavior.doSomething = () =>
                {
                    if (!wave.spawnTarget)
                    {
                        return;
                    }

                    DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Standard, out var minDistance, out var maxDistance);

                    var directorSpawnRequest = new DirectorSpawnRequest(this.jellyfishSpawnCard, new DirectorPlacementRule
                    {
                        minDistance = minDistance,
                        maxDistance = maxDistance,
                        position = wave.spawnTarget.transform.position
                    }, RoR2Application.rng)
                    {
                        teamIndexOverride = TeamIndex.Monster,
                    };

                    var jellyfish = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

                    if (!jellyfish)
                    {
                        return;
                    }

                    wave.combatSquad.AddMember(jellyfish.GetComponent<CharacterMaster>());
                    Utils.ScaleDeathRewards(Utils.GetCharacterBody(jellyfish), 0);
                    Inventory jellyfishInventory = jellyfish.GetComponent<Inventory>();
                    Utils.MakeUnscaledElite(jellyfishInventory, RoR2Content.Elites.Ice);
                };

                wave.EliminateCombatSquadWhenLastMainMemberDies(wave.combatSquad, x => x.GetBody().Is(RoR2Content.BodyPrefabs.ImpBossBody), () => behavior.enabled = false);
            }
        }
    }
}