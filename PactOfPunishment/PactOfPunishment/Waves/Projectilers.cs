using HG;
using PactOfPunishment.Conditions;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Waves
{
    public class Projectilers : MiniBossWaveDefinition<InfiniteTowerBossWaveController>
    {
        protected override string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBoss.prefab";

        protected override UpgradeWaveStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<AllMalachiteWaveStrategy>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerBossWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            var monsterKeys = new string[]
            {
                "RoR2/Base/Vulture/cscVulture.asset",
                "RoR2/DLC1/FlyingVermin/cscFlyingVermin.asset",
                "RoR2/Base/Bell/cscBell.asset",
                "RoR2/DLC2/Child/cscChild.asset",
                "RoR2/Base/LemurianBruiser/cscLemurianBruiser.asset",
                "RoR2/Base/Lemurian/cscLemurian.asset", // Could add some more but they're used in other events
            };

            var monsterCards = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();
            monsterCards.AddCategory("Projectilers", 1);

            foreach (var monsterKey in monsterKeys)
            {
                monsterCards.AddCard(0, new DirectorCard
                {
                    selectionWeight = 1,
                    spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(monsterKey).WaitForCompletion()
                });
            }

            dir.monsterCards = monsterCards;
        }

        public class AllMalachiteWaveStrategy : UpgradeWaveStrategy
        {
            public override void UpgradeWave(InfiniteTowerWaveController wave)
            {
                wave.combatDirector.ActiveEliteDefOverride = RoR2Content.Elites.Poison;
                wave.combatDirector.EnsureComponent<KeepEliteDefOverrideBehavior>().AddCombatDirector(wave.combatDirector);
            }
        }
    }
}