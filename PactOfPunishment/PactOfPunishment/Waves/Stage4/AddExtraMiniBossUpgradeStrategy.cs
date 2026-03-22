using PactOfPunishment.Conditions;
using RoR2;
using System.Collections;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage4
{
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

            ctx.Controller.StartCoroutine(this.WaitThenSpawnExtraBoss(ctx));
        }

        private IEnumerator WaitThenSpawnExtraBoss(EncounterContext ctx)
        {
            yield return new WaitForSeconds(Mathf.Max(this.spawnDelay, 1));
            var bossFightBehavior = ctx.Controller.GetComponent<Stage4MiniBossFightBehavior>();
            bool hadMainBossSpawned = bossFightBehavior.haveMainBossesSpawned;
            bossFightBehavior.haveMainBossesSpawned = false;
            ctx.CombatDirector.Spawn(this.extraBossSpawnInfo.spawnCard, this.extraBossSpawnInfo.eliteDef, ctx.SpawnTarget.transform, this.extraBossSpawnInfo.spawnDistance, this.extraBossSpawnInfo.preventOverhead);
            bossFightBehavior.haveMainBossesSpawned = hadMainBossSpawned;
        }
    }
}