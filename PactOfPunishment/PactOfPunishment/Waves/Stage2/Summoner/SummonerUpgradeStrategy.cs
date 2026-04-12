using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Stage2.Summoner;
using RoR2;
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
                ctx.Controller.GetComponent<SummonerBossFightBehavior>().OnBossSpawnedServer += OnBossSpawnedServer;
                ctx.Controller.EnsureComponent<SafeZoneRadiusCapper>().RadiusMultiplier = 0.75f; // Any smaller risks spawning monsters outside zone. This might even be too small, depending on what's used for the spawn target.
            }

            private static void OnBossSpawnedServer(CharacterBody body)
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