using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage2
{
    public class Projectilers : MiniBossWaveDefinition<InfiniteTowerBossWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> brassContraptionSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/Bell/cscBell.asset");

        protected override string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBoss.prefab";

        protected override UpgradeWaveStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<AllMalachiteWaveStrategy>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerBossWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            var monsterCards = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();
            monsterCards.AddCategory("Projectilers", 1);

            monsterCards.AddCard(0, new DirectorCard
            {
                selectionWeight = 1,
                spawnCard = this.brassContraptionSpawnCard.Value,
            });

            dir.monsterCards = monsterCards;

            wavePrefab.wavePeriodSeconds = 2f; // TODO: test with drizzle, rainstorm and monsoon
        }

        public class AllMalachiteWaveStrategy : UpgradeWaveStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                wave.combatDirector.EnsureComponent<OverrideEliteTiersBehavior>().eliteTiers = new CombatDirector.EliteTierDef[] { Content.EliteTiers.NerfedPoisonTier };
            }

            internal static CombatDirector.EliteTierDef MakeEliteTierDef()
            {
                return new CombatDirector.EliteTierDef
                {
                    canSelectWithoutAvailableEliteDef = false,
                    costMultiplier = 9,
                    eliteTypes = new EliteDef[] { Content.Elites.NerfedPoison },
                    isAvailable = _ => true,
                };
            }

            internal static EliteDef MakeEliteDef()
            {
                var x = Instantiate(RoR2Content.Elites.Poison);
                x.damageBoostCoefficient = 3;
                x.healthBoostCoefficient = 6;
                x.eliteIndex = default;
                var customElite = new CustomElite(x, Enumerable.Empty<CombatDirector.EliteTierDef>());
                EliteAPI.Add(customElite);
                return customElite.EliteDef;
            }
        }
    }
}