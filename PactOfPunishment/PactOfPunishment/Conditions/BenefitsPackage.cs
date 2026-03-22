using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class BenefitsPackage : ConditionDef
    {
        public override int MaxRank => 2;

        public override int GetHeatForRank(int rank) => rank + 1;

        public override void Init()
        {
            On.RoR2.CombatDirector.Awake += this.CombatDirector_Awake;
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            orig(self);

            int rank = this.GetRank(self);

            if (rank > 0)
            {
                this.Logger.LogDebug($"Adding {nameof(ExtraEliteBuffsBehavior)} to combat director...");
                var behavior = self.EnsureComponent<ExtraEliteBuffsBehavior>();
                behavior.extraBuffCount = rank;
                behavior.ReRollExtraEliteDefs();
            }
        }

        public class ExtraEliteBuffsBehavior : MonoBehaviour
        {
            public int extraBuffCount;

            private readonly List<(BuffDef eliteBuff, Func<SpawnCard, bool> canSelectForSpawnCard)> extraEliteDefs = new List<(BuffDef, Func<SpawnCard, bool>)>();

            private CombatDirector combatDirector;

            public void ReRollExtraEliteDefs()
            {
                this.extraEliteDefs.Clear();
                var list = this.combatDirector.GetEliteBuffDefs().ToList();
                Util.ShuffleList(list, this.combatDirector.rng);
                this.extraEliteDefs.AddRange(list);
            }

            public void Awake()
            {
                this.combatDirector = this.GetComponent<CombatDirector>();
                MonsterTracker.TrackCombatDirector(this.combatDirector); // TODO: remove?
            }

            public void OnEnable()
            {
                this.combatDirector.EnsureComponent<InfiniteTowerWaveSpawnListener>().OnSpawnedServer += this.ExtraEliteBuffsBehavior_OnSpawnedServer; // TODO: check that this is late enough. Also, what about enemies spawned without the combat director?
            }

            public void OnDisable()
            {
                this.combatDirector.GetComponent<InfiniteTowerWaveSpawnListener>().OnSpawnedServer -= this.ExtraEliteBuffsBehavior_OnSpawnedServer;
            }

            private void ExtraEliteBuffsBehavior_OnSpawnedServer(SpawnCard.SpawnResult result)
            {
                if (!Utils.TryGetCharacterBody(result.spawnedInstance, out var characterBody) || characterBody.isBoss)
                {
                    return;
                }

                var activeEliteBuffs = BuffCatalog.eliteBuffIndices.Where(characterBody.HasBuff).ToArray();

                if (activeEliteBuffs.Length == 0)
                {
                    return;
                }

                int addedBuffs = 0;

                foreach (var eliteBuff in this.extraEliteDefs.Where(x => x.canSelectForSpawnCard(result.spawnRequest.spawnCard)).Select(x => x.eliteBuff))
                {
                    if (addedBuffs >= this.extraBuffCount)
                    {
                        break;
                    }

                    if (activeEliteBuffs.Contains(eliteBuff.buffIndex))
                    {
                        continue;
                    }

                    characterBody.AddBuff(eliteBuff);
                    addedBuffs++;
                }
            }
        }
    }
}