using PactOfPunishment.Conditions;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage4
{
    public class ExtraMiniBossBehavior : MonoBehaviour
    {
        public InfiniteTowerExplicitSpawnWaveController.SpawnInfo extraBossSpawnInfo;

        public float spawnDelay = -1;

        public EncounterContext? ctx;

        private float timer;

        public bool HasSpawnedExtraBoss { get; private set; }

        public void Update()
        {
            this.ManagedUpdate(Time.deltaTime);
        }

        private void ManagedUpdate(float deltaTime)
        {
            if (this.spawnDelay < 0 || this.ctx is null || this.HasSpawnedExtraBoss)
            {
                return;
            }

            this.timer += deltaTime;

            if (this.timer > this.spawnDelay || (this.timer > 1 && this.ctx.CombatSquad.memberCount == 0))
            {
                this.SpawnExtraBoss(this.ctx);
                this.HasSpawnedExtraBoss = true;
            }
        }

        private void SpawnExtraBoss(EncounterContext ctx)
        {
            var bossFightBehavior = ctx.Controller.GetComponent<Stage4MiniBossFightBehavior>();
            bool hadMainBossSpawned = bossFightBehavior.haveMainBossesSpawned;
            bossFightBehavior.haveMainBossesSpawned = false;
            ctx.CombatDirector.Spawn(this.extraBossSpawnInfo.spawnCard, this.extraBossSpawnInfo.eliteDef, ctx.SpawnTarget.transform, this.extraBossSpawnInfo.spawnDistance, this.extraBossSpawnInfo.preventOverhead);
            bossFightBehavior.haveMainBossesSpawned = hadMainBossSpawned;
        }
    }

    public class AddExtraMiniBossUpgradeStrategy : UpgradeEncounterStrategy
    {
        public InfiniteTowerExplicitSpawnWaveController.SpawnInfo extraBossSpawnInfo;

        public float spawnDelay;

        public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

        public override void PostInitialise(EncounterContext ctx)
        {
            if (ctx.CombatDirector.maxSquadCount > 0)
            {
                ctx.CombatDirector.maxSquadCount++;
            }

            var behavior = ctx.GameObject.AddComponent<ExtraMiniBossBehavior>();
            behavior.ctx = ctx;
            behavior.extraBossSpawnInfo = this.extraBossSpawnInfo;
            behavior.spawnDelay = Mathf.Max(1, this.spawnDelay);
        }
    }
}