using PactOfPunishment.Conditions;
using RoR2;
using System;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves
{
    public class WormAndDistributor : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private static readonly Lazy<CharacterSpawnCard> wormSpawnCard = GetLazySpawnCard("RoR2/DLC2/Scorchling/cscScorchling.asset");

        private static readonly Lazy<CharacterSpawnCard> distributorSpawnCard = GetLazySpawnCard("RoR2/DLC3/MinePod/cscMinePod.asset");

        protected override UpgradeWaveStrategy GetUpgradeMiniBossStrategy()
        {
            return ScriptableObject.CreateInstance<EnableSecondarySkillsStrategy>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = wormSpawnCard.Value,
                },
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = distributorSpawnCard.Value,
                }
            };

            dir.gameObject.AddComponent<WormAndDistributorBossBehavior>();
        }

        public class WormAndDistributorBossBehavior : MonoBehaviour
        {
            public bool disableSecondarySkills = true;

            public void Awake()
            {
                var dir = this.GetComponent<CombatDirector>();
                dir.onSpawnedServer ??= new CombatDirector.OnSpawnedServer();
                dir.onSpawnedServer.AddListener(this.OnBossSpawnedServer);
            }

            private void OnBossSpawnedServer(GameObject spawnedEntity)
            {
                var body = Utils.GetCharacterBody(spawnedEntity);
                if (!body)
                {
                    return;
                }

                if (body!.bodyIndex == DLC2Content.BodyPrefabs.ScorchlingBody.bodyIndex) // TODO: use elite equipment instead of elite def for distributor. The distributor should have nearly 2x the health of the worm, but should not do much damage.
                {
                    body.master.ScaleDifficultyAsBoss(0.68f, 2.2f);

                    if (this.disableSecondarySkills)
                    {
                        Utils.DisableSkill(body, x => x.secondary);
                    }
                }
                else if (body.bodyIndex == DLC3Content.BodyPrefabs.MinePodBody.bodyIndex)
                {
                    body.master.ScaleDifficultyAsBoss(0.2f, 100);
                    Utils.MakeUnscaledElite(body.inventory, RoR2Content.Elites.Ice);

                    if (this.disableSecondarySkills)
                    {
                        Utils.DisableSkill(body, x => x.secondary);
                    }
                }
            }
        }

        public class EnableSecondarySkillsStrategy : UpgradeWaveStrategy
        {
            public override void UpgradeWave(InfiniteTowerWaveController wave)
            {
                var dir = wave.combatDirector;

                if (dir.TryGetComponent<WormAndDistributorBossBehavior>(out var behavior))
                {
                    behavior.disableSecondarySkills = false;
                }
            }
        }
    }
}