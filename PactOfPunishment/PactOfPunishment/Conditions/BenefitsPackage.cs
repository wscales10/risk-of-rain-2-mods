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
            throw new InvalidOperationException("Fix the bug with this condition then remove this");
            On.RoR2.CombatDirector.Awake += this.CombatDirector_Awake;
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            int rank = this.GetRank(self);

            if (rank > 0)
            {
                this.Logger.LogDebug($"Adding {nameof(ExtraEliteBuffsBehavior)} to combat director...");
                var behavior = self.EnsureComponent<ExtraEliteBuffsBehavior>();
                behavior.extraBuffCount = rank;
                behavior.ReRollExtraEliteDefs();
            }

            orig(self);
        }

        public class ExtraEliteBuffsBehavior : MonoBehaviour
        {
            public int extraBuffCount;

            private readonly List<BuffDef> extraEliteDefs = new List<BuffDef>();

            private CombatDirector combatDirector;

            public void ReRollExtraEliteDefs()
            {
                this.extraEliteDefs.Clear();
                var list = Utils.GetEliteDefs(this.combatDirector.currentMonsterCard.GetSpawnCard()).Select(x => x.eliteEquipmentDef.passiveBuffDef).ToList(); // TODO: at the moment I'm only calling this on awake but I'm trying to use the current monster card.
                Util.ShuffleList(list, this.combatDirector.rng);
                this.extraEliteDefs.AddRange(list);
            }

            public void Awake()
            {
                this.combatDirector = this.GetComponent<CombatDirector>();
                this.combatDirector.onSpawnedServer.AddListener(this.OnSpawnedServer); // TODO: check that this is late enough. Also, what about enemies spawned without the combat director?
            }

            private void OnSpawnedServer(GameObject spawnedEntity)
            {
                var characterBody = spawnedEntity.GetComponent<CharacterMaster>().GetBody();
                var activeEliteBuffs = BuffCatalog.eliteBuffIndices.Where(characterBody.HasBuff).ToArray();

                if (activeEliteBuffs.Length == 0)
                {
                    return;
                }

                int addedBuffs = 0;

                foreach (var current in this.extraEliteDefs)
                {
                    if (addedBuffs >= this.extraBuffCount)
                    {
                        break;
                    }

                    if (activeEliteBuffs.Contains(current.buffIndex))
                    {
                        continue;
                    }

                    spawnedEntity.GetComponent<CharacterMaster>().GetBody().AddBuff(current);
                    addedBuffs++;
                }
            }
        }
    }
}