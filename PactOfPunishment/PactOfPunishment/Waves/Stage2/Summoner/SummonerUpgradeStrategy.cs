using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Stage2.Summoner;
using UnityEngine;

namespace PactOfPunishment.Waves.Summoner
{
    public partial class Summoner
    {
        public class SummonerUpgradeStrategy : UpgradeEncounterStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MainBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                ctx.CombatDirector.AddSpawnListener(OnBossSpawnedServer);
            }

            private static void OnBossSpawnedServer(GameObject spawnedInstance)
            {
                if (Utils.TryGetCharacterBody(spawnedInstance, out var body))
                {
                    if (body!.TryGetComponent<SummonerBossBodyBehavior>(out var behavior))
                    {
                        behavior.IsUpgraded = true;
                    }
                    else
                    {
                        Debug.LogWarning($"Unable to find summoner boss body behavior for '{body}'");
                    }
                }
            }
        }
    }
}