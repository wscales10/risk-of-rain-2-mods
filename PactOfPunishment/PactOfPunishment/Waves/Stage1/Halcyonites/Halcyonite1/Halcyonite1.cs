using HG;
using RoR2;
using System.Linq;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1
{
    public class Halcyonite1 : Stage1HalcyoniteBossWaveDefinition
    {
        private readonly AssetPromise<CharacterSpawnCard>[] meleeSpawnCards = new string[] { "", "Nature", "Sandy", "Snowy" }.Select(env => Utils.BeginLoad<CharacterSpawnCard>($"RoR2/Base/Golem/cscGolem{env}.asset")).ToArray();

        private readonly AssetPromise<CharacterSpawnCard> rangedSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Child/cscChild.asset");

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            wavePrefab.EnsureComponent<Halcyonite1BossFightBehavior>();
            wavePrefab.EnsureComponent<KeepCombatDirectorEnabledBehavior>();
            dir.SetupCombatDirectorPrefabForAddsSpawning(Utils.MakeDirectorCardCategorySelection(
                ("Ranged", new[] { this.rangedSpawnCard }),
                ("Melee", this.meleeSpawnCards)
            ), 4, 5, 1, 3.06942795f);
        }
    }
}