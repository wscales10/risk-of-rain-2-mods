using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class Mithrix : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        protected override string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBossBrother.prefab";

        protected override UpgradeWaveStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<UpgradeMithrix>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.EnsureComponent<MithrixMiniBossBehavior>();
        }

        public class MithrixMiniBossBehavior : BossFightBehavior
        {
            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                body.ScaleMaxHealth(this, 0.8f); // TODO: reduce phase 3 max health if encountered as mini-boss.
            }
        }

        public class UpgradeMithrix : UpgradeWaveStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MainBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                wave.combatDirector.AddSpawnListener(OnBossSpawnedServer);
                wave.EnsureComponent<PhaseCounter>().phase = 3;
            }

            private static void OnBossSpawnedServer(GameObject spawnedEntity)
            {
                var body = Utils.GetCharacterBody(spawnedEntity);

                if (body && body.name.Contains("Brother"))
                {
                    body.EnsureComponent<UpgradeMithrixBodyBehavior>();
                }
            }
        }

        public class UpgradeMithrixBodyBehavior : MonoBehaviour
        {
            public void Awake()
            {
                var body = this.GetComponent<CharacterBody>();
                body.ScaleMaxHealth(this, 8f / 7);
                body.inventory.GiveItemPermanent(RoR2Content.Items.SprintBonus, 2);
                body.inventory.GiveItemPermanent(RoR2Content.Items.SecondarySkillMagazine, 2);
                body.inventory.GiveItemPermanent(RoR2Content.Items.BoostAttackSpeed, 40);
            }
        }
    }
}