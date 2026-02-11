using HG;
using PactOfPunishment.Conditions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PactOfPunishment.Waves
{
    public class MithrixModule : Module
    {
        public override void Init()
        {
            On.EntityStates.BrotherMonster.HoldSkyLeap.OnEnter += this.HoldSkyLeap_OnEnter;
        }

        private void HoldSkyLeap_OnEnter(On.EntityStates.BrotherMonster.HoldSkyLeap.orig_OnEnter orig, EntityStates.BrotherMonster.HoldSkyLeap self)
        {
            if (self.TryGetComponent<Mithrix.DipOnLowHealthBehavior>(out var behavior)&& behavior.timeToGo)
            {
                
            }
        }
    }

    public class Mithrix : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        protected override string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBossBrother.prefab";

        protected override UpgradeWaveStrategy GetUpgradeStrategy()
        {
            throw new NotImplementedException();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
        }

        public class MithrixBehavior : MonoBehaviour
        {
            public void Awake()
            {
                (this.GetComponent<CombatDirector>().onSpawnedServer ??= new CombatDirector.OnSpawnedServer()).AddListener(this.OnBossSpawnedServer);
            }

            private void OnBossSpawnedServer(GameObject spawnedEntity)
            {
                var body = Utils.GetCharacterBody(spawnedEntity);

                if (!body)
                {
                    return;
                }

                body.EnsureComponent<MakeAllDamageNonLethalBehavior>();
            }
        }

        public class DipOnLowHealthBehavior : MonoBehaviour, IOnTakeDamageServerReceiver
        {
            public bool timeToGo;

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (damageReport.victim.combinedHealthFraction < 0.2f)
                {
                    this.timeToGo = true;
                    damageReport.victimBody.skillLocator.special.ResetStock();
                }
            }
        }
    }
}