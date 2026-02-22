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

            if (rank > 0) // TODO: somehow don't apply effects during boss waves?
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
                var list = Utils.GetEliteBuffDefs().ToList();
                Util.ShuffleList(list, this.combatDirector.rng);
                this.extraEliteDefs.AddRange(list);
            }

            public void Awake()
            {
                this.combatDirector = this.GetComponent<CombatDirector>();
                MonsterTracker.TrackCombatDirector(this.combatDirector);
            }

            public void OnEnable()
            {
                SpawnCard.onSpawnedServerGlobal += this.SpawnCard_onSpawnedServerGlobal; // TODO: check that this is late enough. Also, what about enemies spawned without the combat director?
            }

            private void SpawnCard_onSpawnedServerGlobal(SpawnCard.SpawnResult result)
            {
                if(!MonsterTracker.Match(this.combatDirector, result))
                {
                    return;
                }

                var characterBody = Utils.GetCharacterBody(result.spawnedInstance);

                if (!characterBody || characterBody!.isBoss)
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

            public void OnDisable()
            {
                SpawnCard.onSpawnedServerGlobal -= this.SpawnCard_onSpawnedServerGlobal;
            }
        }
    }
}