using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class Summoner2 : PortableMiniBossWaveDefinition<Summoner2BossFightBehavior>
    {
        public Summoner2() : base(new ChildMiniBossInfo())
        {
        }

        protected override UpgradeEncounterStrategy? GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<EnableChildTeleportStrategy>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            dir.maxSquadCount = 6; // TODO: make max squad count calculation more intelligent
        }

        public class Summoner2BossBodyBehavior : MonoBehaviour
        {
            public void OnEnable()
            {
                RecalculateStats.Add(this.GetComponent<CharacterBody>(), OnRecalculateStats);
            }

            public void OnDisable()
            {
                RecalculateStats.Remove(this.GetComponent<CharacterBody>(), OnRecalculateStats);
            }

            private static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedTotalMult = 0;
            }
        }

        public class EnableChildTeleportStrategy : UpgradeEncounterStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                ctx.GameObject.GetComponent<Summoner2BossFightBehavior>().disableTeleport = false;
            }
        }
    }
}